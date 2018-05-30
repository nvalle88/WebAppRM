using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class ProcesoJudicialActivoFijo
    {
        public ProcesoJudicialActivoFijo()
        {
            DocumentoActivoFijo = new HashSet<DocumentoActivoFijo>();
        }

        [Key]
        public int IdProcesoJudicialActivoFijo { get; set; }

        [Display(Name = "Número de denuncia:")]
        [Required(ErrorMessage = "Debe introducir el {0}")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        [RegularExpression(@"^\d*$", ErrorMessage = "El {0} solo puede contener números.")]
        public string NumeroDenuncia { get; set; }

        [Display(Name = "Detalle de recepción de activo fijo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdRecepcionActivoFijoDetalle { get; set; }
        public virtual RecepcionActivoFijoDetalle RecepcionActivoFijoDetalle { get; set; }

        public virtual ICollection<DocumentoActivoFijo> DocumentoActivoFijo { get; set; }
    }
}
