using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class PolizaSeguroActivoFijo
    {
        [Key]
        public int IdActivo { get; set; }

        [Display(Name = "Subramo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdSubramo { get; set; }
        public virtual Subramo Subramo { get; set; }

        [Display(Name = "Compañía de seguro:")]
        [Required(ErrorMessage = "Debe seleccionar la {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0}")]
        public int IdCompaniaSeguro { get; set; }
        public virtual CompaniaSeguro CompaniaSeguro { get; set; }

        [Display(Name = "Número de póliza:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        [RegularExpression(@"^\d*$", ErrorMessage = "El {0} solo puede contener números.")]
        public string NumeroPoliza { get; set; }

        public virtual ActivoFijo ActivoFijo { get; set; }
    }
}
