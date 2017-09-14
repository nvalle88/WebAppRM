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
    public class DetalleFacturaController : Controller
    {
        private readonly IApiServicio apiServicio;

        public DetalleFacturaController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        public async Task<IActionResult> Index()
        {
            var lista = new List<DetalleFactura>();
            try
            {
                lista = await apiServicio.Listar<DetalleFactura>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/DetalleFactura/ListarDetallesFactura");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando detalles de factura",
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
            ViewData["IdFactura"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<Factura>(new Uri(WebApp.BaseAddress), "/api/Factura/ListarFacturas"), "IdFactura", "Numero");
            ViewData["IdArticulo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddress), "/api/Articulo/ListarArticulos"), "IdArticulo", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DetalleFactura detalleFactura)
        {
            Response response = new Response();
            try
            {
                response = await apiServicio.InsertarAsync(detalleFactura,
                                                             new Uri(WebApp.BaseAddress),
                                                             "/api/DetalleFactura/InsertarDetalleFactura");
                if (response.IsSuccess)
                {

                    var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        ExceptionTrace = null,
                        Message = "Se ha creado un detalle de factura",
                        UserName = "Usuario 1",
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        EntityID = string.Format("{0} {1}", "Detalle de Factura:", detalleFactura.IdDetalleFactura),
                    });

                    return RedirectToAction("Index");
                }

                ViewData["Error"] = response.Message;
                ViewData["IdFactura"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<Factura>(new Uri(WebApp.BaseAddress), "/api/Factura/ListarFacturas"), "IdFactura", "Numero");
                ViewData["IdArticulo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddress), "/api/Articulo/ListarArticulos"), "IdArticulo", "Nombre");
                return View(detalleFactura);

            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Creando Detalle de factura",
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
                                                                  "/api/DetalleFactura");


                    respuesta.Resultado = JsonConvert.DeserializeObject<DetalleFactura>(respuesta.Resultado.ToString());
                    ViewData["IdFactura"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<Factura>(new Uri(WebApp.BaseAddress), "/api/Factura/ListarFacturas"), "IdFactura", "Numero");
                    ViewData["IdArticulo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddress), "/api/Articulo/ListarArticulos"), "IdArticulo", "Nombre");
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
        public async Task<IActionResult> Edit(string id, DetalleFactura detalleFactura)
        {
            Response response = new Response();
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    response = await apiServicio.EditarAsync(id, detalleFactura, new Uri(WebApp.BaseAddress),
                                                                 "/api/DetalleFactura");

                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                        {
                            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                            EntityID = string.Format("{0} : {1}", "Detalle de Factura", id),
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                            Message = "Se ha actualizado un detalle de factura",
                            UserName = "Usuario 1"
                        });

                        return RedirectToAction("Index");
                    }

                }
                ViewData["Error"] = response.Message;
                ViewData["IdFactura"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<Factura>(new Uri(WebApp.BaseAddress), "/api/Factura/ListarFacturas"), "IdFactura", "Numero");
                ViewData["IdArticulo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddress), "/api/Articulo/ListarArticulos"), "IdArticulo", "Nombre");
                return View(detalleFactura);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Editando un detalle de factura",
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
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddress)
                                                               , "/api/DetalleFactura");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        EntityID = string.Format("{0} : {1}", "Detalle de Factura", id),
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
                    Message = "Eliminar Detalle de Factura",
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