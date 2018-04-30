namespace bd.webapprm.entidades
{
    using bd.webapprm.entidades.Negocio;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class TransferenciaActivoFijo
    {
        [Key]
        public int IdTransferenciaActivoFijo { get; set; }

        [Display(Name = "Empleado que registra:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleadoRegistra { get; set; }
        public virtual Empleado EmpleadoRegistra { get; set; }

        [Display(Name = "Responsable de envío:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleadoResponsableEnvio { get; set; }
        public virtual Empleado EmpleadoResponsableEnvio { get; set; }

        [Display(Name = "Responsable de recibo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleadoResponsableRecibo { get; set; }
        public virtual Empleado EmpleadoResponsableRecibo { get; set; }

        [Display(Name = "Empleado que recibe:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int? IdEmpleadoRecibo { get; set; }
        public virtual Empleado EmpleadoRecibo { get; set; }

        [Display(Name = "Motivo de transferencia:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdMotivoTransferencia { get; set; }
        public virtual MotivoTransferencia MotivoTransferencia { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha de transferencia:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime FechaTransferencia { get; set; }

        [Display(Name = "Origen:")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Origen { get; set; }

        [Display(Name = "Destino:")]
        [Required(ErrorMessage = "Debe introducir el {0}")]
        [StringLength(50, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Destino { get; set; }

        [Display(Name = "Observaciones:")]
        public string Observaciones { get; set; }

        public virtual ICollection<TransferenciaActivoFijoDetalle> TransferenciaActivoFijoDetalle { get; set; }
    }
}
