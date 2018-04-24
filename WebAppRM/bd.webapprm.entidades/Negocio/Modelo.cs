namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Modelo
    {
        [Key]
        public int IdModelo { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Modelo:")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Nombre { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Marca:")]
        [Required(ErrorMessage = "Debe seleccionar la {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0} ")]
        public int IdMarca { get; set; }

        public virtual Marca Marca { get; set; }

        public virtual ICollection<ActivoFijo> ActivoFijo { get; set; }

        public virtual ICollection<Articulo> Articulo { get; set; }
    }
}
