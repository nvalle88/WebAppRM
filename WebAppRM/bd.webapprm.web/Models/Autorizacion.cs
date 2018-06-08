using bd.webapprm.entidades.Utils;
using bd.webapprm.entidades.Utils.Seguridad;
using bd.webapprm.servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace bd.webapprm.web.Models
{
    public class RolesHandler : AuthorizationHandler<RolesRequirement>
    {
        public readonly IApiServicio apiServicio;

        public RolesHandler(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        /// <summary>
        /// Método que válida si el usuario actual tiene permiso para realizar la acción solicitada en el contexto 
        /// El usuario debe tener los permisos en la base de datos
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns>Si la acción solicitada es o no satisfactoria</returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RolesRequirement requirement)
        {
            try
            {
                /// <summary>
                /// Se obtiene el contexto de datos 
                /// </summary>
                /// <returns></returns>
                var recurso = context.Resource as Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext;
                var httpContext = recurso.HttpContext;

                /// <summary>
                /// Se obtiene el path solicitado 
                /// </summary>
                /// <returns></returns>
                var request = httpContext.Request;

                /// <summary>
                /// Se obtiene información del usuario autenticado
                /// </summary>
                /// <returns></returns>
                var claim = context.User.Identities.Where(x => x.NameClaimType == ClaimTypes.Name).FirstOrDefault();
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
                var respuesta = apiServicio.ObtenerElementoAsync<entidades.Utils.Response>(permiso, new Uri(WebApp.BaseAddressSeguridad), "api/Adscpassws/TienePermiso");

                //respuesta.Result.IsSuccess = true;
                if (respuesta.Result.IsSuccess)
                    context.Succeed(requirement);
                else
                    context.Fail();
            }
            catch (Exception)
            {
                context.Fail();
                return Task.CompletedTask;
            }
            return Task.FromResult(0);
        }
    }
}
