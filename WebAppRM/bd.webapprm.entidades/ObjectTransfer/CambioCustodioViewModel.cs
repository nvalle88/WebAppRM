using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades.ObjectTransfer
{
    public class CambioCustodioViewModel
    {
        [Display(Name = "Custodio que entrega:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleadoEntrega { get; set; }

        [Display(Name = "Custodio que recibe:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleadoRecibe { get; set; }
        
        [Display(Name = "Observaciones:")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Las {0} no pueden tener más de {1} y menos de {2}")]
        public string Observaciones { get; set; }

        public ICollection<int> ListadoIdRecepcionActivoFijoDetalle { get; set; }
    }
}
