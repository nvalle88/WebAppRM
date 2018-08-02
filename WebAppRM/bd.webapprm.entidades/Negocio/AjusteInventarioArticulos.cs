using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class AjusteInventarioArticulos
    {
        public AjusteInventarioArticulos()
        {
            InventarioArticulos = new HashSet<InventarioArticulos>();
        }

        [Key]
        public int IdAjusteInventario { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Motivo:")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Motivo { get; set; }

        [Display(Name = "Empleado que autoriza:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleadoAutoriza { get; set; }
        public virtual Empleado EmpleadoAutoriza { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha de ajuste:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Fecha { get; set; }

        [Display(Name = "Bodega:")]
        [Required(ErrorMessage = "Debe seleccionar la {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0}")]
        public int IdBodega { get; set; }
        public virtual Bodega Bodega { get; set; }

        [NotMapped]
        public virtual ICollection<InventarioArticulos> InventarioArticulos { get; set; }
    }
}
