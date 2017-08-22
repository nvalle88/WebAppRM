namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;


    public partial class ClaseActivoFijo
    {

        [Key]
        public int IdClaseActivoFijo { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Clase de activo fijo:")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Nombre { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Tipo de activo fijo:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdTipoActivoFijo { get; set; }
        public virtual TipoActivoFijo TipoActivoFijo { get; set; }

        [Display(Name = "Depreciación")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdTablaDepreciacion { get; set; }
        public virtual TablaDepreciacion TablaDepreciacion { get; set; }

        public virtual ICollection<SubClaseActivoFijo> SubClaseActivoFijo { get; set; }
    }
}
