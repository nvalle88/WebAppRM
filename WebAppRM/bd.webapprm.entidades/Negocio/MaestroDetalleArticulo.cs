namespace bd.webapprm.entidades
{
    using System.ComponentModel.DataAnnotations;

    public partial class MaestroDetalleArticulo
    {
        [Key]
        public int IdMaestroDetalleArticulo { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Cantidad:")]
        [Range(1, 10000, ErrorMessage = "El {0} debe estar entre {1} y {2} ")]
        public int Cantidad { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Maestro de artículo de Sucursal:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdMaestroArticuloSucursal { get; set; }

        public virtual MaestroArticuloSucursal MaestroArticuloSucursal { get; set; }

        [Display(Name = "Artículo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdArticulo { get; set; }

        public virtual Articulo Articulo { get; set; }
    }
}
