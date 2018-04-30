namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class SubClaseArticulo
    {
        [Key]
        public int IdSubClaseArticulo { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Sub clase de artículo:")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "La {0} no puede tener más de {1} y menos de {2}")]
        public string Nombre { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Clase de artículo:")]
        [Required(ErrorMessage = "Debe seleccionar la {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0} ")]
        public int IdClaseArticulo { get; set; }
        public virtual ClaseArticulo ClaseArticulo { get; set; }

        public virtual ICollection<Articulo> Articulo { get; set; }
    }
}
