using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace bd.webapprm.entidades
{
    public partial class ActivoFijoDocumento
    {
        [Key]
        public int IdActivoFijoDocumento { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Documento:")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Dirección:")]
        [StringLength(1024, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Url { get; set; }

        [Display(Name = "ActivoFijo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdActivo { get; set; }

        public virtual ActivoFijo ActivoFijo { get; set; }
    }
}
