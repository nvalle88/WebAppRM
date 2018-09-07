using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.entidades.ObjectTransfer
{
    public class ProveedorArticulosTransfer
    {
        public Proveedor Proveedor { get; set; }
        public List<MaestroArticuloSucursal> ListadoMaestroArticuloSucursal { get; set; }
    }
}
