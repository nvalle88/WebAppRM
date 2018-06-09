using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class FacturaActivoFijo
    {
        public FacturaActivoFijo()
        {
            AltaActivoFijo = new HashSet<AltaActivoFijo>();
            DocumentoActivoFijo = new HashSet<DocumentoActivoFijo>();
        }

        [Key]
        public int IdFacturaActivoFijo { get; set; }

        [Display(Name = "Número de factura:")]
        [Required(ErrorMessage = "Debe introducir el {0}")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        [RegularExpression(@"^\d*$", ErrorMessage = "El {0} solo puede contener números.")]
        public string NumeroFactura { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha de factura:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime FechaFactura { get; set; }

        public virtual ICollection<AltaActivoFijo> AltaActivoFijo { get; set; }
        public virtual ICollection<DocumentoActivoFijo> DocumentoActivoFijo { get; set; }
    }
}
