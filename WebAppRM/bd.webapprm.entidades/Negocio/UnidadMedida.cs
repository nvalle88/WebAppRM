namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
     
     

    public partial class UnidadMedida
    {
        [Key]
        public int IdUnidadMedida { get; set; }

        public string Nombre { get; set; }
        public virtual ICollection<ActivoFijo> ActivoFijo { get; set; }

        public virtual ICollection<Articulo> Articulo { get; set; }
    }
}
