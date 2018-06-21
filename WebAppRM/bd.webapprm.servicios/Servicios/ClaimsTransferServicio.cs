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

        public ClaimsTransfer ObtenerClaimsTransferHttpContext()
        {
            try
            {
                ClaimsTransfer claimsTransfer = new ClaimsTransfer();

                var claimIdSucursal = ObtenerClaimValue(ClaimsTransferNombres.IdSucursal);
                if (!String.IsNullOrEmpty(claimIdSucursal))
                    claimsTransfer.IdSucursal = int.Parse(claimIdSucursal);

                claimsTransfer.NombreSucursal = ObtenerClaimValue(ClaimsTransferNombres.NombreSucursal);
                return claimsTransfer;
            }
            catch (Exception)
            {
                return null;
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
