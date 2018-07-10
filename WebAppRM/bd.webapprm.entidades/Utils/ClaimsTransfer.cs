using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.entidades.Utils
{
    public class ClaimsTransfer
    {
        public int IdSucursal { get; set; }
        public string NombreSucursal { get; set; }
        public int IdDependencia { get; set; }
        public string NombreDependencia { get; set; }
        public int IdEmpleado { get; set; }
        public string NombreEmpleado { get; set; }
    }
}
