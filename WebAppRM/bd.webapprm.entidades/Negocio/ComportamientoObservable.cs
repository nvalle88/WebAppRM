namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;


    public partial class ComportamientoObservable
    {
        [Key]
        public int ComportamientoObservableId { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Comportamiento observable:")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Descripcion { get; set; }


        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Nivel")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int NivelId { get; set; }
        public virtual Nivel Nivel { get; set; }

        [Display(Name = "Denominación de la competencia:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int DenominacionCompetenciaId { get; set; }
        public virtual DenominacionCompetencia DenominacionCompetencia { get; set; }


        public virtual ICollection<IndiceOcupacionalComportamientoObservable> IndiceOcupacionalComportamientoObservable { get; set; }

  
    }
}
