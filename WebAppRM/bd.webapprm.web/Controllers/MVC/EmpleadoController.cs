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
    public class EmpleadoController : Controller
    {
        private readonly IApiServicio apiServicio;

        public EmpleadoController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        public async Task<IActionResult> Index()
        {
            var lista = new List<Empleado>();
            try
            {
                lista = await apiServicio.Listar<Empleado>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/Empleado/ListarEmpleados");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando empleados",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        private async Task<Empleado> ObtenerCiudadValidacion(Empleado empleado)
        {
            try
            {
                if (empleado.IdCiudadLugarNacimiento == 0)
                {
                    try
                    {
                        empleado.CiudadNacimiento = new Ciudad();
                        int IdProvincia = int.Parse(Request.Form["CiudadNacimiento.IdProvincia"].ToString());

                        var listaProvincia = await apiServicio.Listar<Provincia>(new Uri(WebApp.BaseAddress), "/api/Provincia/ListarProvincias");
                        empleado.CiudadNacimiento.Provincia = listaProvincia.SingleOrDefault(c => c.IdProvincia == IdProvincia);
                        empleado.CiudadNacimiento.IdProvincia = IdProvincia;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            empleado.CiudadNacimiento.Provincia.Pais = new Pais();
                            int IdPais = int.Parse(Request.Form["CiudadNacimiento.Provincia.IdPais"].ToString());

                            var listaPais = await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddress), "/api/Pais/ListarPaises");
                            empleado.CiudadNacimiento.Provincia.Pais = listaPais.SingleOrDefault(c => c.IdPais == IdPais);
                            empleado.CiudadNacimiento.Provincia.IdPais = IdPais;
                        }
                        catch (Exception)
                        { }
                    }
                }
                else
                {
                    var listaCiudad = await apiServicio.Listar<Ciudad>(new Uri(WebApp.BaseAddress), "/api/Ciudad/ListarCiudades");
                    empleado.CiudadNacimiento = listaCiudad.SingleOrDefault(c => c.IdCiudad == empleado.IdCiudadLugarNacimiento);
                }
            }
            catch (Exception)
            { }

            return empleado;
        }

        private async Task<Empleado> ObtenerProvinciaSufragioValidacion(Empleado empleado)
        {
            try
            {
                if (empleado.IdProvinciaLugarSufragio == 0)
                {
                    empleado.ProvinciaSufragio = new Provincia();
                    int IdPais = int.Parse(Request.Form["ProvinciaSufragio.IdPais"].ToString());

                    var listaPais = await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddress), "/api/Pais/ListarPaises");
                    empleado.ProvinciaSufragio.Pais = listaPais.SingleOrDefault(c => c.IdPais == IdPais);
                    empleado.ProvinciaSufragio.IdPais = IdPais;
                }
                else
                {
                    var listaProvincia = await apiServicio.Listar<Provincia>(new Uri(WebApp.BaseAddress), "/api/Provincia/ListarProvincias");
                    empleado.ProvinciaSufragio = listaProvincia.SingleOrDefault(c => c.IdProvincia == empleado.IdProvinciaLugarSufragio);
                }
            }
            catch (Exception)
            { }
            return empleado;
        }

        public async Task<IActionResult> Create()
        {
            ViewData["EstadoCivil"] = new SelectList(await apiServicio.Listar<EstadoCivil>(new Uri(WebApp.BaseAddress), "/api/EstadoCivil/ListarEstadosCiviles"), "IdEstadoCivil", "Nombre");
            ViewData["Etnia"] = new SelectList(await apiServicio.Listar<Etnia>(new Uri(WebApp.BaseAddress), "/api/Etnia/ListarEtnias"), "IdEtnia", "Nombre");
            ViewData["Genero"] = new SelectList(await apiServicio.Listar<Genero>(new Uri(WebApp.BaseAddress), "/api/Genero/ListarGeneros"), "IdGenero", "Nombre");
            ViewData["Nacionalidad"] = new SelectList(await apiServicio.Listar<Nacionalidad>(new Uri(WebApp.BaseAddress), "/api/Nacionalidad/ListarNacionalidades"), "IdNacionalidad", "Nombre");
            ViewData["Sexo"] = new SelectList(await apiServicio.Listar<Sexo>(new Uri(WebApp.BaseAddress), "/api/Sexo/ListarSexos"), "IdSexo", "Nombre");
            ViewData["TipoIdentificacion"] = new SelectList(await apiServicio.Listar<TipoIdentificacion>(new Uri(WebApp.BaseAddress), "/api/TipoIdentificacion/ListarTiposIdentificacion"), "IdTipoIdentificacion", "Nombre");
            ViewData["TipoSangre"] = new SelectList(await apiServicio.Listar<TipoSangre>(new Uri(WebApp.BaseAddress), "/api/TipoSangre/ListarTiposSangre"), "IdTipoSangre", "Nombre");

            SelectList selectListPais = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddress), "/api/Pais/ListarPaises"), "IdPais", "Nombre");
            ViewData["Pais"] = selectListPais;
            ViewData["PaisSufragio"] = selectListPais;

            SelectList selectListProvincia = await ObtenerSelectListProvincia((ViewData["Pais"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Pais"] as SelectList).FirstOrDefault().Value) : -1);
            ViewData["Provincia"] = selectListProvincia;
            ViewData["ProvinciaSufragio"] = selectListProvincia;

            ViewData["Ciudad"] = await ObtenerSelectListCiudad((ViewData["Provincia"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Provincia"] as SelectList).FirstOrDefault().Value) : -1);
            ViewData["Dependencia"] = new SelectList(await apiServicio.Listar<Dependencia>(new Uri(WebApp.BaseAddress), "/api/Dependencia/ListarDependencias"), "IdDependencia", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Empleado empleado)
        {
            Response response = new Response();
            try
            {
                empleado = await ObtenerCiudadValidacion(empleado);
                empleado = await ObtenerProvinciaSufragioValidacion(empleado);
                response = await apiServicio.InsertarAsync(empleado,
                                                             new Uri(WebApp.BaseAddress),
                                                             "/api/Empleado/InsertarEmpleado");
                if (response.IsSuccess)
                {

                    var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        ExceptionTrace = null,
                        Message = "Se ha creado un empleado",
                        UserName = "Usuario 1",
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        EntityID = string.Format("{0} {1}", "Empleado:", empleado.IdEmpleado),
                    });

                    return RedirectToAction("Index");
                }

                ViewData["Error"] = response.Message;
                ViewData["EstadoCivil"] = new SelectList(await apiServicio.Listar<EstadoCivil>(new Uri(WebApp.BaseAddress), "/api/EstadoCivil/ListarEstadosCiviles"), "IdEstadoCivil", "Nombre");
                ViewData["Etnia"] = new SelectList(await apiServicio.Listar<Etnia>(new Uri(WebApp.BaseAddress), "/api/Etnia/ListarEtnias"), "IdEtnia", "Nombre");
                ViewData["Genero"] = new SelectList(await apiServicio.Listar<Genero>(new Uri(WebApp.BaseAddress), "/api/Genero/ListarGeneros"), "IdGenero", "Nombre");
                ViewData["Nacionalidad"] = new SelectList(await apiServicio.Listar<Nacionalidad>(new Uri(WebApp.BaseAddress), "/api/Nacionalidad/ListarNacionalidades"), "IdNacionalidad", "Nombre");
                ViewData["Sexo"] = new SelectList(await apiServicio.Listar<Sexo>(new Uri(WebApp.BaseAddress), "/api/Sexo/ListarSexos"), "IdSexo", "Nombre");
                ViewData["TipoIdentificacion"] = new SelectList(await apiServicio.Listar<TipoIdentificacion>(new Uri(WebApp.BaseAddress), "/api/TipoIdentificacion/ListarTiposIdentificacion"), "IdTipoIdentificacion", "Nombre");
                ViewData["TipoSangre"] = new SelectList(await apiServicio.Listar<TipoSangre>(new Uri(WebApp.BaseAddress), "/api/TipoSangre/ListarTiposSangre"), "IdTipoSangre", "Nombre");

                SelectList selectListPais = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddress), "/api/Pais/ListarPaises"), "IdPais", "Nombre");
                ViewData["Pais"] = selectListPais;
                ViewData["PaisSufragio"] = selectListPais;

                ViewData["Provincia"] = await ObtenerSelectListProvincia(empleado.CiudadNacimiento?.Provincia?.IdPais ?? -1);
                ViewData["ProvinciaSufragio"] = await ObtenerSelectListProvincia(empleado.ProvinciaSufragio?.IdPais ?? -1);

                ViewData["Ciudad"] = await ObtenerSelectListCiudad(empleado?.CiudadNacimiento?.IdProvincia ?? -1);
                ViewData["Dependencia"] = new SelectList(await apiServicio.Listar<Dependencia>(new Uri(WebApp.BaseAddress), "/api/Dependencia/ListarDependencias"), "IdDependencia", "Nombre");
                return View(empleado);

            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Creando Empleado",
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
                                                                  "/api/Empleado");

                    respuesta.Resultado = JsonConvert.DeserializeObject<Empleado>(respuesta.Resultado.ToString());
                    if (respuesta.IsSuccess)
                    {
                        Empleado empleado = respuesta.Resultado as Empleado;
                        ViewData["EstadoCivil"] = new SelectList(await apiServicio.Listar<EstadoCivil>(new Uri(WebApp.BaseAddress), "/api/EstadoCivil/ListarEstadosCiviles"), "IdEstadoCivil", "Nombre");
                        ViewData["Etnia"] = new SelectList(await apiServicio.Listar<Etnia>(new Uri(WebApp.BaseAddress), "/api/Etnia/ListarEtnias"), "IdEtnia", "Nombre");
                        ViewData["Genero"] = new SelectList(await apiServicio.Listar<Genero>(new Uri(WebApp.BaseAddress), "/api/Genero/ListarGeneros"), "IdGenero", "Nombre");
                        ViewData["Nacionalidad"] = new SelectList(await apiServicio.Listar<Nacionalidad>(new Uri(WebApp.BaseAddress), "/api/Nacionalidad/ListarNacionalidades"), "IdNacionalidad", "Nombre");
                        ViewData["Sexo"] = new SelectList(await apiServicio.Listar<Sexo>(new Uri(WebApp.BaseAddress), "/api/Sexo/ListarSexos"), "IdSexo", "Nombre");
                        ViewData["TipoIdentificacion"] = new SelectList(await apiServicio.Listar<TipoIdentificacion>(new Uri(WebApp.BaseAddress), "/api/TipoIdentificacion/ListarTiposIdentificacion"), "IdTipoIdentificacion", "Nombre");
                        ViewData["TipoSangre"] = new SelectList(await apiServicio.Listar<TipoSangre>(new Uri(WebApp.BaseAddress), "/api/TipoSangre/ListarTiposSangre"), "IdTipoSangre", "Nombre");
                        
                        SelectList selectListPais = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddress), "/api/Pais/ListarPaises"), "IdPais", "Nombre");
                        ViewData["Pais"] = selectListPais;
                        ViewData["PaisSufragio"] = selectListPais;

                        ViewData["Provincia"] = await ObtenerSelectListProvincia(empleado.CiudadNacimiento?.Provincia?.IdPais ?? -1);
                        ViewData["ProvinciaSufragio"] = await ObtenerSelectListProvincia(empleado.ProvinciaSufragio?.IdPais ?? -1);

                        ViewData["Ciudad"] = await ObtenerSelectListCiudad(empleado?.CiudadNacimiento?.IdProvincia ?? -1);
                        ViewData["Dependencia"] = new SelectList(await apiServicio.Listar<Dependencia>(new Uri(WebApp.BaseAddress), "/api/Dependencia/ListarDependencias"), "IdDependencia", "Nombre");
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
        public async Task<IActionResult> Edit(string id, Empleado empleado)
        {
            Response response = new Response();
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    empleado = await ObtenerCiudadValidacion(empleado);
                    empleado = await ObtenerProvinciaSufragioValidacion(empleado);
                    response = await apiServicio.EditarAsync(id, empleado, new Uri(WebApp.BaseAddress),
                                                                 "/api/Empleado");

                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                        {
                            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                            EntityID = string.Format("{0} : {1}", "Empleado", id),
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                            Message = "Se ha actualizado un registro empleado",
                            UserName = "Usuario 1"
                        });

                        return RedirectToAction("Index");
                    }

                }
                ViewData["Error"] = response.Message;
                ViewData["EstadoCivil"] = new SelectList(await apiServicio.Listar<EstadoCivil>(new Uri(WebApp.BaseAddress), "/api/EstadoCivil/ListarEstadosCiviles"), "IdEstadoCivil", "Nombre");
                ViewData["Etnia"] = new SelectList(await apiServicio.Listar<Etnia>(new Uri(WebApp.BaseAddress), "/api/Etnia/ListarEtnias"), "IdEtnia", "Nombre");
                ViewData["Genero"] = new SelectList(await apiServicio.Listar<Genero>(new Uri(WebApp.BaseAddress), "/api/Genero/ListarGeneros"), "IdGenero", "Nombre");
                ViewData["Nacionalidad"] = new SelectList(await apiServicio.Listar<Nacionalidad>(new Uri(WebApp.BaseAddress), "/api/Nacionalidad/ListarNacionalidades"), "IdNacionalidad", "Nombre");
                ViewData["Sexo"] = new SelectList(await apiServicio.Listar<Sexo>(new Uri(WebApp.BaseAddress), "/api/Sexo/ListarSexos"), "IdSexo", "Nombre");
                ViewData["TipoIdentificacion"] = new SelectList(await apiServicio.Listar<TipoIdentificacion>(new Uri(WebApp.BaseAddress), "/api/TipoIdentificacion/ListarTiposIdentificacion"), "IdTipoIdentificacion", "Nombre");
                ViewData["TipoSangre"] = new SelectList(await apiServicio.Listar<TipoSangre>(new Uri(WebApp.BaseAddress), "/api/TipoSangre/ListarTiposSangre"), "IdTipoSangre", "Nombre");

                SelectList selectListPais = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddress), "/api/Pais/ListarPaises"), "IdPais", "Nombre");
                ViewData["Pais"] = selectListPais;
                ViewData["PaisSufragio"] = selectListPais;

                ViewData["Provincia"] = await ObtenerSelectListProvincia(empleado.CiudadNacimiento?.Provincia?.IdPais ?? -1);
                ViewData["ProvinciaSufragio"] = await ObtenerSelectListProvincia(empleado.ProvinciaSufragio?.IdPais ?? -1);

                ViewData["Ciudad"] = await ObtenerSelectListCiudad(empleado?.CiudadNacimiento?.IdProvincia ?? -1);
                ViewData["Dependencia"] = new SelectList(await apiServicio.Listar<Dependencia>(new Uri(WebApp.BaseAddress), "/api/Dependencia/ListarDependencias"), "IdDependencia", "Nombre"); ViewData["Error"] = response.Message;
                return View(empleado);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Editando un empleado",
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
                                                               , "/api/Empleado");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        EntityID = string.Format("{0} : {1}", "Empleado", id),
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
                    Message = "Eliminar Empleado",
                    ExceptionTrace = ex,
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
        public async Task<IActionResult> Provincia_SelectResult(int idPais, string partialView)
        {
            ViewBag.Provincia = await ObtenerSelectListProvincia(idPais);
            ViewData[partialView == "_ProvinciaSelect" ? "Provincia" : "ProvinciaSufragio"] = await ObtenerSelectListProvincia(idPais);
            return PartialView(partialView, new Empleado());
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
            return PartialView("_CiudadSelect", new Empleado());
        }
        #endregion
    }
}