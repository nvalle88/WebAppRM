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
using Microsoft.AspNetCore.Http;
using System.IO;
using bd.webapprm.entidades.ObjectTransfer;
using bd.webapprm.servicios.Extensores;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Filters;

namespace bd.webapprm.web.Controllers.MVC
{
    public class ActivoFijoController : Controller
    {
        private readonly IApiServicio apiServicio;

        public ActivoFijoController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        #region Recepción de Activos
        public IActionResult Index()
        {
            return RedirectToAction("Recepcion");
        }

        public async Task<IActionResult> Recepcion()
        {
            try
            {
                ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo((ViewData["TipoActivoFijo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["TipoActivoFijo"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["SubClaseActivoFijo"] = await ObtenerSelectListSubClaseActivoFijo((ViewData["ClaseActivoFijo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["ClaseActivoFijo"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["MotivoRecepcion"] = new SelectList(await apiServicio.Listar<MotivoRecepcion>(new Uri(WebApp.BaseAddressRM), "api/MotivoRecepcion/ListarMotivoRecepcion"), "IdMotivoRecepcion", "Descripcion");
                ViewData["FondoFinanciamiento"] = new SelectList(await apiServicio.Listar<FondoFinanciamiento>(new Uri(WebApp.BaseAddressRM), "api/FondoFinanciamiento/ListarFondoFinanciamiento"), "IdFondoFinanciamiento", "Nombre");
                ViewData["Proveedor"] = new SelectList((await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedores")).Select(c => new { c.IdProveedor, NombreApellidos = $"{c.Nombre} {c.Apellidos}" }), "IdProveedor", "NombreApellidos");

                ViewData["Sucursal"] = new SelectList(await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal"), "IdSucursal", "Nombre");
                ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo((ViewData["Sucursal"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Sucursal"] as SelectList).FirstOrDefault().Value) : -1);

                ViewData["Marca"] = new SelectList(await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddressRM), "api/Marca/ListarMarca"), "IdMarca", "Nombre");
                ViewData["Modelo"] = await ObtenerSelectListModelo((ViewData["Marca"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Marca"] as SelectList).FirstOrDefault().Value) : -1);

                ViewData["Ramo"] = new SelectList(await apiServicio.Listar<Ramo>(new Uri(WebApp.BaseAddressRM), "api/Ramo/ListarRamo"), "IdRamo", "Nombre");
                ViewData["Subramo"] = await ObtenerSelectListSubramo((ViewData["Ramo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Ramo"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["CompaniaSeguro"] = new SelectList(await apiServicio.Listar<CompaniaSeguro>(new Uri(WebApp.BaseAddressRM), "api/CompaniaSeguro/ListarCompaniaSeguro"), "IdCompaniaSeguro", "Nombre");

                ViewData["IdCategoria"] = 3;
                ViewData["Categoria"] = new SelectList(new[] { new { Id = 1, Valor = "Inmueble" }, new { Id = 2, Valor = "Vehículo" }, new { Id = 3, Valor = "Otro" } }, "Id", "Valor", ViewData["IdCategoria"]);
                ViewData["ListadoRecepcionActivoFijoDetalle"] = new List<RecepcionActivoFijoDetalle>();
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ActivosFijosRecepcionados));
            }
        }

        public async Task<IActionResult> EditarRecepcionAR(string id) => await EditarRecepcion(id, nameof(ActivosFijosRecepcionados), nameof(EditarRecepcionAR), Estados.Recepcionado);
        public async Task<IActionResult> EditarRecepcionVT(string id) => await EditarRecepcion(id, nameof(ActivosFijosValidacionTecnica), nameof(EditarRecepcionVT), Estados.ValidacionTecnica);
        public async Task<IActionResult> EditarRecepcion(string id, string nombreVistaError, string accionVistaEditar, string estado)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>($"{id}/{estado}", new Uri(WebApp.BaseAddressRM), "api/ActivosFijos");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nombreVistaError);

                    var activoFijo = JsonConvert.DeserializeObject<ActivoFijo>(respuesta.Resultado.ToString());
                    var recepcionActivoFijoDetalle = activoFijo.RecepcionActivoFijoDetalle.FirstOrDefault();
                    recepcionActivoFijoDetalle.ActivoFijo = activoFijo;

                    ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                    ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo(activoFijo?.SubClaseActivoFijo?.ClaseActivoFijo?.IdTipoActivoFijo ?? -1);
                    ViewData["SubClaseActivoFijo"] = await ObtenerSelectListSubClaseActivoFijo(activoFijo?.SubClaseActivoFijo?.IdClaseActivoFijo ?? -1);
                    ViewData["MotivoRecepcion"] = new SelectList(await apiServicio.Listar<MotivoRecepcion>(new Uri(WebApp.BaseAddressRM), "api/MotivoRecepcion/ListarMotivoRecepcion"), "IdMotivoRecepcion", "Descripcion");
                    ViewData["FondoFinanciamiento"] = new SelectList(await apiServicio.Listar<FondoFinanciamiento>(new Uri(WebApp.BaseAddressRM), "api/FondoFinanciamiento/ListarFondoFinanciamiento"), "IdFondoFinanciamiento", "Nombre");

                    ViewData["Sucursal"] = new SelectList(await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal"), "IdSucursal", "Nombre");
                    ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo(recepcionActivoFijoDetalle?.UbicacionActivoFijoActual?.LibroActivoFijo?.IdSucursal ?? -1);

                    ViewData["Proveedor"] = new SelectList((await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedores")).Select(c => new { c.IdProveedor, NombreApellidos = $"{c.Nombre} {c.Apellidos}" }), "IdProveedor", "NombreApellidos");
                    ViewData["Marca"] = new SelectList(await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddressRM), "api/Marca/ListarMarca"), "IdMarca", "Nombre");
                    ViewData["Modelo"] = await ObtenerSelectListModelo(activoFijo?.Modelo?.IdMarca ?? -1);

                    ViewData["Ramo"] = new SelectList(await apiServicio.Listar<Ramo>(new Uri(WebApp.BaseAddressRM), "api/Ramo/ListarRamo"), "IdRamo", "Nombre");
                    ViewData["Subramo"] = await ObtenerSelectListSubramo(activoFijo?.PolizaSeguroActivoFijo?.Subramo?.IdRamo ?? -1);
                    ViewData["CompaniaSeguro"] = new SelectList(await apiServicio.Listar<CompaniaSeguro>(new Uri(WebApp.BaseAddressRM), "api/CompaniaSeguro/ListarCompaniaSeguro"), "IdCompaniaSeguro", "Nombre");

                    if (!recepcionActivoFijoDetalle.RecepcionActivoFijo.ValidacionTecnica)
                        try { recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.Consecutivo = int.Parse(String.Join("", activoFijo.CodigoActivoFijo.Codigosecuencial.Except(ObtenerCodigoSecuencial(recepcionActivoFijoDetalle?.UbicacionActivoFijoActual?.LibroActivoFijo?.IdSucursal?.ToString(), activoFijo?.SubClaseActivoFijo?.ClaseActivoFijo?.Nombre, activoFijo?.SubClaseActivoFijo?.ClaseActivoFijo?.TipoActivoFijo?.Nombre)))); } catch (Exception) { activoFijo.CodigoActivoFijo.Consecutivo = 1; }

                    ViewData["IdCategoria"] = recepcionActivoFijoDetalle.NumeroChasis != null || recepcionActivoFijoDetalle.NumeroMotor != null || recepcionActivoFijoDetalle.Placa != null ? 2 : recepcionActivoFijoDetalle.NumeroClaveCatastral != null ? 1 : 3;
                    ViewData["Categoria"] = new SelectList(new[] { new { Id = 1, Valor = "Inmueble" }, new { Id = 2, Valor = "Vehículo" }, new { Id = 3, Valor = "Otro" } }, "Id", "Valor", ViewData["IdCategoria"]);

                    ViewData["ListadoRecepcionActivoFijoDetalle"] = activoFijo.RecepcionActivoFijoDetalle.ToList();
                    ViewData["UbicacionActivoFijo"] = recepcionActivoFijoDetalle?.UbicacionActivoFijoActual;
                    ViewData["AccionVistaEditar"] = accionVistaEditar;
                    return View("EditarRecepcion", recepcionActivoFijoDetalle);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nombreVistaError);
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nombreVistaError);
            }
        }

        public async Task<IActionResult> GestionRecepcionActivoFijoDetalle(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle, LibroActivoFijo libroActivoFijo)
        {
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Recepcionado }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.ValidacionTecnica }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                var listaEstado = await apiServicio.Listar<Estado>(new Uri(WebApp.BaseAddressTH), "api/Estados/ListarEstados");
                recepcionActivoFijoDetalle.Estado = listaEstado.SingleOrDefault(c => c.Nombre == (!recepcionActivoFijoDetalle.RecepcionActivoFijo.ValidacionTecnica ? Estados.Recepcionado : Estados.ValidacionTecnica));
                recepcionActivoFijoDetalle.IdEstado = recepcionActivoFijoDetalle.Estado.IdEstado;

                if (recepcionActivoFijoDetalle.IdRecepcionActivoFijo == 0)
                    recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.Codigosecuencial = ObtenerCodigoSecuencial(libroActivoFijo.IdSucursal.ToString(), recepcionActivoFijoDetalle.ActivoFijo.SubClaseActivoFijo.IdClaseActivoFijo.ToString(), recepcionActivoFijoDetalle.ActivoFijo.IdSubClaseActivoFijo.ToString(), recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.Consecutivo);

                var response = new Response();
                int idActivoFijo = 0;
                var listaRecepcionActivoFijoUbicacionTransfer = new List<RecepcionActivoFijoDetalle>();
                var listaFormDatosEspecificos = Request.Form.Where(c => c.Key.StartsWith("hBodega_"));
                foreach (var item in listaFormDatosEspecificos)
                {
                    int posFormDatoEspecifico = int.Parse(item.Key.ToString().Split('_')[1]);
                    int? idBodega = Utiles.TryParseInt(Request.Form[$"hBodega_{posFormDatoEspecifico}"]);
                    int? idEmpleado = Utiles.TryParseInt(Request.Form[$"hEmpleado_{posFormDatoEspecifico}"]);
                    int? idRecepcionActivoFijoDetalle = Utiles.TryParseInt(Request.Form[$"hIdRecepcionActivoFijoDetalle_{posFormDatoEspecifico}"]);
                    int? idUbicacionActivoFijo = Utiles.TryParseInt(Request.Form[$"hUbicacion_{posFormDatoEspecifico}"]);
                    listaRecepcionActivoFijoUbicacionTransfer.Add(new RecepcionActivoFijoDetalle { IdRecepcionActivoFijoDetalle = idRecepcionActivoFijoDetalle ?? 0, IdEstado = recepcionActivoFijoDetalle.IdEstado, IdRecepcionActivoFijo = recepcionActivoFijoDetalle.IdRecepcionActivoFijo, IdActivoFijo = recepcionActivoFijoDetalle.IdActivoFijo, ActivoFijo = recepcionActivoFijoDetalle.ActivoFijo, RecepcionActivoFijo = recepcionActivoFijoDetalle.RecepcionActivoFijo, Estado = recepcionActivoFijoDetalle.Estado, Serie = !String.IsNullOrEmpty(Request.Form[$"hSerie_{posFormDatoEspecifico}"].ToString()) ? Request.Form[$"hSerie_{posFormDatoEspecifico}"].ToString() : null, NumeroChasis = !String.IsNullOrEmpty(Request.Form[$"hNumeroChasis_{posFormDatoEspecifico}"].ToString()) ? Request.Form[$"hNumeroChasis_{posFormDatoEspecifico}"].ToString() : null, NumeroMotor = !String.IsNullOrEmpty(Request.Form[$"hNumeroMotor_{posFormDatoEspecifico}"].ToString()) ? Request.Form[$"hNumeroMotor_{posFormDatoEspecifico}"].ToString() : null, Placa = !String.IsNullOrEmpty(Request.Form[$"hPlaca_{posFormDatoEspecifico}"].ToString()) ? Request.Form[$"hPlaca_{posFormDatoEspecifico}"].ToString() : null, NumeroClaveCatastral = !String.IsNullOrEmpty(Request.Form[$"hNumeroClaveCatastral_{posFormDatoEspecifico}"].ToString()) ? Request.Form[$"hNumeroClaveCatastral_{posFormDatoEspecifico}"].ToString() : null, UbicacionActivoFijo = new List<UbicacionActivoFijo>() { new UbicacionActivoFijo { IdUbicacionActivoFijo = idUbicacionActivoFijo ?? 0, IdEmpleado = idEmpleado, IdBodega = idBodega, Bodega = idBodega != null ? JsonConvert.DeserializeObject<Bodega>((await apiServicio.SeleccionarAsync<Response>(idBodega.ToString(), new Uri(WebApp.BaseAddressRM), "api/Bodega")).Resultado.ToString()) : null, Empleado = idEmpleado != null ? JsonConvert.DeserializeObject<Empleado>((await apiServicio.SeleccionarAsync<Response>(idEmpleado.ToString(), new Uri(WebApp.BaseAddressTH), "api/Empleados")).Resultado.ToString()) : null, FechaUbicacion = recepcionActivoFijoDetalle.RecepcionActivoFijo.FechaRecepcion, IdLibroActivoFijo = int.Parse(Request.Form["IdLibroActivoFijo"].ToString()), LibroActivoFijo = JsonConvert.DeserializeObject<LibroActivoFijo>((await apiServicio.SeleccionarAsync<Response>(Request.Form["IdLibroActivoFijo"].ToString(), new Uri(WebApp.BaseAddressRM), "api/LibroActivoFijo")).Resultado.ToString()) } } });
                }

                if (recepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle == 0)
                {
                    response = await apiServicio.InsertarAsync(listaRecepcionActivoFijoUbicacionTransfer, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/InsertarRecepcionActivoFijo");
                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha recepcionado un activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Activo Fijo:", recepcionActivoFijoDetalle.ActivoFijo.IdActivoFijo) });
                        idActivoFijo = JsonConvert.DeserializeObject<List<RecepcionActivoFijoDetalle>>(response.Resultado.ToString()).FirstOrDefault().IdActivoFijo;
                    }
                }
                else
                {
                    idActivoFijo = recepcionActivoFijoDetalle.IdActivoFijo;
                    response = await apiServicio.EditarAsync(recepcionActivoFijoDetalle.IdActivoFijo.ToString(), listaRecepcionActivoFijoUbicacionTransfer, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos");
                    if (response.IsSuccess)
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha editado una recepción de activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Activo Fijo:", recepcionActivoFijoDetalle.ActivoFijo.IdActivoFijo) });
                }

                var responseFile = new Response { IsSuccess = true };
                if (response.IsSuccess)
                {
                    if (Request.Form.Files.Count > 0)
                    {
                        foreach (var item in Request.Form.Files)
                        {
                            byte[] data;
                            using (var br = new BinaryReader(item.OpenReadStream()))
                                data = br.ReadBytes((int)item.OpenReadStream().Length);

                            if (data.Length > 0)
                            {
                                var activoFijoDocumentoTransfer = new DocumentoActivoFijoTransfer { Nombre = item.FileName, Fichero = data, IdActivoFijo = idActivoFijo };
                                responseFile = await apiServicio.InsertarAsync(activoFijoDocumentoTransfer, new Uri(WebApp.BaseAddressRM), "api/DocumentoActivoFijo/UploadFiles");
                                if (responseFile != null && responseFile.IsSuccess)
                                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha subido un archivo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Documento de Activo Fijo:", activoFijoDocumentoTransfer.Nombre) });
                            }
                        }
                    }
                }

                if (response.IsSuccess)
                    return this.Redireccionar(responseFile.IsSuccess ? $"{Mensaje.Informacion}|{Mensaje.Satisfactorio}" : $"{Mensaje.Aviso}|{Mensaje.ErrorUploadFiles}", !recepcionActivoFijoDetalle.RecepcionActivoFijo.ValidacionTecnica ? nameof(ActivosFijosRecepcionados) : nameof(ActivosFijosValidacionTecnica));

                ViewData["Error"] = response.Message;
                ViewData["UbicacionActivoFijo"] = new UbicacionActivoFijo { LibroActivoFijo = libroActivoFijo };
                ViewData["ListadoRecepcionActivoFijoDetalle"] = listaRecepcionActivoFijoUbicacionTransfer;
                ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo(recepcionActivoFijoDetalle?.ActivoFijo?.SubClaseActivoFijo?.ClaseActivoFijo?.IdTipoActivoFijo ?? -1);
                ViewData["SubClaseActivoFijo"] = await ObtenerSelectListSubClaseActivoFijo(recepcionActivoFijoDetalle?.ActivoFijo?.SubClaseActivoFijo?.IdClaseActivoFijo ?? -1);
                ViewData["MotivoRecepcion"] = new SelectList(await apiServicio.Listar<MotivoRecepcion>(new Uri(WebApp.BaseAddressRM), "api/MotivoRecepcion/ListarMotivoRecepcion"), "IdMotivoRecepcion", "Descripcion");
                ViewData["FondoFinanciamiento"] = new SelectList(await apiServicio.Listar<FondoFinanciamiento>(new Uri(WebApp.BaseAddressRM), "api/FondoFinanciamiento/ListarFondoFinanciamiento"), "IdFondoFinanciamiento", "Nombre");

                ViewData["Sucursal"] = new SelectList(await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal"), "IdSucursal", "Nombre");
                ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo(libroActivoFijo?.IdSucursal ?? -1);

                ViewData["Proveedor"] = new SelectList((await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedores")).Select(c => new { c.IdProveedor, NombreApellidos = $"{c.Nombre} {c.Apellidos}" }), "IdProveedor", "NombreApellidos");
                ViewData["Marca"] = new SelectList(await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddressRM), "api/Marca/ListarMarca"), "IdMarca", "Nombre");
                ViewData["Modelo"] = await ObtenerSelectListModelo(recepcionActivoFijoDetalle?.ActivoFijo?.Modelo?.IdMarca ?? -1);

                ViewData["Ramo"] = new SelectList(await apiServicio.Listar<Ramo>(new Uri(WebApp.BaseAddressRM), "api/Ramo/ListarRamo"), "IdRamo", "Nombre");
                ViewData["Subramo"] = await ObtenerSelectListSubramo(recepcionActivoFijoDetalle?.ActivoFijo?.PolizaSeguroActivoFijo?.Subramo?.IdRamo ?? -1);
                ViewData["CompaniaSeguro"] = new SelectList(await apiServicio.Listar<CompaniaSeguro>(new Uri(WebApp.BaseAddressRM), "api/CompaniaSeguro/ListarCompaniaSeguro"), "IdCompaniaSeguro", "Nombre");

                ViewData["IdCategoria"] = int.Parse(Request.Form["IdCategoria"].ToString());
                ViewData["Categoria"] = new SelectList(new[] { new { Id = 1, Valor = "Inmueble" }, new { Id = 2, Valor = "Vehículo" }, new { Id = 3, Valor = "Otro" } }, "Id", "Valor", ViewData["IdCategoria"]);
                return recepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle == 0 ? View(recepcionActivoFijoDetalle) : View("EditarRecepcion", recepcionActivoFijoDetalle);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando recepción Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(Recepcion));
            }
        }

        public async Task<IActionResult> DeleteRecepcion(string id, bool activoFijoRecepcionado)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Sistema", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webappth" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}", activoFijoRecepcionado ? nameof(ActivosFijosRecepcionados) : nameof(ActivosFijosValidacionTecnica));
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}", activoFijoRecepcionado ? nameof(ActivosFijosRecepcionados) : nameof(ActivosFijosValidacionTecnica));
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar Marca", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}", activoFijoRecepcionado ? nameof(ActivosFijosRecepcionados) : nameof(ActivosFijosValidacionTecnica));
            }
        }

        private string ObtenerCodigoSecuencial(string idSucursal, string idClaseActivoFijo, string idSubClaseActivoFijo, int? numeroConsecutivo = null)
        {
            try
            {
                string codigoSecuencial = $"{idSucursal}. {AgregarCeros(idClaseActivoFijo, 3)}. {AgregarCeros(idSubClaseActivoFijo, 3)}";
                return numeroConsecutivo != null ? $"{codigoSecuencial}. {numeroConsecutivo}" : codigoSecuencial;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string AgregarCeros(string valor, int cantidadCeros)
        {
            if (valor.Length < cantidadCeros)
            {
                var longitud = cantidadCeros - valor.Length;
                for (var i = 0; i < longitud; i++)
                    valor = "0" + valor;
            }
            return valor;
        }

        [HttpPost]
        public async Task<JsonResult> ValidarCodigoBarras(ActivoFijo activoFijo)
        {
            try
            {
                var listaCodigoActivoFijo = await apiServicio.Listar<CodigoActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/CodigoActivoFijo/ListarCodigosActivoFijo");
                return Json(!(activoFijo.CodigoActivoFijo.IdCodigoActivoFijo == 0 ? listaCodigoActivoFijo.Any(c => c.CodigoBarras == activoFijo.CodigoActivoFijo.CodigoBarras.Trim()) : listaCodigoActivoFijo.Where(c => c.CodigoBarras == activoFijo.CodigoActivoFijo.CodigoBarras.Trim()).Any(c => c.IdCodigoActivoFijo != activoFijo.CodigoActivoFijo.IdCodigoActivoFijo)));
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        [HttpPost]
        public async Task<JsonResult> ValidarCodigoUnico(ActivoFijo activoFijo)
        {
            try
            {
                var listaCodigoActivoFijo = await apiServicio.Listar<CodigoActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/CodigoActivoFijo/ListarCodigosActivoFijo");
                string codigoSecuencial = ObtenerCodigoSecuencial(activoFijo.CodigoActivoFijo.SUC, activoFijo.CodigoActivoFijo.CAF, activoFijo.CodigoActivoFijo.SUBCAF, activoFijo.CodigoActivoFijo.Consecutivo);
                return Json(!(activoFijo.CodigoActivoFijo.IdCodigoActivoFijo == 0 ? listaCodigoActivoFijo.Any(c => c.Codigosecuencial == codigoSecuencial.Trim()) : listaCodigoActivoFijo.Where(c => c.Codigosecuencial == codigoSecuencial.Trim()).Any(c => c.IdCodigoActivoFijo != activoFijo.CodigoActivoFijo.IdCodigoActivoFijo)));
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recepcion(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle, LibroActivoFijo libroActivoFijo) => await GestionRecepcionActivoFijoDetalle(recepcionActivoFijoDetalle, libroActivoFijo);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRecepcionAR(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle, LibroActivoFijo libroActivoFijo) => await GestionRecepcionActivoFijoDetalle(recepcionActivoFijoDetalle, libroActivoFijo);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRecepcionVT(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle, LibroActivoFijo libroActivoFijo) => await GestionRecepcionActivoFijoDetalle(recepcionActivoFijoDetalle, libroActivoFijo);

        public async Task<IActionResult> ObtenerRecepcionActivoFijo(string id, string estado, string nombreVistaError)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>($"{id}/{estado}", new Uri(WebApp.BaseAddressRM), "api/ActivosFijos");
                    if (respuesta.IsSuccess)
                    {
                        var activoFijo = JsonConvert.DeserializeObject<ActivoFijo>(respuesta.Resultado.ToString());
                        return View(activoFijo);
                    }
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nombreVistaError);
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nombreVistaError);
            }
        }

        public async Task<IActionResult> ActivosFijosRecepcionados()
        {
            var lista = new List<ActivoFijo>();
            ViewData["IsConfiguracionRecepcion"] = true;
            ViewData["Titulo"] = "Activos Fijos Recepcionados";
            ViewData["UrlEditar"] = nameof(EditarRecepcionAR);
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<ActivoFijo>>(Estados.Recepcionado, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivoFijoPorEstado");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos recepcionados", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoActivoFijo", lista);
        }

        public async Task<IActionResult> ActivosFijosValidacionTecnica()
        {
            var lista = new List<ActivoFijo>();
            ViewData["IsConfiguracionRecepcion"] = true;
            ViewData["Titulo"] = "Activos Fijos que requieren Validación Técnica";
            ViewData["UrlEditar"] = nameof(EditarRecepcionVT);
            ViewData["UrlRevision"] = nameof(RevisionActivoFijo);
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<ActivoFijo>>(Estados.ValidacionTecnica, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivoFijoPorEstado");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos que requieren validación técnica", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoActivoFijo", lista);
        }

        public async Task<IActionResult> GestionarAprobacionActivoFijo(int id, bool id2)
        {
            try
            {
                if (id2)
                {
                    await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Recepcionado }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                    await apiServicio.InsertarAsync(new AprobacionActivoFijoTransfer { IdActivoFijo = id, NuevoEstadoActivoFijo = Estados.Recepcionado, ValidacionTecnica = false }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/AprobacionActivoFijo");
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ActivosFijosRecepcionados));
                }
                else
                {
                    await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Desaprobado }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                    await apiServicio.InsertarAsync(new AprobacionActivoFijoTransfer { IdActivoFijo = id, NuevoEstadoActivoFijo = Estados.Desaprobado, ValidacionTecnica = true }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/AprobacionActivoFijo");
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ActivosFijosValidacionTecnica));
                }
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando aprobación Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ActivosFijosValidacionTecnica));
            }
        }

