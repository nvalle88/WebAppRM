using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace bd.webapprm.entidades.Utils.Seguridad
{
    public partial class Adscpassw
    {
        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Nombre Usuario")]
        [StringLength(32, MinimumLength = 4, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string AdpsLogin { get; set; }
        public string AdpsPassword { get; set; }
        public DateTime? AdpsFechaCambio { get; set; }
        public DateTime? AdpsFechaVencimiento { get; set; }
        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Login admin")]
        public string AdpsLoginAdm { get; set; }
        public int? AdpsIntentos { get; set; }
        public string AdpsPasswCg { get; set; }
        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Tipo")]
        public string AdpsTipoUso { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Id Contacto")]
        public string AdpsIdContacto { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Id Entidad")]
        public string AdpsIdEntidad { get; set; }
        public string AdpsPreguntaRecuperacion { get; set; }
        public string AdpsRespuestaRecuperacion { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Código de empleado")]
        public string AdpsCodigoEmpleado { get; set; }
        public string AdpsPasswPoint { get; set; }
        public string AdpsToken { get; set; }
        public string AdpsTokenTemp { get; set; }
    }
}
