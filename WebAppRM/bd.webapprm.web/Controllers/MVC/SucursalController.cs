using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using bd.webapprm.servicios.Interfaces;
using bd.webapprm.entidades.Negocio;
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
    public class SucursalController : Controller
    {
        private readonly IApiServicio apiServicio;

        public SucursalController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        private async Task<Sucursal> ObtenerSucursalValidacion(Sucursal sucursal)
        {
            try
            {
                if (sucursal.IdCiudad == 0)
                {
                    try
                    {
                        sucursal.Ciudad = new Ciudad();
                        int IdProvincia = int.Parse(Request.Form["Ciudad.IdProvincia"].ToString());

                        var listaProvincia = await apiServicio.Listar<Provincia>(new Uri(WebApp.BaseAddress), "/api/Provincia/ListarProvincias");
                        sucursal.Ciudad.Provincia = listaProvincia.SingleOrDefault(c => c.IdProvincia == IdProvincia);
                        sucursal.Ciudad.IdProvincia = IdProvincia;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            sucursal.Ciudad.Provincia.Pais = new Pais();
                            int IdPais = int.Parse(Request.Form["Ciudad.Provincia.IdPais"].ToString());

                            var listaPais = await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddress), "/api/Pais/ListarPaises");
                            sucursal.Ciudad.Provincia.Pais = listaPais.SingleOrDefault(c => c.IdPais == IdPais);
                            sucursal.Ciudad.Provincia.IdPais = IdPais;
                        }
                        catch (Exception)
                        { }
                    }
                }
                else
                {
                    var listaCiudad = await apiServicio.Listar<Ciudad>(new Uri(WebApp.BaseAddress), "/api/Ciudad/ListarCiudades");
                    sucursal.Ciudad = listaCiudad.SingleOrDefault(c => c.IdCiudad == sucursal.IdCiudad);
                }
            }
            catch (Exception)
            { }
            
            return sucursal;
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddress), "/api/Pais/ListarPaises"), "IdPais", "Nombre");
            ViewData["Provincia"] = await ObtenerSelectListProvincia((ViewData["Pais"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Pais"] as SelectList).FirstOrDefault().Value) : -1);
            ViewData["Ciudad"] = await ObtenerSelectListCiudad((ViewData["Provincia"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Provincia"] as SelectList).FirstOrDefault().Value) : -1);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sucursal sucursal)
        {
            Response response = new Response();
            try
            {
                sucursal = await ObtenerSucursalValidacion(sucursal);
                response = await apiServicio.InsertarAsync(sucursal,
                                                             new Uri(WebApp.BaseAddress),
                                                             "/api/Sucursal/InsertarSucursal");
                if (response.IsSuccess)
                {

                    var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        ExceptionTrace = null,
                        Message = "Se ha creado una Sucursal",
                        UserName = "Usuario 1",
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        EntityID = string.Format("{0} {1}", "Sucursal:", sucursal.IdSucursal),
                    });
                    return RedirectToAction("Index");
                }

                ViewData["Error"] = response.Message;
                ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddress), "/api/Pais/ListarPaises"), "IdPais", "Nombre");
                ViewData["Provincia"] = await ObtenerSelectListProvincia(sucursal.Ciudad?.Provincia?.IdPais ?? -1);
                ViewData["Ciudad"] = await ObtenerSelectListCiudad(sucursal?.Ciudad?.IdProvincia ?? -1);
                return View(sucursal);

            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Creando Sucursal",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppRM"
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
                                                                  "/api/Sucursal");


                    respuesta.Resultado = JsonConvert.DeserializeObject<Sucursal>(respuesta.Resultado.ToString());
                    if (respuesta.IsSuccess)
                    {
                        Sucursal sucursal = respuesta.Resultado as Sucursal;
                        ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddress), "/api/Pais/ListarPaises"), "IdPais", "Nombre");
                        ViewData["Provincia"] = await ObtenerSelectListProvincia(sucursal.Ciudad?.Provincia?.IdPais ?? -1);
                        ViewData["Ciudad"] = await ObtenerSelectListCiudad(sucursal?.Ciudad?.IdProvincia ?? -1);
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
        public async Task<IActionResult> Edit(string id, Sucursal sucursal)
        {
            Response response = new Response();
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    sucursal = await ObtenerSucursalValidacion(sucursal);
                    response = await apiServicio.EditarAsync(id, sucursal, new Uri(WebApp.BaseAddress),
                                                                 "/api/Sucursal");

                    if (response.IsSuccess)
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

                }
                ViewData["Error"] = response.Message;
                ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddress), "/api/Pais/ListarPaises"), "IdPais", "Nombre");
                ViewData["Provincia"] = await ObtenerSelectListProvincia(sucursal.Ciudad?.Provincia?.IdPais ?? -1);
                ViewData["Ciudad"] = await ObtenerSelectListCiudad(sucursal?.Ciudad?.IdProvincia ?? -1);
                return View(sucursal);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Editando una Sucursal",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppRM"
                });

                return BadRequest();
            }
        }

        public async Task<IActionResult> Index()
        {
            var lista = new List<Sucursal>();
            try
            {
                lista = await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/Sucursal/ListarSucursales");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando Sucursal",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppRM"
                });
                return BadRequest();
            }
        }

        public async Task<IActionResult> Delete(string id)
        {

            try
            {
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddress)
                                                               , "/api/Sucursal");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        EntityID = string.Format("{0} : {1}", "Sistema", id),
                        Message = "Registro eliminado",
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        UserName = "Usuario APP WebAppRM"
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
                    Message = "Eliminar Sucursal",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppRM"
                });

                return BadRequest();
            }
        }

        #region AJAX_Provincia
        public async Task<SelectList> ObtenerSelectListProvincia(int idPais)
        {
            try
            {
                var listaProvincia = await apiServicio.Listar<Provincia>(new Uri(WebApp.BaseAddress), "/api/Provincia/ListarProvincias");
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
            return PartialView("_ProvinciaSelect", new Sucursal());
        }
        #endregion

        #region AJAX_Ciudad
        public async Task<SelectList> ObtenerSelectListCiudad(int idProvincia)
        {
            try
            {
                var listaCiudad = await apiServicio.Listar<Ciudad>(new Uri(WebApp.BaseAddress), "/api/Ciudad/ListarCiudades");
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
            return PartialView("_CiudadSelect", new Sucursal());
        }
        #endregion
    }
}