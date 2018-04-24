namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Articulo
    {
        [Key]
        public int IdArticulo { get; set; }

        [Required(ErrorMessage = "Debe introducir  {0}")]
        [StringLength(50)]
        public string Nombre { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Sub clase de artículo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdSubClaseArticulo { get; set; }
        public virtual SubClaseArticulo SubClaseArticulo { get; set; }

        [Display(Name = "Unidad de medida:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int? IdUnidadMedida { get; set; }
        public virtual UnidadMedida UnidadMedida { get; set; }

        [Display(Name = "Modelo:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int? IdModelo { get; set; }
        public virtual Modelo Modelo { get; set; }

        public virtual ICollection<AltaProveeduria> AltaProveeduria { get; set; }

        public virtual ICollection<MaestroDetalleArticulo> MaestroDetalleArticulo { get; set; }

        public virtual ICollection<RecepcionArticulos> RecepcionArticulos { get; set; }

        public virtual ICollection<SolicitudProveeduriaDetalle> SolicitudProveeduriaDetalle { get; set; }

        public virtual ICollection<DetalleFactura> DetalleFactura { get; set; }

        public virtual ICollection<TranferenciaArticulo> TranferenciaArticulo { get; set; }

        public virtual ExistenciaArticuloProveeduria ExistenciaArticuloProveeduria { get; set; }
    }
}
