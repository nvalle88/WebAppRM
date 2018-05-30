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

        [Display(Name = "Empleado responsable:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleadoResponsable { get; set; }

        [Display(Name = "Empleado que solicita:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleadoSolicita { get; set; }

        [Display(Name = "Empleado que retira:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleadoRetira { get; set; }

        [Display(Name = "Empleado autorizado:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleadoAutorizado { get; set; }

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
