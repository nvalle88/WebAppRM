using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class MovilizacionActivoFijoDetalle
    {
        [Key]
        [Column(Order = 0)]
        [Display(Name = "Detalle de recepción de activo fijo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdRecepcionActivoFijoDetalle { get; set; }
        public virtual RecepcionActivoFijoDetalle RecepcionActivoFijoDetalle { get; set; }

        [Key]
        [Column(Order = 1)]
        [Display(Name = "Movilización de activo fijo:")]
        [Required(ErrorMessage = "Debe seleccionar la {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0}")]
        public int IdMovilizacionActivoFijo { get; set; }
        public virtual MovilizacionActivoFijo MovilizacionActivoFijo { get; set; }

        [Display(Name = "Observaciones:")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Las {0} no pueden tener más de {1} y menos de {2}")]
        public string Observaciones { get; set; }

        [NotMapped]
        [Display(Name = "Componentes")]
        public string Componentes { get; set; }
    }
}
