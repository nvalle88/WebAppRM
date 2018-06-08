namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class RecepcionActivoFijo
    {
        public RecepcionActivoFijo()
        {
            RecepcionActivoFijoDetalle = new HashSet<RecepcionActivoFijoDetalle>();
        }

        [Key]
        public int IdRecepcionActivoFijo { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha de recepción:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime FechaRecepcion { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Cantidad:")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "¿Validacion técnica?")]
        public bool ValidacionTecnica { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Orden de compra:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string OrdenCompra { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Motivo de Recepción:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdMotivoRecepcion { get; set; }
        public virtual MotivoRecepcion MotivoRecepcion { get; set; }

        [Display(Name = "Proveedor:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdProveedor { get; set; }
        public virtual Proveedor Proveedor { get; set; }

        [Display(Name = "Fondo de financiamiento:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdFondoFinanciamiento { get; set; }
        public virtual FondoFinanciamiento FondoFinanciamiento { get; set; }

        public virtual ICollection<RecepcionActivoFijoDetalle> RecepcionActivoFijoDetalle { get; set; }
    }
}
