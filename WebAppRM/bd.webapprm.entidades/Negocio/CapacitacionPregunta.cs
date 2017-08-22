namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class CapacitacionPregunta
    {
        [Key]
        public int IdCapacitacionPregunta { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Pregunta:")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Descripcion { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Encuesta:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public string IdCapacitacionEncuesta { get; set; }
        public virtual CapacitacionEncuesta CapacitacionEncuesta { get; set; }

        [Display(Name = "Tipo de pregunta:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdCapacitacionTipoPregunta { get; set; }
        public virtual CapacitacionTipoPregunta CapacitacionTipoPregunta { get; set; }

        public virtual ICollection<CapacitacionRespuesta> CapacitacionRespuesta { get; set; }

    }
}
