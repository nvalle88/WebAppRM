using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class InventarioActivoFijo
    {
        public InventarioActivoFijo()
        {
            InventarioActivoFijoDetalle = new HashSet<InventarioActivoFijoDetalle>();
        }

        [Key]
        public int IdInventarioActivoFijo { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha de corte de inventario:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime FechaCorteInventario { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha de informe:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime FechaInforme { get; set; }

        [Display(Name = "Número de informe:")]
        [Required(ErrorMessage = "Debe introducir el {0}")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        [RegularExpression(@"^\d*$", ErrorMessage = "El {0} solo puede contener números.")]
        public string NumeroInforme { get; set; }

        [Display(Name = "Estado:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEstado { get; set; }
        public virtual Estado Estado { get; set; }

        public virtual ICollection<InventarioActivoFijoDetalle> InventarioActivoFijoDetalle { get; set; }
    }
}
