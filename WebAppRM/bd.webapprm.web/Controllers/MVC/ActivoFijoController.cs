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
using Microsoft.AspNetCore.Hosting;
using bd.webapprm.entidades.ObjectTransfer;
using bd.webapprm.servicios.Extensores;
using Microsoft.AspNetCore.Routing;

namespace bd.webapprm.web.Controllers.MVC
{
    public class ActivoFijoController : Controller
    {
        private readonly IApiServicio apiServicio;
        private IHostingEnvironment _hostingEnvironment;

        public ActivoFijoController(IApiServicio apiServicio, IHostingEnvironment environment)
        {
            this.apiServicio = apiServicio;
            this._hostingEnvironment = environment;
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

                ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "api/Pais/ListarPais"), "IdPais", "Nombre");
                ViewData["Provincia"] = await ObtenerSelectListProvincia((ViewData["Pais"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Pais"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["Ciudad"] = await ObtenerSelectListCiudad((ViewData["Provincia"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Provincia"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["Sucursal"] = await ObtenerSelectListSucursal((ViewData["Ciudad"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Ciudad"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo((ViewData["Sucursal"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Sucursal"] as SelectList).FirstOrDefault().Value) : -1);

                ViewData["Proveedor"] = new SelectList((await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedores")).Select(c => new { c.IdProveedor, NombreApellidos = $"{c.Nombre} {c.Apellidos}" }), "IdProveedor", "NombreApellidos");
                ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");

                ViewData["Marca"] = new SelectList(await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddressRM), "api/Marca/ListarMarca"), "IdMarca", "Nombre");
                ViewData["Modelo"] = await ObtenerSelectListModelo((ViewData["Marca"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Marca"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["UnidadMedida"] = new SelectList(await apiServicio.Listar<UnidadMedida>(new Uri(WebApp.BaseAddressRM), "api/UnidadMedida/ListarUnidadMedida"), "IdUnidadMedida", "Nombre");
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ActivosRecepcionados));
            }
        }

        public async Task<IActionResult> EditarRecepcionAR(string id) => await EditarRecepcion(id, nameof(ActivosRecepcionados));
        public async Task<IActionResult> EditarRecepcionVT(string id) => await EditarRecepcion(id, nameof(ActivoValidacionTecnica));
        public async Task<IActionResult> EditarRecepcion(string id, string nombreVistaError)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nombreVistaError);

