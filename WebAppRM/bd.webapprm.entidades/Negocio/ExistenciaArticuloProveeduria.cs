using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class ExistenciaArticuloProveeduria
    {
        public int IdArticulo { get; set; }
        public int Existencia { get; set; }

        public virtual Articulo Articulo { get; set; }
    }
}
