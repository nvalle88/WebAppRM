using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.entidades.ObjectTransfer
{
    public class AprobacionActivoFijoTransfer
    {
        public List<int> IdsActivoFijo { get; set; }
        public string NuevoEstadoActivoFijo { get; set; }
        public bool ValidacionTecnica { get; set; }
    }
}
