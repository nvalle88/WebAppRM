namespace bd.webapprm.entidades
{
    using System.ComponentModel.DataAnnotations;

    public partial class DetalleFactura
    {
        [Key]
        public int IdDetalleFactura { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Precio:")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal? Precio { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Cantidad:")]
        [Range(1, 100, ErrorMessage = "La {0} debe estar entre {1} y {2} ")]
        public int Cantidad { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Factura:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdFactura { get; set; }
        public virtual Factura Factura { get; set; }
        

        [Display(Name = "Artículo:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdArticulo { get; set; }
        public virtual Articulo Articulo { get; set; }




        
    }
}
