using bd.webapprm.entidades.Utils;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace bd.webapprm.servicios.Interfaces
{
    public interface IClaimsTransfer
    {
        ClaimsTransfer ObtenerClaimsTransferHttpContext();
        string ObtenerClaimValue(string claimType);
        IEnumerable<string> ObtenerClaimsValue(string claimType);
        bool IsADMIGrupo(string admiGrupo);
    }
}
