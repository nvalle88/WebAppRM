using bd.webappcompartido.servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using bd.webappcompartido.entidades.Utils;
using ds.core.data;
using System.Security.Claims;

namespace bd.webappcompartido.servicios.Servicios
{
    public class SeguridadServicio : Controller,ISeguridadServicio
    {
        private readonly CoreDbContext db;

        public SeguridadServicio(CoreDbContext db)
        {
             
        }

        public Response TienePermiso(string controlador, string accion, ClaimsPrincipal user)
        {
            if ((controlador == null || controlador == "")&&(accion == null || accion == ""))
            {
                return new Response
                {
                    IsSuccess=false,
                    Message="No tiene Permiso"
                };
            }

           var a= user.Identity.Name;
            var opcion = db.MenuOpciones.Where(p => p.Url == controlador).Include(p => p.Role).FirstOrDefault();

            if (user.IsInRole(opcion.Role.Name))
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Tiene Permiso"
                };
            }

            return new Response
            {
                IsSuccess = false,
                Message = "Tiene Permiso"
            };
        }
    }
}
