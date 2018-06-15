using bd.webapprm.entidades.Utils;
using bd.webapprm.entidades.Utils.Seguridad;
using bd.webapprm.servicios.Interfaces;
using bd.webapprm.servicios.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace bd.webapprm.web.Models
{
    public class Filtro : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                /// <summary>
                /// Se obtiene el contexto de datos 
                /// </summary>
                /// <returns></returns>
                /// con
                var httpContext = context.HttpContext;
                
                /// <summary>
                /// Se obtiene el path solicitado 
                /// </summary>
                /// <returns></returns>
                var request = httpContext.Request;

                /// <summary>
                /// Se obtiene información del usuario autenticado
                /// </summary>
                /// <returns></returns>
                var claim = context.HttpContext.User.Identities.Where(x => x.NameClaimType == ClaimTypes.Name).FirstOrDefault();
                var token = claim.Claims.Where(c => c.Type == ClaimTypes.SerialNumber).FirstOrDefault().Value;
                var NombreUsuario = claim.Claims.Where(c => c.Type == ClaimTypes.Name).FirstOrDefault().Value;

                var permiso = new PermisoUsuario
                {
                    Contexto = request.Path,
                    Token = token,
                    Usuario = NombreUsuario,
                };

                /// <summary>
                /// Se valida que la información del usuario actual tenga permiso para acceder al path solicitado... 
                /// </summary>
                /// <returns></returns>
                ApiServicio a = new ApiServicio();
                var respuestaToken = a.ObtenerElementoAsync<Response>(permiso, new Uri(WebApp.BaseAddressSeguridad), "api/Adscpassws/ExisteToken");

                if (!respuestaToken.Result.IsSuccess)
                {
                    context.HttpContext.Authentication.SignOutAsync("Cookies");
                    var result = new ViewResult { ViewName = "SeccionCerrada" };
                    context.Result = result;
                }
                else
                {
                    var respuesta = a.ObtenerElementoAsync<Response>(permiso, new Uri(WebApp.BaseAddressSeguridad), "api/Adscpassws/TienePermiso");
                    if (!respuesta.Result.IsSuccess)
                    {
                        var result = new ViewResult { ViewName = "AccesoDenegado" };
                        context.Result = result;
                    }
                }
            }
            catch (Exception)
            {
                var result = new RedirectResult(WebApp.BaseAddressWebAppLogin);
                context.Result = result;
            }
        }
    }
}
