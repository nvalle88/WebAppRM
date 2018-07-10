using bd.webapprm.entidades.Utils;
using bd.webapprm.servicios.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace bd.webapprm.servicios.Servicios
{
    public class ClaimsTransferServicio : IClaimsTransfer
    {
        private readonly IHttpContextAccessor httpContext;

        public ClaimsTransferServicio(IHttpContextAccessor httpContext)
        {
            this.httpContext = httpContext;
        }

        public bool IsADMIGrupo(string admiGrupo)
        {
            try
            {
                var listadoGrupos = ObtenerClaimsValue("ADMI_Grupo");
                return listadoGrupos.Contains(admiGrupo);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public ClaimsTransfer ObtenerClaimsTransferHttpContext()
        {
            try
            {
                ClaimsTransfer claimsTransfer = new ClaimsTransfer();

                var claimIdSucursal = ObtenerClaimValue("IdSucursal");
                if (!String.IsNullOrEmpty(claimIdSucursal))
                    claimsTransfer.IdSucursal = int.Parse(claimIdSucursal);

                claimsTransfer.NombreSucursal = ObtenerClaimValue("NombreSucursal");
                claimsTransfer.NombreDependencia = ObtenerClaimValue("NombreDependencia");
                claimsTransfer.NombreEmpleado = ObtenerClaimValue("NombreEmpleado");

                var claimIdDependencia = ObtenerClaimValue("IdDependencia");
                if (!String.IsNullOrEmpty(claimIdDependencia))
                    claimsTransfer.IdDependencia = int.Parse(claimIdDependencia);

                var claimIdEmpleado = ObtenerClaimValue("IdEmpleado");
                if (!String.IsNullOrEmpty(claimIdEmpleado))
                    claimsTransfer.IdEmpleado = int.Parse(claimIdEmpleado);

                return claimsTransfer;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public IEnumerable<string> ObtenerClaimsValue(string claimType)
        {
            try
            {
                var claim = httpContext.HttpContext.User.Identities.Where(x => x.NameClaimType == ClaimTypes.Name).FirstOrDefault();
                return claim.Claims.Where(c => c.Type == claimType).Select(c=> c.Value);
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        public string ObtenerClaimValue(string claimType)
        {
            try
            {
                var claim = httpContext.HttpContext.User.Identities.Where(x => x.NameClaimType == ClaimTypes.Name).FirstOrDefault();
                var claimValue = claim.Claims.Where(c => c.Type == claimType).FirstOrDefault();
                return claimValue != null ? claimValue.Value : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