        public async Task<IActionResult> RevisionActivoFijo(string id) => await ObtenerRecepcionActivoFijo(id, Estados.ValidacionTecnica, nameof(ActivosFijosValidacionTecnica));

        [HttpPost]
        public async Task<IActionResult> ModalDatosEspecificosResult(RecepcionActivoFijoDetalle rafd, int categoria, int idSucursal, int? idBodega, int? idEmpleado)
        {
            ViewData["Categoria"] = categoria;
            ViewData["Empleado"] = await ObtenerSelectListEmpleado(idSucursal);
            ViewData["Bodega"] = await ObtenerSelectListBodega(idSucursal);
            ViewData["IdTipoUbicacion"] = idBodega != null ? 1 : idEmpleado != null ? 2 : 1;
            ViewData["TipoUbicacion"] = new SelectList(new[] { new { Id = 1, Valor = "Bodega" }, new { Id = 2, Valor = "Custodio" } }, "Id", "Valor", ViewData["IdTipoUbicacion"]);
            return PartialView("_ModalDatosEspecificos", rafd);
        }

        [HttpPost]
        public async Task<IActionResult> ValidacionDatosEspecificosResult(RecepcionActivoFijoDetalleDatosEspecificosViewModel rafd)
        {
            var listaPropiedadValorErrores = rafd.Validar();
            try
            {
                if (listaPropiedadValorErrores.Count == 0)
                {
                    var response = await apiServicio.InsertarAsync(new RecepcionActivoFijoDetalle { IdRecepcionActivoFijoDetalle = rafd.IdRecepcionActivoFijoDetalle, Serie = rafd.Serie, NumeroChasis = rafd.NumeroChasis, NumeroMotor = rafd.NumeroMotor, Placa = rafd.Placa, NumeroClaveCatastral = rafd.NumeroClaveCatastral }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ValidacionRecepcionActivoFijoDetalleDatosEspecificos");
                    if (response.IsSuccess)
                        listaPropiedadValorErrores = JsonConvert.DeserializeObject<List<PropiedadValor>>(response.Resultado.ToString());
                    else
                        listaPropiedadValorErrores.Add(new PropiedadValor { Propiedad = "IdBodega", Valor = "Ha ocurrido un error al intentar validar los datos." });
                }
            }
            catch (Exception)
            { }
            return Json(listaPropiedadValorErrores);
        }

        [HttpPost]
        public async Task<IActionResult> DetallesActivoFijoResult(List<IdRecepcionActivoFijoDetalleSeleccionado> listadoRecepcionActivoFijoDetalleSeleccionado, List<PropiedadValor> arrConfiguraciones)
        {
            var lista = new List<RecepcionActivoFijoDetalleSeleccionado>();
            try
            {
                ViewData["Configuraciones"] = arrConfiguraciones;
                lista = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(listadoRecepcionActivoFijoDetalleSeleccionado, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijo");
            }
            catch (Exception)
            { }
            return PartialView("_ListadoDetallesActivosFijos", lista);
        }
        #endregion

        #region Póliza de Seguro
        public async Task<IActionResult> ActivosFijosRecepcionadosSinPoliza()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/ListarRecepcionActivoFijoSinPoliza");
                ViewData["Titulo"] = "Activos Fijos sin Póliza de Seguro";
                ViewData["textoColumna"] = "Asignar Póliza";
                ViewData["url"] = "AsignarPoliza";
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con estado Recepcionado sin número de póliza asignado", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoActivoFijo", lista);
        }

