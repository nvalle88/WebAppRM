using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class InventarioActivoFijoDetalle
    {
        [Key]
        [Column(Order = 0)]
        [Display(Name = "Detalle de recepción de activo fijo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdRecepcionActivoFijoDetalle { get; set; }
        public virtual RecepcionActivoFijoDetalle RecepcionActivoFijoDetalle { get; set; }

        [Key]
        [Column(Order = 1)]
        [Display(Name = "Inventario de activo fijo:")]
        [Required(ErrorMessage = "Debe seleccionar la {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0} ")]
        public int IdInventarioActivoFijo { get; set; }
        public virtual InventarioActivoFijo InventarioActivoFijo { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "¿Constatado?")]
        public bool Constatado { get; set; }
    }
}
