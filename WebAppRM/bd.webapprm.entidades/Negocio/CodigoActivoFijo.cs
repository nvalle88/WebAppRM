namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class CodigoActivoFijo
    {
        [Key]
        public int IdCodigoActivoFijo { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Código secuencial:")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Codigosecuencial { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Código de barras:")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string CodigoBarras { get; set; }

        //Propiedades Virtuales Referencias a otras clases
        public virtual ICollection<ActivoFijo> ActivoFijo { get; set; }
    }
}
