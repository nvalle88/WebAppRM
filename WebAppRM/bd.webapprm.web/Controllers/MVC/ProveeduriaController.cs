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

namespace bd.webapprm.web.Controllers.MVC
{
    public class ProveeduriaController : Controller
    {
        private readonly IApiServicio apiServicio;
        public static List<Factura> ListadoFacturasSeleccionadas = new List<Factura>();
        public static List<Factura> ListadoFacturas = new List<Factura>();

        public ProveeduriaController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        #region Recepción de Proveeduría
        public IActionResult Index()
        {
            return RedirectToAction("ListadoRecepcion");
        }

        public async Task<IActionResult> ListadoRecepcion()
        {
            var lista = new List<RecepcionArticulos>();
            try
            {
                lista = await apiServicio.Listar<RecepcionArticulos>(new Uri(WebApp.BaseAddressRM), "api/RecepcionArticulo/ListarRecepcionArticulos");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando artículos recepcionados", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> Recepcion()
        {
            try
            {
                ViewData["TipoArticulo"] = new SelectList(await apiServicio.Listar<TipoArticulo>(new Uri(WebApp.BaseAddressRM), "api/TipoArticulo/ListarTipoArticulo"), "IdTipoArticulo", "Nombre");
                ViewData["ClaseArticulo"] = await ObtenerSelectListClaseArticulo((ViewData["TipoArticulo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["TipoArticulo"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["SubClaseArticulo"] = await ObtenerSelectListSubClaseArticulo((ViewData["ClaseArticulo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["ClaseArticulo"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["Articulo"] = await ObtenerSelectListArticulo((ViewData["SubClaseArticulo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["SubClaseArticulo"] as SelectList).FirstOrDefault().Value) : -1);

                ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "api/Pais/ListarPais"), "IdPais", "Nombre");
                ViewData["Provincia"] = await ObtenerSelectListProvincia((ViewData["Pais"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Pais"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["Ciudad"] = await ObtenerSelectListCiudad((ViewData["Provincia"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Provincia"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["Sucursal"] = await ObtenerSelectListSucursal((ViewData["Ciudad"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Ciudad"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["MaestroArticuloSucursal"] = await ObtenerSelectListMaestroArticuloSucursal((ViewData["Sucursal"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Sucursal"] as SelectList).FirstOrDefault().Value) : -1);

                ViewData["Proveedor"] = new SelectList((await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedores")).Select(c => new { c.IdProveedor, NombreApellidos = String.Format("{0} {1}", c.Nombre, c.Apellidos) }), "IdProveedor", "NombreApellidos");
                ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recepcion(RecepcionArticulos recepcionArticulo) => await GestionarRecepcionArticulos(recepcionArticulo);

        public async Task<IActionResult> EditarRecepcion(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM), "api/RecepcionArticulo");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");
                    
                    var recepcionArticulos = JsonConvert.DeserializeObject<RecepcionArticulos>(respuesta.Resultado.ToString());
                    if (respuesta.IsSuccess)
                    {
                        ViewData["TipoArticulo"] = new SelectList(await apiServicio.Listar<TipoArticulo>(new Uri(WebApp.BaseAddressRM), "api/TipoArticulo/ListarTipoArticulo"), "IdTipoArticulo", "Nombre");
                        ViewData["ClaseArticulo"] = await ObtenerSelectListClaseArticulo(recepcionArticulos?.Articulo?.SubClaseArticulo?.ClaseArticulo?.TipoArticulo?.IdTipoArticulo ?? -1);
                        ViewData["SubClaseArticulo"] = await ObtenerSelectListSubClaseArticulo(recepcionArticulos?.Articulo?.SubClaseArticulo?.ClaseArticulo?.IdClaseArticulo ?? -1);
                        ViewData["Articulo"] = await ObtenerSelectListArticulo(recepcionArticulos?.Articulo?.SubClaseArticulo?.IdSubClaseArticulo ?? -1);

                        ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "api/Pais/ListarPais"), "IdPais", "Nombre");
                        ViewData["Provincia"] = await ObtenerSelectListProvincia(recepcionArticulos?.MaestroArticuloSucursal?.Sucursal?.Ciudad?.Provincia?.Pais?.IdPais ?? -1);
                        ViewData["Ciudad"] = await ObtenerSelectListCiudad(recepcionArticulos?.MaestroArticuloSucursal?.Sucursal?.Ciudad?.Provincia?.IdProvincia ?? -1);
                        ViewData["Sucursal"] = await ObtenerSelectListSucursal(recepcionArticulos?.MaestroArticuloSucursal?.Sucursal?.Ciudad?.IdCiudad ?? -1);
                        ViewData["MaestroArticuloSucursal"] = await ObtenerSelectListMaestroArticuloSucursal(recepcionArticulos?.MaestroArticuloSucursal?.Sucursal?.IdSucursal ?? -1);
                        
                        ViewData["Proveedor"] = new SelectList((await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedores")).Select(c => new { c.IdProveedor, NombreApellidos = String.Format("{0} {1}", c.Nombre, c.Apellidos) }), "IdProveedor", "NombreApellidos");
                        ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                        return View(recepcionArticulos);
                    }
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRecepcion(RecepcionArticulos recepcionArticulo) => await GestionarRecepcionArticulos(recepcionArticulo);

        private async Task<IActionResult> GestionarRecepcionArticulos(RecepcionArticulos recepcionArticulo)
        {
            try
            {
                recepcionArticulo.MaestroArticuloSucursal = JsonConvert.DeserializeObject<MaestroArticuloSucursal>((await apiServicio.SeleccionarAsync<Response>(recepcionArticulo.IdMaestroArticuloSucursal.ToString(), new Uri(WebApp.BaseAddressRM), "api/MaestroArticuloSucursal")).Resultado.ToString());
                recepcionArticulo.IdMaestroArticuloSucursal = recepcionArticulo.MaestroArticuloSucursal.IdMaestroArticuloSucursal;
                recepcionArticulo.Proveedor = JsonConvert.DeserializeObject<Proveedor>((await apiServicio.SeleccionarAsync<Response>(recepcionArticulo.IdProveedor.ToString(), new Uri(WebApp.BaseAddressRM), "api/Proveedor")).Resultado.ToString());
                recepcionArticulo.IdProveedor = recepcionArticulo.Proveedor.IdProveedor;
                recepcionArticulo.Articulo = JsonConvert.DeserializeObject<Articulo>((await apiServicio.SeleccionarAsync<Response>(recepcionArticulo.IdArticulo.ToString(), new Uri(WebApp.BaseAddressRM), "api/Articulo")).Resultado.ToString());
                recepcionArticulo.IdArticulo = recepcionArticulo.Articulo.IdArticulo;

                var response = new Response();
                if (recepcionArticulo.IdRecepcionArticulos == 0)
                    response = await apiServicio.InsertarAsync(recepcionArticulo, new Uri(WebApp.BaseAddressRM), "api/RecepcionArticulo/InsertarRecepcionArticulo");
                else
                    response = await apiServicio.EditarAsync<RecepcionArticulos>(recepcionArticulo.IdRecepcionArticulos.ToString(), recepcionArticulo, new Uri(WebApp.BaseAddressRM), "api/RecepcionArticulo");

                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = recepcionArticulo.IdRecepcionArticulos == 0 ? "Se ha recepcionado un artículo" : "Se ha editado la recepción de un artículo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Artículo:", recepcionArticulo.IdArticulo) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", "ListadoRecepcion");
                }

                ViewData["TipoArticulo"] = new SelectList(await apiServicio.Listar<TipoArticulo>(new Uri(WebApp.BaseAddressRM), "api/TipoArticulo/ListarTipoArticulo"), "IdTipoArticulo", "Nombre");
                ViewData["ClaseArticulo"] = await ObtenerSelectListClaseArticulo(recepcionArticulo?.Articulo?.SubClaseArticulo?.ClaseArticulo?.IdTipoArticulo ?? -1);
                ViewData["SubClaseArticulo"] = await ObtenerSelectListSubClaseArticulo(recepcionArticulo?.Articulo?.SubClaseArticulo?.ClaseArticulo?.IdClaseArticulo ?? -1);
                ViewData["Articulo"] = await ObtenerSelectListArticulo(recepcionArticulo?.Articulo?.SubClaseArticulo?.IdSubClaseArticulo ?? -1);

                ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "api/Pais/ListarPais"), "IdPais", "Nombre");
                ViewData["Provincia"] = await ObtenerSelectListProvincia(recepcionArticulo?.MaestroArticuloSucursal?.Sucursal?.Ciudad?.Provincia?.Pais?.IdPais ?? -1);
                ViewData["Ciudad"] = await ObtenerSelectListCiudad(recepcionArticulo?.MaestroArticuloSucursal?.Sucursal?.Ciudad?.Provincia?.IdProvincia ?? -1);
                ViewData["Sucursal"] = await ObtenerSelectListSucursal(recepcionArticulo?.MaestroArticuloSucursal?.Sucursal?.Ciudad?.IdCiudad ?? -1);
                ViewData["MaestroArticuloSucursal"] = await ObtenerSelectListMaestroArticuloSucursal(recepcionArticulo?.MaestroArticuloSucursal?.Sucursal?.IdSucursal ?? -1);
                
                ViewData["Proveedor"] = new SelectList((await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedores")).Select(c => new { c.IdProveedor, NombreApellidos = String.Format("{0} {1}", c.Nombre, c.Apellidos) }), "IdProveedor", "NombreApellidos");
                ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                ViewData["Error"] = response.Message;
                return View(recepcionArticulo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando recepción Artículo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}");
            }
        }
        #endregion

        #region Reportes
        public async Task<IActionResult> ProveeduriaReporteAltas()
        {
            try
            {
                var lista = await apiServicio.Listar<RecepcionArticulos>(new Uri(WebApp.BaseAddressRM), "api/ProveeduriaReportes/ProveeduriaAltasReporte");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando Artículos en Alta", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }
        public async Task<IActionResult> ProveeduriaReporteBajas()
        {
            try
            {
                var lista = await apiServicio.Listar<RecepcionArticulos>(new Uri(WebApp.BaseAddressRM), "api/ProveeduriaReportes/ProveeduriaBajasReporte");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando Artículos en Baja", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }
        public async Task<IActionResult> EstadisticasConsumoAreaReporte()
        {
            try
            {
                var lista = await apiServicio.Listar<SolicitudProveeduriaDetalle>(new Uri(WebApp.BaseAddressRM), "api/ProveeduriaReportes/EstadisticasConsumoAreaReporte");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando Artículos Recepcionados", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }
        public async Task<IActionResult> AlertaVencimientoReporte()
        {
            try
            {
                var lista = await apiServicio.Listar<RecepcionArticulos>(new Uri(WebApp.BaseAddressRM), "api/ProveeduriaReportes/AlertaVencimientoReporte");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando Artículos en alerta de vencimiento", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }
        public async Task<IActionResult> ConsolidadoInventarioReporte()
        {
            try
            {
                var lista = await apiServicio.Listar<SolicitudProveeduriaDetalle>(new Uri(WebApp.BaseAddressRM), "api/ProveeduriaReportes/ConsolidadoInventarioReporte");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando Artículos recepcionados en Alta", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }
        public async Task<IActionResult> ConsolidadoSolicitudReporte()
        {
            try
            {
                var lista = await apiServicio.Listar<SolicitudProveeduriaDetalle>(new Uri(WebApp.BaseAddressRM), "api/ProveeduriaReportes/ConsolidadoSolicitudReporte");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando Artículos recepcionados en Alta", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }
        public async Task<IActionResult> MinMaxReporte()
        {
            try
            {
                var lista = await apiServicio.Listar<RecepcionArticulos>(new Uri(WebApp.BaseAddressRM), "api/ProveeduriaReportes/ProveeduriaMinMaxReporte");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando Artículos Mínimos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }
        #endregion

        #region AJAX_ClaseArticulo
        public async Task<SelectList> ObtenerSelectListClaseArticulo(int idTipoArticulo)
        {
            try
            {
                var listaClaseArticulo = idTipoArticulo != -1 ? await apiServicio.Listar<ClaseArticulo>(new Uri(WebApp.BaseAddressRM), $"api/ClaseArticulo/ListarClaseArticuloPorTipoArticulo/{idTipoArticulo}") : new List<ClaseArticulo>();
                return new SelectList(listaClaseArticulo, "IdClaseArticulo", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<ClaseArticulo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClaseArticulo_SelectResult(int idTipoArticulo)
        {
            ViewBag.ClaseArticulo = await ObtenerSelectListClaseArticulo(idTipoArticulo);
            return PartialView("_ClaseArticuloSelect", new RecepcionArticulos());
        }
        #endregion

        #region AJAX_SubClaseArticulo
        public async Task<SelectList> ObtenerSelectListSubClaseArticulo(int idClaseArticulo)
        {
            try
            {
                var listaSubClaseArticulo = idClaseArticulo != -1 ? await apiServicio.Listar<SubClaseArticulo>(new Uri(WebApp.BaseAddressRM), $"api/SubClaseArticulo/ListarSubClaseArticulosPorClase/{idClaseArticulo}") : new List<SubClaseArticulo>();
                return new SelectList(listaSubClaseArticulo, "IdSubClaseArticulo", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<SubClaseArticulo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubClaseArticulo_SelectResult(int idClaseArticulo)
        {
            ViewBag.SubClaseArticulo = await ObtenerSelectListSubClaseArticulo(idClaseArticulo);
            return PartialView("_SubClaseArticuloSelect", new RecepcionArticulos());
        }
        #endregion

        #region AJAX_Articulo
        public async Task<SelectList> ObtenerSelectListArticulo(int idSubClaseArticulo)
        {
            try
            {
                var listaArticulo = idSubClaseArticulo != -1 ? await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddressRM), $"api/Articulo/ListarArticulosPorSubClase/{idSubClaseArticulo}") : new List<Articulo>();
                return new SelectList(listaArticulo, "IdArticulo", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<Articulo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Articulo_SelectResult(int idSubClaseArticulo)
        {
            ViewBag.Articulo = await ObtenerSelectListArticulo(idSubClaseArticulo);
            return PartialView("_ArticuloSelect", new RecepcionArticulos());
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
            return PartialView("_ProvinciaSelect", new RecepcionArticulos());
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
            return PartialView("_CiudadSelect", new RecepcionArticulos());
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
            return PartialView("_SucursalSelect", new RecepcionArticulos());
        }
        #endregion

        #region AJAX_MaestroArticuloSucursal
        public async Task<SelectList> ObtenerSelectListMaestroArticuloSucursal(int idSucursal)
        {
            try
            {
                var listaMaestroArticuloSucursal = idSucursal != -1 ? await apiServicio.Listar<MaestroArticuloSucursal>(new Uri(WebApp.BaseAddressRM), $"api/MaestroArticuloSucursal/ListarMaestroArticuloSucursalPorSucursal/{idSucursal}") : new List<MaestroArticuloSucursal>();
                return new SelectList(listaMaestroArticuloSucursal.Select(c => new { c.IdMaestroArticuloSucursal, Maestro = String.Format("Mínimo: {0} - Máximo: {1}", c.Minimo, c.Maximo) }), "IdMaestroArticuloSucursal", "Maestro");
            }
            catch (Exception)
            {
                return new SelectList(new List<MaestroArticuloSucursal>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> MaestroArticuloSucursal_SelectResult(int idSucursal)
        {
            ViewBag.MaestroArticuloSucursal = await ObtenerSelectListMaestroArticuloSucursal(idSucursal);
            return PartialView("_MaestroArticuloSucursalSelect", new RecepcionArticulos());
        }
        #endregion

        #region Alta de Proveeduría
        public async Task<IActionResult> ArticulosADarAlta()
        {
            try
            {
                var lista = await apiServicio.Listar<RecepcionArticulos>(new Uri(WebApp.BaseAddressRM), "api/RecepcionArticulo/ListarRecepcionArticulos");
                return View("ArticulosAlta", lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando articulos recepcionados", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> FormularioAltaArticulo(int ID)
        {
            try
            {
                var respuesta = await apiServicio.SeleccionarAsync<Response>(ID.ToString(), new Uri(WebApp.BaseAddressRM), "api/RecepcionArticulo");
                respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionArticulos>(respuesta.Resultado.ToString());
                RecepcionArticulos RecepcionArticulos = respuesta.Resultado as RecepcionArticulos;
                ViewBag.Acreditacion = new SelectList(new List<string> { "Facturas", "Documentos" });
                if (respuesta.IsSuccess)
                    return View("FormularioAltaArticulo", RecepcionArticulos);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando un objeto de RecepcionArticulos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return BadRequest();
            }
            return View();
        }

        public async Task<IActionResult> CargarTablaFacturasExcluidas(int ID)
        {
            try
            {
                var respuesta = await apiServicio.SeleccionarAsync<Response>(ID.ToString(), new Uri(WebApp.BaseAddressRM), "api/RecepcionArticulo");
                respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionArticulos>(respuesta.Resultado.ToString());
                RecepcionArticulos RecepcionArticulos = respuesta.Resultado as RecepcionArticulos;
                if (respuesta.IsSuccess)
                {
                    try
                    {
                        List<DetalleFactura> listaDetalleFactura = RecepcionArticulos.Articulo.DetalleFactura.ToList();
                        foreach (var item in listaDetalleFactura)
                        {
                            respuesta = await apiServicio.SeleccionarAsync<Response>(item.IdFactura.ToString(), new Uri(WebApp.BaseAddressRM), "api/Factura");
                            respuesta.Resultado = JsonConvert.DeserializeObject<Factura>(respuesta.Resultado.ToString());
                            Factura factura = respuesta.Resultado as Factura;
                            var respuestaOtra = await apiServicio.SeleccionarAsync<Response>(factura.Numero.ToString(), new Uri(WebApp.BaseAddressRM), "api/FacturaPorAltaProveeduria");
                            if ((respuestaOtra.Resultado == null) && (respuesta.IsSuccess))
                            {
                                bool siEsta = false;
                                foreach (var _item in ListadoFacturas)
                                {
                                    if (_item.IdFactura == factura.IdFactura)
                                    {
                                        siEsta = true;
                                        break;
                                    }
                                }

                                if (siEsta)
                                {

                                }
                                else
                                {
                                    ListadoFacturas.Add(factura);
                                }
                            }
                        }
                        ViewBag.data = ListadoFacturas;
                        return PartialView("_FacturasExcluidas"); //return PartialView("_FacturasExcluidas", ListadoFacturas);
                    }
                    catch (Exception ex)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando facturas", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                        return BadRequest();
                    }
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando un objeto de RecepcionArticulos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> CargarTablaFacturasIncluidas(int ID)
        {
            try
            {
                var respuesta = await apiServicio.SeleccionarAsync<Response>(ID.ToString(), new Uri(WebApp.BaseAddressRM), "api/RecepcionArticulo");
                respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionArticulos>(respuesta.Resultado.ToString());
                RecepcionArticulos RecepcionArticulos = respuesta.Resultado as RecepcionArticulos;
                if (respuesta.IsSuccess)
                {
                    try
                    {
                        List<DetalleFactura> listaDetalleFactura = RecepcionArticulos.Articulo.DetalleFactura.ToList();
                        foreach (var item in listaDetalleFactura)
                        {
                            respuesta = await apiServicio.SeleccionarAsync<Response>(item.IdFactura.ToString(), new Uri(WebApp.BaseAddressRM), "api/Factura");
                            respuesta.Resultado = JsonConvert.DeserializeObject<Factura>(respuesta.Resultado.ToString());
                            Factura factura = respuesta.Resultado as Factura;
                            var respuestaOtra = await apiServicio.SeleccionarAsync<Response>(factura.Numero.ToString(), new Uri(WebApp.BaseAddressRM), "api/FacturasPorAltaProveeduria");
                            if (respuestaOtra.Resultado != null)
                            {
                                bool siEsta = false;
                                foreach (var _item in ListadoFacturasSeleccionadas)
                                {
                                    if (_item.IdFactura == factura.IdFactura)
                                    {
                                        siEsta = true;
                                        break;
                                    }
                                }
                                if (siEsta)
                                {

                                }
                                else
                                {
                                    ListadoFacturasSeleccionadas.Add(factura);
                                }
                            }
                        }
                        ViewBag.data = ListadoFacturasSeleccionadas;
                        return PartialView("_FacturasIncluidas"); //return PartialView("_FacturasExcluidas", ListadoFacturas);
                    }
                    catch (Exception ex)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando facturas", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                        return BadRequest();
                    }
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando un objeto de RecepcionArticulos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> IncluirFacturasEnAlta(int idFactura)
        {
            try
            {
                var respuesta = await apiServicio.SeleccionarAsync<Response>(idFactura.ToString(), new Uri(WebApp.BaseAddressRM), "api/Factura");
                if (respuesta.IsSuccess)
                {
                    respuesta.Resultado = JsonConvert.DeserializeObject<Factura>(respuesta.Resultado.ToString());
                    Factura factura = respuesta.Resultado as Factura;
                    ListadoFacturasSeleccionadas.Add(factura);
                    ViewBag.data = ListadoFacturasSeleccionadas;
                    return PartialView("_FacturasIncluidas");
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Incluyendo una factura a un objeto de Alta", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> RefrescarTablaExcluidos(int idFactura)
        {
            var respuesta = await apiServicio.SeleccionarAsync<Response>(idFactura.ToString(), new Uri(WebApp.BaseAddressRM), "api/Factura");
            if (respuesta.IsSuccess)
            {
                respuesta.Resultado = JsonConvert.DeserializeObject<Factura>(respuesta.Resultado.ToString());
                Factura factura = respuesta.Resultado as Factura;
                List<Factura> temporal = new List<Factura>();
                foreach (var item in ListadoFacturas)
                {
                    if (item.IdFactura != factura.IdFactura)
                        temporal.Add(item);
                }
                ListadoFacturas = temporal;
                ViewBag.data = ListadoFacturas;
                return PartialView("_FacturasExcluidas");
            }
            return BadRequest();
        }

        public async Task<IActionResult> ExcluirFacturasEnAlta(int idFactura)
        {
            try
            {
                var respuesta = await apiServicio.SeleccionarAsync<Response>(idFactura.ToString(), new Uri(WebApp.BaseAddressRM), "api/Factura");
                if (respuesta.IsSuccess)
                {
                    respuesta.Resultado = JsonConvert.DeserializeObject<Factura>(respuesta.Resultado.ToString());
                    Factura factura = respuesta.Resultado as Factura;
                    ListadoFacturas.Add(factura);
                    ViewBag.data = ListadoFacturas;
                    return PartialView("_FacturasExcluidas");
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Excluyendo una factura a un objeto de Alta", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> RefrescarTablaIncluidos(int idFactura)
        {
            var respuesta = await apiServicio.SeleccionarAsync<Response>(idFactura.ToString(), new Uri(WebApp.BaseAddressRM), "api/Factura");
            if (respuesta.IsSuccess)
            {
                respuesta.Resultado = JsonConvert.DeserializeObject<Factura>(respuesta.Resultado.ToString());
                Factura factura = respuesta.Resultado as Factura;
                List<Factura> temporal = new List<Factura>();
                foreach (var item in ListadoFacturasSeleccionadas)
                {
                    if (item.IdFactura != factura.IdFactura)
                        temporal.Add(item);
                }
                ListadoFacturasSeleccionadas = temporal;
                ViewBag.data = ListadoFacturasSeleccionadas;
                return PartialView("_FacturasIncluidas");
            }
            return BadRequest();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> AprobarAltaArticulo(RecepcionArticulos recepcionArticulos)
        {
            try
            {
                int idRecepcionArticulo = recepcionArticulos.IdRecepcionArticulos;
                int idArticulo = recepcionArticulos.IdArticulo;
                int idProveedor = recepcionArticulos.IdProveedor;

                var fechaAlta = DateTime.Now;
                AltaProveeduria alta = new AltaProveeduria { IdArticulo = idArticulo, IdProveedor = idProveedor, Acreditacion = null, FechaAlta = fechaAlta };
                var response = await apiServicio.InsertarAsync(alta, new Uri(WebApp.BaseAddressRM), "api/AltaProveeduria/InsertarAltaProveeduria");
                if (response.IsSuccess)
                {
                    response.Resultado = JsonConvert.DeserializeObject<AltaProveeduria>(response.Resultado.ToString());
                    AltaProveeduria altaProv = response.Resultado as AltaProveeduria;
                    foreach (var item in ListadoFacturasSeleccionadas)
                    {
                        FacturasPorAltaProveeduria facturasPorAltaProveeduria = new FacturasPorAltaProveeduria { IdAlta = altaProv.IdAlta, NumeroFactura = item.Numero };
                        try
                        {
                            response = await apiServicio.InsertarAsync(facturasPorAltaProveeduria, new Uri(WebApp.BaseAddressRM), "api/FacturaPorAltaProveeduria/InsertarFacturasPorAltaProveeduria");
                            if (response.IsSuccess)
                            {

                            }
                            else
                            {
                                try
                                {
                                    var eliminar = await apiServicio.EliminarAsync(altaProv.IdAlta.ToString(), new Uri(WebApp.BaseAddressRM), "api/AltaProveeduria");
                                    if (eliminar.IsSuccess)
                                        break;
                                }
                                catch (Exception ex)
                                {
                                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminando un objeto de AltaProveeduria", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                                    return BadRequest();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Insertando un objeto de FacturasPorAltaProveeduria", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                            return BadRequest();
                        }
                    }
                    return RedirectToAction("ArticulosADarAlta");
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando un objeto de RecepcionArticulos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> IngresarFacturas()
        {
            try
            {
                var lista = await apiServicio.Listar<MaestroArticuloSucursal>(new Uri(WebApp.BaseAddressRM), "api/MaestroArticuloSucursal/ListarMaestroArticuloSucursal");
                return View("IngresarFacturas", lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando maestros de artículos de sucursal", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarFacturas(DetalleFactura detalleFactura)
        {
            int idMAS = detalleFactura.Factura.IdMaestroArticuloSucursal;
            int idProveedor = detalleFactura.Factura.IdProveedor;
            string numero = detalleFactura.Factura.Numero;
            int cantidad = detalleFactura.Cantidad;
            decimal? precio = decimal.Parse(Request.Form["Precio"].ToString().Replace('.', ','));

            try
            {
                Factura factura = new Factura();
                factura.IdMaestroArticuloSucursal = idMAS;
                factura.IdProveedor = idProveedor;
                factura.Numero = numero;
                var response = await apiServicio.InsertarAsync(factura, new Uri(WebApp.BaseAddressRM), "api/Factura/InsertarFactura");
                if (response.IsSuccess)
                {
                    try
                    {
                        var respuesta = await apiServicio.SeleccionarAsync<Response>(numero, new Uri(WebApp.BaseAddressRM), "api/Factura/FacturaPorNumero");
                        respuesta.Resultado = JsonConvert.DeserializeObject<Factura>(respuesta.Resultado.ToString());
                        Factura respuestaFactura = respuesta.Resultado as Factura;
                        try
                        {
                            //detalleFactura.Factura = respuestaFactura;
                            detalleFactura.IdFactura = respuestaFactura.IdFactura;
                            detalleFactura.Precio = precio;
                            response = await apiServicio.InsertarAsync(detalleFactura, new Uri(WebApp.BaseAddressRM), "api/DetalleFactura/InsertarDetalleFactura");

                            if (response.IsSuccess)
                                return RedirectToAction("IngresarFacturas");
                        }
                        catch (Exception ex)
                        {
                            await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Insertando Detalle de una Factura", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                            return BadRequest();
                        }
                    }
                    catch (Exception ex)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Obteniendo factura por Numero", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                        return BadRequest();
                    }
                }
                return RedirectToAction("FormularioAltaArticulo", idMAS);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Insertando una Factura", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> DetallesFactura(int ID)
        {
            var detalleFactura = new DetalleFactura();
            try
            {
                ViewData["listaProveedor"] = new SelectList((await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedores")).Select(c => new { c.IdProveedor, NombreApellidos = String.Format("{0} {1}", c.Nombre, c.Apellidos) }), "IdProveedor", "NombreApellidos");
                try
                {
                    var listaArticulos = new SelectList(await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddressRM), "api/Articulo/ListarArticulos"), "IdArticulo", "Nombre");
                    ViewBag.listaArticulos = listaArticulos;
                    ViewBag.ID = ID;
                    return View("DetallesFactura", detalleFactura);
                }
                catch (Exception ex)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando artículos en el ingreso de una factura", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando proveedores en el ingreso de una factura", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }
        #endregion

        #region Baja de Proveeduría
        public async Task<IActionResult> ArticulosADarBaja()
        {
            try
            {
                var lista = await apiServicio.Listar<RecepcionArticulos>(new Uri(WebApp.BaseAddressRM), "api/RecepcionArticulo/ListarArticulosAlta");
                return View("ArticulosBaja", lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando articulos recepcionados", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> FormularioBajaArticulo(int id)
        {
            try
            {
                var respuesta = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/RecepcionArticulo");
                respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionArticulos>(respuesta.Resultado.ToString());
                RecepcionArticulos RecepcionArticulos = respuesta.Resultado as RecepcionArticulos;

                if (respuesta.IsSuccess)
                    return View("FormularioBajaArticulo", RecepcionArticulos);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando un objeto de RecepcionArticulos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
            }
            return BadRequest();
        }

        public async Task<IActionResult> FormularioBajaArticuloDirecto(int id)
        {
            try
            {
                var respuesta = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/RecepcionArticulo");
                respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionArticulos>(respuesta.Resultado.ToString());
                RecepcionArticulos RecepcionArticulos = respuesta.Resultado as RecepcionArticulos;

                var response = await apiServicio.SeleccionarAsync<Response>(RecepcionArticulos.IdArticulo.ToString(), new Uri(WebApp.BaseAddressRM), "api/ExistenciaArticuloProveeduria");
                if (response.IsSuccess)
                {
                    var existenciaArticuloProveeduria = JsonConvert.DeserializeObject<ExistenciaArticuloProveeduria>(response.Resultado.ToString());
                    ViewBag.CantidadMaxima = existenciaArticuloProveeduria.Existencia;
                    return View(RecepcionArticulos);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando un objeto de RecepcionArticulos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> ListarSolicitudesDeBaja()
        {
            try
            {
                var lista = await apiServicio.Listar<SolicitudProveeduriaDetalle>(new Uri(WebApp.BaseAddressRM), "api/SolicitudDetalleProveeduria/ListarSolicitudProveeduriasDetalle");
                return View("SolicitudesDeBaja", lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando un objeto de RecepcionArticulos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> AprobarBajaArticulo(int id)
        {
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = "Baja Aprobada" }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                var listaEstado = await apiServicio.Listar<Estado>(new Uri(WebApp.BaseAddressTH), "api/Estados/ListarEstados");
                var respuesta = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/SolicitudDetalleProveeduria");
                respuesta.Resultado = JsonConvert.DeserializeObject<SolicitudProveeduriaDetalle>(respuesta.Resultado.ToString());
                SolicitudProveeduriaDetalle solProvDet = respuesta.Resultado as SolicitudProveeduriaDetalle;
                if (respuesta.IsSuccess)
                {
                    solProvDet.CantidadAprobada = solProvDet.CantidadSolicitada;
                    solProvDet.FechaAprobada = DateTime.Now;
                    solProvDet.IdEstado = listaEstado.SingleOrDefault(c => c.Nombre == "Baja Aprobada").IdEstado;
                    respuesta = await apiServicio.EditarAsync(id.ToString(), solProvDet, new Uri(WebApp.BaseAddressRM), "api/SolicitudDetalleProveeduria");
                    if (respuesta.IsSuccess)
                        return RedirectToAction("ListarSolicitudesDeBaja");

                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando un objeto de RecepcionArticulos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return BadRequest();
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarSolicitudBajaArticulo(RecepcionArticulos recepcionArticulos)
        {
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = "Baja Solicitada" }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                var listaEstado = await apiServicio.Listar<Estado>(new Uri(WebApp.BaseAddressTH), "api/Estados/ListarEstados");
                SolicitudProveeduria solProv = new SolicitudProveeduria { IdEmpleado = int.Parse(Request.Form["Empleado.IdEmpleado"].ToString()) };
                var respuesta = await apiServicio.InsertarAsync(solProv, new Uri(WebApp.BaseAddressRM), "api/SolicitudProveeduria/InsertarSolicitudProveeduria");
                if (respuesta.IsSuccess)
                {
                    respuesta.Resultado = JsonConvert.DeserializeObject<SolicitudProveeduria>(respuesta.Resultado.ToString());
                    solProv = respuesta.Resultado as SolicitudProveeduria;
                    SolicitudProveeduriaDetalle solProvDetalle = new SolicitudProveeduriaDetalle();
                    DateTime ahora = DateTime.Now;

                    solProvDetalle.CantidadAprobada = 1;
                    solProvDetalle.FechaAprobada = ahora;
                    solProvDetalle.CantidadSolicitada = int.Parse(Request.Form["Cantidad"].ToString());
                    solProvDetalle.IdEstado = listaEstado.SingleOrDefault(c => c.Nombre == "Baja Solicitada").IdEstado;
                    solProvDetalle.FechaSolicitud = ahora;
                    solProvDetalle.IdArticulo = int.Parse(Request.Form["IdArticulo"].ToString());
                    solProvDetalle.IdMaestroArticuloSucursal = int.Parse(Request.Form["IdMaestroArticuloSucursal"].ToString());
                    solProvDetalle.IdSolicitudProveeduria = solProv.IdSolicitudProveeduria;
                    respuesta = await apiServicio.InsertarAsync(solProvDetalle, new Uri(WebApp.BaseAddressRM), "api/SolicitudDetalleProveeduria/InsertarSolicitudProveeduriaDetalle");

                    //debe ir if (respuesta.IsSucces)

                    return RedirectToAction("ArticulosADarBaja");
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Dando baja a un Artículo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return BadRequest();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarSolicitudBajaArticuloDirecto(RecepcionArticulos recepcionArticulos)
        {
            try
            {
                var response = await apiServicio.SeleccionarAsync<Response>(recepcionArticulos.IdArticulo.ToString(), new Uri(WebApp.BaseAddressRM), "api/ExistenciaArticuloProveeduria");
                if (response.IsSuccess)
                {
                    var existenciaArticuloProveeduria = JsonConvert.DeserializeObject<ExistenciaArticuloProveeduria>(response.Resultado.ToString());
                    existenciaArticuloProveeduria.Existencia -= recepcionArticulos.Cantidad;
                    await apiServicio.EditarAsync<ExistenciaArticuloProveeduria>(recepcionArticulos.IdArticulo.ToString(), existenciaArticuloProveeduria, new Uri(WebApp.BaseAddressRM), "api/ExistenciaArticuloProveeduria");
                    return RedirectToAction("ArticulosADarBaja");
                }
                return View(recepcionArticulos);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Dando baja a un Artículo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return BadRequest();
            }
        }
        #endregion
    }
}
