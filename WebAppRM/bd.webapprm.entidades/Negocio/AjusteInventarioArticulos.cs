using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class AjusteInventarioArticulos
    {
        [Key]
        public int IdAjusteInventario { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Motivo:")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Motivo { get; set; }

        [Display(Name = "Empleado que autoriza:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleadoAutoriza { get; set; }
        public virtual Empleado EmpleadoAutoriza { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha de ajuste:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Fecha { get; set; }
    }
}
