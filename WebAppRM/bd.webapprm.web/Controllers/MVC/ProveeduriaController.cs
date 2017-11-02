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
                lista = await apiServicio.Listar<RecepcionArticulos>(new Uri(WebApp.BaseAddressRM)
                                                                    , "/api/RecepcionArticulo/ListarRecepcionArticulos");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando artículos recepcionados",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        public async Task<IActionResult> Recepcion()
        {
            ViewData["TipoArticulo"] = new SelectList(await apiServicio.Listar<TipoArticulo>(new Uri(WebApp.BaseAddressRM), "/api/TipoArticulo/ListarTipoArticulo"), "IdTipoArticulo", "Nombre");
            ViewData["ClaseArticulo"] = await ObtenerSelectListClaseArticulo((ViewData["TipoArticulo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["TipoArticulo"] as SelectList).FirstOrDefault().Value) : -1);
            ViewData["SubClaseArticulo"] = await ObtenerSelectListSubClaseArticulo((ViewData["ClaseArticulo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["ClaseArticulo"] as SelectList).FirstOrDefault().Value) : -1);
            ViewData["Articulo"] = await ObtenerSelectListArticulo((ViewData["SubClaseArticulo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["SubClaseArticulo"] as SelectList).FirstOrDefault().Value) : -1);

            ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "/api/Pais/ListarPais"), "IdPais", "Nombre");
            ViewData["Provincia"] = await ObtenerSelectListProvincia((ViewData["Pais"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Pais"] as SelectList).FirstOrDefault().Value) : -1);
            ViewData["Ciudad"] = await ObtenerSelectListCiudad((ViewData["Provincia"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Provincia"] as SelectList).FirstOrDefault().Value) : -1);
            ViewData["Sucursal"] = await ObtenerSelectListSucursal((ViewData["Ciudad"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Ciudad"] as SelectList).FirstOrDefault().Value) : -1);
            ViewData["MaestroArticuloSucursal"] = await ObtenerSelectListMaestroArticuloSucursal((ViewData["Sucursal"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Sucursal"] as SelectList).FirstOrDefault().Value) : -1);

            var listaProveedor = await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddressRM), "/api/Proveedor/ListarProveedores");
            var tlistaProveedor = listaProveedor.Select(c => new { IdProveedor = c.IdProveedor, NombreApellidos = String.Format("{0} {1}", c.Nombre, c.Apellidos) });
            ViewData["Proveedor"] = new SelectList(tlistaProveedor, "IdProveedor", "NombreApellidos");

            var listaEmpleado = await apiServicio.Listar<Empleado>(new Uri(WebApp.BaseAddressTH), "/api/Empleados/ListarEmpleados");
            var tlistaEmpleado = listaEmpleado.Select(c => new { IdEmpleado = c.IdEmpleado, NombreApellidos = String.Format("{0} {1}", c.Persona.Nombres, c.Persona.Apellidos) });
            ViewData["Empleado"] = new SelectList(tlistaEmpleado, "IdEmpleado", "NombreApellidos");
            return View();
        }

        public async Task<IActionResult> EditarRecepcion(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM),
                                                                  "/api/RecepcionArticulo");

                    respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionArticulos>(respuesta.Resultado.ToString());
                    var recepcionArticulos = respuesta.Resultado as RecepcionArticulos;
                    if (respuesta.IsSuccess)
                    {
                        ViewData["TipoArticulo"] = new SelectList(await apiServicio.Listar<TipoArticulo>(new Uri(WebApp.BaseAddressRM), "/api/TipoArticulo/ListarTipoArticulo"), "IdTipoArticulo", "Nombre");
                        ViewData["ClaseArticulo"] = await ObtenerSelectListClaseArticulo(recepcionArticulos?.Articulo?.SubClaseArticulo?.ClaseArticulo?.TipoArticulo?.IdTipoArticulo ?? -1);
                        ViewData["SubClaseArticulo"] = await ObtenerSelectListSubClaseArticulo(recepcionArticulos?.Articulo?.SubClaseArticulo?.ClaseArticulo?.IdClaseArticulo ?? -1);
                        ViewData["Articulo"] = await ObtenerSelectListArticulo(recepcionArticulos?.Articulo?.SubClaseArticulo?.IdSubClaseArticulo ?? -1);

                        ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "/api/Pais/ListarPais"), "IdPais", "Nombre");
                        ViewData["Provincia"] = await ObtenerSelectListProvincia(recepcionArticulos?.MaestroArticuloSucursal?.Sucursal?.Ciudad?.Provincia?.Pais?.IdPais ?? -1);
                        ViewData["Ciudad"] = await ObtenerSelectListCiudad(recepcionArticulos?.MaestroArticuloSucursal?.Sucursal?.Ciudad?.Provincia?.IdProvincia ?? -1);
                        ViewData["Sucursal"] = await ObtenerSelectListSucursal(recepcionArticulos?.MaestroArticuloSucursal?.Sucursal?.Ciudad?.IdCiudad ?? -1);
                        ViewData["MaestroArticuloSucursal"] = await ObtenerSelectListMaestroArticuloSucursal(recepcionArticulos?.MaestroArticuloSucursal?.Sucursal?.IdSucursal ?? -1);

                        var listaProveedor = await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddressRM), "/api/Proveedor/ListarProveedores");
                        var tlistaProveedor = listaProveedor.Select(c => new { IdProveedor = c.IdProveedor, NombreApellidos = String.Format("{0} {1}", c.Nombre, c.Apellidos) });
                        ViewData["Proveedor"] = new SelectList(tlistaProveedor, "IdProveedor", "NombreApellidos");

                        var listaEmpleado = await apiServicio.Listar<Empleado>(new Uri(WebApp.BaseAddressTH), "/api/Empleados/ListarEmpleados");
                        var tlistaEmpleado = listaEmpleado.Select(c => new { IdEmpleado = c.IdEmpleado, NombreApellidos = String.Format("{0} {1}", c.Persona.Nombres, c.Persona.Apellidos) });
                        ViewData["Empleado"] = new SelectList(tlistaEmpleado, "IdEmpleado", "NombreApellidos");

                        return View(recepcionArticulos);
                    }
                }
                return BadRequest();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recepcion(RecepcionArticulos recepcionArticulo) => await GestionarRecepcionArticulos(recepcionArticulo);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRecepcion(RecepcionArticulos recepcionArticulo) => await GestionarRecepcionArticulos(recepcionArticulo);

        private async Task<IActionResult> GestionarRecepcionArticulos(RecepcionArticulos recepcionArticulo)
        {
            Response response = new Response();
            try
            {
                if (recepcionArticulo.IdArticulo == 0)
                {
                    try
                    {
                        recepcionArticulo.Articulo = new Articulo();
                        int IdSubClaseArticulo = int.Parse(Request.Form["Articulo.IdSubClaseArticulo"].ToString());
                        var listaSubClaseArticulo = await apiServicio.Listar<SubClaseArticulo>(new Uri(WebApp.BaseAddressRM), "/api/SubClaseArticulo/ListarSubClaseArticulos");
                        recepcionArticulo.Articulo.SubClaseArticulo = listaSubClaseArticulo.SingleOrDefault(c => c.IdSubClaseArticulo == IdSubClaseArticulo);
                        recepcionArticulo.Articulo.IdSubClaseArticulo = IdSubClaseArticulo;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            recepcionArticulo.Articulo.SubClaseArticulo = new SubClaseArticulo();
                            int IdClaseArticulo = int.Parse(Request.Form["Articulo.SubClaseArticulo.IdClaseArticulo"].ToString());
                            var listaClaseArticulo = await apiServicio.Listar<ClaseArticulo>(new Uri(WebApp.BaseAddressRM), "/api/ClaseArticulo/ListarClaseArticulo");
                            recepcionArticulo.Articulo.SubClaseArticulo.ClaseArticulo = listaClaseArticulo.SingleOrDefault(c => c.IdClaseArticulo == IdClaseArticulo);
                            recepcionArticulo.Articulo.SubClaseArticulo.IdClaseArticulo = IdClaseArticulo;
                        }
                        catch (Exception)
                        {
                            try
                            {
                                recepcionArticulo.Articulo.SubClaseArticulo.ClaseArticulo = new ClaseArticulo();
                                int IdTipoArticulo = int.Parse(Request.Form["Articulo.SubClaseArticulo.ClaseArticulo.IdTipoArticulo"].ToString());
                                var listaTipoArticulo = await apiServicio.Listar<TipoArticulo>(new Uri(WebApp.BaseAddressRM), "/api/TipoArticulo/ListarTipoArticulo");
                                recepcionArticulo.Articulo.SubClaseArticulo.ClaseArticulo.TipoArticulo = listaTipoArticulo.SingleOrDefault(c => c.IdTipoArticulo == IdTipoArticulo);
                                recepcionArticulo.Articulo.SubClaseArticulo.ClaseArticulo.IdTipoArticulo = IdTipoArticulo;
                            }
                            catch (Exception)
                            { }
                        }
                    }
                }
                else
                {
                    var listaArticulo = await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddressRM), "/api/Articulo/ListarArticulos");
                    recepcionArticulo.Articulo = listaArticulo.SingleOrDefault(c => c.IdArticulo == recepcionArticulo.IdArticulo);
                }

                if (recepcionArticulo.IdMaestroArticuloSucursal == 0)
                {
                    try
                    {
                        recepcionArticulo.MaestroArticuloSucursal = new MaestroArticuloSucursal();
                        int IdSucursal = int.Parse(Request.Form["MaestroArticuloSucursal.IdSucursal"].ToString());

                        var listaSucursal = await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "/api/Sucursal/ListarSucursal");
                        recepcionArticulo.MaestroArticuloSucursal.Sucursal = listaSucursal.SingleOrDefault(c => c.IdSucursal == IdSucursal);
                        recepcionArticulo.MaestroArticuloSucursal.IdSucursal = IdSucursal;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            recepcionArticulo.MaestroArticuloSucursal.Sucursal = new Sucursal();
                            int IdCiudad = int.Parse(Request.Form["MaestroArticuloSucursal.Sucursal.IdCiudad"].ToString());

                            var listaCiudad = await apiServicio.Listar<Ciudad>(new Uri(WebApp.BaseAddressTH), "/api/Ciudad/ListarCiudad");
                            recepcionArticulo.MaestroArticuloSucursal.Sucursal.Ciudad = listaCiudad.SingleOrDefault(c => c.IdCiudad == IdCiudad);
                            recepcionArticulo.MaestroArticuloSucursal.Sucursal.IdCiudad = IdCiudad;
                        }
                        catch (Exception)
                        {
                            try
                            {
                                recepcionArticulo.MaestroArticuloSucursal.Sucursal.Ciudad = new Ciudad();
                                int IdProvincia = int.Parse(Request.Form["MaestroArticuloSucursal.Sucursal.Ciudad.IdProvincia"].ToString());

                                var listaProvincia = await apiServicio.Listar<Provincia>(new Uri(WebApp.BaseAddressTH), "/api/Provincia/ListarProvincia");
                                recepcionArticulo.MaestroArticuloSucursal.Sucursal.Ciudad.Provincia = listaProvincia.SingleOrDefault(c => c.IdProvincia == IdProvincia);
                                recepcionArticulo.MaestroArticuloSucursal.Sucursal.Ciudad.IdProvincia = IdProvincia;
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    recepcionArticulo.MaestroArticuloSucursal.Sucursal.Ciudad.Provincia = new Provincia();
                                    int IdPais = int.Parse(Request.Form["MaestroArticuloSucursal.Sucursal.Ciudad.Provincia.IdPais"].ToString());

                                    var listaPais = await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "/api/Pais/ListarPais");
                                    recepcionArticulo.MaestroArticuloSucursal.Sucursal.Ciudad.Provincia.Pais = listaPais.SingleOrDefault(c => c.IdPais == IdPais);
                                    recepcionArticulo.MaestroArticuloSucursal.Sucursal.Ciudad.Provincia.IdPais = IdPais;
                                }
                                catch (Exception)
                                { }
                            }
                        }
                    }
                }
                else
                {
                    var listaMaestroSucursal = await apiServicio.Listar<MaestroArticuloSucursal>(new Uri(WebApp.BaseAddressRM), "/api/MaestroArticuloSucursal/ListarMaestroArticuloSucursal");
                    recepcionArticulo.MaestroArticuloSucursal = listaMaestroSucursal.SingleOrDefault(c => c.IdMaestroArticuloSucursal == recepcionArticulo.IdMaestroArticuloSucursal);
                }
                TryValidateModel(recepcionArticulo);

                if (recepcionArticulo.IdRecepcionArticulos == 0)
                    response = await apiServicio.InsertarAsync(recepcionArticulo, new Uri(WebApp.BaseAddressRM), "/api/RecepcionArticulo/InsertarRecepcionArticulo");
                else
                    response = await apiServicio.EditarAsync<RecepcionArticulos>(recepcionArticulo.IdRecepcionArticulos.ToString(), recepcionArticulo, new Uri(WebApp.BaseAddressRM), "/api/RecepcionArticulo");

                if (response.IsSuccess)
                {
                    var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        ExceptionTrace = null,
                        Message = recepcionArticulo.IdRecepcionArticulos == 0 ? "Se ha recepcionado un artículo" : "Se ha editado la recepción de un artículo",
                        UserName = "Usuario 1",
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        EntityID = string.Format("{0} {1}", "Artículo:", recepcionArticulo.IdArticulo),
                    });
                    return RedirectToAction("ListadoRecepcion");
                }

                ViewData["TipoArticulo"] = new SelectList(await apiServicio.Listar<TipoArticulo>(new Uri(WebApp.BaseAddressRM), "/api/TipoArticulo/ListarTipoArticulo"), "IdTipoArticulo", "Nombre");
                ViewData["ClaseArticulo"] = await ObtenerSelectListClaseArticulo(recepcionArticulo?.Articulo?.SubClaseArticulo?.ClaseArticulo?.IdTipoArticulo ?? -1);
                ViewData["SubClaseArticulo"] = await ObtenerSelectListSubClaseArticulo(recepcionArticulo?.Articulo?.SubClaseArticulo?.ClaseArticulo?.IdClaseArticulo ?? -1);
                ViewData["Articulo"] = await ObtenerSelectListArticulo(recepcionArticulo?.Articulo?.SubClaseArticulo?.IdSubClaseArticulo ?? -1);

                ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "/api/Pais/ListarPais"), "IdPais", "Nombre");
                ViewData["Provincia"] = await ObtenerSelectListProvincia(recepcionArticulo?.MaestroArticuloSucursal?.Sucursal?.Ciudad?.Provincia?.Pais?.IdPais ?? -1);
                ViewData["Ciudad"] = await ObtenerSelectListCiudad(recepcionArticulo?.MaestroArticuloSucursal?.Sucursal?.Ciudad?.Provincia?.IdProvincia ?? -1);
                ViewData["Sucursal"] = await ObtenerSelectListSucursal(recepcionArticulo?.MaestroArticuloSucursal?.Sucursal?.Ciudad?.IdCiudad ?? -1);
                ViewData["MaestroArticuloSucursal"] = await ObtenerSelectListMaestroArticuloSucursal(recepcionArticulo?.MaestroArticuloSucursal?.Sucursal?.IdSucursal ?? -1);

                var listaProveedor = await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddressRM), "/api/Proveedor/ListarProveedores");
                var tlistaProveedor = listaProveedor.Select(c => new { IdProveedor = c.IdProveedor, NombreApellidos = String.Format("{0} {1}", c.Nombre, c.Apellidos) });
                ViewData["Proveedor"] = new SelectList(tlistaProveedor, "IdProveedor", "NombreApellidos");

                var listaEmpleado = await apiServicio.Listar<Empleado>(new Uri(WebApp.BaseAddressTH), "/api/Empleados/ListarEmpleados");
                var tlistaEmpleado = listaEmpleado.Select(c => new { IdEmpleado = c.IdEmpleado, NombreApellidos = String.Format("{0} {1}", c.Persona.Nombres, c.Persona.Apellidos) });
                ViewData["Empleado"] = new SelectList(tlistaEmpleado, "IdEmpleado", "NombreApellidos");

                ViewData["Error"] = response.Message;
                return View(recepcionArticulo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Creando recepción Artículo",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });
                return BadRequest();
            }
        }
        #endregion

        #region Reportes
        public async Task<IActionResult> ProveeduriaReporteAltas()
        {
            var lista = new List<RecepcionArticulos>();
            try
            {
                lista = await apiServicio.Listar<RecepcionArticulos>(new Uri(WebApp.BaseAddressRM)
                                                                    , "/api/RecepcionArticulo/ListarRecepcionArticulos");

                var listaAltas = lista.Where(c => c.Cantidad > 0).ToList();
                return View(listaAltas);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando Artículos recepcionados en Alta",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }
        public async Task<IActionResult> ProveeduriaReporteBajas()
        {
            var lista = new List<RecepcionArticulos>();
            try
            {
                lista = await apiServicio.Listar<RecepcionArticulos>(new Uri(WebApp.BaseAddressRM)
                                                                    , "/api/RecepcionArticulo/ListarRecepcionArticulos");

                var listaBajas = lista.Where(c => c.Cantidad == 0).ToList();
                return View(listaBajas);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando Artículos recepcionados en Alta",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }
        public async Task<IActionResult> EstadisticasConsumoAreaReporte()
        {
            var lista = new List<RecepcionArticulos>();
            try
            {
                lista = await apiServicio.Listar<RecepcionArticulos>(new Uri(WebApp.BaseAddressRM)
                                                                    , "/api/RecepcionArticulo/ListarRecepcionArticulos");

                var listaBajas = lista.Where(c => c.Cantidad == 0).ToList();
                return View(listaBajas);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando Artículos recepcionados en Alta",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }
        public async Task<IActionResult> AlertaVencimientoReporte()
        {
            var lista = new List<RecepcionArticulos>();
            try
            {
                lista = await apiServicio.Listar<RecepcionArticulos>(new Uri(WebApp.BaseAddressRM)
                                                                    , "/api/RecepcionArticulo/ListarRecepcionArticulos");

                var listaBajas = lista.Where(c => c.Cantidad == 0).ToList();
                return View(listaBajas);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando Artículos recepcionados en Alta",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }
        public async Task<IActionResult> ConsolidadoInventarioReporte()
        {
            var lista = new List<RecepcionArticulos>();
            try
            {
                lista = await apiServicio.Listar<RecepcionArticulos>(new Uri(WebApp.BaseAddressRM)
                                                                    , "/api/RecepcionArticulo/ListarRecepcionArticulos");

                var listaBajas = lista.Where(c => c.Cantidad == 0).ToList();
                return View(listaBajas);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando Artículos recepcionados en Alta",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }
        public async Task<IActionResult> ConsolidadoSolicitudReporte()
        {
            var lista = new List<RecepcionArticulos>();
            try
            {
                lista = await apiServicio.Listar<RecepcionArticulos>(new Uri(WebApp.BaseAddressRM)
                                                                    , "/api/RecepcionArticulo/ListarRecepcionArticulos");

                var listaBajas = lista.Where(c => c.Cantidad == 0).ToList();
                return View(listaBajas);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando Artículos recepcionados en Alta",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }
        public async Task<IActionResult> MinMaxReporte()
        {
            var lista = new List<RecepcionArticulos>();
            try
            {
                lista = await apiServicio.Listar<RecepcionArticulos>(new Uri(WebApp.BaseAddressRM)
                                                                    , "/api/RecepcionArticulo/ListarRecepcionArticulos");

                var listaBajas = lista.Where(c => c.Cantidad == 0).ToList();
                return View(listaBajas);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando Artículos recepcionados en Alta",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }
        #endregion

        #region AJAX_ClaseArticulo
        public async Task<SelectList> ObtenerSelectListClaseArticulo(int idTipoArticulo)
        {
            try
            {
                var listaClaseArticulo = await apiServicio.Listar<ClaseArticulo>(new Uri(WebApp.BaseAddressRM), "/api/ClaseArticulo/ListarClaseArticulo");
                listaClaseArticulo = idTipoArticulo != -1 ? listaClaseArticulo.Where(c => c.IdTipoArticulo == idTipoArticulo).ToList() : new List<ClaseArticulo>();
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
                var listaSubClaseArticulo = await apiServicio.Listar<SubClaseArticulo>(new Uri(WebApp.BaseAddressRM), "/api/SubClaseArticulo/ListarSubClaseArticulos");
                listaSubClaseArticulo = idClaseArticulo != -1 ? listaSubClaseArticulo.Where(c => c.IdClaseArticulo == idClaseArticulo).ToList() : new List<SubClaseArticulo>();
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
                var listaArticulo = await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddressRM), "/api/Articulo/ListarArticulos");
                listaArticulo = idSubClaseArticulo != -1 ? listaArticulo.Where(c => c.IdSubClaseArticulo == idSubClaseArticulo).ToList() : new List<Articulo>();
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
                var listaProvincia = await apiServicio.Listar<Provincia>(new Uri(WebApp.BaseAddressTH), "/api/Provincia/ListarProvincia");
                listaProvincia = idPais != -1 ? listaProvincia.Where(c => c.IdPais == idPais).ToList() : new List<Provincia>();
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
                var listaCiudad = await apiServicio.Listar<Ciudad>(new Uri(WebApp.BaseAddressTH), "/api/Ciudad/ListarCiudad");
                listaCiudad = idProvincia != -1 ? listaCiudad.Where(c => c.IdProvincia == idProvincia).ToList() : new List<Ciudad>();
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
                var listaSucursal = await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "/api/Sucursal/ListarSucursal");
                listaSucursal = idCiudad != -1 ? listaSucursal.Where(c => c.IdCiudad == idCiudad).ToList() : new List<Sucursal>();
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
                var listaMaestroArticuloSucursal = await apiServicio.Listar<MaestroArticuloSucursal>(new Uri(WebApp.BaseAddressRM), "/api/MaestroArticuloSucursal/ListarMaestroArticuloSucursal");
                listaMaestroArticuloSucursal = idSucursal != -1 ? listaMaestroArticuloSucursal.Where(c => c.IdSucursal == idSucursal).ToList() : new List<MaestroArticuloSucursal>();
                var tlistaMaestroArticuloSucursal = listaMaestroArticuloSucursal.Select(c => new { IdMaestroArticuloSucursal = c.IdMaestroArticuloSucursal, Maestro = String.Format("Mínimo: {0} - Máximo: {1}", c.Minimo, c.Maximo) });
                return new SelectList(tlistaMaestroArticuloSucursal, "IdMaestroArticuloSucursal", "Maestro");
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

        #endregion#region Alta de Proveeduría

        #region Alta de Proveeduría
        public async Task<IActionResult> ArticulosADarAlta()
        {
            var lista = new List<RecepcionArticulos>();
            try
            {
                lista = await apiServicio.Listar<RecepcionArticulos>(new Uri(WebApp.BaseAddressRM)
                                                                    , "/api/RecepcionArticulo/ListarRecepcionArticulos");

                var listaArticulosRecepcionados = lista.Select(c => c).ToList();

                return View("ArticulosAlta", listaArticulosRecepcionados);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando articulos recepcionados",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        public async Task<IActionResult> FormularioAltaArticulo(int ID)
        {
            try
            {
                var respuesta = await apiServicio.SeleccionarAsync<Response>(ID.ToString(), new Uri(WebApp.BaseAddressRM), "/api/RecepcionArticulo");

                respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionArticulos>(respuesta.Resultado.ToString());

                RecepcionArticulos RecepcionArticulos = respuesta.Resultado as RecepcionArticulos;

                ViewBag.Acreditacion = new SelectList(new List<string> { "Facturas", "Documentos" });

                if (respuesta.IsSuccess)
                {
                    try
                    {
                        return View("FormularioAltaArticulo", RecepcionArticulos);
                    }
                    catch (Exception ex)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                        {
                            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                            Message = "Listando facturas",
                            ExceptionTrace = ex,
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                            UserName = "Usuario APP WebAppTh"
                        });

                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando un objeto de RecepcionArticulos",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });

                return BadRequest();
            }

            return View();
        }

        public async Task<IActionResult> CargarTablaFacturasExcluidas(int ID)
        {
            try
            {
                var respuesta = await apiServicio.SeleccionarAsync<Response>(ID.ToString(), new Uri(WebApp.BaseAddressRM), "/api/RecepcionArticulo");

                respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionArticulos>(respuesta.Resultado.ToString());

                RecepcionArticulos RecepcionArticulos = respuesta.Resultado as RecepcionArticulos;

                if (respuesta.IsSuccess)
                {
                    try
                    {
                        List<DetalleFactura> listaDetalleFactura = RecepcionArticulos.Articulo.DetalleFactura.ToList();

                        foreach (var item in listaDetalleFactura)
                        {
                            respuesta = await apiServicio.SeleccionarAsync<Response>(item.IdFactura.ToString(), new Uri(WebApp.BaseAddressRM), "/api/Factura");

                            respuesta.Resultado = JsonConvert.DeserializeObject<Factura>(respuesta.Resultado.ToString());

                            Factura factura = respuesta.Resultado as Factura;

                            var respuestaOtra = await apiServicio.SeleccionarAsync<Response>(factura.Numero.ToString(), new Uri(WebApp.BaseAddressRM), "/api/FacturaPorAltaProveeduria");

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
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                        {
                            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                            Message = "Listando facturas",
                            ExceptionTrace = ex,
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                            UserName = "Usuario APP WebAppTh"
                        });

                        return BadRequest();
                    }
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando un objeto de RecepcionArticulos",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });

                return BadRequest();
            }
        }

        public async Task<IActionResult> CargarTablaFacturasIncluidas(int ID)
        {
            try
            {
                var respuesta = await apiServicio.SeleccionarAsync<Response>(ID.ToString(), new Uri(WebApp.BaseAddressRM), "/api/RecepcionArticulo");

                respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionArticulos>(respuesta.Resultado.ToString());

                RecepcionArticulos RecepcionArticulos = respuesta.Resultado as RecepcionArticulos;

                if (respuesta.IsSuccess)
                {
                    try
                    {
                        List<DetalleFactura> listaDetalleFactura = RecepcionArticulos.Articulo.DetalleFactura.ToList();

                        foreach (var item in listaDetalleFactura)
                        {
                            respuesta = await apiServicio.SeleccionarAsync<Response>(item.IdFactura.ToString(), new Uri(WebApp.BaseAddressRM), "/api/Factura");

                            respuesta.Resultado = JsonConvert.DeserializeObject<Factura>(respuesta.Resultado.ToString());

                            Factura factura = respuesta.Resultado as Factura;

                            var respuestaOtra = await apiServicio.SeleccionarAsync<Response>(factura.Numero.ToString(), new Uri(WebApp.BaseAddressRM), "/api/FacturasPorAltaProveeduria");

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
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                        {
                            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                            Message = "Listando facturas",
                            ExceptionTrace = ex,
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                            UserName = "Usuario APP WebAppTh"
                        });

                        return BadRequest();
                    }
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando un objeto de RecepcionArticulos",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });

                return BadRequest();
            }
        }

        public async Task<IActionResult> IncluirFacturasEnAlta(int idFactura)
        {
            try
            {
                var respuesta = await apiServicio.SeleccionarAsync<Response>(idFactura.ToString(), new Uri(WebApp.BaseAddressRM), "/api/Factura");

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
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Incluyendo una factura a un objeto de Alta",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });

                return BadRequest();
            }
        }

        public async Task<IActionResult> RefrescarTablaExcluidos(int idFactura)
        {
            var respuesta = await apiServicio.SeleccionarAsync<Response>(idFactura.ToString(), new Uri(WebApp.BaseAddressRM), "/api/Factura");

            if (respuesta.IsSuccess)
            {
                respuesta.Resultado = JsonConvert.DeserializeObject<Factura>(respuesta.Resultado.ToString());

                Factura factura = respuesta.Resultado as Factura;

                List<Factura> temporal = new List<Factura>();

                foreach (var item in ListadoFacturas)
                {
                    if (item.IdFactura != factura.IdFactura)
                    {
                        temporal.Add(item);
                    }
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
                var respuesta = await apiServicio.SeleccionarAsync<Response>(idFactura.ToString(), new Uri(WebApp.BaseAddressRM), "/api/Factura");

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
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Excluyendo una factura a un objeto de Alta",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });

                return BadRequest();
            }
        }

        public async Task<IActionResult> RefrescarTablaIncluidos(int idFactura)
        {
            var respuesta = await apiServicio.SeleccionarAsync<Response>(idFactura.ToString(), new Uri(WebApp.BaseAddressRM), "/api/Factura");

            if (respuesta.IsSuccess)
            {
                respuesta.Resultado = JsonConvert.DeserializeObject<Factura>(respuesta.Resultado.ToString());

                Factura factura = respuesta.Resultado as Factura;

                List<Factura> temporal = new List<Factura>();

                foreach (var item in ListadoFacturasSeleccionadas)
                {
                    if (item.IdFactura != factura.IdFactura)
                    {
                        temporal.Add(item);
                    }
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

                Response response = new Response();

                response = await apiServicio.InsertarAsync(alta,
                                                                 new Uri(WebApp.BaseAddressRM),
                                                                 "/api/AltaProveeduria/InsertarAltaProveeduria");

                if (response.IsSuccess)
                {
                    response.Resultado = JsonConvert.DeserializeObject<AltaProveeduria>(response.Resultado.ToString());

                    AltaProveeduria altaProv = response.Resultado as AltaProveeduria;

                    foreach (var item in ListadoFacturasSeleccionadas)
                    {
                        FacturasPorAltaProveeduria facturasPorAltaProveeduria = new FacturasPorAltaProveeduria { IdAlta = altaProv.IdAlta, NumeroFactura = item.Numero };

                        try
                        {
                            response = await apiServicio.InsertarAsync(facturasPorAltaProveeduria, new Uri(WebApp.BaseAddressRM), "/api/FacturaPorAltaProveeduria/InsertarFacturasPorAltaProveeduria");

                            if (response.IsSuccess)
                            {

                            }
                            else
                            {
                                try
                                {
                                    var eliminar = await apiServicio.EliminarAsync(altaProv.IdAlta.ToString(), new Uri(WebApp.BaseAddressRM), "/api/AltaProveeduria");

                                    if (eliminar.IsSuccess)
                                    {
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                                    {
                                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                                        Message = "Eliminando un objeto de AltaProveeduria",
                                        ExceptionTrace = ex,
                                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                                        LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                                        UserName = "Usuario APP WebAppTh"
                                    });

                                    return BadRequest();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                            {
                                ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                                Message = "Insertando un objeto de FacturasPorAltaProveeduria",
                                ExceptionTrace = ex,
                                LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                                LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                                UserName = "Usuario APP WebAppTh"
                            });

                            return BadRequest();
                        }
                    }

                    return RedirectToAction("ArticulosADarAlta");
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando un objeto de RecepcionArticulos",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });

                return BadRequest();
            }
        }

        public async Task<IActionResult> IngresarFacturas()
        {
            var lista = new List<MaestroArticuloSucursal>();
            try
            {
                lista = await apiServicio.Listar<MaestroArticuloSucursal>(new Uri(WebApp.BaseAddressRM)
                                                                    , "/api/MaestroArticuloSucursal/ListarMaestroArticuloSucursal");
                return View("IngresarFacturas", lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando maestros de artículos de sucursal",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
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

                Response response = new Response();

                response = await apiServicio.InsertarAsync(factura, new Uri(WebApp.BaseAddressRM)
                                                                    , "/api/Factura/InsertarFactura");

                if (response.IsSuccess)
                {
                    try
                    {
                        var respuesta = await apiServicio.SeleccionarAsync<Response>(numero, new Uri(WebApp.BaseAddressRM),
                                                                                        "/api/Factura/FacturaPorNumero");

                        respuesta.Resultado = JsonConvert.DeserializeObject<Factura>(respuesta.Resultado.ToString());

                        Factura respuestaFactura = respuesta.Resultado as Factura;

                        try
                        {
                            //detalleFactura.Factura = respuestaFactura;
                            detalleFactura.IdFactura = respuestaFactura.IdFactura;
                            detalleFactura.Precio = precio;

                            response = await apiServicio.InsertarAsync(detalleFactura, new Uri(WebApp.BaseAddressRM)
                                                                    , "/api/DetalleFactura/InsertarDetalleFactura");

                            if (response.IsSuccess)
                            {
                                return RedirectToAction("IngresarFacturas");
                            }
                        }
                        catch (Exception ex)
                        {
                            await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                            {
                                ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                                Message = "Insertando Detalle de una Factura",
                                ExceptionTrace = ex,
                                LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                                LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                                UserName = "Usuario APP webappth"
                            });
                            return BadRequest();
                        }

                    }
                    catch (Exception ex)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                        {
                            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                            Message = "Obteniendo factura por Numero",
                            ExceptionTrace = ex,
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                            UserName = "Usuario APP webappth"
                        });
                        return BadRequest();
                    }

                }

                return RedirectToAction("FormularioAltaArticulo", idMAS);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Insertando una Factura",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        public async Task<IActionResult> DetallesFactura(int ID)
        {
            var detalleFactura = new DetalleFactura();
            try
            {
                var listaProveedor = await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddressRM), "/api/Proveedor/ListarProveedores");
                var tlistaProveedor = listaProveedor.Select(c => new { IdProveedor = c.IdProveedor, NombreApellidos = String.Format("{0} {1}", c.Nombre, c.Apellidos) });
                ViewData["listaProveedor"] = new SelectList(tlistaProveedor, "IdProveedor", "NombreApellidos");

                try
                {
                    var listaArticulos = new SelectList(await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddressRM)
                                                                        , "/api/Articulo/ListarArticulos"), "IdArticulo", "Nombre");
                    ViewBag.listaArticulos = listaArticulos;
                    ViewBag.ID = ID;

                    return View("DetallesFactura", detalleFactura);
                }
                catch (Exception ex)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        Message = "Listando artículos en el ingreso de una factura",
                        ExceptionTrace = ex,
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                        UserName = "Usuario APP webappth"
                    });
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando proveedores en el ingreso de una factura",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }
        #endregion

        #region Baja de Proveeduría

        public async Task<IActionResult> ArticulosADarBaja()
        {
            var lista = new List<RecepcionArticulos>();
            try
            {
                lista = await apiServicio.Listar<RecepcionArticulos>(new Uri(WebApp.BaseAddressRM)
                                                                    , "/api/RecepcionArticulo/ListarRecepcionArticulos");

                var listaArticulosRecepcionados = lista.Select(c => c).ToList();

                return View("ArticulosBaja", listaArticulosRecepcionados);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando articulos recepcionados",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        public async Task<IActionResult> FormularioBajaArticulo(int ID)
        {
            try
            {
                var respuesta = await apiServicio.SeleccionarAsync<Response>(ID.ToString(), new Uri(WebApp.BaseAddressRM), "/api/RecepcionArticulo");

                respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionArticulos>(respuesta.Resultado.ToString());

                RecepcionArticulos RecepcionArticulos = respuesta.Resultado as RecepcionArticulos;

                if (respuesta.IsSuccess)
                {
                    return View("FormularioBajaArticulo", RecepcionArticulos);
                }
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando un objeto de RecepcionArticulos",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });

                return BadRequest();
            }

            return View();

        }

        public async Task<IActionResult> ListarSolicitudesDeBaja()
        {
            try
            {
                List<SolicitudProveduriaDetalle> respuesta = await apiServicio.Listar<SolicitudProveduriaDetalle>(new Uri(WebApp.BaseAddressRM), "/api/SolicitudDetalleProveeduria/ListarSolicitudProveeduriasDetalle");

                return View("SolicitudesDeBaja", respuesta);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando un objeto de RecepcionArticulos",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });

                return BadRequest();
            }

            return View();
        }

        public async Task<IActionResult> AprobarBajaArticulo(int ID)
        {
            try
            {
                var respuesta = await apiServicio.SeleccionarAsync<Response>(ID.ToString(), new Uri(WebApp.BaseAddressRM), "/api/SolicitudDetalleProveeduria");

                respuesta.Resultado = JsonConvert.DeserializeObject<SolicitudProveduriaDetalle>(respuesta.Resultado.ToString());

                SolicitudProveduriaDetalle solProvDet = respuesta.Resultado as SolicitudProveduriaDetalle;

                if (respuesta.IsSuccess)
                {
                    solProvDet.CantidadAprobada = solProvDet.CantidadSolicitada;
                    solProvDet.FechaAprobada = DateTime.Now;
                    solProvDet.IdEstado = 9;

                    respuesta = await apiServicio.EditarAsync(ID.ToString(), solProvDet,
                                                            new Uri(WebApp.BaseAddressRM), "/api/SolicitudDetalleProveeduria");

                    if (respuesta.IsSuccess)
                    {
                        return RedirectToAction("ListarSolicitudesDeBaja");
                    }

                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando un objeto de RecepcionArticulos",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });

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
                SolicitudProveduria solProv = new SolicitudProveduria { IdEmpleado = int.Parse(Request.Form["Empleado.IdEmpleado"].ToString()) };

                var respuesta = await apiServicio.InsertarAsync(solProv, new Uri(WebApp.BaseAddressRM), "/api/SolicitudProveeduria/InsertarSolicitudProveeduria");

                if (respuesta.IsSuccess)
                {
                    respuesta.Resultado = JsonConvert.DeserializeObject<SolicitudProveduria>(respuesta.Resultado.ToString());

                    solProv = respuesta.Resultado as SolicitudProveduria;

                    SolicitudProveduriaDetalle solProvDetalle = new SolicitudProveduriaDetalle();

                    DateTime ahora = DateTime.Now;

                    solProvDetalle.CantidadAprobada = 1;
                    solProvDetalle.FechaAprobada = ahora;
                    solProvDetalle.CantidadSolicitada = int.Parse(Request.Form["Cantidad"].ToString());
                    solProvDetalle.IdEstado = 8; //hacer consulta a servicio que devuelva el id en base a un nombre de estado
                    solProvDetalle.FechaSolicitud = ahora;
                    solProvDetalle.IdArticulo = int.Parse(Request.Form["IdArticulo"].ToString());
                    solProvDetalle.IdMaestroArticuloSucursal = int.Parse(Request.Form["IdMaestroArticuloSucursal"].ToString());
                    solProvDetalle.IdSolicitudProveduria = solProv.IdSolicitudProveduria;

                    respuesta = await apiServicio.InsertarAsync(solProvDetalle, new Uri(WebApp.BaseAddressRM), "/api/SolicitudDetalleProveeduria/InsertarSolicitudProveeduriaDetalle");

                    //debe ir if (respuesta.IsSucces)

                    return RedirectToAction("ArticulosADarBaja");
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando un objeto de RecepcionArticulos",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });

                return BadRequest();
            }
        }
        #endregion
    }
}
