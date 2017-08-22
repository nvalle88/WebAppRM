namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;


    public partial class EscalaEvaluacionTotal
    {
        [Key]
        public int IdEscalaEvaluacionTotal { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Nombre:")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Descripción:")]
        [DataType(DataType.Text)]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Porciento inicial:")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal PorcientoDesde { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Porciento final:")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal PorcientoHasta { get; set; }

        public virtual ICollection<Eval001> Eval001 { get; set; }
    }
}
