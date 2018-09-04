using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using bd.webapprm.servicios.Interfaces;
using bd.webapprm.entidades.Utils;
using bd.log.guardar.Servicios;
using bd.log.guardar.ObjectTranfer;
using bd.log.guardar.Enumeradores;
using Newtonsoft.Json;
using bd.webapprm.entidades;
using bbd.webapprm.servicios.Enumeradores;
using Microsoft.AspNetCore.Mvc.Rendering;
using bd.webapprm.entidades.ObjectTransfer;
using bd.webapprm.servicios.Extensores;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace bd.webapprm.web.Controllers.MVC
{
    public class ProveeduriaController : Controller
    {
        private readonly IApiServicio apiServicio;
        private readonly IClaimsTransfer claimsTransfer;

        public ProveeduriaController(IApiServicio apiServicio, IClaimsTransfer claimsTransfer)
        {
            this.apiServicio = apiServicio;
            this.claimsTransfer = claimsTransfer;
        }

        #region Orden de compra
        public async Task<IActionResult> ListadoOrdenCompraEntramite() => await ListadoOrdenCompra(Estados.EnTramite);

        public async Task<IActionResult> ListadoOrdenCompraProcesadas() => await ListadoOrdenCompra(Estados.Procesada);

        public async Task<IActionResult> ListadoOrdenCompra(string estado)
        {
            var lista = new List<OrdenCompra>();
            ViewData["Estado"] = estado;
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<OrdenCompra>>(estado, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ListadoOrdenCompraPorEstado");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando órdenes de compra", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoOrdenCompra", lista);
        }

        public async Task<IActionResult> GestionarOrdenCompraEnTramite(int? id)
        {
            try
            {
                ViewData["Proveedor"] = new SelectList(await apiServicio.ObtenerElementoAsync<List<Proveedor>>(new ProveedorTransfer { LineaServicio = LineasServicio.Proveeduria, Activo = true }, new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedoresPorLineaServicioEstado"), "IdProveedor", "RazonSocial");
                ViewData["MotivoRecepcionArticulos"] = new SelectList(await apiServicio.Listar<MotivoRecepcionArticulos>(new Uri(WebApp.BaseAddressRM), "api/MotivoRecepcionArticulos/ListarMotivoRecepcionArticulos"), "IdMotivoRecepcionArticulos", "Descripcion");
                var claimTransfer = claimsTransfer.ObtenerClaimsTransferHttpContext();
                ViewData["Sucursal"] = new SelectList(claimsTransfer.IsADMIGrupo(ADMI_Grupos.AdminNacionalProveeduria) ? await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal") : new List<Sucursal> { new Sucursal { IdSucursal = claimTransfer.IdSucursal, Nombre = claimTransfer.NombreSucursal } }, "IdSucursal", "Nombre");

                if (claimsTransfer.IsADMIGrupo(ADMI_Grupos.AdminZonalProveeduria))
                    ViewData["Bodega"] = await apiServicio.ObtenerElementoAsync<Bodega>(claimTransfer.IdDependencia, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/BodegaPorDependencia");

                if (id != null)
                {
                    var response = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ObtenerOrdenCompra");
                    if (!response.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoOrdenCompraEntramite));

                    var ordenCompra = JsonConvert.DeserializeObject<OrdenCompra>(response.Resultado.ToString());

                    if (claimsTransfer.IsADMIGrupo(ADMI_Grupos.AdminNacionalProveeduria))
                        ViewData["Bodega"] = await ObtenerSelectListBodega(ordenCompra?.Bodega?.IdSucursal ?? -1);

                    ViewData["Empleado"] = await ObtenerSelectListEmpleado(ordenCompra.Bodega.IdSucursal, ordenCompra.IdEmpleadoDevolucion);
                    return View(ordenCompra);
                }

                if (claimsTransfer.IsADMIGrupo(ADMI_Grupos.AdminNacionalProveeduria))
                    ViewData["Bodega"] = await ObtenerSelectListBodega((ViewData["Sucursal"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Sucursal"] as SelectList).FirstOrDefault().Value) : -1);

                ViewData["Empleado"] = await ObtenerSelectListEmpleado((ViewData["Sucursal"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Sucursal"] as SelectList).FirstOrDefault().Value) : -1);
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoOrdenCompraEntramite));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GestionarOrdenCompraEnTramite(OrdenCompra ordenCompra, List<IFormFile> file)
        {
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.EnTramite }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                var listaFormDatosEspecificos = Request.Form.Where(c => c.Key.StartsWith("hIdRecepcionActivoFijoDetalle_"));
                ordenCompra.OrdenCompraDetalles = new List<OrdenCompraDetalles>();
                var response = new Response();
                foreach (var item in listaFormDatosEspecificos)
                {
                    int posFormDatoEspecifico = int.Parse(item.Key.ToString().Split('_')[1]);
                    int cantidad = int.Parse(Request.Form[$"cantidad_{posFormDatoEspecifico}"].ToString().Replace(",", ""));
                    decimal valorUnitario = decimal.Parse(Request.Form[$"valorUnitario_{posFormDatoEspecifico}"].ToString().Replace(",", ""));
                    response = await apiServicio.SeleccionarAsync<Response>(posFormDatoEspecifico.ToString(), new Uri(WebApp.BaseAddressRM), "api/MaestroArticuloSucursal");
                    var maestroArticuloSucursal = JsonConvert.DeserializeObject<MaestroArticuloSucursal>(response.Resultado.ToString());

                    ordenCompra.OrdenCompraDetalles.Add(new OrdenCompraDetalles
                    {
                        IdOrdenCompra = ordenCompra.IdOrdenCompra,
                        IdMaestroArticuloSucursal = posFormDatoEspecifico,
                        ValorUnitario = valorUnitario,
                        Cantidad = cantidad,
                        MaestroArticuloSucursal = maestroArticuloSucursal
                    });
                }
                int idOrdenCompra = 0;
                int idFactura = 0;
                if (ordenCompra.IdOrdenCompra == 0)
                {
                    response = await apiServicio.InsertarAsync(ordenCompra, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/InsertarOrdenCompra");
                    if (response.IsSuccess)
                    {
                        var ordenCompraAux = JsonConvert.DeserializeObject<OrdenCompra>(response.Resultado.ToString());
                        idOrdenCompra = ordenCompraAux.IdOrdenCompra;
                        idFactura = ordenCompraAux.IdFacturaActivoFijo;
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha realizado una orden de compra", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Orden de compra:", ordenCompraAux.IdOrdenCompra) });
                    }
                }
                else
                {
                    idOrdenCompra = ordenCompra.IdOrdenCompra;
                    idFactura = ordenCompra.IdFacturaActivoFijo;
                    response = await apiServicio.EditarAsync(ordenCompra.IdOrdenCompra.ToString(), ordenCompra, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/EditarOrdenCompra");
                    if (response.IsSuccess)
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha editado una orden de compra", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Orden de compra:", ordenCompra.IdOrdenCompra) });
                }
                var responseFile = new Response { IsSuccess = true };
                if (response.IsSuccess)
                {
                    if (file.Count > 0)
                    {
                        foreach (var item in file)
                        {
                            byte[] data;
                            using (var br = new BinaryReader(item.OpenReadStream()))
                                data = br.ReadBytes((int)item.OpenReadStream().Length);

                            if (data.Length > 0)
                            {
                                var activoFijoDocumentoTransfer = new DocumentoActivoFijoTransfer { Nombre = item.FileName, Fichero = data, IdFacturaActivoFijo = idFactura };
                                responseFile = await apiServicio.InsertarAsync(activoFijoDocumentoTransfer, new Uri(WebApp.BaseAddressRM), "api/DocumentoActivoFijo/UploadFiles");
                                if (responseFile != null && responseFile.IsSuccess)
                                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha subido un archivo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Documento de Orden de compra:", activoFijoDocumentoTransfer.Nombre) });
                            }
                        }
                    }
                }
                if (response.IsSuccess)
                    return this.Redireccionar(responseFile.IsSuccess ? $"{Mensaje.Informacion}|{Mensaje.Satisfactorio}" : $"{Mensaje.Aviso}|{Mensaje.ErrorUploadFiles}", nameof(DetallesOrdenCompraEnTramite), routeValues: new { id = idOrdenCompra });

                ViewData["Proveedor"] = new SelectList(await apiServicio.ObtenerElementoAsync<List<Proveedor>>(new ProveedorTransfer { LineaServicio = LineasServicio.Proveeduria, Activo = true }, new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedoresPorLineaServicioEstado"), "IdProveedor", "RazonSocial", ordenCompra.IdProveedor);
                ViewData["MotivoRecepcionArticulos"] = new SelectList(await apiServicio.Listar<MotivoRecepcionArticulos>(new Uri(WebApp.BaseAddressRM), "api/MotivoRecepcionArticulos/ListarMotivoRecepcionArticulos"), "IdMotivoRecepcionArticulos", "Descripcion");

                var claimTransfer = claimsTransfer.ObtenerClaimsTransferHttpContext();
                ViewData["Sucursal"] = new SelectList(claimsTransfer.IsADMIGrupo(ADMI_Grupos.AdminNacionalProveeduria) ? await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal") : new List<Sucursal> { new Sucursal { IdSucursal = claimTransfer.IdSucursal, Nombre = claimTransfer.NombreSucursal } }, "IdSucursal", "Nombre");

                if (claimsTransfer.IsADMIGrupo(ADMI_Grupos.AdminNacionalProveeduria))
                    ViewData["Bodega"] = await ObtenerSelectListBodega(ordenCompra?.Bodega?.IdSucursal ?? -1);
                else
                    ViewData["Bodega"] = await apiServicio.ObtenerElementoAsync<Bodega>(claimTransfer.IdDependencia, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/BodegaPorDependencia");

                ViewData["Empleado"] = await ObtenerSelectListEmpleado(ordenCompra.Bodega.IdSucursal, ordenCompra.IdEmpleadoDevolucion);

                ViewData["Error"] = response.Message;
                return View(ordenCompra);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando orden de compra", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListadoOrdenCompraEntramite));
            }
        }

        public async Task<IActionResult> DeleteOrdenCompraEnTramite(string id)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/EliminarOrdenCompra");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Sistema", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webapprm" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}", nameof(ListadoOrdenCompraEntramite));
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}", nameof(ListadoOrdenCompraEntramite));
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar orden de compra", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}", nameof(ListadoOrdenCompraEntramite));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProveedorResult(string idProveedor)
        {
            var proveedor = new Proveedor();
            try
            {
                var response = await apiServicio.SeleccionarAsync<Response>(idProveedor, new Uri(WebApp.BaseAddressRM), "api/Proveedor");
                if (response.IsSuccess)
                    proveedor = JsonConvert.DeserializeObject<Proveedor>(response.Resultado.ToString());
            }
            catch (Exception)
            { }
            return PartialView("_ProveedorOrdenCompra", proveedor);
        }

        [HttpPost]
        public async Task<IActionResult> ArticulosResult(List<IdRecepcionActivoFijoDetalleSeleccionado> listadoRecepcionActivoFijoDetalleSeleccionado, int objAdicional)
        {
            var lista = new List<MaestroArticuloSucursalSeleccionado>();
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<MaestroArticuloSucursalSeleccionado>>(new IdSucursalIdRecepcionActivoFijoDetalleSeleccionado { IdSucursal = objAdicional, ListaIdRecepcionActivoFijoDetalleSeleccionado = listadoRecepcionActivoFijoDetalleSeleccionado }, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/DetallesMaestroArticulo");
            }
            catch (Exception)
            { }
            return PartialView("_ListadoArticulos", lista);
        }

        public async Task<IActionResult> DetallesOrdenCompraEnTramite(int? id)
        {
            try
            {
                if (id != null)
                {
                    var response = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ObtenerOrdenCompra");
                    if (!response.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoOrdenCompraEntramite));

                    var ordenCompra = JsonConvert.DeserializeObject<OrdenCompra>(response.Resultado.ToString());
                    return View(ordenCompra);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoOrdenCompraEntramite));
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoOrdenCompraEntramite));
            }
        }

        public async Task<IActionResult> ProcesarOrdenCompraEnTramite(int? id)
        {
            try
            {
                if (id != null)
                {
                    var response = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ObtenerOrdenCompra");
                    if (!response.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoOrdenCompraEntramite));

                    var ordenCompra = JsonConvert.DeserializeObject<OrdenCompra>(response.Resultado.ToString());
                    return View(ordenCompra);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoOrdenCompraEntramite));
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoOrdenCompraEntramite));
            }
        }

        public async Task<IActionResult> ProcesarOrdenCompra(int? id)
        {
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Procesada }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                var response = await apiServicio.InsertarAsync(id, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ProcesarOrdenCompra");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha procesado una orden de compra", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Procesando orden de compra:", id) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ListadoOrdenCompraProcesadas));
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoOrdenCompraEntramite));
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}", nameof(ListadoOrdenCompraEntramite));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProveedorCompraResult(string idOrdenCompra)
        {
            try
            {
                ViewData["Proveedor"] = new SelectList(await apiServicio.ObtenerElementoAsync<List<Proveedor>>(new ProveedorTransfer { LineaServicio = LineasServicio.Proveeduria, Activo = true }, new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedoresPorLineaServicioEstado"), "IdProveedor", "RazonSocial");
            }
            catch (Exception)
            { }
            return PartialView("_ProveedorSelectOrdenCompra", new OrdenCompra());
        }

        [HttpPost]
        public async Task<IActionResult> EmpleadoDevolucionSelectResult(int? idSucursal)
        {
            try
            {
                ViewData["Empleado"] = idSucursal != null ? await ObtenerSelectListEmpleado((int)idSucursal) : new SelectList(new List<ListaEmpleadoViewModel>(), "IdEmpleado", "NombreApellido");
            }
            catch (Exception)
            { }
            return PartialView("_EmpleadoDevolucionSelect", new OrdenCompra());
        }

        [HttpPost]
        public async Task<IActionResult> BodegaResult(int idSucursal)
        {
            try
            {
                ViewData["Bodega"] = await ObtenerSelectListBodega(idSucursal);
            }
            catch (Exception)
            { }
            return PartialView("_BodegaProcesarSelect", new OrdenCompra());
        }

        public async Task<IActionResult> ExportarExcelOrdenCompra(int? id)
        {
            try
            {
                var fileContents = await apiServicio.ObtenerElementoAsync<byte[]>(id, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ExcelOrdenCompra");
                if (fileContents.Length > 0)
                {
                    var fileName = "Orden de compra.xlsx";
                    return File(fileContents, MimeTypes.GetMimeType(fileName), fileName);
                }
            }
            catch (Exception)
            { }
            return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorReporte}", nameof(DetallesOrdenCompraEnTramite), routeValues: new { id });
        }
        #endregion

        #region Requerimientos
        public async Task<IActionResult> ListadoRequerimientosSolicitados() => await ListadoRequerimientos(Estados.Solicitado);

        public async Task<IActionResult> ListadoRequerimientosDesaprobados() => await ListadoRequerimientos(Estados.Desaprobado);

        public async Task<IActionResult> ListadoRequerimientosDespachados() => await ListadoRequerimientos(Estados.Despachado);

        public async Task<IActionResult> ListadoRequerimientos(string estado)
        {
            var lista = new List<RequerimientoArticulos>();
            ViewData["Estado"] = estado;
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<RequerimientoArticulos>>(estado, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ListadoRequerimientoArticulosPorEstado");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando requerimientos de artículos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoRequerimientos", lista);
        }

        public async Task<IActionResult> GestionarRequerimiento(int? id)
        {
            try
            {
                if (DateTime.Now.Day <= WebApp.DiasPedido)
                {
                    var claimTransfer = claimsTransfer.ObtenerClaimsTransferHttpContext();
                    ViewData["Bodega"] = await apiServicio.ObtenerElementoAsync<Bodega>(claimTransfer.IdDependencia, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/BodegaPorDependencia");
                    if (id != null)
                    {
                        var response = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ObtenerRequerimientoArticulos");
                        if (!response.IsSuccess)
                            return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoRequerimientosSolicitados));

                        var requerimientoArticulos = JsonConvert.DeserializeObject<RequerimientoArticulos>(response.Resultado.ToString());
                        return View(requerimientoArticulos);
                    }
                    return View();
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorDiasPedido}", nameof(ListadoRequerimientosSolicitados));
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoRequerimientosSolicitados));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GestionarRequerimiento(RequerimientoArticulos requerimientoArticulos)
        {
            try
            {
                if (DateTime.Now.Day <= WebApp.DiasPedido)
                {
                    await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Solicitado }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                    var listaFormDatosEspecificos = Request.Form.Where(c => c.Key.StartsWith("hIdRecepcionActivoFijoDetalle_"));
                    requerimientoArticulos.RequerimientosArticulosDetalles = new List<RequerimientosArticulosDetalles>();
                    var response = new Response();
                    foreach (var item in listaFormDatosEspecificos)
                    {
                        int posFormDatoEspecifico = int.Parse(item.Key.ToString().Split('_')[1]);
                        int cantidad = int.Parse(Request.Form[$"cantidad_{posFormDatoEspecifico}"].ToString().Replace(",", ""));
                        response = await apiServicio.SeleccionarAsync<Response>(posFormDatoEspecifico.ToString(), new Uri(WebApp.BaseAddressRM), "api/MaestroArticuloSucursal");
                        var maestroArticuloSucursal = JsonConvert.DeserializeObject<MaestroArticuloSucursal>(response.Resultado.ToString());

                        requerimientoArticulos.RequerimientosArticulosDetalles.Add(new RequerimientosArticulosDetalles
                        {
                            IdRequerimientosArticulos = requerimientoArticulos.IdRequerimientoArticulos,
                            IdMaestroArticuloSucursal = posFormDatoEspecifico,
                            CantidadSolicitada = cantidad,
                            MaestroArticuloSucursal = maestroArticuloSucursal
                        });
                    }
                    int idRequerimientoArticulos = 0;
                    if (requerimientoArticulos.IdRequerimientoArticulos == 0)
                    {
                        response = await apiServicio.InsertarAsync(requerimientoArticulos, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/InsertarRequerimientoArticulos");
                        if (response.IsSuccess)
                        {
                            var requerimientoArticulosAux = JsonConvert.DeserializeObject<RequerimientoArticulos>(response.Resultado.ToString());
                            idRequerimientoArticulos = requerimientoArticulosAux.IdRequerimientoArticulos;
                            await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado un requerimiento de artículos", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Requerimiento de artículos:", requerimientoArticulos.IdRequerimientoArticulos) });
                        }
                    }
                    else
                    {
                        idRequerimientoArticulos = requerimientoArticulos.IdRequerimientoArticulos;
                        response = await apiServicio.EditarAsync(requerimientoArticulos.IdRequerimientoArticulos.ToString(), requerimientoArticulos, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/EditarRequerimientoArticulos");
                        if (response.IsSuccess)
                            await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha editado un requerimiento de artículos", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Requerimiento de artículos:", requerimientoArticulos.IdRequerimientoArticulos) });
                    }

                    if (response.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(DetallesRequerimientoArticulos), routeValues: new { id = idRequerimientoArticulos });

                    var claimTransfer = claimsTransfer.ObtenerClaimsTransferHttpContext();
                    ViewData["Bodega"] = await apiServicio.ObtenerElementoAsync<Bodega>(claimTransfer.IdDependencia, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/BodegaPorDependencia");

                    ViewData["Error"] = response.Message;
                    return View(requerimientoArticulos);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorDiasPedido}", nameof(ListadoRequerimientosSolicitados));
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando requerimiento", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListadoRequerimientosSolicitados));
            }
        }

        public async Task<IActionResult> DetallesRequerimientoArticulos(int? id)
        {
            try
            {
                if (id != null)
                {
                    var response = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ObtenerRequerimientoArticulos");
                    if (!response.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoRequerimientosSolicitados));

                    var requerimientoArticulos = JsonConvert.DeserializeObject<RequerimientoArticulos>(response.Resultado.ToString());
                    return View(requerimientoArticulos);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoRequerimientosSolicitados));
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoRequerimientosSolicitados));
            }
        }

        public async Task<IActionResult> DeleteRequerimiento(string id)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/EliminarRequerimientoArticulos");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Sistema", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webapprm" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}", nameof(ListadoRequerimientosSolicitados));
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}", nameof(ListadoRequerimientosSolicitados));
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar requerimiento de artículos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}", nameof(ListadoRequerimientosSolicitados));
            }
        }

        public async Task<IActionResult> DenegarRequerimiento(string id)
        {
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Desaprobado }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                await apiServicio.InsertarAsync(id, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/DenegarRequerimientoArticulos");
                return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ListadoRequerimientosDesaprobados));
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Denegando requerimiento", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoRequerimientosSolicitados));
            }
        }

        public async Task<IActionResult> DespacharRequerimiento(int? id)
        {
            try
            {
                var salidaArticulos = new SalidaArticulos();
                if (id != null)
                {
                    var response = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ObtenerRequerimientoArticulos");
                    if (!response.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoRequerimientosSolicitados));
                    
                    var requerimientoArticulos = JsonConvert.DeserializeObject<RequerimientoArticulos>(response.Resultado.ToString());
                    salidaArticulos.RequerimientoArticulos = requerimientoArticulos;
                    salidaArticulos.IdRequerimientoArticulos = requerimientoArticulos.IdRequerimientoArticulos;
                    ViewData["MotivoSalidaArticulos"] = new SelectList(await apiServicio.Listar<MotivoSalidaArticulos>(new Uri(WebApp.BaseAddressRM), "api/MotivoSalidaArticulos/ListarMotivoSalidaArticulos"), "IdMotivoSalidaArticulos", "Descripcion");
                    ViewData["Bodega"] = await apiServicio.ObtenerElementoAsync<Bodega>(requerimientoArticulos.FuncionarioSolicitante.IdDependencia, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/BodegaPorDependencia");
                    return View(salidaArticulos);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoRequerimientosSolicitados));
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoRequerimientosSolicitados));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DespacharRequerimiento(SalidaArticulos salidaArticulos)
        {
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Despachado }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");

                string motivoSalidaArticulos = Request.Form["MotSalida"].ToString();
                if (motivoSalidaArticulos == "Baja de inventarios")
                    salidaArticulos.IdEmpleadoRealizaBaja = int.Parse(Request.Form["IdEmpleadoDevolucion"].ToString());

                if (motivoSalidaArticulos == "Despacho")
                    salidaArticulos.IdEmpleadoDespacho = int.Parse(Request.Form["IdEmpleadoDevolucion"].ToString());

                var response = await apiServicio.SeleccionarAsync<Response>(salidaArticulos.IdRequerimientoArticulos.ToString(), new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ObtenerRequerimientoArticulos");
                if (!response.IsSuccess)
                    return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoRequerimientosSolicitados));
                
                var requerimientoArticulos = JsonConvert.DeserializeObject<RequerimientoArticulos>(response.Resultado.ToString());
                salidaArticulos.RequerimientoArticulos = requerimientoArticulos;
                salidaArticulos.IdRequerimientoArticulos = requerimientoArticulos.IdRequerimientoArticulos;
                var listaRequerimientoArticulosDetalles = new List<RequerimientosArticulosDetalles>();

                foreach (var item in requerimientoArticulos.RequerimientosArticulosDetalles)
                {
                    string cantidadAprobada = Request.Form[$"cantidadAprobada_{item.IdMaestroArticuloSucursal}"].ToString();
                    if (!String.IsNullOrEmpty(cantidadAprobada))
                        item.CantidadAprobada = int.Parse(cantidadAprobada);

                    string cantidadEntregada = Request.Form[$"cantidadEntregada_{item.IdMaestroArticuloSucursal}"].ToString();
                    if (!String.IsNullOrEmpty(cantidadEntregada))
                        item.CantidadEntregada = int.Parse(cantidadEntregada);

                    listaRequerimientoArticulosDetalles.Add(item);
                }
                salidaArticulos.RequerimientoArticulos.RequerimientosArticulosDetalles = listaRequerimientoArticulosDetalles;

                response = await apiServicio.InsertarAsync(salidaArticulos, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/DespacharRequerimientoArticulos");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha despachado un requerimiento de artículos", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Despachando requerimiento de artículos:", salidaArticulos.IdSalidaArticulos) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(DetallesRequerimientoArticulos), routeValues: new { id = salidaArticulos.IdRequerimientoArticulos });
                }

                ViewData["MotivoSalidaArticulos"] = new SelectList(await apiServicio.Listar<MotivoSalidaArticulos>(new Uri(WebApp.BaseAddressRM), "api/MotivoSalidaArticulos/ListarMotivoSalidaArticulos"), "IdMotivoSalidaArticulos", "Descripcion");
                ViewData["Bodega"] = await apiServicio.ObtenerElementoAsync<Bodega>(requerimientoArticulos.FuncionarioSolicitante.IdDependencia, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/BodegaPorDependencia");
                ViewData["Error"] = response.Message;
                return View(salidaArticulos);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Despachando requerimiento de artículos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListadoRequerimientosSolicitados));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProveedorSelectResult(int idProveedor)
        {
            try
            {
                ViewData["Proveedor"] = new SelectList(await apiServicio.ObtenerElementoAsync<List<Proveedor>>(new ProveedorTransfer { LineaServicio = LineasServicio.Proveeduria, Activo = true }, new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedoresPorLineaServicioEstado"), "IdProveedor", "RazonSocial");
            }
            catch (Exception)
            { }
            return PartialView("_ProveedorSelect", new SalidaArticulos());
        }

        [HttpPost]
        public async Task<IActionResult> BodegaExisteResult(int idDependencia)
        {
            try
            {
                var response = await apiServicio.ObtenerElementoAsync<Response>(idDependencia, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ExisteBodegaParaDependencia");
                if (response.IsSuccess)
                    return Json(true);
            }
            catch (Exception)
            { }
            return StatusCode(500);
        }

        public async Task<IActionResult> ExportarExcelRequerimiento(int? id)
        {
            try
            {
                var fileContents = await apiServicio.ObtenerElementoAsync<byte[]>(id, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ExcelRequerimientoArticulos");
                if (fileContents.Length > 0)
                {
                    var fileName = "Requerimiento de artículos.xlsx";
                    return File(fileContents, MimeTypes.GetMimeType(fileName), fileName);
                }
            }
            catch (Exception)
            { }
            return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorReporte}", nameof(DetallesRequerimientoArticulos), routeValues: new { id });
        }
        #endregion

        #region Inventario
        public async Task<IActionResult> ConsultarInventario()
        {
            var nuevoAjusteInventarioArticulos = new AjusteInventarioArticulos();
            nuevoAjusteInventarioArticulos.Fecha = DateTime.Now;
            nuevoAjusteInventarioArticulos.InventarioArticulos = new List<InventarioArticulos>();
            try
            {
                var claimTransfer = claimsTransfer.ObtenerClaimsTransferHttpContext();
                ViewData["Sucursal"] = new SelectList(claimsTransfer.IsADMIGrupo(ADMI_Grupos.AdminNacionalProveeduria) ? await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal") : new List<Sucursal> { new Sucursal { IdSucursal = claimTransfer.IdSucursal, Nombre = claimTransfer.NombreSucursal } }, "IdSucursal", "Nombre");

                if (claimsTransfer.IsADMIGrupo(ADMI_Grupos.AdminZonalProveeduria))
                    ViewData["Bodega"] = await apiServicio.ObtenerElementoAsync<Bodega>(claimTransfer.IdDependencia, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/BodegaPorDependencia");
                else
                    ViewData["Bodega"] = await ObtenerSelectListBodega((ViewData["Sucursal"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Sucursal"] as SelectList).FirstOrDefault().Value) : -1);

                int idBodega = claimsTransfer.IsADMIGrupo(ADMI_Grupos.AdminZonalProveeduria) ? (ViewData["Bodega"] as Bodega) != null ? (ViewData["Bodega"] as Bodega).IdBodega : -1 : (ViewData["Bodega"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Bodega"] as SelectList).FirstOrDefault().Value) : -1;
                nuevoAjusteInventarioArticulos.InventarioArticulos = await apiServicio.ObtenerElementoAsync<List<InventarioArticulos>>(new IdBodegaFecha { Fecha = nuevoAjusteInventarioArticulos.Fecha, IdBodega = idBodega }, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ListadoInventarioPorBodegaFecha");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Consultando inventario de artículos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}";
            }
            return View(nuevoAjusteInventarioArticulos);
        }

        [HttpPost]
        public async Task<IActionResult> ArticulosBodegaFechaResult(int idBodega, DateTime fecha)
        {
            var lista = new List<InventarioArticulos>();
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<InventarioArticulos>>(new IdBodegaFecha { Fecha = fecha, IdBodega = idBodega }, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ListadoInventarioPorBodegaFecha");
            }
            catch (Exception)
            { }
            return PartialView("_DetallesInventario", lista);
        }

        public async Task<IActionResult> AjustarInventario()
        {
            try
            {
                var claimTransfer = claimsTransfer.ObtenerClaimsTransferHttpContext();
                ViewData["Sucursal"] = new SelectList(claimsTransfer.IsADMIGrupo(ADMI_Grupos.AdminNacionalProveeduria) ? await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal") : new List<Sucursal> { new Sucursal { IdSucursal = claimTransfer.IdSucursal, Nombre = claimTransfer.NombreSucursal } }, "IdSucursal", "Nombre");

                if (claimsTransfer.IsADMIGrupo(ADMI_Grupos.AdminZonalProveeduria))
                    ViewData["Bodega"] = await apiServicio.ObtenerElementoAsync<Bodega>(claimTransfer.IdDependencia, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/BodegaPorDependencia");
                else
                    ViewData["Bodega"] = await ObtenerSelectListBodega((ViewData["Sucursal"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Sucursal"] as SelectList).FirstOrDefault().Value) : -1);

                ViewData["Empleado"] = await ObtenerSelectListEmpleado((ViewData["Sucursal"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Sucursal"] as SelectList).FirstOrDefault().Value) : -1);

                var nuevoAjusteInventario = new AjusteInventarioArticulos();
                int idBodega = claimsTransfer.IsADMIGrupo(ADMI_Grupos.AdminZonalProveeduria) ? (ViewData["Bodega"] as Bodega) != null ? (ViewData["Bodega"] as Bodega).IdBodega : -1 : (ViewData["Bodega"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Bodega"] as SelectList).FirstOrDefault().Value) : -1;
                nuevoAjusteInventario.InventarioArticulos = await apiServicio.ObtenerElementoAsync<List<InventarioArticulos>>(idBodega, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ListadoInventarioPorBodega");
                return View(nuevoAjusteInventario);
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoAjusteInventario));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AjustarInventario(AjusteInventarioArticulos ajusteInventarioArticulos)
        {
            try
            {
                var listaFormDatosEspecificos = Request.Form.Where(c => c.Key.StartsWith("hIdRecepcionActivoFijoDetalle_"));
                ajusteInventarioArticulos.InventarioArticulos = new List<InventarioArticulos>();
                var response = new Response();
                foreach (var item in listaFormDatosEspecificos)
                {
                    int posFormDatoEspecifico = int.Parse(item.Key.ToString().Split('_')[1]);
                    int cantidad = int.Parse(Request.Form[$"cantidad_{posFormDatoEspecifico}"].ToString().Replace(",", ""));
                    response = await apiServicio.SeleccionarAsync<Response>(posFormDatoEspecifico.ToString(), new Uri(WebApp.BaseAddressRM), "api/MaestroArticuloSucursal");
                    var maestroArticuloSucursal = JsonConvert.DeserializeObject<MaestroArticuloSucursal>(response.Resultado.ToString());

                    ajusteInventarioArticulos.InventarioArticulos.Add(new InventarioArticulos
                    {
                        IdMaestroArticuloSucursal = posFormDatoEspecifico,
                        IdBodega = ajusteInventarioArticulos.IdBodega,
                        Cantidad = cantidad,
                        MaestroArticuloSucursal = maestroArticuloSucursal
                    });
                }
                response = await apiServicio.InsertarAsync(ajusteInventarioArticulos, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/InsertarAjusteInventarioArticulos");
                if (response.IsSuccess)
                {
                    var ajusteInventarioArticulosAux = JsonConvert.DeserializeObject<AjusteInventarioArticulos>(response.Resultado.ToString());
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado un ajuste de inventario de artículos", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Ajuste de inventario de artículos:", ajusteInventarioArticulos.IdAjusteInventario) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(DetallesAjusteInventario), routeValues: new { id = ajusteInventarioArticulosAux.IdAjusteInventario });
                }
                var claimTransfer = claimsTransfer.ObtenerClaimsTransferHttpContext();
                ViewData["Sucursal"] = new SelectList(claimsTransfer.IsADMIGrupo(ADMI_Grupos.AdminNacionalProveeduria) ? await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal") : new List<Sucursal> { new Sucursal { IdSucursal = claimTransfer.IdSucursal, Nombre = claimTransfer.NombreSucursal } }, "IdSucursal", "Nombre");
                if (!claimsTransfer.IsADMIGrupo(ADMI_Grupos.AdminNacionalProveeduria))
                    ViewData["Bodega"] = await apiServicio.ObtenerElementoAsync<Bodega>(claimTransfer.IdDependencia, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/BodegaPorDependencia");
                else
                    ViewData["Bodega"] = await ObtenerSelectListBodega(ajusteInventarioArticulos?.Bodega?.IdSucursal ?? -1);

                ViewData["Empleado"] = await ObtenerSelectListEmpleado(ajusteInventarioArticulos?.Bodega?.IdSucursal ?? -1);
                ViewData["Error"] = response.Message;
                return View(ajusteInventarioArticulos);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Ajustando inventario", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListadoAjusteInventario));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EmpleadoAutorizaSelectResult(int? idSucursal)
        {
            try
            {
                ViewData["Empleado"] = idSucursal != null ? await ObtenerSelectListEmpleado((int)idSucursal) : new SelectList(new List<ListaEmpleadoViewModel>(), "IdEmpleado", "NombreApellido");
            }
            catch (Exception)
            { }
            return PartialView("_EmpleadoAutorizaSelect", new AjusteInventarioArticulos());
        }

        [HttpPost]
        public async Task<IActionResult> ArticulosAjusteInventarioResult(int idBodega)
        {
            var listadoInventarioArticulos = new List<InventarioArticulos>();
            try
            {
                listadoInventarioArticulos = await apiServicio.ObtenerElementoAsync<List<InventarioArticulos>>(idBodega, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ListadoInventarioPorBodega");
            }
            catch (Exception)
            { }
            return PartialView("_ListadoArticulosAjusteInventario", listadoInventarioArticulos);
        }

        public async Task<IActionResult> ListadoAjusteInventario()
        {
            var lista = new List<AjusteInventarioArticulos>();
            try
            {
                lista = await apiServicio.Listar<AjusteInventarioArticulos>(new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ListadoAjusteInventarioArticulos");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando ajustes de inventario de artículos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> DetallesAjusteInventario(int? id)
        {
            try
            {
                if (id != null)
                {
                    var response = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ObtenerAjusteInventarioArticulos");
                    if (!response.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoAjusteInventario));

                    var ajusteInventarioArticulosActual = JsonConvert.DeserializeObject<AjusteInventarioArticulos>(response.Resultado.ToString());
                    ViewData["InventarioArticulosAnterior"] = await apiServicio.ObtenerElementoAsync<List<InventarioArticulos>>(ajusteInventarioArticulosActual.Fecha, new Uri(WebApp.BaseAddressRM), "api/Proveeduria/ListadoInventarioArticulosAnterior");
                    return View(ajusteInventarioArticulosActual);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoAjusteInventario));
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoAjusteInventario));
            }
        }
        #endregion

        #region Descargar Archivos
        public async Task<IActionResult> DescargarArchivo(int id)
        {
            return await new ActivoFijoController(apiServicio, null, null).DescargarArchivo(id);
        }
        #endregion

        #region AJAX_Bodega
        public async Task<SelectList> ObtenerSelectListBodega(int idSucursal, int? idBodega = null)
        {
            try
            {
                var listaBodega = idSucursal != -1 ? await apiServicio.Listar<Bodega>(new Uri(WebApp.BaseAddressRM), $"api/Bodega/ListarBodegaPorSucursal/{idSucursal}") : new List<Bodega>();
                return new SelectList(listaBodega, "IdBodega", "Nombre", idBodega);
            }
            catch (Exception)
            {
                return new SelectList(new List<Bodega>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Bodega_SelectResult(int idSucursal)
        {
            ViewBag.Bodega = await ObtenerSelectListBodega(idSucursal);
            return PartialView("_BodegaSelect", new UbicacionActivoFijo());
        }
        #endregion

        #region AJAX_Empleado
        public async Task<SelectList> ObtenerSelectListEmpleado(int idSucursal, int? idEmpleado = null)
        {
            try
            {
                var listaEmpleado = idSucursal != -1 ? await apiServicio.ObtenerElementoAsync<List<DatosBasicosEmpleadoViewModel>>(new EmpleadosPorSucursalViewModel { IdSucursal = idSucursal, EmpleadosActivos = true }, new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleadosPorSucursal") : new List<DatosBasicosEmpleadoViewModel>();
                return new SelectList(listaEmpleado.Select(c => new ListaEmpleadoViewModel { IdEmpleado = c.IdEmpleado, NombreApellido = $"{c.Nombres} {c.Apellidos}" }), "IdEmpleado", "NombreApellido", idEmpleado);
            }
            catch (Exception)
            {
                return new SelectList(new List<ListaEmpleadoViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Empleado_SelectResult(int idSucursal)
        {
            ViewBag.Empleado = await ObtenerSelectListEmpleado(idSucursal);
            return PartialView("_EmpleadoSelect", new UbicacionActivoFijo());
        }
        #endregion
    }
}
