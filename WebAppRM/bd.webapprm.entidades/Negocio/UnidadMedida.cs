namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class UnidadMedida
    {
        [Key]
        public int IdUnidadMedida { get; set; }

        [Display(Name = "Unidad de medida:")]
        [Required(ErrorMessage = "Debe introducir la {0}")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "La {0} no puede tener más de {1} y menos de {2}")]
        public string Nombre { get; set; }

        public virtual ICollection<Articulo> Articulo { get; set; }
    }
}
