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
    public class MaestroArticuloSucursalController : Controller
    {
        private readonly IApiServicio apiServicio;

        public MaestroArticuloSucursalController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        public async Task<IActionResult> Index()
        {
            var lista = new List<MaestroArticuloSucursal>();
            try
            {
                lista = await apiServicio.Listar<MaestroArticuloSucursal>(new Uri(WebApp.BaseAddressRM)
                                                                    , "api/MaestroArticuloSucursal/ListarMaestroArticuloSucursal");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando maestros de artículos de sucursal",
                    ExceptionTrace = ex.Message,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "api/Pais/ListarPais"), "IdPais", "Nombre");
            ViewData["Provincia"] = await ObtenerSelectListProvincia((ViewData["Pais"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Pais"] as SelectList).FirstOrDefault().Value) : -1);
            ViewData["Ciudad"] = await ObtenerSelectListCiudad((ViewData["Provincia"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Provincia"] as SelectList).FirstOrDefault().Value) : -1);
            ViewData["Sucursal"] = await ObtenerSelectListSucursal((ViewData["Ciudad"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Ciudad"] as SelectList).FirstOrDefault().Value) : -1);
            return View();
        }

        private async Task<MaestroArticuloSucursal> ObtenerMaestroArticuloSucursalValidacion(MaestroArticuloSucursal maestroArticuloSucursal)
        {
            try
            {
                if (maestroArticuloSucursal.IdSucursal == 0)
                {
                    try
                    {
                        maestroArticuloSucursal.Sucursal = new Sucursal();
                        int IdCiudad = int.Parse(Request.Form["Sucursal.IdCiudad"].ToString());

                        var listaCiudad = await apiServicio.Listar<Ciudad>(new Uri(WebApp.BaseAddressTH), "api/Ciudad/ListarCiudad");
                        maestroArticuloSucursal.Sucursal.Ciudad = listaCiudad.SingleOrDefault(c => c.IdCiudad == IdCiudad);
                        maestroArticuloSucursal.Sucursal.IdCiudad = IdCiudad;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            maestroArticuloSucursal.Sucursal.Ciudad = new Ciudad();
                            int IdProvincia = int.Parse(Request.Form["Sucursal.Ciudad.IdProvincia"].ToString());

                            var listaProvincia = await apiServicio.Listar<Provincia>(new Uri(WebApp.BaseAddressTH), "api/Provincia/ListarProvincia");
                            maestroArticuloSucursal.Sucursal.Ciudad.Provincia = listaProvincia.SingleOrDefault(c => c.IdProvincia == IdProvincia);
                            maestroArticuloSucursal.Sucursal.Ciudad.IdProvincia = IdProvincia;
                        }
                        catch (Exception)
                        {
                            try
                            {
                                maestroArticuloSucursal.Sucursal.Ciudad.Provincia = new Provincia();
                                int IdPais = int.Parse(Request.Form["Sucursal.Ciudad.Provincia.IdPais"].ToString());

                                var listaPais = await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "api/Pais/ListarPais");
                                maestroArticuloSucursal.Sucursal.Ciudad.Provincia.Pais = listaPais.SingleOrDefault(c => c.IdPais == IdPais);
                                maestroArticuloSucursal.Sucursal.Ciudad.Provincia.IdPais = IdPais;
                            }
                            catch (Exception)
                            { }
                        }
                    }
                }
                else
                {
                    var listaSucursal = await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal");
                    maestroArticuloSucursal.Sucursal = listaSucursal.SingleOrDefault(c => c.IdSucursal == maestroArticuloSucursal.IdSucursal);
                }
            }
            catch (Exception)
            { }
            return maestroArticuloSucursal;
        }
        private bool ValidacionMinMax(MaestroArticuloSucursal maestroArticuloSucursal, Response response)
        {
            if (maestroArticuloSucursal.Minimo > 0 && maestroArticuloSucursal.Maximo > 0)
            {
                if (maestroArticuloSucursal.Minimo > maestroArticuloSucursal.Maximo)
                {
                    ModelState.AddModelError("Minimo", "El Mínimo no puede ser mayor que el Máximo");
                    response.Message = "El Módelo es inválido";
                    return false;
                }
                return true;
            }
            return false;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaestroArticuloSucursal maestroArticuloSucursal)
        {
            Response response = new Response();
            try
            {
                maestroArticuloSucursal = await ObtenerMaestroArticuloSucursalValidacion(maestroArticuloSucursal);
                TryValidateModel(maestroArticuloSucursal);

                if (ValidacionMinMax(maestroArticuloSucursal, response))
                {
                    response = await apiServicio.InsertarAsync(maestroArticuloSucursal,
                                                             new Uri(WebApp.BaseAddressRM),
                                                             "api/MaestroArticuloSucursal/InsertarMaestroArticuloSucursal");
                    if (response.IsSuccess)
                    {

                        var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                        {
                            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                            ExceptionTrace = null,
                            Message = "Se ha creado un maestro artículo de sucursal",
                            UserName = "Usuario 1",
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                            EntityID = string.Format("{0} {1}", "Maestro Artículo de Sucursal:", maestroArticuloSucursal.IdMaestroArticuloSucursal),
                        });

                        return RedirectToAction("Index");
                    }
                }

                ViewData["Error"] = response.Message;
                ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "api/Pais/ListarPais"), "IdPais", "Nombre");
                ViewData["Provincia"] = await ObtenerSelectListProvincia(maestroArticuloSucursal?.Sucursal.Ciudad?.Provincia?.IdPais ?? -1);
                ViewData["Ciudad"] = await ObtenerSelectListCiudad(maestroArticuloSucursal?.Sucursal?.Ciudad?.IdProvincia ?? -1);
                ViewData["Sucursal"] = await ObtenerSelectListSucursal(maestroArticuloSucursal?.Sucursal?.IdCiudad ?? -1);
                return View(maestroArticuloSucursal);

            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Creando Maestro Artículo de Sucursal",
                    ExceptionTrace = ex.Message,
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
                                                                  "api/MaestroArticuloSucursal");


                    respuesta.Resultado = JsonConvert.DeserializeObject<MaestroArticuloSucursal>(respuesta.Resultado.ToString());
                    if (respuesta.IsSuccess)
                    {
                        MaestroArticuloSucursal maestroArticuloSucursal = respuesta.Resultado as MaestroArticuloSucursal;
                        ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "api/Pais/ListarPais"), "IdPais", "Nombre");
                        ViewData["Provincia"] = await ObtenerSelectListProvincia(maestroArticuloSucursal?.Sucursal.Ciudad?.Provincia?.IdPais ?? -1);
                        ViewData["Ciudad"] = await ObtenerSelectListCiudad(maestroArticuloSucursal?.Sucursal?.Ciudad?.IdProvincia ?? -1);
                        ViewData["Sucursal"] = await ObtenerSelectListSucursal(maestroArticuloSucursal?.Sucursal?.IdCiudad ?? -1);
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
        public async Task<IActionResult> Edit(string id, MaestroArticuloSucursal maestroArticuloSucursal)
        {
            Response response = new Response();
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    maestroArticuloSucursal = await ObtenerMaestroArticuloSucursalValidacion(maestroArticuloSucursal);
                    TryValidateModel(maestroArticuloSucursal);

                    if (ValidacionMinMax(maestroArticuloSucursal, response))
                    {
                        response = await apiServicio.EditarAsync(id, maestroArticuloSucursal, new Uri(WebApp.BaseAddressRM),
                                                                 "api/MaestroArticuloSucursal");

                        if (response.IsSuccess)
                        {
                            await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                            {
                                ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                                EntityID = string.Format("{0} : {1}", "Maestro Artículo de Sucursal", id),
                                LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit),
                                LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                                Message = "Se ha actualizado un registro maestro artículo de sucursal",
                                UserName = "Usuario 1"
                            });

                            return RedirectToAction("Index");
                        }
                    }
                }
                ViewData["Error"] = response.Message;
                ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "api/Pais/ListarPais"), "IdPais", "Nombre");
                ViewData["Provincia"] = await ObtenerSelectListProvincia(maestroArticuloSucursal?.Sucursal.Ciudad?.Provincia?.IdPais ?? -1);
                ViewData["Ciudad"] = await ObtenerSelectListCiudad(maestroArticuloSucursal?.Sucursal?.Ciudad?.IdProvincia ?? -1);
                ViewData["Sucursal"] = await ObtenerSelectListSucursal(maestroArticuloSucursal?.Sucursal?.IdCiudad ?? -1);
                return View(maestroArticuloSucursal);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Editando un maestro artículo de sucursal",
                    ExceptionTrace = ex.Message,
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
                                                               , "api/MaestroArticuloSucursal");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        EntityID = string.Format("{0} : {1}", "Maestro Artículo de Sucursal", id),
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
                    Message = "Eliminar Maestro Artículo de Sucursal",
                    ExceptionTrace = ex.Message,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });

                return BadRequest();
            }
        }

        #region AJAX_Provincia
        public async Task<SelectList> ObtenerSelectListProvincia(int idPais)
        {
            try
            {
                var listaProvincia = await apiServicio.Listar<Provincia>(new Uri(WebApp.BaseAddressTH), "api/Provincia/ListarProvincia");
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
            return PartialView("_ProvinciaSelect", new MaestroArticuloSucursal());
        }
        #endregion

        #region AJAX_Ciudad
        public async Task<SelectList> ObtenerSelectListCiudad(int idProvincia)
        {
            try
            {
                var listaCiudad = await apiServicio.Listar<Ciudad>(new Uri(WebApp.BaseAddressTH), "api/Ciudad/ListarCiudad");
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
            return PartialView("_CiudadSelect", new MaestroArticuloSucursal());
        }
        #endregion

        #region AJAX_Sucursal
        public async Task<SelectList> ObtenerSelectListSucursal(int idCiudad)
        {
            try
            {
                var listaSucursal = await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal");
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
            return PartialView("_SucursalSelect", new MaestroArticuloSucursal());
        }
        #endregion
    }
}