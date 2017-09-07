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

        public async Task<IActionResult> Recepcion()
        {
            ViewData["TipoActivoFijo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddress), "/api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recepcion(ActivoFijo activoFijo)
        {
            Response response = new Response();
            try
            {
                response = await apiServicio.InsertarAsync(activoFijo,
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
                }

                ViewData["Error"] = response.Message;
                return View(activoFijo);

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

        [HttpPost]
        public async Task<IActionResult> ClaseActivoFijo_SelectResult(int idTipoActivoFijo)
        {
            var listaClaseActivoFijo = await apiServicio.Listar<ClaseActivoFijo>(new Uri(WebApp.BaseAddress), "/api/ClaseActivoFijo/ListarClaseActivoFijo");
            listaClaseActivoFijo = idTipoActivoFijo != -1 ? listaClaseActivoFijo.Where(c => c.IdTipoActivoFijo == idTipoActivoFijo).ToList() : new List<ClaseActivoFijo>();
            ViewBag.ClaseActivoFijo = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(listaClaseActivoFijo, "IdClaseActivoFijo", "Nombre");
            return PartialView("_ClaseActivoFijoSelect", new ActivoFijo());
        }

        [HttpPost]
        public async Task<IActionResult> SubClaseActivoFijo_SelectResult(int idClaseActivoFijo)
        {
            var listaSubClaseActivoFijo = await apiServicio.Listar<SubClaseActivoFijo>(new Uri(WebApp.BaseAddress), "/api/SubClaseActivoFijo/ListarSubClasesActivoFijo");
            listaSubClaseActivoFijo = idClaseActivoFijo != -1 ? listaSubClaseActivoFijo.Where(c => c.IdClaseActivoFijo == idClaseActivoFijo).ToList() : new List<SubClaseActivoFijo>();
            ViewBag.SubClaseActivoFijo = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(listaSubClaseActivoFijo, "IdSubClaseActivoFijo", "Nombre");
            return PartialView("_SubClaseActivoFijoSelect", new ActivoFijo());
        }
    }
}