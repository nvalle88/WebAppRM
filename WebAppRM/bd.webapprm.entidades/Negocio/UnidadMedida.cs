namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class UnidadMedida
    {
        [Key]
        public int IdUnidadMedida { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Nombre { get; set; }

        public virtual ICollection<ActivoFijo> ActivoFijo { get; set; }

        public virtual ICollection<Articulo> Articulo { get; set; }
    }
}
