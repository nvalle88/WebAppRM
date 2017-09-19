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

namespace bd.webapprm.web.Controllers.MVC
{
    public class ActivoFijoController : Controller
    {
        private readonly IApiServicio apiServicio;

        public ActivoFijoController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Recepcion");
        }

        public async Task<IActionResult> ActivoValidacionTecnica()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/RecepcionActivoFijo/ListarRecepcionActivoFijo");

                var listaActivosFijosValidacionTecnica = lista.Where(c => c.Estado.Nombre == "Validación Técnica").ToList();
                return View(listaActivosFijosValidacionTecnica);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando activos fijos que requieren validación técnica",
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
            ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddress), "/api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
            ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo((ViewData["TipoActivoFijo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["TipoActivoFijo"] as SelectList).FirstOrDefault().Value) : -1);
            ViewData["SubClaseActivoFijo"] = await ObtenerSelectListSubClaseActivoFijo((ViewData["ClaseActivoFijo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["ClaseActivoFijo"] as SelectList).FirstOrDefault().Value) : -1);
            ViewData["MotivoRecepcion"] = new SelectList(await apiServicio.Listar<MotivoRecepcion>(new Uri(WebApp.BaseAddress), "/api/MotivoRecepcion/ListarMotivoRecepcion"), "IdMotivoRecepcion", "Descripcion");

            var listaProveedor = await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddress), "/api/Proveedor/ListarProveedores");
            var tlistaProveedor = listaProveedor.Select(c => new { IdProveedor = c.IdProveedor, NombreApellidos = String.Format("{0} {1}", c.Nombre, c.Apellidos) });
            ViewData["Proveedor"] = new SelectList(tlistaProveedor, "IdProveedor", "NombreApellidos");

            var listaEmpleado = await apiServicio.Listar<Empleado>(new Uri(WebApp.BaseAddress), "/api/Empleado/ListarEmpleados");
            var tlistaEmpleado = listaEmpleado.Select(c => new { IdEmpleado = c.IdEmpleado, NombreApellidos = String.Format("{0} {1}", c.Persona.Nombres, c.Persona.Apellidos) });
            ViewData["Empleado"] = new SelectList(tlistaEmpleado, "IdEmpleado", "NombreApellidos");

            ViewData["Marca"] = new SelectList(await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddress), "/api/Marca/ListarMarca"), "IdMarca", "Nombre");
            ViewData["Modelo"] = await ObtenerSelectListModelo((ViewData["Marca"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Marca"] as SelectList).FirstOrDefault().Value) : -1);
            ViewData["UnidadMedida"] = new SelectList(await apiServicio.Listar<UnidadMedida>(new Uri(WebApp.BaseAddress), "/api/UnidadMedida/ListarUnidadMedida"), "IdUnidadMedida", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recepcion(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle)
        {
            Response response = new Response();
            try
            {
                var listaTipoActivoFijo = await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddress), "/api/TipoActivoFijo/ListarTipoActivoFijos");
                var listaMarca = await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddress), "/api/Marca/ListarMarca");

                var listaSubClaseActivoFijo = await apiServicio.Listar<SubClaseActivoFijo>(new Uri(WebApp.BaseAddress), "/api/SubClaseActivoFijo/ListarSubClasesActivoFijo");
                recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo = listaSubClaseActivoFijo.SingleOrDefault(c => c.IdSubClaseActivoFijo == recepcionActivoFijoDetalle?.RecepcionActivoFijo?.IdSubClaseActivoFijo);

                if (recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo == null)
                {
                    try
                    {
                        recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo = new SubClaseActivoFijo();
                        int IdClaseActivoFijo = int.Parse(Request.Form["RecepcionActivoFijo.SubClaseActivoFijo.IdClaseActivoFijo"].ToString());

                        var listaClaseActivoFijo = await apiServicio.Listar<ClaseActivoFijo>(new Uri(WebApp.BaseAddress), "/api/ClaseActivoFijo/ListarClaseActivoFijo");
                        recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo.ClaseActivoFijo = listaClaseActivoFijo.SingleOrDefault(c => c.IdClaseActivoFijo == IdClaseActivoFijo);
                        recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo.IdClaseActivoFijo = IdClaseActivoFijo;
                    }
                    catch (Exception)
                    {
                        recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo.ClaseActivoFijo = new ClaseActivoFijo();
                        try
                        {
                            int IdTipoActivoFijo = int.Parse(Request.Form["RecepcionActivoFijo.SubClaseActivoFijo.ClaseActivoFijo.IdTipoActivoFijo"].ToString());
                            recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo.ClaseActivoFijo.TipoActivoFijo = listaTipoActivoFijo.SingleOrDefault(c => c.IdTipoActivoFijo == IdTipoActivoFijo);
                            recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo.ClaseActivoFijo.IdTipoActivoFijo = IdTipoActivoFijo;
                        }
                        catch (Exception)
                        { }
                    }
                }

                var listaModelo = await apiServicio.Listar<Modelo>(new Uri(WebApp.BaseAddress), "/api/Modelo/ListarModelos");
                recepcionActivoFijoDetalle.ActivoFijo.Modelo = listaModelo.SingleOrDefault(c => c.IdModelo == recepcionActivoFijoDetalle?.ActivoFijo?.IdModelo);

                if (recepcionActivoFijoDetalle.ActivoFijo.Modelo == null)
                {
                    try
                    {
                        recepcionActivoFijoDetalle.ActivoFijo.Modelo = new Modelo();
                        int IdMarca = int.Parse(Request.Form["ActivoFijo.Modelo.IdMarca"].ToString());
                        recepcionActivoFijoDetalle.ActivoFijo.Modelo.Marca = listaMarca.SingleOrDefault(c => c.IdMarca == IdMarca);
                        recepcionActivoFijoDetalle.ActivoFijo.Modelo.IdMarca = IdMarca;
                    }
                    catch (Exception)
                    { }
                }

                var listaUnidadMedida = await apiServicio.Listar<UnidadMedida>(new Uri(WebApp.BaseAddress), "/api/UnidadMedida/ListarUnidadMedida");
                recepcionActivoFijoDetalle.ActivoFijo.UnidadMedida = listaUnidadMedida.SingleOrDefault(c => c.IdUnidadMedida == recepcionActivoFijoDetalle.ActivoFijo.IdUnidadMedida);

                TryValidateModel(recepcionActivoFijoDetalle);

                /*response = await apiServicio.InsertarAsync(activoFijo,
                                                             new Uri(WebApp.BaseAddress),
                                                             "/api/ActivoFijo/InsertarActivoFijo");
                if (response.IsSuccess)
                {

                    var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        ExceptionTrace = null,
                        Message = "Se ha creado un activo fijo",
                        UserName = "Usuario 1",
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        EntityID = string.Format("{0} {1}", "Activo Fijo:", activoFijo.IdActivoFijo),
                    });

                    return RedirectToAction("Index");
                }*/

                ViewData["Error"] = response.Message;
                ViewData["TipoActivoFijo"] = new SelectList(listaTipoActivoFijo, "IdTipoActivoFijo", "Nombre");
                ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo(recepcionActivoFijoDetalle?.RecepcionActivoFijo?.SubClaseActivoFijo?.ClaseActivoFijo?.TipoActivoFijo?.IdTipoActivoFijo ?? -1);
                ViewData["SubClaseActivoFijo"] = await ObtenerSelectListSubClaseActivoFijo(recepcionActivoFijoDetalle?.RecepcionActivoFijo?.SubClaseActivoFijo?.ClaseActivoFijo?.IdClaseActivoFijo ?? -1);
                ViewData["MotivoRecepcion"] = new SelectList(await apiServicio.Listar<MotivoRecepcion>(new Uri(WebApp.BaseAddress), "/api/MotivoRecepcion/ListarMotivoRecepcion"), "IdMotivoRecepcion", "Descripcion");

                var listaProveedor = await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddress), "/api/Proveedor/ListarProveedores");
                var tlistaProveedor = listaProveedor.Select(c => new { IdProveedor = c.IdProveedor, NombreApellidos = String.Format("{0} {1}", c.Nombre, c.Apellidos) });
                ViewData["Proveedor"] = new SelectList(tlistaProveedor, "IdProveedor", "NombreApellidos");

                var listaEmpleado = await apiServicio.Listar<Empleado>(new Uri(WebApp.BaseAddress), "/api/Empleado/ListarEmpleados");
                var tlistaEmpleado = listaEmpleado.Select(c => new { IdEmpleado = c.IdEmpleado, NombreApellidos = String.Format("{0} {1}", c.Persona.Nombres, c.Persona.Apellidos) });
                ViewData["Empleado"] = new SelectList(tlistaEmpleado, "IdEmpleado", "NombreApellidos");

                ViewData["Marca"] = new SelectList(listaMarca, "IdMarca", "Nombre");
                ViewData["Modelo"] = await ObtenerSelectListModelo(recepcionActivoFijoDetalle?.ActivoFijo?.Modelo?.Marca?.IdMarca ?? -1);
                ViewData["UnidadMedida"] = new SelectList(await apiServicio.Listar<UnidadMedida>(new Uri(WebApp.BaseAddress), "/api/UnidadMedida/ListarUnidadMedida"), "IdUnidadMedida", "Nombre");

                return View(recepcionActivoFijoDetalle);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Creando Activo Fijo",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });

                return BadRequest();
            }
        }

        public async Task<IActionResult> ActivosFijosRecepcionados()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/RecepcionActivoFijo/ListarRecepcionActivoFijo");

                var listaActivosFijosRecepcionados = lista.Where(c => c.Estado.Nombre == "Recepcionado").ToList();
                return View(listaActivosFijosRecepcionados);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando activos fijos con estado Recepcionado",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        #region AJAX_ClaseActivoFijo
        public async Task<SelectList> ObtenerSelectListClaseActivoFijo(int idTipoActivoFijo)
        {
            try
            {
                var listaClaseActivoFijo = await apiServicio.Listar<ClaseActivoFijo>(new Uri(WebApp.BaseAddress), "/api/ClaseActivoFijo/ListarClaseActivoFijo");
                listaClaseActivoFijo = idTipoActivoFijo != -1 ? listaClaseActivoFijo.Where(c => c.IdTipoActivoFijo == idTipoActivoFijo).ToList() : new List<ClaseActivoFijo>();
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
                var listaSubClaseActivoFijo = await apiServicio.Listar<SubClaseActivoFijo>(new Uri(WebApp.BaseAddress), "/api/SubClaseActivoFijo/ListarSubClasesActivoFijo");
                listaSubClaseActivoFijo = idClaseActivoFijo != -1 ? listaSubClaseActivoFijo.Where(c => c.IdClaseActivoFijo == idClaseActivoFijo).ToList() : new List<SubClaseActivoFijo>();
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
                var listaModelo = await apiServicio.Listar<Modelo>(new Uri(WebApp.BaseAddress), "/api/Modelo/ListarModelos");
                listaModelo = idMarca != -1 ? listaModelo.Where(c => c.IdMarca == idMarca).ToList() : new List<Modelo>();
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
    }
}