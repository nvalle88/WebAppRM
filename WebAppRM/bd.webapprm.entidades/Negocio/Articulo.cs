namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Articulo
    {
        public Articulo()
        {
            MaestroArticuloSucursal = new HashSet<MaestroArticuloSucursal>();
            RequerimientosArticulosDetalles = new HashSet<RequerimientosArticulosDetalles>();
        }

        [Key]
        public int IdArticulo { get; set; }

        [Display(Name = "Artículo:")]
        [Required(ErrorMessage = "Debe introducir el {0}")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Nombre { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Sub clase de artículo:")]
        [Required(ErrorMessage = "Debe seleccionar la {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0}")]
        public int IdSubClaseArticulo { get; set; }
        public virtual SubClaseArticulo SubClaseArticulo { get; set; }

        [Display(Name = "Unidad de medida:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0}")]
        public int? IdUnidadMedida { get; set; }
        public virtual UnidadMedida UnidadMedida { get; set; }

        [Display(Name = "Modelo:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int? IdModelo { get; set; }
        public virtual Modelo Modelo { get; set; }

        public virtual ICollection<MaestroArticuloSucursal> MaestroArticuloSucursal { get; set; }

        public virtual ICollection<RequerimientosArticulosDetalles> RequerimientosArticulosDetalles { get; set; }
    }
}
