using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class RecepcionActivoFijoDetalleEdificio
    {
        [Key]
        public int IdRecepcionActivoFijoDetalle { get; set; }
        public virtual RecepcionActivoFijoDetalle RecepcionActivoFijoDetalle { get; set; }

        [Display(Name = "Número de clave catastral:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        [RegularExpression(@"^[-A-Z0-9a-z-]*$", ErrorMessage = "El {0} tiene que ser alfanumérico.")]
        public string NumeroClaveCatastral { get; set; }
    }
}
