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

namespace bd.webapprm.web.Controllers.MVC
{
    public class ClaseActivoFijoController : Controller
    {
        private readonly IApiServicio apiServicio;

        public ClaseActivoFijoController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        public async Task<IActionResult> Index()
        {
            var lista = new List<ClaseActivoFijo>();
            try
            {
                lista = await apiServicio.Listar<ClaseActivoFijo>(new Uri(WebApp.BaseAddressRM)
                                                                    , "/api/ClaseActivoFijo/ListarClaseActivoFijo");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando clase de activo fijo",
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
            ViewData["IdTipoActivoFijo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "/api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
            ViewData["IdTablaDepreciacion"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<TablaDepreciacion>(new Uri(WebApp.BaseAddressRM), "/api/TablaDepreciacion/ListarTablaDepreciacion"), "IdTablaDepreciacion", "IndiceDepreciacion");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClaseActivoFijo claseActivoFijo)
        {
            Response response = new Response();
            try
            {
                response = await apiServicio.InsertarAsync(claseActivoFijo,
                                                             new Uri(WebApp.BaseAddressRM),
                                                             "/api/ClaseActivoFijo/InsertarClaseActivoFijo");
                if (response.IsSuccess)
                {

                    var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        ExceptionTrace = null,
                        Message = "Se ha creado una clase de activo fijo",
                        UserName = "Usuario 1",
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        EntityID = string.Format("{0} {1}", "Clase de Activo Fijo:", claseActivoFijo.IdClaseActivoFijo),
                    });

                    return RedirectToAction("Index");
                }

                ViewData["Error"] = response.Message;
                ViewData["IdTipoActivoFijo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "/api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                ViewData["IdTablaDepreciacion"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<TablaDepreciacion>(new Uri(WebApp.BaseAddressRM), "/api/TablaDepreciacion/ListarTablaDepreciacion"), "IdTablaDepreciacion", "IndiceDepreciacion");
                return View(claseActivoFijo);

            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Creando una Clase de Activo Fijo",
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
                                                                  "/api/ClaseActivoFijo");


                    respuesta.Resultado = JsonConvert.DeserializeObject<ClaseActivoFijo>(respuesta.Resultado.ToString());
                    ViewData["IdTipoActivoFijo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "/api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                    ViewData["IdTablaDepreciacion"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<TablaDepreciacion>(new Uri(WebApp.BaseAddressRM), "/api/TablaDepreciacion/ListarTablaDepreciacion"), "IdTablaDepreciacion", "IndiceDepreciacion");
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
        public async Task<IActionResult> Edit(string id, ClaseActivoFijo claseActivoFijo)
        {
            Response response = new Response();
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    response = await apiServicio.EditarAsync(id, claseActivoFijo, new Uri(WebApp.BaseAddressRM),
                                                                 "/api/ClaseActivoFijo");

                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                        {
                            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                            EntityID = string.Format("{0} : {1}", "Clase de Activo Fijo", id),
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                            Message = "Se ha actualizado un registro clase de activo fijo",
                            UserName = "Usuario 1"
                        });

                        return RedirectToAction("Index");
                    }

                }
                ViewData["Error"] = response.Message;
                ViewData["IdTipoActivoFijo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "/api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                ViewData["IdTablaDepreciacion"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<TablaDepreciacion>(new Uri(WebApp.BaseAddressRM), "/api/TablaDepreciacion/ListarTablaDepreciacion"), "IdTablaDepreciacion", "IndiceDepreciacion");
                return View(claseActivoFijo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Editando una clase de activo fijo",
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
                                                               , "/api/ClaseActivoFijo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        EntityID = string.Format("{0} : {1}", "Clase de Activo Fijo", id),
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
                    Message = "Eliminar Clase de Activo Fijo",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });

                return BadRequest();
            }
        }
    }
}