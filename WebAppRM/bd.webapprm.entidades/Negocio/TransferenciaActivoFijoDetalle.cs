namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class TransferenciaActivoFijoDetalle
    {
        [Key]
        public int IdTransferenciaActivoFijoDetalle { get; set; }

        [Display(Name = "Transferencia de Activo Fijo:")]
        [Required(ErrorMessage = "Debe seleccionar la {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0}")]
        public int IdTransferenciaActivoFijo { get; set; }
        public virtual TransferenciaActivoFijo TransferenciaActivoFijo { get; set; }

        [Display(Name = "Activo Fijo:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int? IdActivoFijo { get; set; }
        public virtual ActivoFijo ActivoFijo { get; set; }
    }
}
