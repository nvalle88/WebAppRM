using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using bbd.webapprm.servicios.Enumeradores;
using bd.log.guardar.Enumeradores;
using bd.log.guardar.ObjectTranfer;
using bd.log.guardar.Servicios;
using bd.log.guardar.Utiles;
using bd.webapprm.entidades.Utils;
using bd.webapprm.entidades.Utils.Seguridad;
using bd.webapprm.servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace bd.webapprm.web.Controllers.MVC
{
    public class LoginController : Controller
    {
        private readonly IApiServicio apiServicio;

        public LoginController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        private void InicializarMensaje(string mensaje)
        {
            if (mensaje == null)
                mensaje = "";

            ViewData["Error"] = mensaje;
        }

        public IActionResult Index(string mensaje, string returnUrl = null)
        {
            InicializarMensaje(mensaje);
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        public async Task<IActionResult> Login()
        {
            try
            {
                if (Request.Query.Count != 2)
                    return Redirect(WebApp.BaseAddressWebAppLogin);

                Adscpassw adscpassw = new Adscpassw();
                var queryStrings = Request.Query;
                var qsList = new List<string>();
                foreach (var key in queryStrings.Keys)
                {
                    qsList.Add(queryStrings[key]);
                }
                var adscpasswSend = new Adscpassw
                {
                    AdpsLogin = qsList[0],
                    AdpsTokenTemp = qsList[1]
                };
                adscpassw = await GetAdscPassws(adscpasswSend);

                if (adscpassw != null)
                {
                    var response = await EliminarTokenTemp(adscpassw);
                    if (response.IsSuccess)
                    {
                        var responseLog = new EntradaLog
                        {
                            ExceptionTrace = null,
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Permission),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.INFO),
                            ObjectPrevious = null,
                            ObjectNext = JsonConvert.SerializeObject(response.Resultado),
                        };
                        await apiServicio.SalvarLog<entidades.Utils.Response>(HttpContext, responseLog);
                        return RedirectToActionPermanent(nameof(HomesController.Index), "Homes");
                    }
                    else
                        return Redirect(WebApp.BaseAddressWebAppLogin);
                }
                return Redirect(WebApp.BaseAddressWebAppLogin);
            }
            catch (Exception ex)
            {
                var responseLog = new EntradaLog
                {
                    ExceptionTrace = ex.Message,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    ObjectPrevious = null,
                    ObjectNext = null,
                };
                await apiServicio.SalvarLog<entidades.Utils.Response>(HttpContext, responseLog);
                return Redirect(WebApp.BaseAddressWebAppLogin);
            }
        }

        public async Task<IActionResult> Salir()
        {
            try
            {
                var claim = HttpContext.User.Identities.Where(x => x.NameClaimType == ClaimTypes.Name).FirstOrDefault();
                var token = claim.Claims.Where(c => c.Type == ClaimTypes.SerialNumber).FirstOrDefault().Value;
                var NombreUsuario = claim.Claims.Where(c => c.Type == ClaimTypes.Name).FirstOrDefault().Value;

                var adscpasswSend = new Adscpassw
                {
                    AdpsLogin = NombreUsuario,
                    AdpsToken = token
                };

                Adscpassw adscpassw = new Adscpassw();
                adscpassw = await GetAdscPassws(adscpasswSend);
                var response = await EliminarToken(adscpassw);
                if (response.IsSuccess)
                {
                    await HttpContext.Authentication.SignOutAsync("Cookies");
                    var responseLog = new EntradaLog
                    {
                        ExceptionTrace = null,
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Permission),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.INFO),
                        ObjectPrevious = null,
                        ObjectNext = null,
                    };
                    await apiServicio.SalvarLog<entidades.Utils.Response>(HttpContext, responseLog);
                    return RedirectPermanent(WebApp.BaseAddressWebAppLogin);
                }
                return RedirectPermanent(WebApp.BaseAddressWebAppLogin);
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(LoginController.Index), "Login");
            }
        }

        private async Task<Adscpassw> GetAdscPassws(Adscpassw adscpassw)
        {
            try
            {
                if (!adscpassw.Equals(null))
                {
                    var respuesta = await apiServicio.ObtenerElementoAsync<entidades.Utils.Response>(adscpassw, new Uri(WebApp.BaseAddressSeguridad), "api/Adscpassws/SeleccionarMiembroLogueado");
                    if (respuesta.IsSuccess)
                    {
                        var obje = JsonConvert.DeserializeObject<Adscpassw>(respuesta.Resultado.ToString());
                        return obje;
                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<entidades.Utils.Response> EliminarToken(Adscpassw adscpassw)
        {
            entidades.Utils.Response response = new entidades.Utils.Response();
            try
            {
                if (!string.IsNullOrEmpty(adscpassw.AdpsLogin))
                {
                    response = await apiServicio.EditarAsync<entidades.Utils.Response>(adscpassw, new Uri(WebApp.BaseAddressSeguridad), "api/Adscpassws/EliminarToken");
                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Sistema", adscpassw.AdpsLogin), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha actualizado un estado civil", UserName = "Usuario 1" });
                        return response;
                    }
                }
                return null;
            }
            catch (Exception)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Editando un estado civil", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                return null;
            }
        }

        private async Task<entidades.Utils.Response> EliminarTokenTemp(Adscpassw adscpassw)
        {
            entidades.Utils.Response response = new entidades.Utils.Response();
            if (!string.IsNullOrEmpty(adscpassw.AdpsLogin))
            {
                response = await apiServicio.EditarAsync<entidades.Utils.Response>(adscpassw, new Uri(WebApp.BaseAddressSeguridad), "api/Adscpassws/EliminarTokenTemp");
                if (response.IsSuccess)
                    return response;
            }
            return null;
        }
    }
}