namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class EscalaGrados
    {
        [Key]
        public int IdEscalaGrados { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Grado:")]
        [Range(1, 22, ErrorMessage = "El {0} debe estar entre {1} y {2} ")]
        public int Grado { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Remuneración:")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Remuneracion { get; set; }


        //Propiedades Virtuales Referencias a otras clases
        [Display(Name = "Grupo ocupacional:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdGrupoOcupacional { get; set; }
        public virtual GrupoOcupacional GrupoOcupacional { get; set; }

        public virtual ICollection<IndiceOcupacional> IndiceOcupacional { get; set; }

       
    }
}
