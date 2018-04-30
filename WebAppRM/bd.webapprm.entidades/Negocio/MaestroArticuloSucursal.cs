namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class MaestroArticuloSucursal
    {
        [Key]
        public int IdMaestroArticuloSucursal { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Mínimo:")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int Minimo { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Máximo:")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int Maximo { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Sucursal:")]
        [Required(ErrorMessage = "Debe seleccionar la {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0}")]
        public int IdSucursal { get; set; }
        public virtual Sucursal Sucursal { get; set; }

        public virtual ICollection<Factura> Factura { get; set; }

        public virtual ICollection<MaestroDetalleArticulo> MaestroDetalleArticulo { get; set; }

        public virtual ICollection<RecepcionArticulos> RecepcionArticulos { get; set; }

        public virtual ICollection<SolicitudProveeduriaDetalle> SolicitudProveeduriaDetalle { get; set; }

        public virtual ICollection<TranferenciaArticulo> TranferenciaArticulo { get; set; }

        public virtual ICollection<TranferenciaArticulo> TranferenciaArticulo1 { get; set; }
    }
}
