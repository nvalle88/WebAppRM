using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.entidades.ObjectTransfer
{
    public class ListaEmpleadoViewModel
    {
        public int IdEmpleado { get; set; }
        public int IdPersona { get; set; }
        public string NombreApellido { get; set; }
        public string Identificacion { get; set; }
        public string TelefonoPrivado { get; set; }
        public string CorreoPrivado { get; set; }
        public string Dependencia { get; set; }
        public string RolPuesto { get; set; }
        public bool TipoCuenta { get; set; }
        public string NoCuenta { get; set; }
        public string InstitucionBancaria { get; set; }
        public string FondoFinanciamiento { get; set; }
        public int IdConfiguracionViatico { get; set; }
        public DateTime FechaIngreso { get; set; }
    }
}
