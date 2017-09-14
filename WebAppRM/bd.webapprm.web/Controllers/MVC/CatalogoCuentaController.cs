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
    public class CatalogoCuentaController : Controller
    {
        private readonly IApiServicio apiServicio;

        public CatalogoCuentaController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        public async Task<IActionResult> Index()
        {
            var lista = new List<CatalogoCuenta>();
            try
            {
                lista = await apiServicio.Listar<CatalogoCuenta>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/CatalogoCuenta/ListarCatalogosCuenta");
                foreach (var item in lista)
                {
                    try
                    {
                        item.CatalogoCuentaReferencia = lista.SingleOrDefault(c => c.IdCatalogoCuenta == item.IdCatalogoCuentaHijo);
                    }
                    catch (Exception)
                    { }
                }

                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando catálogos de cuenta",
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
            var lista_catalogos_cuenta = await apiServicio.Listar<CatalogoCuenta>(new Uri(WebApp.BaseAddress), "/api/CatalogoCuenta/ListarCatalogosCuenta");
            ViewData["IdCatalogoCuentaHijoVisible"] = lista_catalogos_cuenta.Count > 0;
            ViewData["IdCatalogoCuentaHijo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(lista_catalogos_cuenta, "IdCatalogoCuenta", "Codigo");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CatalogoCuenta catalogoCuenta)
        {
            Response response = new Response();
            try
            {
                var lista_catalogos_cuenta = await apiServicio.Listar<CatalogoCuenta>(new Uri(WebApp.BaseAddress), "/api/CatalogoCuenta/ListarCatalogosCuenta");
                response = await apiServicio.InsertarAsync(catalogoCuenta,
                                                             new Uri(WebApp.BaseAddress),
                                                             "/api/CatalogoCuenta/InsertarCatalogoCuenta");
                if (response.IsSuccess)
                {

                    var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        ExceptionTrace = null,
                        Message = "Se ha creado un catálogo de cuenta",
                        UserName = "Usuario 1",
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        EntityID = string.Format("{0} {1}", "Catálogo de Cuenta:", catalogoCuenta.IdCatalogoCuenta),
                    });

                    return RedirectToAction("Index");
                }

                ViewData["Error"] = response.Message;
                ViewData["IdCatalogoCuentaHijoVisible"] = lista_catalogos_cuenta.Count > 0;
                ViewData["IdCatalogoCuentaHijo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(lista_catalogos_cuenta, "IdCatalogoCuenta", "Codigo");
                return View(catalogoCuenta);

            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Creando Catálogo de Cuenta",
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
                                                                  "/api/CatalogoCuenta");


                    respuesta.Resultado = JsonConvert.DeserializeObject<CatalogoCuenta>(respuesta.Resultado.ToString());
                    ViewData["IdCatalogoCuentaHijo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<CatalogoCuenta>(new Uri(WebApp.BaseAddress), "/api/CatalogoCuenta/ListarCatalogosCuenta"), "IdCatalogoCuenta", "Codigo");
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
        public async Task<IActionResult> Edit(string id, CatalogoCuenta catalogoCuenta)
        {
            Response response = new Response();
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    response = await apiServicio.EditarAsync(id, catalogoCuenta, new Uri(WebApp.BaseAddress),
                                                                 "/api/CatalogoCuenta");

                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                        {
                            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                            EntityID = string.Format("{0} : {1}", "Catálogo de Cuenta", id),
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                            Message = "Se ha actualizado un registro catálogo de cuenta",
                            UserName = "Usuario 1"
                        });

                        return RedirectToAction("Index");
                    }

                }
                ViewData["Error"] = response.Message;
                ViewData["IdCatalogoCuentaHijo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<CatalogoCuenta>(new Uri(WebApp.BaseAddress), "/api/CatalogoCuenta/ListarCatalogosCuenta"), "IdCatalogoCuenta", "Codigo");
                return View(catalogoCuenta);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Editando un catálogo de cuenta",
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
                                                               , "/api/CatalogoCuenta");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        EntityID = string.Format("{0} : {1}", "Catálogo de Cuenta", id),
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
                    Message = "Eliminar Catálogo de Cuenta",
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