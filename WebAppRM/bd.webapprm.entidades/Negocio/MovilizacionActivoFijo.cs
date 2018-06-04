using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class MovilizacionActivoFijo
    {
        public MovilizacionActivoFijo()
        {
            MovilizacionActivoFijoDetalle = new HashSet<MovilizacionActivoFijoDetalle>();
        }

        [Key]
        public int IdMovilizacionActivoFijo { get; set; }

        [Display(Name = "Autorizado a:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleadoResponsable { get; set; }
        public virtual Empleado EmpleadoResponsable { get; set; }

        [Display(Name = "Solicitado por:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleadoSolicita { get; set; }
        public virtual Empleado EmpleadoSolicita { get; set; }

        [Display(Name = "Autorizado por:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleadoAutorizado { get; set; }
        public virtual Empleado EmpleadoAutorizado { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha de salida:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime FechaSalida { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha de retorno:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime FechaRetorno { get; set; }

        [Display(Name = "Motivo de traslado:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdMotivoTraslado { get; set; }
        public virtual MotivoTraslado MotivoTraslado { get; set; }

        public virtual ICollection<MovilizacionActivoFijoDetalle> MovilizacionActivoFijoDetalle { get; set; }
    }
}
