using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.entidades.ObjectTransfer
{
    public class ArticuloProveedoresTransfer
    {
        public Articulo Articulo { get; set; }
        public List<Proveedor> ListadoProveedores { get; set; }
    }
}
