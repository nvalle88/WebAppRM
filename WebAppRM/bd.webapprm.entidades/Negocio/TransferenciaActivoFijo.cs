namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class TransferenciaActivoFijo
    {
        public TransferenciaActivoFijo()
        {
            TransferenciaActivoFijoDetalle = new HashSet<TransferenciaActivoFijoDetalle>();
        }

        [Key]
        public int IdTransferenciaActivoFijo { get; set; }

        [Display(Name = "Responsable de envío:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int? IdEmpleadoResponsableEnvio { get; set; }
        public virtual Empleado EmpleadoResponsableEnvio { get; set; }

        [Display(Name = "Responsable de recibo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int? IdEmpleadoResponsableRecibo { get; set; }
        public virtual Empleado EmpleadoResponsableRecibo { get; set; }

        [Display(Name = "Motivo de transferencia:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdMotivoTransferencia { get; set; }
        public virtual MotivoTransferencia MotivoTransferencia { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha de transferencia:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime FechaTransferencia { get; set; }

        [Display(Name = "Observaciones:")]
        public string Observaciones { get; set; }

        [Display(Name = "Estado:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEstado { get; set; }
        public virtual Estado Estado { get; set; }

        [NotMapped]
        [Display(Name = "Sucursal de origen:")]
        public Sucursal SucursalOrigen { get; set; }

        [NotMapped]
        [Display(Name = "Sucursal de destino:")]
        public Sucursal SucursalDestino { get; set; }

        public virtual ICollection<TransferenciaActivoFijoDetalle> TransferenciaActivoFijoDetalle { get; set; }
    }
}