        public async Task<IActionResult> ActivosFijosRecepcionadosConPoliza()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/ListarRecepcionActivoFijoConPoliza");
                ViewData["Titulo"] = "Activos Fijos con Póliza de Seguro";
                ViewData["textoColumna"] = "Editar Póliza";
                ViewData["url"] = "AsignarPoliza";
                ViewData["poliza"] = true;
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con estado Recepcionado con número de póliza asignado", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoActivoFijo", lista);
        }

        public async Task<IActionResult> AsignarPoliza(string id) => await ObtenerRecepcionActivoFijo(id, Estados.Recepcionado, nameof(ActivosFijosRecepcionadosSinPoliza));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarPoliza(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle)
        {
            try
            {
                var response = await apiServicio.InsertarAsync(recepcionActivoFijoDetalle, new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/AsignarPoliza");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha asignado el número de póliza", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Recepción Activo Fijo Detalle:", recepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ActivosFijosRecepcionadosConPoliza));
                }
                ViewData["Error"] = response.Message;
                return View(recepcionActivoFijoDetalle);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Asignando Póliza", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ActivosFijosRecepcionadosSinPoliza));
            }
        }
        #endregion

        #region Alta de Activos
        public async Task<IActionResult> AsignarNumeroFactura(string id) => await ObtenerRecepcionActivoFijo(id, Estados.Recepcionado, nameof(ActivosFijosRecepcionados));
        
        public async Task<IActionResult> ActivosFijosAltas()
        {
            try
            {
                var lista = await apiServicio.ObtenerElementoAsync<List<ActivoFijo>>(Estados.Alta, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivoFijoPorEstado");
                ViewData["Titulo"] = "Activos Fijos con Estado Alta";
                return View("ListadoActivoFijoAlta", lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con estado Alta", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> AprobarAlta(int id)
        {
            Response response = new Response();
            try
            {
                RecepcionActivoFijoDetalle RecepcionActivoFijoDetalle = new RecepcionActivoFijoDetalle();
                var respuesta = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo");
                respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionActivoFijoDetalle>(respuesta.Resultado.ToString());
                RecepcionActivoFijoDetalle = respuesta.Resultado as RecepcionActivoFijoDetalle;
                
                if (respuesta.IsSuccess)
                {
                    try
                    {
                        AltaActivoFijo AFA = new AltaActivoFijo { IdRecepcionActivoFijoDetalle = RecepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle, FechaAlta = DateTime.Now };
                        response = await apiServicio.InsertarAsync(AFA, new Uri(WebApp.BaseAddressRM), "api/ActivosFijosAlta/InsertarActivosFijosAlta");
                        if (response.IsSuccess)
                        {
                            await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha insertado un Activo Fijo con el estado ALTA", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Activo Fijo con estado Alta:", AFA.IdFactura) });
                            ViewData["mensaje"] = "Activo Fijo Alta insertado correctamente";

                            try
                            {
                                RecepcionActivoFijoDetalle.Estado = new Estado { Nombre = Estados.Alta };
                                response = await apiServicio.EditarAsync(RecepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle.ToString(), RecepcionActivoFijoDetalle, new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/EstadoActivoFijo");
                                if (response.IsSuccess)
                                {
                                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Estado de Activo Fijo", RecepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha actualizado un registro estado (Alta) de activo fijo", UserName = "Usuario 1" });
                                    return RedirectToAction("ActivosFijosRecepcionados");
                                }
                            }
                            catch (Exception ex)
                            {
                                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Cambiando Estado de un Activo Fijo a Alta", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                                return BadRequest();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Insertando Alta de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Seleccionando un Recepción Activo Fijo Detalle", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return BadRequest();
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarNumeroFactura(RecepcionActivoFijoDetalle recepcionAFD)
        {
            Response response = new Response();
            int fact = int.Parse(Request.Form["facturas"]);
            AltaActivoFijo AFA = new AltaActivoFijo { IdRecepcionActivoFijoDetalle = recepcionAFD.IdRecepcionActivoFijoDetalle, IdFactura = fact, FechaAlta = DateTime.Now };
            try
            {
                response = await apiServicio.InsertarAsync(AFA, new Uri(WebApp.BaseAddressRM), "api/ActivosFijosAlta/InsertarActivosFijosAlta");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha insertado un Activo Fijo con el estado ALTA", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Activo Fijo con estado Alta:", AFA.IdFactura) });
                    ViewData["mensaje"] = "Activo Fijo Alta insertado correctamente";

                    try
                    {
                        int IdRecepcionActivoFijoDetalle = int.Parse(Request.Form["IdRecepcionActivoFijoDetalle"]);
                        RecepcionActivoFijoDetalle RecepcionActivoFijoDetalle = new RecepcionActivoFijoDetalle();
                        var respuesta = await apiServicio.SeleccionarAsync<Response>(IdRecepcionActivoFijoDetalle.ToString(), new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo");
                        respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionActivoFijoDetalle>(respuesta.Resultado.ToString());
                        RecepcionActivoFijoDetalle = respuesta.Resultado as RecepcionActivoFijoDetalle;
                        RecepcionActivoFijoDetalle.Estado = new Estado { Nombre = Estados.Alta };

                        if (respuesta.IsSuccess)
                        {
                            try
                            {
                                response = await apiServicio.EditarAsync(RecepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle.ToString(), RecepcionActivoFijoDetalle, new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/EstadoActivoFijo");
                                if (response.IsSuccess)
                                {
                                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Estado de Activo Fijo", IdRecepcionActivoFijoDetalle), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha actualizado un registro estado (Alta) de activo fijo", UserName = "Usuario 1" });
                                    return RedirectToAction("ActivosFijosRecepcionados");
                                }
                            }
                            catch (Exception ex)
                            {
                                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Cambiando Estado de un Activo Fijo a Alta", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                                return BadRequest();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Seleccionando un Activo Fijo Recepcion Detalle", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Insertando Alta de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return BadRequest();
            }
            return View();
        }

        public async Task<IActionResult> AddComponentes(string id)
        {
            Response response = new Response();
            try
            {
                RecepcionActivoFijoDetalle RecepcionActivoFijoDetalle = new RecepcionActivoFijoDetalle();
                var respuesta = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo");
                respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionActivoFijoDetalle>(respuesta.Resultado.ToString());
                RecepcionActivoFijoDetalle = respuesta.Resultado as RecepcionActivoFijoDetalle;
                if (respuesta.IsSuccess)
                {
                    try
                    {
                        var lista = (await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/ListarRecepcionActivoFijo")).Where(c => c.Estado.Nombre == Estados.Recepcionado && c.RecepcionActivoFijo.IdMotivoRecepcion == 2).ToList();
                        ViewBag.objeto = RecepcionActivoFijoDetalle;
                        try
                        {
                            //List<ActivosFijosAdicionados> listaActivosFijosAdicionados = new List<ActivosFijosAdicionados>();
                            //listaActivosFijosAdicionados = await apiServicio.Listar<ActivosFijosAdicionados>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijosAdicionados/ListarActivosFijosAdicionados");
                            //ViewBag.listaActivosFijosAdicionados = listaActivosFijosAdicionados;
                        }
                        catch (Exception)
                        { }
                        return View("AdicionarComponentes", lista);

                    }
                    catch (Exception ex)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con estado Recepcionado para adicionar componentes", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                        return BadRequest();
                    }                    
                }
            }
            catch (Exception)
            { }
            return View("AdicionarComponentes");
        }

        public IActionResult AdicionarActivo(string id, string id1, string id2)
        {
            //ActivosFijosAdicionados ActivosFijosAdicionados = new ActivosFijosAdicionados { idActivoFijoOrigen = int.Parse(id1), idActivoFijoDestino = int.Parse(id), fechaAdicion = DateTime.Now };
            //var response = await apiServicio.InsertarAsync(ActivosFijosAdicionados, new Uri(WebApp.BaseAddressRM), "api/ActivosFijosAdicionados/InsertarActivosFijosAdicionados");
            //if (response.IsSuccess)
            //{
            //    List<ActivosFijosAdicionados> listaActivosFijosAdicionados = new List<ActivosFijosAdicionados>();
            //    listaActivosFijosAdicionados = await apiServicio.Listar<ActivosFijosAdicionados>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijosAdicionados/ListarActivosFijosAdicionados");
            //    ViewBag.listaActivosFijosAdicionados = listaActivosFijosAdicionados;
            //}
            return RedirectToAction("AddComponentes", new { id = id2});
        }

        public IActionResult EliminarActivoAdicionado(string idAdicion, string id2)
        {
            //ActivosFijosAdicionados ActivosFijosAdicionados = new ActivosFijosAdicionados();
            //var respuesta = await apiServicio.SeleccionarAsync<Response>(idAdicion, new Uri(WebApp.BaseAddressRM), "api/ActivosFijosAdicionados");
            //if (respuesta.IsSuccess)
            //{
            //    var response = await apiServicio.EliminarAsync(idAdicion, new Uri(WebApp.BaseAddressRM), "api/ActivosFijosAdicionados");
            //    if (response.IsSuccess)
            //        return RedirectToAction("AddComponentes", new { id = id2 });
            //}
            return RedirectToAction("AddComponentes", id2);
        }

        public async Task<IActionResult> ActivosFijosReporteAltas()
        {
            try
            {
                var lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/ListarRecepcionActivoFijo");
                ViewData["listaAFA"] = await apiServicio.Listar<AltaActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijosAlta/ListarAltasActivosFijos");
                return View("AltaReporte", lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos que han estado en Alta", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }
        #endregion

        #region Transferencias
        public async Task<IActionResult> ActivosFijosATransferir()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/ListarRecepcionActivoFijo");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con estado Recepcionado", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> TransferirActivoFijo(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ActivosFijosATransferir));

                    var recepcionActivoFijoDetalle = JsonConvert.DeserializeObject<RecepcionActivoFijoDetalle>(respuesta.Resultado.ToString());
                    ViewData["MotivoTransferencia"] = new SelectList(await apiServicio.Listar<MotivoTransferencia>(new Uri(WebApp.BaseAddressRM), "api/MotivoTransferencia/ListarMotivoTransferencia"), "IdMotivoTransferencia", "Motivo_Transferencia");

                    var listaPais = await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "api/Pais/ListarPais");
                    ViewData["Pais"] = new SelectList(listaPais, "IdPais", "Nombre");
                    ViewData["Provincia"] = await ObtenerSelectListProvincia((ViewData["Pais"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Pais"] as SelectList).FirstOrDefault().Value) : -1);
                    ViewData["Ciudad"] = await ObtenerSelectListCiudad((ViewData["Provincia"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Provincia"] as SelectList).FirstOrDefault().Value) : -1);
                    ViewData["Sucursal"] = await ObtenerSelectListSucursal((ViewData["Ciudad"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Ciudad"] as SelectList).FirstOrDefault().Value) : -1);
                    ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo((ViewData["Sucursal"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Sucursal"] as SelectList).FirstOrDefault().Value) : -1);

                    //ViewData["PaisOrigen"] = new SelectList(listaPais, "IdPais", "Nombre", recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.IdPais ?? -1);
                    //ViewData["ProvinciaOrigen"] = await ObtenerSelectListProvincia(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.IdPais ?? -1);
                    //ViewData["CiudadOrigen"] = await ObtenerSelectListCiudad(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.IdProvincia ?? -1);
                    //ViewData["SucursalOrigen"] = await ObtenerSelectListSucursal(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.IdCiudad ?? -1);
                    //ViewData["LibroActivoFijoOrigen"] = await ObtenerSelectListLibroActivoFijo(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.IdSucursal ?? -1);

                    ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                    ViewData["IdActivoFijo"] = recepcionActivoFijoDetalle.IdActivoFijo;
                    return View();
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ActivosFijosATransferir));
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando Transferencia de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ActivosFijosATransferir));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferirActivoFijo(int IdActivoFijo, TransferenciaActivoFijo transferenciaActivoFijo)
        {
            try
            {
                var activoFijoOrigen = JsonConvert.DeserializeObject<ActivoFijo>((await apiServicio.SeleccionarAsync<Response>(IdActivoFijo.ToString(), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos")).Resultado.ToString());
                //transferenciaActivoFijoDetalle.TransferenciaActivoFijo.Origen = activoFijoOrigen?.LibroActivoFijo?.Sucursal?.Nombre ?? activoFijoOrigen?.Ciudad?.Nombre;

                //var libroActivoFijoDestino = JsonConvert.DeserializeObject<LibroActivoFijo>((await apiServicio.SeleccionarAsync<Response>(transferenciaActivoFijoDetalle.ActivoFijo.IdLibroActivoFijo.ToString(), new Uri(WebApp.BaseAddressRM), "api/LibroActivoFijo")).Resultado.ToString());
                //transferenciaActivoFijoDetalle.TransferenciaActivoFijo.Destino = libroActivoFijoDestino.Sucursal.Nombre;
                
                //int idPaisDestino = transferenciaActivoFijoDetalle.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.IdPais ?? -1;
                //int idProvinciaDestino = transferenciaActivoFijoDetalle.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.IdProvincia ?? -1;
                //int idCiudadDestino = transferenciaActivoFijoDetalle.ActivoFijo?.LibroActivoFijo?.Sucursal?.IdCiudad ?? -1;
                //int idSucursalDestino = transferenciaActivoFijoDetalle.ActivoFijo?.LibroActivoFijo?.IdSucursal ?? -1;

                //transferenciaActivoFijo.ActivoFijo.IdActivoFijo = IdActivoFijo;
                //transferenciaActivoFijoDetalle.ActivoFijo.LibroActivoFijo = libroActivoFijoDestino;
                var response = await apiServicio.InsertarAsync(transferenciaActivoFijo, new Uri(WebApp.BaseAddressRM), "api/TransferenciaActivoFijoDetalle/InsertarTransferenciaActivoFijoDetalle");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado una transferencia de Activo Fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Detalle de Transferencia:", transferenciaActivoFijo.IdTransferenciaActivoFijo) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ActivosFijosATransferir));
                }
                ViewData["Error"] = response.Message;
                ViewData["MotivoTransferencia"] = new SelectList(await apiServicio.Listar<MotivoTransferencia>(new Uri(WebApp.BaseAddressRM), "api/MotivoTransferencia/ListarMotivoTransferencia"), "IdMotivoTransferencia", "Motivo_Transferencia");

                var listaPais = await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "api/Pais/ListarPais");
                //ViewData["Pais"] = new SelectList(listaPais, "IdPais", "Nombre", idPaisDestino);
                //ViewData["Provincia"] = await ObtenerSelectListProvincia(idPaisDestino);
                //ViewData["Ciudad"] = await ObtenerSelectListCiudad(idProvinciaDestino);
                //ViewData["Sucursal"] = await ObtenerSelectListSucursal(idCiudadDestino);
                //ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo(idSucursalDestino);

                //ViewData["PaisOrigen"] = new SelectList(listaPais, "IdPais", "Nombre", activoFijoOrigen?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.IdPais ?? -1);
                //ViewData["ProvinciaOrigen"] = await ObtenerSelectListProvincia(activoFijoOrigen?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.IdPais ?? -1);
                //ViewData["CiudadOrigen"] = await ObtenerSelectListCiudad(activoFijoOrigen?.LibroActivoFijo?.Sucursal?.Ciudad?.IdProvincia ?? -1);
                //ViewData["SucursalOrigen"] = await ObtenerSelectListSucursal(activoFijoOrigen?.LibroActivoFijo?.Sucursal?.IdCiudad ?? -1);
                //ViewData["LibroActivoFijoOrigen"] = await ObtenerSelectListLibroActivoFijo(activoFijoOrigen?.LibroActivoFijo?.IdSucursal ?? -1);

                ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                ViewData["IdActivoFijo"] = IdActivoFijo;
                return View(transferenciaActivoFijo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando un Modelo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ActivosFijosATransferir));
            }
        }
        #endregion

        #region Baja de Activos
        public async Task<IActionResult> ActivoFijoBaja(string id)
        {
            ViewData["MotivoActivoFijoBaja"] = new SelectList(await apiServicio.Listar<MotivoBaja>(new Uri(WebApp.BaseAddressRM), "api/ActivoFijoMotivoBaja/ListarActivoFijoMotivoBaja"), "IdActivoFijoMotivoBaja", "Nombre");
            return await ObtenerRecepcionActivoFijo(id, Estados.Alta, nameof(ActivosFijosBajas));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivoFijoBaja(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle)
        {
            try
            {
                //BajaActivoFijoDetalle activosFijosBaja = new BajaActivoFijoDetalle {FechaBaja = DateTime.Now, IdMotivoBaja = recepcionActivoFijoDetalle.ActivoFijo.ActivosFijosBaja.IdMotivoBaja, IdActivo = recepcionActivoFijoDetalle.IdActivoFijo };
                //var response = await apiServicio.InsertarAsync(activosFijosBaja, new Uri(WebApp.BaseAddressRM), "api/ActivosFijosBaja/InsertarActivosFijosBaja");
                //if (response.IsSuccess)
                //    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado una Baja de Activo Fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "ActivosFijosBaja:", activosFijosBaja.IdMotivoBaja) });

                var response = await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Baja }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                recepcionActivoFijoDetalle.Estado = new Estado { Nombre = Estados.Baja };
                response = await apiServicio.EditarAsync(recepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle.ToString(), recepcionActivoFijoDetalle, new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/EstadoActivoFijo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Estado de Activo Fijo", recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.IdCodigoActivoFijo.ToString()), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha actualizado un registro estado de activo fijo", UserName = "Usuario 1" });
                    return RedirectToAction("ActivosFijosRecepcionadosBaja");
                }
                ViewData["Error"] = response.Message;
                ViewData["MotivoActivoFijoBaja"] = new SelectList(await apiServicio.Listar<MotivoBaja>(new Uri(WebApp.BaseAddressRM), "api/ActivoFijoMotivoBaja/ListarActivoFijoMotivoBaja"), "IdActivoFijoMotivoBaja", "Nombre");
                return View(recepcionActivoFijoDetalle);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando Tipo Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> ActivosFijosBajas()
        {
            try
            {
                var lista = await apiServicio.ObtenerElementoAsync<List<ActivoFijo>>(Estados.Baja, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivoFijoPorEstado");
                ViewData["Titulo"] = "Activos Fijos con Estado Baja";
                return View("ListadoActivoFijoBaja", lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con estado Baja", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> ActivosFijosRecepcionadosBaja()
        {
            try
            {
                var lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivoFijoPorEstado/Alta");
                ViewData["Titulo"] = "Activos Fijos de Alta";
                ViewData["textoColumna"] = "Dar Baja";
                ViewData["url"] = "ActivoFijoBaja";
                return View("ListadoActivoFijo", lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con estado Recepcionado", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }
        #endregion

        #region Mantenimiento de Activos
        public async Task<IActionResult> ListarMantenimientosActivosFijos()
        {
            var lista = new List<ActivoFijo>();
            try
            {
                ViewData["IsConfiguracionMantenimiento"] = true;
                ViewData["Titulo"] = "Activos Fijos en Alta";
                ViewData["UrlEditar"] = nameof(EditarMantenimiento);
                lista = await apiServicio.ObtenerElementoAsync<List<ActivoFijo>>(Estados.Alta, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivoFijoPorEstado");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando Mantenimientos de Activos Fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoActivoFijo", lista);
        }

        public async Task<IActionResult> ListarMantenimientos(string id)
        {
            var lista = new List<MantenimientoActivoFijo>();
            ViewData["IdRecepcionActivoFijoDetalle"] = id;
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<MantenimientoActivoFijo>>(id, new Uri(WebApp.BaseAddressRM), "api/MantenimientoActivoFijo/ListarMantenimientosActivoFijoPorIdDetalleActivoFijo");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando Mantenimientos de Activos Fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> CrearMantenimiento(int id)
        {
            try
            {
                ViewData["IdRecepcionActivoFijoDetalle"] = id;
                ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListarMantenimientos), routeValues: new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearMantenimiento(MantenimientoActivoFijo mantenimientoActivoFijo)
        {
            try
            {
                var response = new Response();
                if (ModelState.IsValid)
                {
                    response = await apiServicio.InsertarAsync(mantenimientoActivoFijo, new Uri(WebApp.BaseAddressRM), "api/MantenimientoActivoFijo/InsertarMantenimientoActivoFijo");
                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado un Mantenimiento Activo Fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Mantenimiento Activo Fijo:", mantenimientoActivoFijo.IdMantenimientoActivoFijo) });
                        return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ListarMantenimientos), routeValues: new { id = mantenimientoActivoFijo.IdRecepcionActivoFijoDetalle });
                    }
                }
                else
                    response.Message = Mensaje.ModeloInvalido;

                ViewData["Error"] = response.Message;
                ViewData["IdRecepcionActivoFijoDetalle"] = mantenimientoActivoFijo.IdRecepcionActivoFijoDetalle;
                ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                return View(response.Resultado);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando Mantenimiento Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListarMantenimientos), routeValues: new { id = mantenimientoActivoFijo.IdRecepcionActivoFijoDetalle });
            }
        }

        public async Task<IActionResult> EditarMantenimiento(string id, string id2)
        {
            try
            {
                if (!string.IsNullOrEmpty(id2))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id2, new Uri(WebApp.BaseAddressRM), "api/MantenimientoActivoFijo");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListarMantenimientos), routeValues: new { id });

                    respuesta.Resultado = JsonConvert.DeserializeObject<MantenimientoActivoFijo>(respuesta.Resultado.ToString());
                    ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                    return View(respuesta.Resultado);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListarMantenimientos), routeValues: new { id });
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListarMantenimientos), routeValues: new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarMantenimiento(MantenimientoActivoFijo mantenimientoActivoFijo)
        {
            try
            {
                if (!string.IsNullOrEmpty(mantenimientoActivoFijo.IdMantenimientoActivoFijo.ToString()))
                {
                    var response = new Response();
                    if (ModelState.IsValid)
                    {
                        response = await apiServicio.EditarAsync(mantenimientoActivoFijo.IdMantenimientoActivoFijo.ToString(), mantenimientoActivoFijo, new Uri(WebApp.BaseAddressRM), "api/MantenimientoActivoFijo");
                        if (response.IsSuccess)
                        {
                            await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Mantenimiento", mantenimientoActivoFijo.IdMantenimientoActivoFijo.ToString()), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha actualizado un registro Mantenimiento Activo Fijo", UserName = "Usuario 1" });
                            return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ListarMantenimientos), routeValues: new { id = mantenimientoActivoFijo.IdRecepcionActivoFijoDetalle });
                        }
                    }
                    else
                        response.Message = Mensaje.ModeloInvalido;

                    ViewData["Error"] = response.Message;
                    ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                    return View(mantenimientoActivoFijo);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListarMantenimientos), routeValues: new { id = mantenimientoActivoFijo.IdRecepcionActivoFijoDetalle });
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Editando un Mantenimiento Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorEditar}", nameof(ListarMantenimientos), routeValues: new { id = mantenimientoActivoFijo.IdRecepcionActivoFijoDetalle });
            }
        }

        public async Task<IActionResult> EliminarMantenimiento(string id, string id2)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id2, new Uri(WebApp.BaseAddressRM), "api/MantenimientoActivoFijo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Mantenimiento Activo Fijo", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webappth" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}", nameof(ListarMantenimientos), routeValues: new { id });
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}", nameof(ListarMantenimientos), routeValues: new { id });
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar Mantenimiento Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}", nameof(ListarMantenimientos), routeValues: new { id });
            }
        }
        #endregion

        #region Reportes
        public async Task<IActionResult> HojaVidaActivoFijo(string id) => await ObtenerRecepcionActivoFijo(id, null, nameof(HojaVidaReporte));

        public async Task<IActionResult> HojaVidaReporte()
        {
            try
            {
                var lista = await apiServicio.ObtenerElementoAsync<List<ActivoFijo>>(Estados.Alta, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivoFijoPorEstado");
                ViewData["Titulo"] = "Activos Fijos";
                ViewData["textoColumna"] = "Ver Hoja de Vida";
                ViewData["url"] = "HojaVidaActivoFijo";
                return View("ListadoActivoFijo", lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con estado Recepcionado", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> BienesReporte()
        {
            try
            {
                var lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/BienesReporte");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos por área usuaria y clasificado por funcionario", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> MantenimientosReporte()
        {
            try
            {
                var lista = await apiServicio.Listar<MantenimientoActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/MantenimientoActivoFijo/ListarMantenimientosActivoFijo");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando mantenimientos de activos fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> PolizasReporte()
        {
            try
            {
                var lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/ListarRecepcionActivoFijoConPoliza");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con estado Recepcionado con número de póliza asignado", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }
        #endregion

        #region AJAX_ClaseActivoFijo
        public async Task<SelectList> ObtenerSelectListClaseActivoFijo(int idTipoActivoFijo)
        {
            try
            {
                var listaClaseActivoFijo = idTipoActivoFijo != -1 ? await apiServicio.Listar<ClaseActivoFijo>(new Uri(WebApp.BaseAddressRM), $"api/ClaseActivoFijo/ListarClaseActivoFijoPorTipoActivoFijo/{idTipoActivoFijo}") : new List<ClaseActivoFijo>();
                return new SelectList(listaClaseActivoFijo, "IdClaseActivoFijo", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<ClaseActivoFijo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClaseActivoFijo_SelectResult(int idTipoActivoFijo)
        {
            ViewBag.ClaseActivoFijo = await ObtenerSelectListClaseActivoFijo(idTipoActivoFijo);
            return PartialView("_ClaseActivoFijoSelect", new RecepcionActivoFijoDetalle());
        }
        #endregion

        #region AJAX_SubClaseActivoFijo
        public async Task<SelectList> ObtenerSelectListSubClaseActivoFijo(int idClaseActivoFijo)
        {
            try
            {
                var listaSubClaseActivoFijo = idClaseActivoFijo != -1 ? await apiServicio.Listar<SubClaseActivoFijo>(new Uri(WebApp.BaseAddressRM), $"api/SubClaseActivoFijo/ListarSubClasesActivoFijoPorClase/{idClaseActivoFijo}") : new List<SubClaseActivoFijo>();
                return new SelectList(listaSubClaseActivoFijo, "IdSubClaseActivoFijo", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<SubClaseActivoFijo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubClaseActivoFijo_SelectResult(int idClaseActivoFijo)
        {
            ViewBag.SubClaseActivoFijo = await ObtenerSelectListSubClaseActivoFijo(idClaseActivoFijo);
            return PartialView("_SubClaseActivoFijoSelect", new RecepcionActivoFijoDetalle());
        }
        #endregion

        #region AJAX_Modelo
        public async Task<SelectList> ObtenerSelectListModelo(int idMarca)
        {
            try
            {
                var listaModelo = idMarca != -1 ? await apiServicio.Listar<Modelo>(new Uri(WebApp.BaseAddressRM), $"api/Modelo/ListarModelosPorMarca/{idMarca}") : new List<Modelo>();
                return new SelectList(listaModelo, "IdModelo", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<Modelo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Modelo_SelectResult(int idMarca)
        {
            ViewBag.Modelo = await ObtenerSelectListModelo(idMarca);
            return PartialView("_ModeloSelect", new RecepcionActivoFijoDetalle());
        }
        #endregion

        #region AJAX_Provincia
        public async Task<SelectList> ObtenerSelectListProvincia(int idPais)
        {
            try
            {
                var listaProvincia = idPais != -1 ? (await apiServicio.Listar<Provincia>(new Uri(WebApp.BaseAddressTH), "api/Provincia/ListarProvincia")).Where(c => c.IdPais == idPais).ToList() : new List<Provincia>();
                return new SelectList(listaProvincia, "IdProvincia", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<Provincia>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Provincia_SelectResult(int idPais)
        {
            ViewBag.Provincia = await ObtenerSelectListProvincia(idPais);
            return PartialView("_ProvinciaSelect", new UbicacionActivoFijo());
        }
        #endregion

        #region AJAX_Ciudad
        public async Task<SelectList> ObtenerSelectListCiudad(int idProvincia)
        {
            try
            {
                var listaCiudad = idProvincia != -1 ? (await apiServicio.Listar<Ciudad>(new Uri(WebApp.BaseAddressTH), "api/Ciudad/ListarCiudad")).Where(c => c.IdProvincia == idProvincia).ToList() : new List<Ciudad>();
                return new SelectList(listaCiudad, "IdCiudad", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<Ciudad>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Ciudad_SelectResult(int idProvincia)
        {
            ViewBag.Ciudad = await ObtenerSelectListCiudad(idProvincia);
            return PartialView("_CiudadSelect", new UbicacionActivoFijo());
        }
        #endregion

        #region AJAX_Sucursal
        public async Task<SelectList> ObtenerSelectListSucursal(int idCiudad)
        {
            try
            {
                var listaSucursal = idCiudad != -1 ? (await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal")).Where(c => c.IdCiudad == idCiudad).ToList() : new List<Sucursal>();
                return new SelectList(listaSucursal, "IdSucursal", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<Sucursal>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Sucursal_SelectResult(int idCiudad)
        {
            ViewBag.Sucursal = await ObtenerSelectListSucursal(idCiudad);
            return PartialView("_SucursalSelect", new UbicacionActivoFijo());
        }
        #endregion

        #region AJAX_LibroActivoFijo
        public async Task<SelectList> ObtenerSelectListLibroActivoFijo(int idSucursal)
        {
            try
            {
                var listaLibroActivoFijo = idSucursal != -1 ? await apiServicio.Listar<LibroActivoFijo>(new Uri(WebApp.BaseAddressRM), $"api/LibroActivoFijo/ListarLibrosActivoFijoPorSucursal/{idSucursal}") : new List<LibroActivoFijo>();
                return new SelectList(listaLibroActivoFijo, "IdLibroActivoFijo", "IdLibroActivoFijo");
            }
            catch (Exception)
            {
                return new SelectList(new List<LibroActivoFijo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> LibroActivoFijo_SelectResult(int idSucursal)
        {
            ViewBag.LibroActivoFijo = await ObtenerSelectListLibroActivoFijo(idSucursal);
            return PartialView("_LibroActivoFijoSelect", new UbicacionActivoFijo());
        }
        #endregion

        #region AJAX_Subramo
        public async Task<SelectList> ObtenerSelectListSubramo(int idRamo)
        {
            try
            {
                var listaSubramo = idRamo != -1 ? await apiServicio.Listar<Subramo>(new Uri(WebApp.BaseAddressRM), $"api/Subramo/ListarSubramoPorRamo/{idRamo}") : new List<Subramo>();
                return new SelectList(listaSubramo, "IdSubramo", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<ClaseActivoFijo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Subramo_SelectResult(int idRamo)
        {
            ViewBag.Subramo = await ObtenerSelectListSubramo(idRamo);
            return PartialView("_SubramoSelect", new RecepcionActivoFijoDetalle());
        }
        #endregion

        #region AJAX_Bodega
        public async Task<SelectList> ObtenerSelectListBodega(int idSucursal)
        {
            try
            {
                var listaBodega = idSucursal != -1 ? await apiServicio.Listar<Bodega>(new Uri(WebApp.BaseAddressRM), $"api/Bodega/ListarBodegaPorSucursal/{idSucursal}") : new List<Bodega>();
                return new SelectList(listaBodega, "IdBodega", "Nombre");
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
        public async Task<SelectList> ObtenerSelectListEmpleado(int idSucursal)
        {
            try
            {
                var listaEmpleado = idSucursal != -1 ? await apiServicio.ObtenerElementoAsync<List<DatosBasicosEmpleadoViewModel>>(new EmpleadosPorSucursalViewModel { IdSucursal = idSucursal, EmpleadosActivos = true }, new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleadosPorSucursal") : new List<DatosBasicosEmpleadoViewModel>();
                return new SelectList(listaEmpleado.Select(c=> new ListaEmpleadoViewModel { IdEmpleado = c.IdEmpleado, NombreApellido = $"{c.Nombres} {c.Apellidos}" }), "IdEmpleado", "NombreApellido");
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