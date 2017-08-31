using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using bd.webapprm.servicios.Interfaces;
using bd.webapprm.entidades;
using bd.webapprm.entidades.Utils;
using bd.log.guardar.Servicios;
using bd.log.guardar.ObjectTranfer;
using bbd.webapprm.servicios.Enumeradores;
using bd.log.guardar.Enumeradores;
using Newtonsoft.Json;

namespace bd.webapprm.web.Controllers.MVC
{
    public class MaestroDetalleArticuloController : Controller
    {
        private readonly IApiServicio apiServicio;


        public MaestroDetalleArticuloController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        public async Task<IActionResult> Create()
        {
            ViewData["IdArticulo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddress), "/api/Articulo/ListarArticulos"), "IdArticulo", "Nombre");
            ViewData["Minimo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<MaestroArticuloSucursal>(new Uri(WebApp.BaseAddress), "/api/MaestroArticuloSucursal/ListarMaestroArticuloSucursal"), "IdMaestroArticuloSucursal", "Minimo");
            ViewData["Maximo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddress), "/api/MaestroArticuloSucursal/ListarMaestroArticuloSucursal"), "IdMaestroArticuloSucursal", "Maximo");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaestroDetalleArticulo maestroDetalleArticulo)
        {            
            Response response = new Response();
            try
            {
                response = await apiServicio.InsertarAsync(maestroDetalleArticulo,
                                                             new Uri(WebApp.BaseAddress),
                                                             "/api/MaestroDetalleArticulo/InsertarMaestroDetalleArticulo");
                if (response.IsSuccess)
                {

                    var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        ExceptionTrace = null,
                        Message = "Se ha creado un MaestroDetalleArticulo",
                        UserName = "Usuario 1",
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        EntityID = string.Format("{0} {1}", "MaestroDetalleArticulo:", maestroDetalleArticulo.IdMaestroDetalleArticulo),
                    });

                    return RedirectToAction("Index");
                }

                ViewData["Error"] = response.Message;
                return View(maestroDetalleArticulo);

            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Creando MaestroDetalleArticulo",
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
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddress),
                                                                  "/api/MaestroDetalleArticulo");


                    respuesta.Resultado = JsonConvert.DeserializeObject<MaestroDetalleArticulo>(respuesta.Resultado.ToString());
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
        public async Task<IActionResult> Edit(string id, MaestroDetalleArticulo maestroDetalleArticulo)
        {
            Response response = new Response();
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    response = await apiServicio.EditarAsync(id, maestroDetalleArticulo, new Uri(WebApp.BaseAddress),
                                                                 "/api/MaestroDetalleArticulo");

                    if (!response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                        {
                            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                            EntityID = string.Format("{0} : {1}", "Sistema", id),
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                            Message = "Se ha actualizado un registro sistema",
                            UserName = "Usuario 1"
                        });

                        return RedirectToAction("Index");
                    }
                    ViewData["Error"] = response.Message;
                    return View(maestroDetalleArticulo);

                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Editando una MaestroDetalleArticulo",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });

                return BadRequest();
            }
        }

        public async Task<IActionResult> Index()
        {

            var lista = new List<MaestroDetalleArticulo>();
            try
            {
                lista = await apiServicio.Listar<MaestroDetalleArticulo>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/MaestroDetalleArticulo/ListarMaestroDetalleArticulo");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando MaestroDetalleArticulo",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
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
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddress)
                                                               , "/api/MaestroDetalleArticulo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        EntityID = string.Format("{0} : {1}", "Sistema", id),
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
                    Message = "Eliminar MaestroDetalleArticulo",
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