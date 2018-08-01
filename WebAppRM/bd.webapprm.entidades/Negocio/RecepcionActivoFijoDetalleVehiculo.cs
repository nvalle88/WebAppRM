using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class RecepcionActivoFijoDetalleVehiculo
    {
        [Key]
        public int IdRecepcionActivoFijoDetalle { get; set; }
        public virtual RecepcionActivoFijoDetalle RecepcionActivoFijoDetalle { get; set; }

        [Display(Name = "Número de chasis:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        [RegularExpression(@"^[-A-Z0-9a-z-]*$", ErrorMessage = "El {0} tiene que ser alfanumérico.")]
        public string NumeroChasis { get; set; }

        [Display(Name = "Número de motor:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        [RegularExpression(@"^[-A-Z0-9a-z-]*$", ErrorMessage = "El {0} tiene que ser alfanumérico.")]
        public string NumeroMotor { get; set; }

        [Display(Name = "Placa:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "La {0} no puede tener más de {1} y menos de {2}")]
        [RegularExpression(@"^[-A-Z0-9a-z-]*$", ErrorMessage = "La {0} tiene que ser alfanumérica.")]
        public string Placa { get; set; }
    }
}
