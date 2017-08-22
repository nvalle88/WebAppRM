namespace bd.webapprm.entidades
{
    using System.ComponentModel.DataAnnotations;


    public partial class CapacitacionRespuesta
    {
        [Key]
        public int IdCapacitacionRespuesta { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Respuesta:")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "¿Es correcta?:")]
        public bool EsCorrecta { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Pregunta:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdCapacitacionPregunta { get; set; }
        public virtual CapacitacionPregunta CapacitacionPregunta { get; set; }
    }
}
