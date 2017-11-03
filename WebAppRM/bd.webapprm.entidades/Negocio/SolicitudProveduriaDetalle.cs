namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    public partial class SolicitudProveeduriaDetalle
    {
        [Key]
        public int IdSolicitudProveeduriaDetalle { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Fecha de la solicitud:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaSolicitud { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Fecha aprobada:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FechaAprobada { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Cantidad solicitada:")]
        [Range(1, 100, ErrorMessage = "El {0} tiene que estar entre {1} y {2}")]
        public int? CantidadSolicitada { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Cantidad aprobada:")]
        [Range(1, 100, ErrorMessage = "El {0} tiene que estar entre {1} y {2}")]
        public int? CantidadAprobada { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Solicitud de Proveeduría:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdSolicitudProveeduria { get; set; }
        public virtual SolicitudProveeduria SolicitudProveeduria { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Estado:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEstado { get; set; }
        public virtual Estado Estado { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Maestro de árticulo de la sucursal:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdMaestroArticuloSucursal { get; set; }
        public virtual MaestroArticuloSucursal MaestroArticuloSucursal { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Árticulo:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdArticulo { get; set; }
        public virtual Articulo Articulo { get; set; }

       



    }
}
