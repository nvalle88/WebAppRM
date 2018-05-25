using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class TipoUtilizacionAlta
    {
        public TipoUtilizacionAlta()
        {
            AltaActivoFijoDetalle = new HashSet<AltaActivoFijoDetalle>();
        }

        [Key]
        public int IdTipoUtilizacionAlta { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Tipo de utilización:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener menos de {2} y más de {1}")]
        public string Nombre { get; set; }

        public virtual ICollection<AltaActivoFijoDetalle> AltaActivoFijoDetalle { get; set; }
    }
}
