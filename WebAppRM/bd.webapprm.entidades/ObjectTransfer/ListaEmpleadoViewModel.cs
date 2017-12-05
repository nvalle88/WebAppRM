using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.entidades.ObjectTransfer
{
    public class ListaEmpleadoViewModel
    {
        public int IdEmpleado { get; set; }
        public string NombreApellido { get; set; }
        public string Identificacion { get; set; }
        public string TelefonoPrivado { get; set; }
        public string CorreoPrivado { get; set; }
    }
}
