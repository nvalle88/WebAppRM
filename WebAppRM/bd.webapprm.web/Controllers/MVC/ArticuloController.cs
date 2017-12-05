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
    public class ArticuloController : Controller
    {
        private readonly IApiServicio apiServicio;

        public ArticuloController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        public async Task<IActionResult> Index()
        {
            var lista = new List<Articulo>();
            try
            {
                lista = await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddressRM)
                                                                    , "/api/Articulo/ListarArticulos");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando artículos",
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
            ViewData["IdSubClaseArticulo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<SubClaseArticulo>(new Uri(WebApp.BaseAddressRM), "api/SubClaseArticulo/ListarSubClaseArticulos"), "IdSubClaseArticulo", "Nombre");
            ViewData["IdUnidadMedida"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<UnidadMedida>(new Uri(WebApp.BaseAddressRM), "api/UnidadMedida/ListarUnidadMedida"), "IdUnidadMedida", "Nombre");
            ViewData["IdModelo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<Modelo>(new Uri(WebApp.BaseAddressRM), "api/Modelo/ListarModelos"), "IdModelo", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Articulo articulo)
        {
            Response response = new Response();
            try
            {
                response = await apiServicio.InsertarAsync(articulo,
                                                             new Uri(WebApp.BaseAddressRM),
                                                             "api/Articulo/InsertarArticulo");
                if (response.IsSuccess)
                {

                    var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        ExceptionTrace = null,
                        Message = "Se ha creado un artículo",
                        UserName = "Usuario 1",
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        EntityID = string.Format("{0} {1}", "Artículo:", articulo.IdArticulo),
                    });

                    return RedirectToAction("Index");
                }

                ViewData["Error"] = response.Message;
                ViewData["IdSubClaseArticulo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<SubClaseArticulo>(new Uri(WebApp.BaseAddressRM), "api/SubClaseArticulo/ListarSubClaseArticulos"), "IdSubClaseArticulo", "Nombre");
                ViewData["IdUnidadMedida"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<UnidadMedida>(new Uri(WebApp.BaseAddressRM), "api/UnidadMedida/ListarUnidadMedida"), "IdUnidadMedida", "Nombre");
                ViewData["IdModelo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<Modelo>(new Uri(WebApp.BaseAddressRM), "api/Modelo/ListarModelos"), "IdModelo", "Nombre");
                return View(articulo);

            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Creando Artículo",
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
                                                                  "api/Articulo");


                    respuesta.Resultado = JsonConvert.DeserializeObject<Articulo>(respuesta.Resultado.ToString());
                    ViewData["IdSubClaseArticulo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<SubClaseArticulo>(new Uri(WebApp.BaseAddressRM), "api/SubClaseArticulo/ListarSubClaseArticulos"), "IdSubClaseArticulo", "Nombre");
                    ViewData["IdUnidadMedida"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<UnidadMedida>(new Uri(WebApp.BaseAddressRM), "api/UnidadMedida/ListarUnidadMedida"), "IdUnidadMedida", "Nombre");
                    ViewData["IdModelo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<Modelo>(new Uri(WebApp.BaseAddressRM), "api/Modelo/ListarModelos"), "IdModelo", "Nombre");
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
        public async Task<IActionResult> Edit(string id, Articulo articulo)
        {
            Response response = new Response();
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    response = await apiServicio.EditarAsync(id, articulo, new Uri(WebApp.BaseAddressRM),
                                                                 "api/Articulo");

                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                        {
                            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                            EntityID = string.Format("{0} : {1}", "Artículo", id),
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                            Message = "Se ha actualizado un registro artículo",
                            UserName = "Usuario 1"
                        });

                        return RedirectToAction("Index");
                    }

                }
                ViewData["Error"] = response.Message;
                ViewData["IdSubClaseArticulo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<SubClaseArticulo>(new Uri(WebApp.BaseAddressRM), "api/SubClaseArticulo/ListarSubClaseArticulos"), "IdSubClaseArticulo", "Nombre");
                ViewData["IdUnidadMedida"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<UnidadMedida>(new Uri(WebApp.BaseAddressRM), "api/UnidadMedida/ListarUnidadMedida"), "IdUnidadMedida", "Nombre");
                ViewData["IdModelo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<Modelo>(new Uri(WebApp.BaseAddressRM), "api/Modelo/ListarModelos"), "IdModelo", "Nombre");
                return View(articulo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Editando un artículo",
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
                                                               , "api/Articulo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        EntityID = string.Format("{0} : {1}", "Artículo", id),
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
                    Message = "Eliminar Artículo",
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