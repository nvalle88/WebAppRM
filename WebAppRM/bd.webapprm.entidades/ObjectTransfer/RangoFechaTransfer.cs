using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.entidades.ObjectTransfer
{
    public class RangoFechaTransfer
    {
        public DateTime? FechaInicial { get; set; }
        public DateTime? FechaFinal { get; set; }
    }

    public class RangoFechaEstadoTransfer
    {
        public RangoFechaTransfer RangoFechaTransfer { get; set; }
        public List<string> Estados { get; set; }
    }
}
