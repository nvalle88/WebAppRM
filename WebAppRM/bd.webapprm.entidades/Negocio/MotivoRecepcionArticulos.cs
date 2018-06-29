using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class MotivoRecepcionArticulos
    {
        public MotivoRecepcionArticulos()
        {
            RecepcionArticulos = new HashSet<RecepcionArticulos>();
        }

        [Key]
        public int IdMotivoRecepcionArticulos { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Motivo de recepción:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Descripcion { get; set; }

        public virtual ICollection<RecepcionArticulos> RecepcionArticulos { get; set; }
    }
}