                    var recepcionActivoFijoDetalle = JsonConvert.DeserializeObject<RecepcionActivoFijoDetalle>(respuesta.Resultado.ToString());
                    if (recepcionActivoFijoDetalle.Estado.Nombre == "Recepcionado" || recepcionActivoFijoDetalle.Estado.Nombre == "Validación Técnica")
                    {
                        ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                        ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo(recepcionActivoFijoDetalle?.RecepcionActivoFijo?.SubClaseActivoFijo?.ClaseActivoFijo?.TipoActivoFijo?.IdTipoActivoFijo ?? -1);
                        ViewData["SubClaseActivoFijo"] = await ObtenerSelectListSubClaseActivoFijo(recepcionActivoFijoDetalle?.RecepcionActivoFijo?.SubClaseActivoFijo?.ClaseActivoFijo?.IdClaseActivoFijo ?? -1);
                        ViewData["MotivoRecepcion"] = new SelectList(await apiServicio.Listar<MotivoRecepcion>(new Uri(WebApp.BaseAddressRM), "api/MotivoRecepcion/ListarMotivoRecepcion"), "IdMotivoRecepcion", "Descripcion");

                        ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "api/Pais/ListarPais"), "IdPais", "Nombre");
                        ViewData["Provincia"] = await ObtenerSelectListProvincia(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.Pais?.IdPais ?? -1);
                        ViewData["Ciudad"] = await ObtenerSelectListCiudad(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.IdProvincia ?? -1);
                        ViewData["Sucursal"] = await ObtenerSelectListSucursal(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.IdCiudad ?? -1);
                        ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.IdSucursal ?? -1);

                        ViewData["Proveedor"] = new SelectList((await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedores")).Select(c => new { c.IdProveedor, NombreApellidos = $"{c.Nombre} {c.Apellidos}" }), "IdProveedor", "NombreApellidos");
                        ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                        ViewData["Marca"] = new SelectList(await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddressRM), "api/Marca/ListarMarca"), "IdMarca", "Nombre");
                        ViewData["Modelo"] = await ObtenerSelectListModelo(recepcionActivoFijoDetalle?.ActivoFijo?.Modelo?.Marca?.IdMarca ?? -1);
                        ViewData["UnidadMedida"] = new SelectList(await apiServicio.Listar<UnidadMedida>(new Uri(WebApp.BaseAddressRM), "api/UnidadMedida/ListarUnidadMedida"), "IdUnidadMedida", "Nombre");

                        if (!recepcionActivoFijoDetalle.RecepcionActivoFijo.ValidacionTecnica)
                            try { recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.Consecutivo = int.Parse(String.Join("", recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.Codigosecuencial.Except(ObtenerCodigoSecuencial(recepcionActivoFijoDetalle?.RecepcionActivoFijo?.SubClaseActivoFijo?.ClaseActivoFijo?.Nombre, recepcionActivoFijoDetalle?.RecepcionActivoFijo?.SubClaseActivoFijo?.ClaseActivoFijo?.TipoActivoFijo?.Nombre)))); } catch (Exception) { recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.Consecutivo = 1; }
                        return View("EditarRecepcion", recepcionActivoFijoDetalle);
                    }
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nombreVistaError);
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nombreVistaError);
            }
        }

        public async Task<IActionResult> GestionRecepcionActivoFijoDetalle(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle)
        {
            try
            {
                recepcionActivoFijoDetalle.ActivoFijo.SubClaseActivoFijo = JsonConvert.DeserializeObject<SubClaseActivoFijo>((await apiServicio.SeleccionarAsync<Response>(recepcionActivoFijoDetalle.RecepcionActivoFijo.IdSubClaseActivoFijo.ToString(), new Uri(WebApp.BaseAddressRM), "api/SubClaseActivoFijo")).Resultado.ToString());
                recepcionActivoFijoDetalle.ActivoFijo.IdSubClaseActivoFijo = recepcionActivoFijoDetalle.ActivoFijo.SubClaseActivoFijo.IdSubClaseActivoFijo;
                recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo = JsonConvert.DeserializeObject<LibroActivoFijo>((await apiServicio.SeleccionarAsync<Response>(recepcionActivoFijoDetalle.ActivoFijo.IdLibroActivoFijo.ToString(), new Uri(WebApp.BaseAddressRM), "api/LibroActivoFijo")).Resultado.ToString());
                recepcionActivoFijoDetalle.ActivoFijo.Ciudad = recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad;
                recepcionActivoFijoDetalle.ActivoFijo.IdCiudad = recepcionActivoFijoDetalle.ActivoFijo.Ciudad.IdCiudad;
                recepcionActivoFijoDetalle.ActivoFijo.UnidadMedida = JsonConvert.DeserializeObject<UnidadMedida>((await apiServicio.SeleccionarAsync<Response>(recepcionActivoFijoDetalle.ActivoFijo.IdUnidadMedida.ToString(), new Uri(WebApp.BaseAddressRM), "api/UnidadMedida")).Resultado.ToString());
                recepcionActivoFijoDetalle.ActivoFijo.Modelo = JsonConvert.DeserializeObject<Modelo>((await apiServicio.SeleccionarAsync<Response>(recepcionActivoFijoDetalle.ActivoFijo.IdModelo.ToString(), new Uri(WebApp.BaseAddressRM), "api/Modelo")).Resultado.ToString());

                recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo = recepcionActivoFijoDetalle.ActivoFijo.SubClaseActivoFijo;
                recepcionActivoFijoDetalle.RecepcionActivoFijo.LibroActivoFijo = recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo;
                recepcionActivoFijoDetalle.RecepcionActivoFijo.Empleado = JsonConvert.DeserializeObject<Empleado>((await apiServicio.SeleccionarAsync<Response>(recepcionActivoFijoDetalle.RecepcionActivoFijo.IdEmpleado.ToString(), new Uri(WebApp.BaseAddressTH), "api/Empleados")).Resultado.ToString());
                recepcionActivoFijoDetalle.RecepcionActivoFijo.MotivoRecepcion = JsonConvert.DeserializeObject<MotivoRecepcion>((await apiServicio.SeleccionarAsync<Response>(recepcionActivoFijoDetalle.RecepcionActivoFijo.IdMotivoRecepcion.ToString(), new Uri(WebApp.BaseAddressRM), "api/MotivoRecepcion")).Resultado.ToString());
                recepcionActivoFijoDetalle.RecepcionActivoFijo.Proveedor = JsonConvert.DeserializeObject<Proveedor>((await apiServicio.SeleccionarAsync<Response>(recepcionActivoFijoDetalle.RecepcionActivoFijo.IdProveedor.ToString(), new Uri(WebApp.BaseAddressRM), "api/Proveedor")).Resultado.ToString());
                recepcionActivoFijoDetalle.RecepcionActivoFijo.IdLibroActivoFijo = recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.IdLibroActivoFijo;

                await apiServicio.InsertarAsync(new Estado { Nombre = "Recepcionado" }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                await apiServicio.InsertarAsync(new Estado { Nombre = "Validación Técnica" }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                var listaEstado = await apiServicio.Listar<Estado>(new Uri(WebApp.BaseAddressTH), "api/Estados/ListarEstados");
                recepcionActivoFijoDetalle.Estado = listaEstado.SingleOrDefault(c => c.Nombre == (!recepcionActivoFijoDetalle.RecepcionActivoFijo.ValidacionTecnica ? "Recepcionado" : "Validación Técnica"));
                recepcionActivoFijoDetalle.IdEstado = recepcionActivoFijoDetalle.Estado.IdEstado;

                if (!recepcionActivoFijoDetalle.RecepcionActivoFijo.ValidacionTecnica)
                    recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.Codigosecuencial = ObtenerCodigoSecuencial(recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo.ClaseActivoFijo.Nombre, recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo.ClaseActivoFijo.TipoActivoFijo.Nombre, recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.Consecutivo);
                else
                    recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo = null;

                var response = new Response();
                int idActivoFijo = 0;
                if (recepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle == 0)
                {
                    response = await apiServicio.InsertarAsync(recepcionActivoFijoDetalle, new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/InsertarRecepcionActivoFijo");
                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha recepcionado un activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Activo Fijo:", recepcionActivoFijoDetalle.ActivoFijo.IdActivoFijo) });
                        idActivoFijo = JsonConvert.DeserializeObject<RecepcionActivoFijoDetalle>(response.Resultado.ToString()).IdActivoFijo;
                    }
                }
                else
                {
                    idActivoFijo = recepcionActivoFijoDetalle.IdActivoFijo;
                    response = await apiServicio.EditarAsync<RecepcionActivoFijoDetalle>(recepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle.ToString(), recepcionActivoFijoDetalle, new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo");
                    if (response.IsSuccess)
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha editado una recepción de activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Activo Fijo:", recepcionActivoFijoDetalle.ActivoFijo.IdActivoFijo) });
                }

                if (response.IsSuccess)
                {
                    if (Request.Form.Files.Count > 0)
                    {
                        foreach (var item in Request.Form.Files)
                        {
                            byte[] data;
                            using (var br = new BinaryReader(item.OpenReadStream()))
                                data = br.ReadBytes((int)item.OpenReadStream().Length);

                            var activoFijoDocumentoTransfer = new ActivoFijoDocumentoTransfer { Nombre = item.FileName, Fichero = data, IdActivoFijo = idActivoFijo };
                            response = await apiServicio.InsertarAsync(activoFijoDocumentoTransfer, new Uri(WebApp.BaseAddressRM), "api/ActivoFijoDocumento/UploadFiles");
                            if (response.IsSuccess)
                                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha subido un archivo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Documento de Activo Fijo:", activoFijoDocumentoTransfer.Nombre) });
                        }
                    }
                }

                if (response.IsSuccess)
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", !recepcionActivoFijoDetalle.RecepcionActivoFijo.ValidacionTecnica ? nameof(ActivosFijosRecepcionados) : nameof(ActivoValidacionTecnica));

                ViewData["Error"] = response.Message;
                ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo(recepcionActivoFijoDetalle?.RecepcionActivoFijo?.SubClaseActivoFijo?.ClaseActivoFijo?.IdTipoActivoFijo ?? -1);
                ViewData["SubClaseActivoFijo"] = await ObtenerSelectListSubClaseActivoFijo(recepcionActivoFijoDetalle?.RecepcionActivoFijo?.SubClaseActivoFijo?.IdClaseActivoFijo ?? -1);
                ViewData["MotivoRecepcion"] = new SelectList(await apiServicio.Listar<MotivoRecepcion>(new Uri(WebApp.BaseAddressRM), "api/MotivoRecepcion/ListarMotivoRecepcion"), "IdMotivoRecepcion", "Descripcion");

                ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "api/Pais/ListarPais"), "IdPais", "Nombre");
                ViewData["Provincia"] = await ObtenerSelectListProvincia(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.IdPais ?? -1);
                ViewData["Ciudad"] = await ObtenerSelectListCiudad(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.IdProvincia ?? -1);
                ViewData["Sucursal"] = await ObtenerSelectListSucursal(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.IdCiudad ?? -1);
                ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.IdSucursal ?? -1);

                ViewData["Proveedor"] = new SelectList((await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedores")).Select(c => new { c.IdProveedor, NombreApellidos = $"{c.Nombre} {c.Apellidos}" }), "IdProveedor", "NombreApellidos");
                ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                ViewData["Marca"] = new SelectList(await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddressRM), "api/Marca/ListarMarca"), "IdMarca", "Nombre");
                ViewData["Modelo"] = await ObtenerSelectListModelo(recepcionActivoFijoDetalle?.ActivoFijo?.Modelo?.IdMarca ?? -1);
                ViewData["UnidadMedida"] = new SelectList(await apiServicio.Listar<UnidadMedida>(new Uri(WebApp.BaseAddressRM), "api/UnidadMedida/ListarUnidadMedida"), "IdUnidadMedida", "Nombre");
                return View(recepcionActivoFijoDetalle);
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
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Sistema", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webappth" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}", activoFijoRecepcionado ? nameof(ActivosFijosRecepcionados) : nameof(ActivoValidacionTecnica));
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}", activoFijoRecepcionado ? nameof(ActivosFijosRecepcionados) : nameof(ActivoValidacionTecnica));
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar Marca", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}", activoFijoRecepcionado ? nameof(ActivosFijosRecepcionados) : nameof(ActivoValidacionTecnica));
            }
        }

        private string ObtenerCodigoSecuencial(string claseActivoFijo, string tipoActivoFijo, int? numeroConsecutivo = null)
        {
            try
            {
                string codigoSecuencial = $"{claseActivoFijo}{tipoActivoFijo}";
                return numeroConsecutivo != null ? $"{codigoSecuencial}{numeroConsecutivo}" : codigoSecuencial;
            }
            catch (Exception)
            {
                return null;
            }
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
                string codigoSecuencial = ObtenerCodigoSecuencial(activoFijo.CodigoActivoFijo.CAF, activoFijo.CodigoActivoFijo.TAF, activoFijo.CodigoActivoFijo.Consecutivo);
                return Json(!(activoFijo.CodigoActivoFijo.IdCodigoActivoFijo == 0 ? listaCodigoActivoFijo.Any(c => c.Codigosecuencial == codigoSecuencial.Trim()) : listaCodigoActivoFijo.Where(c => c.Codigosecuencial == codigoSecuencial.Trim()).Any(c => c.IdCodigoActivoFijo != activoFijo.CodigoActivoFijo.IdCodigoActivoFijo)));
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recepcion(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle) => await GestionRecepcionActivoFijoDetalle(recepcionActivoFijoDetalle);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRecepcion(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle) => await GestionRecepcionActivoFijoDetalle(recepcionActivoFijoDetalle);

        public async Task<IActionResult> ObtenerRecepcionActivoFijo(string id, List<string> estados, string nombreVistaError)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo");
                    respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionActivoFijoDetalle>(respuesta.Resultado.ToString());
                    if (respuesta.IsSuccess)
                    {
                        if (estados.Count > 0)
                        {
                            if (estados.Contains((respuesta.Resultado as RecepcionActivoFijoDetalle).Estado.Nombre))
                                return View(respuesta.Resultado);
                        }
                        else
                            return View(respuesta.Resultado);
                    }
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nombreVistaError);
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nombreVistaError);
            }
        }

        public async Task<IActionResult> ActivosRecepcionados()
        {
            try
            {
                var lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/ListarRecepcionActivoFijoPorEstado/Recepcionado");
                ViewData["titulo"] = "Activos Fijos Recepcionados";
                ViewData["textoColumna"] = "Editar";
                ViewData["url"] = "EditarRecepcionAR";
                ViewData["eliminarRecepcionActivoFijo"] = true;
                ViewData["urlCodificacion"] = nameof(CodificacionActivoFijo);
                return View("ListadoActivoFijo", lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos recepcionados", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }

        private string ObtenerDireccionCarpetaTemporal()
        {
            string filePath = "";
            Guid guid;
            do
            {
                guid = Guid.NewGuid();
                filePath = $"{_hostingEnvironment.ContentRootPath}\\wwwroot\\images\\ActivoFijo\\{guid}";
            } while (Directory.Exists(filePath));
            return guid.ToString();
        }

        [HttpPost]
        public IActionResult SubirArchivos()
        {
            if (!Directory.Exists($"{_hostingEnvironment.ContentRootPath}\\wwwroot\\images\\ActivoFijo"))
                Directory.CreateDirectory($"{_hostingEnvironment.ContentRootPath}\\wwwroot\\images\\ActivoFijo");

            var dir = Request.Form["dir"].ToString();
            var nombreCarpeta = dir != null && dir != "" ? dir : ObtenerDireccionCarpetaTemporal();
            var folderPath = $"{_hostingEnvironment.ContentRootPath}\\wwwroot\\images\\ActivoFijo\\{nombreCarpeta}";

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            foreach (var formFile in Request.Form.Files)
            {
                try
                {
                    if (formFile.Length > 0)
                    {
                        using (Stream stream = new FileStream($"{folderPath}\\{formFile.FileName}", FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        {
                            formFile.CopyTo(stream);
                        }
                    }
                }
                catch (Exception)
                {
                    return StatusCode(500);
                }
            }
            return StatusCode(200, new JsonResult(nombreCarpeta));
        }

        [HttpPost]
        public IActionResult EliminarArchivo(string fileName, string dir)
        {
            try
            {
                System.IO.File.Delete($"{_hostingEnvironment.ContentRootPath}\\wwwroot\\images\\ActivoFijo\\{dir}\\{fileName}");
                return StatusCode(200);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
        #endregion

        #region Codificación de Activos
        public async Task<IActionResult> ActivoValidacionTecnica()
        {
            try
            {
                var lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/ListarRecepcionActivoFijoPorEstado/Validación Técnica");
                ViewData["titulo"] = "Activos Fijos que requieren Validación Técnica";
                ViewData["textoColumna"] = "Revisar";
                ViewData["url"] = "RevisionActivoFijo";
                ViewData["urlEditar"] = "EditarRecepcionVT";
                ViewData["eliminarRecepcionActivoFijo"] = true;
                return View("ListadoActivoFijo", lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos que requieren validación técnica", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> DesaprobarActivoFijo(string id)
        {
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = "Desaprobado" }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                await apiServicio.InsertarAsync(int.Parse(id), new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/DesaprobarActivoFijo");
                return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ActivoValidacionTecnica));
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando desaprobación Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ActivoValidacionTecnica));
            }
        }

        public async Task<IActionResult> CodificacionActivoFijo(string id) => await ObtenerRecepcionActivoFijo(id, new List<string> { "Validación Técnica", "Recepcionado" }, nameof(ActivoValidacionTecnica));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CodificacionActivoFijo(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle)
        {
            try
            {
                if (!string.IsNullOrEmpty(recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.IdCodigoActivoFijo.ToString()))
                {
                    recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.Codigosecuencial = ObtenerCodigoSecuencial(recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.CAF, recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.TAF, recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.Consecutivo);
                    var response = await apiServicio.EditarAsync(recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.IdCodigoActivoFijo.ToString(), recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo, new Uri(WebApp.BaseAddressRM), "api/CodigoActivoFijo");
                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Código de Activo Fijo", recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.IdCodigoActivoFijo.ToString()), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha actualizado un registro clase de código de activo fijo", UserName = "Usuario 1" });
                        recepcionActivoFijoDetalle.Estado = new Estado { Nombre = "Recepcionado" };
                        response = await apiServicio.EditarAsync(recepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle.ToString(), recepcionActivoFijoDetalle, new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/EstadoActivoFijo");
                        if (response.IsSuccess)
                        {
                            await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Estado de Activo Fijo", recepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle.ToString()), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha actualizado un registro estado de activo fijo", UserName = "Usuario 1" });
                            return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ActivosFijosRecepcionados));
                        }
                    }
                    ViewData["Error"] = response.Message;
                    return View(recepcionActivoFijoDetalle);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ActivoValidacionTecnica));
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Generando y asignando Código Único de Activo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorEditar}", nameof(ActivoValidacionTecnica));
            }
        }

        public async Task<IActionResult> RevisionActivoFijo(string id) => await ObtenerRecepcionActivoFijo(id, new List<string> { "Validación Técnica" }, nameof(ActivoValidacionTecnica));
        #endregion

        #region Póliza de Seguro
        public async Task<IActionResult> ActivosFijosRecepcionadosSinPoliza()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/ListarRecepcionActivoFijoSinPoliza");
                ViewData["titulo"] = "Activos Fijos sin Póliza de Seguro";
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
                ViewData["titulo"] = "Activos Fijos con Póliza de Seguro";
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

        public async Task<IActionResult> AsignarPoliza(string id) => await ObtenerRecepcionActivoFijo(id, new List<string> { "Recepcionado" }, nameof(ActivosFijosRecepcionadosSinPoliza));

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
        public async Task<IActionResult> AsignarNumeroFactura(string id) => await ObtenerRecepcionActivoFijo(id, new List<string> { "Recepcionado" }, nameof(ActivosFijosRecepcionados));

        public async Task<IActionResult> ActivosFijosRecepcionados()
        {
            try
            {
                var lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/ListarRecepcionActivoFijoPorEstado/Recepcionado");
                ViewData["titulo"] = "Activos Fijos Recepcionados";
                ViewData["textoColumna"] = "Dar Alta";
                ViewData["url"] = "AsignarNumeroFactura";
                ViewData["RequerirAlta"] = true;
                return View("ListadoActivoFijo", lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con estado Recepcionado", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> ActivosFijosAltas()
        {
            try
            {
                var lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/ListarRecepcionActivoFijoPorEstado/Alta");
                ViewData["titulo"] = "Activos Fijos con Estado Alta";
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
                        AltaActivoFijoDetalle AFA = new AltaActivoFijoDetalle { IdActivoFijo = RecepcionActivoFijoDetalle.IdActivoFijo, FechaAlta = DateTime.Now };
                        response = await apiServicio.InsertarAsync(AFA, new Uri(WebApp.BaseAddressRM), "api/ActivosFijosAlta/InsertarActivosFijosAlta");
                        if (response.IsSuccess)
                        {
                            await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha insertado un Activo Fijo con el estado ALTA", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Activo Fijo con estado Alta:", AFA.IdFactura) });
                            ViewData["mensaje"] = "Activo Fijo Alta insertado correctamente";

                            try
                            {
                                RecepcionActivoFijoDetalle.Estado = new Estado { Nombre = "Alta" };
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
            AltaActivoFijoDetalle AFA = new AltaActivoFijoDetalle { IdActivoFijo = recepcionAFD.IdActivoFijo, IdFactura = fact, FechaAlta = DateTime.Now };
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
                        RecepcionActivoFijoDetalle.Estado = new Estado { Nombre = "Alta" };

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
                        var lista = (await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/ListarRecepcionActivoFijo")).Where(c => c.Estado.Nombre == "Recepcionado" && c.RecepcionActivoFijo.IdMotivoRecepcion == 2).ToList();
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
                ViewData["listaAFA"] = await apiServicio.Listar<AltaActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijosAlta/ListarAltasActivosFijos");
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

                    ViewData["PaisOrigen"] = new SelectList(listaPais, "IdPais", "Nombre", recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.IdPais ?? -1);
                    ViewData["ProvinciaOrigen"] = await ObtenerSelectListProvincia(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.IdPais ?? -1);
                    ViewData["CiudadOrigen"] = await ObtenerSelectListCiudad(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.IdProvincia ?? -1);
                    ViewData["SucursalOrigen"] = await ObtenerSelectListSucursal(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.IdCiudad ?? -1);
                    ViewData["LibroActivoFijoOrigen"] = await ObtenerSelectListLibroActivoFijo(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.IdSucursal ?? -1);

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
        public async Task<IActionResult> TransferirActivoFijo(int IdActivoFijo, TransferenciaActivoFijoDetalle transferenciaActivoFijoDetalle)
        {
            try
            {
                var activoFijoOrigen = JsonConvert.DeserializeObject<ActivoFijo>((await apiServicio.SeleccionarAsync<Response>(IdActivoFijo.ToString(), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos")).Resultado.ToString());
                transferenciaActivoFijoDetalle.TransferenciaActivoFijo.Origen = activoFijoOrigen?.LibroActivoFijo?.Sucursal?.Nombre ?? activoFijoOrigen?.Ciudad?.Nombre;

                var libroActivoFijoDestino = JsonConvert.DeserializeObject<LibroActivoFijo>((await apiServicio.SeleccionarAsync<Response>(transferenciaActivoFijoDetalle.ActivoFijo.IdLibroActivoFijo.ToString(), new Uri(WebApp.BaseAddressRM), "api/LibroActivoFijo")).Resultado.ToString());
                transferenciaActivoFijoDetalle.TransferenciaActivoFijo.Destino = libroActivoFijoDestino.Sucursal.Nombre;
                
                int idPaisDestino = transferenciaActivoFijoDetalle.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.IdPais ?? -1;
                int idProvinciaDestino = transferenciaActivoFijoDetalle.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.IdProvincia ?? -1;
                int idCiudadDestino = transferenciaActivoFijoDetalle.ActivoFijo?.LibroActivoFijo?.Sucursal?.IdCiudad ?? -1;
                int idSucursalDestino = transferenciaActivoFijoDetalle.ActivoFijo?.LibroActivoFijo?.IdSucursal ?? -1;

                transferenciaActivoFijoDetalle.ActivoFijo.IdActivoFijo = IdActivoFijo;
                transferenciaActivoFijoDetalle.ActivoFijo.LibroActivoFijo = libroActivoFijoDestino;
                var response = await apiServicio.InsertarAsync(transferenciaActivoFijoDetalle, new Uri(WebApp.BaseAddressRM), "api/TransferenciaActivoFijoDetalle/InsertarTransferenciaActivoFijoDetalle");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado una transferencia de Activo Fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Detalle de Transferencia:", transferenciaActivoFijoDetalle.IdTransferenciaActivoFijoDetalle) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ActivosFijosATransferir));
                }
                ViewData["Error"] = response.Message;
                ViewData["MotivoTransferencia"] = new SelectList(await apiServicio.Listar<MotivoTransferencia>(new Uri(WebApp.BaseAddressRM), "api/MotivoTransferencia/ListarMotivoTransferencia"), "IdMotivoTransferencia", "Motivo_Transferencia");

                var listaPais = await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "api/Pais/ListarPais");
                ViewData["Pais"] = new SelectList(listaPais, "IdPais", "Nombre", idPaisDestino);
                ViewData["Provincia"] = await ObtenerSelectListProvincia(idPaisDestino);
                ViewData["Ciudad"] = await ObtenerSelectListCiudad(idProvinciaDestino);
                ViewData["Sucursal"] = await ObtenerSelectListSucursal(idCiudadDestino);
                ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo(idSucursalDestino);

                ViewData["PaisOrigen"] = new SelectList(listaPais, "IdPais", "Nombre", activoFijoOrigen?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.IdPais ?? -1);
                ViewData["ProvinciaOrigen"] = await ObtenerSelectListProvincia(activoFijoOrigen?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.IdPais ?? -1);
                ViewData["CiudadOrigen"] = await ObtenerSelectListCiudad(activoFijoOrigen?.LibroActivoFijo?.Sucursal?.Ciudad?.IdProvincia ?? -1);
                ViewData["SucursalOrigen"] = await ObtenerSelectListSucursal(activoFijoOrigen?.LibroActivoFijo?.Sucursal?.IdCiudad ?? -1);
                ViewData["LibroActivoFijoOrigen"] = await ObtenerSelectListLibroActivoFijo(activoFijoOrigen?.LibroActivoFijo?.IdSucursal ?? -1);

                ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                ViewData["IdActivoFijo"] = IdActivoFijo;
                return View(transferenciaActivoFijoDetalle);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando un Modelo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ActivosFijosATransferir));
            }
        }
        #endregion

        #region Depreciación
        #endregion

        #region Baja de Activos
        public async Task<IActionResult> ActivoFijoBaja(string id)
        {
            ViewData["MotivoActivoFijoBaja"] = new SelectList(await apiServicio.Listar<MotivoBaja>(new Uri(WebApp.BaseAddressRM), "api/ActivoFijoMotivoBaja/ListarActivoFijoMotivoBaja"), "IdActivoFijoMotivoBaja", "Nombre");
            return await ObtenerRecepcionActivoFijo(id, new List<string> { "Alta" }, nameof(ActivosFijosBajas));
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

                var response = await apiServicio.InsertarAsync(new Estado { Nombre = "Baja" }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                recepcionActivoFijoDetalle.Estado = new Estado { Nombre = "Baja" };
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
                var lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/ListarRecepcionActivoFijoPorEstado/Baja");
                ViewData["titulo"] = "Activos Fijos con Estado Baja";
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
                var lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/ListarRecepcionActivoFijoPorEstado/Alta");
                ViewData["titulo"] = "Activos Fijos de Alta";
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
        public async Task<IActionResult> ListarMantenimientos()
        {
            var lista = new List<MantenimientoActivoFijo>();
            try
            {
                lista = await apiServicio.Listar<MantenimientoActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/MantenimientoActivoFijo/ListarMantenimientosActivoFijo");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando Mantenimientos de Activos Fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> CrearMantenimiento()
        {
            try
            {
                ViewData["ActivoFijo"] = new SelectList(await apiServicio.Listar<ActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivosFijos"), "IdActivoFijo", "Nombre");
                ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListarMantenimientos));
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
                        return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ListarMantenimientos));
                    }
                }
                else
                    response.Message = Mensaje.ModeloInvalido;

                ViewData["Error"] = response.Message;
                ViewData["ActivoFijo"] = new SelectList(await apiServicio.Listar<ActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivosFijos"), "IdActivoFijo", "Nombre");
                ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                return View(response.Resultado);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando Mantenimiento Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListarMantenimientos));
            }
        }

        public async Task<IActionResult> EditarMantenimiento(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM), "api/MantenimientoActivoFijo");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListarMantenimientos));

                    respuesta.Resultado = JsonConvert.DeserializeObject<MantenimientoActivoFijo>(respuesta.Resultado.ToString());
                    ViewData["ActivoFijo"] = new SelectList(await apiServicio.Listar<ActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivosFijos"), "IdActivoFijo", "Nombre");
                    ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                    return View(respuesta.Resultado);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListarMantenimientos));
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListarMantenimientos));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarMantenimiento(string id, MantenimientoActivoFijo mantenimientoActivoFijo)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var response = new Response();
                    if (ModelState.IsValid)
                    {
                        response = await apiServicio.EditarAsync(id, mantenimientoActivoFijo, new Uri(WebApp.BaseAddressRM), "api/MantenimientoActivoFijo");
                        if (response.IsSuccess)
                        {
                            await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Motivo de Asiento", id), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha actualizado un registro Mantenimiento Activo Fijo", UserName = "Usuario 1" });
                            return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ListarMantenimientos));
                        }
                    }
                    else
                        response.Message = Mensaje.ModeloInvalido;

                    ViewData["Error"] = response.Message;
                    ViewData["ActivoFijo"] = new SelectList(await apiServicio.Listar<ActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivosFijos"), "IdActivoFijo", "Nombre");
                    ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                    return View(mantenimientoActivoFijo);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListarMantenimientos));
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Editando un Mantenimiento Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorEditar}", nameof(ListarMantenimientos));
            }
        }

        public async Task<IActionResult> EliminarMantenimiento(string id)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddressRM), "api/MantenimientoActivoFijo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Mantenimiento Activo Fijo", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webappth" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}", nameof(ListarMantenimientos));
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}", nameof(ListarMantenimientos));
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar Mantenimiento Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}", nameof(ListarMantenimientos));
            }
        }
        #endregion

        #region Reportes
        public async Task<IActionResult> HojaVidaActivoFijo(string id) => await ObtenerRecepcionActivoFijo(id, null, nameof(HojaVidaReporte));

        public async Task<IActionResult> HojaVidaReporte()
        {
            try
            {
                var lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/ListarRecepcionActivoFijoPorEstado/Alta");
                ViewData["titulo"] = "Activos Fijos";
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
            return PartialView("_ProvinciaSelect", new RecepcionActivoFijoDetalle());
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
            return PartialView("_CiudadSelect", new RecepcionActivoFijoDetalle());
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
            return PartialView("_SucursalSelect", new RecepcionActivoFijoDetalle());
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
            return PartialView("_LibroActivoFijoSelect", new RecepcionActivoFijoDetalle());
        }
        #endregion
    }
}