using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.entidades.ObjectTransfer
{
    public class RecepcionActivoFijoDetalleComponentes
    {
        public int idFila { get; set; }
        public int idRecepcionActivoDetalleOrigen { get; set; }
        public List<int> arrIdsComponentes { get; set; }
    }

    public class ComponenteActivoFijoTransfer
    {
        public RecepcionActivoFijoDetalleComponentes ComponentesActivoFijo { get; set; }
        public List<int> IdsComponentesExcluir { get; set; }
    }
}
