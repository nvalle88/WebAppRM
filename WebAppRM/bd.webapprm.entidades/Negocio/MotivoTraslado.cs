using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class MotivoTraslado
    {
        public MotivoTraslado()
        {
            MovilizacionActivoFijo = new HashSet<MovilizacionActivoFijo>();
        }

        [Key]
        public int IdMotivoTraslado { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Motivo de traslado:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener menos de {2} y más de {1}")]
        public string Descripcion { get; set; }

        public virtual ICollection<MovilizacionActivoFijo> MovilizacionActivoFijo { get; set; }
    }
}
