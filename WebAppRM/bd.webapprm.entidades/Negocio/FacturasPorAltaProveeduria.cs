using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class FacturasPorAltaProveeduria
    {
        public int IdFacturasPorAlta { get; set; }
        public string NumeroFactura { get; set; }
        public int? IdAlta { get; set; }

        public virtual AltaProveeduria IdAltaNavigation { get; set; }
    }
}
