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
    public class SubClaseActivoFijoController : Controller
    {
        private readonly IApiServicio apiServicio;

        public SubClaseActivoFijoController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        public async Task<IActionResult> Index()
        {
            var lista = new List<SubClaseActivoFijo>();
            try
            {
                lista = await apiServicio.Listar<SubClaseActivoFijo>(new Uri(WebApp.BaseAddressRM)
                                                                    , "/api/SubClaseActivoFijo/ListarSubClasesActivoFijo");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando sub clases de activo fijo",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        public async Task<IActionResult> Create()
        {
            ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "/api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
            ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo((ViewData["TipoActivoFijo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["TipoActivoFijo"] as SelectList).FirstOrDefault().Value) : -1);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubClaseActivoFijo subClaseActivoFijo)
        {
            Response response = new Response();
            try
            {
                var listaClaseActivoFijo = await apiServicio.Listar<ClaseActivoFijo>(new Uri(WebApp.BaseAddressRM), "/api/ClaseActivoFijo/ListarClaseActivoFijo");
                subClaseActivoFijo.ClaseActivoFijo = listaClaseActivoFijo.SingleOrDefault(c => c.IdClaseActivoFijo == subClaseActivoFijo.IdClaseActivoFijo);
                response = await apiServicio.InsertarAsync(subClaseActivoFijo,
                                                             new Uri(WebApp.BaseAddressRM),
                                                             "/api/SubClaseActivoFijo/InsertarSubClaseActivoFijo");
                if (response.IsSuccess)
                {

                    var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        ExceptionTrace = null,
                        Message = "Se ha creado una sub clase de activo fijo",
                        UserName = "Usuario 1",
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        EntityID = string.Format("{0} {1}", "Sub Clase de Activo Fijo:", subClaseActivoFijo.IdClaseActivoFijo),
                    });

                    return RedirectToAction("Index");
                }

                ViewData["Error"] = response.Message;
                ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "/api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo(subClaseActivoFijo?.ClaseActivoFijo?.TipoActivoFijo?.IdTipoActivoFijo ?? -1);
                return View(subClaseActivoFijo);

            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Creando una Sub Clase de Activo Fijo",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });

                return BadRequest();
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM),
                                                                  "/api/SubClaseActivoFijo");


                    respuesta.Resultado = JsonConvert.DeserializeObject<SubClaseActivoFijo>(respuesta.Resultado.ToString());
                    ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "/api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                    ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo((respuesta.Resultado as SubClaseActivoFijo).ClaseActivoFijo.IdTipoActivoFijo);
                    if (respuesta.IsSuccess)
                    {
                        return View(respuesta.Resultado);
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
        public async Task<IActionResult> Edit(string id, SubClaseActivoFijo subClaseActivoFijo)
        {
            Response response = new Response();
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var listaClaseActivoFijo = await apiServicio.Listar<ClaseActivoFijo>(new Uri(WebApp.BaseAddressRM), "/api/ClaseActivoFijo/ListarClaseActivoFijo");
                    subClaseActivoFijo.ClaseActivoFijo = listaClaseActivoFijo.SingleOrDefault(c => c.IdClaseActivoFijo == subClaseActivoFijo.IdClaseActivoFijo);
                    response = await apiServicio.EditarAsync(id, subClaseActivoFijo, new Uri(WebApp.BaseAddressRM),
                                                                 "/api/SubClaseActivoFijo");

                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                        {
                            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                            EntityID = string.Format("{0} : {1}", "Sub Clase de Activo Fijo", id),
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                            Message = "Se ha actualizado un registro sub clase de activo fijo",
                            UserName = "Usuario 1"
                        });

                        return RedirectToAction("Index");
                    }
                }
                ViewData["Error"] = response.Message;
                ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "/api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo(subClaseActivoFijo?.ClaseActivoFijo?.TipoActivoFijo?.IdTipoActivoFijo ?? -1);
                return View(subClaseActivoFijo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Editando una sub clase de activo fijo",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });

                return BadRequest();
            }
        }

        public async Task<IActionResult> Delete(string id)
        {

            try
            {
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddressRM)
                                                               , "/api/SubClaseActivoFijo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        EntityID = string.Format("{0} : {1}", "Sub Clase de Activo Fijo", id),
                        Message = "Registro eliminado",
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        UserName = "Usuario APP webappth"
                    });
                    return RedirectToAction("Index");
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Eliminar Sub Clase de Activo Fijo",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });

                return BadRequest();
            }
        }

        public async Task<SelectList> ObtenerSelectListClaseActivoFijo(int idTipoActivoFijo)
        {
            try
            {
                var listaClaseActivoFijo = await apiServicio.Listar<ClaseActivoFijo>(new Uri(WebApp.BaseAddressRM), "/api/ClaseActivoFijo/ListarClaseActivoFijo");
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
            return PartialView("_ClaseActivoFijoSelect", new ClaseActivoFijo());
        }
    }
}