namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
     
     
    public partial class Titulo
    {
        [Key]
        public int IdTitulo { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Título:")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Nombre { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Estudio:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEstudio { get; set; }
        public virtual Estudio Estudio { get; set; }

        public virtual ICollection<PersonaEstudio> PersonaEstudio { get; set; }
    }
}
