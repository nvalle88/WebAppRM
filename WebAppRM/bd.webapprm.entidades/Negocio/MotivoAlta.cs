using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class MotivoAlta
    {
        public MotivoAlta()
        {
            AltaActivoFijo = new HashSet<AltaActivoFijo>();
        }

        [Key]
        public int IdMotivoAlta { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Motivo de alta:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Descripcion { get; set; }

        public virtual ICollection<AltaActivoFijo> AltaActivoFijo { get; set; }
    }
}
