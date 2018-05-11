using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.entidades.ObjectTransfer
{
    public class AprobacionActivoFijoTransfer
    {
        public int IdActivoFijo { get; set; }
        public string NuevoEstadoActivoFijo { get; set; }
        public bool ValidacionTecnica { get; set; }
    }
}
