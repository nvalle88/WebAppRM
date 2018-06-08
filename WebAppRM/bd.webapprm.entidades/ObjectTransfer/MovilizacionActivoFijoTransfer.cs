using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.entidades.ObjectTransfer
{
    public class MovilizacionActivoFijoTransfer
    {
        public List<IdRecepcionActivoFijoDetalleSeleccionado> ListadoRecepcionActivoFijoDetalleSeleccionado { get; set; }
        public bool SeleccionarTodasAltas { get; set; }
        public int? IdMovilizacionActivoFijo { get; set; }
    }
}
