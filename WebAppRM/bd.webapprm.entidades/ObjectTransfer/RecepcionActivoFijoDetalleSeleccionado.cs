using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.entidades.ObjectTransfer
{
    public class IdRecepcionActivoFijoDetalleSeleccionado
    {
        public int idRecepcionActivoFijoDetalle { get; set; }
        public bool seleccionado { get; set; }
    }

    public class RecepcionActivoFijoDetalleSeleccionado
    {
        public RecepcionActivoFijoDetalle RecepcionActivoFijoDetalle { get; set; }
        public bool Seleccionado { get; set; }
    }
}
