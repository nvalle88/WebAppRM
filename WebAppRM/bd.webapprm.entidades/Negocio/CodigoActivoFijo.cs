namespace bd.webapprm.entidades
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class CodigoActivoFijo
    {
        public CodigoActivoFijo()
        {
            RecepcionActivoFijoDetalle = new HashSet<RecepcionActivoFijoDetalle>();
            TransferenciaActivoFijoDetalle = new HashSet<TransferenciaActivoFijoDetalle>();
        }

        [Key]
        public int IdCodigoActivoFijo { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Código secuencial:")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Codigosecuencial { get; set; }

        [NotMapped]
        [Display(Name = "Código de barras:")]
        public string CodigoBarras { get; set; }

        [NotMapped]
        [Display(Name = "Código secuencial:")]
        [Remote("ValidarCodigoUnico", "ActivoFijo", AdditionalFields = "IdCodigoActivoFijo,SUBCAF,CAF,SUC", ErrorMessage = "El {0} ya existe.", HttpMethod = "POST")]
        public int Consecutivo { get; set; }

        //Propiedades Virtuales Referencias a otras clases
        public virtual ICollection<RecepcionActivoFijoDetalle> RecepcionActivoFijoDetalle { get; set; }

        public virtual ICollection<TransferenciaActivoFijoDetalle> TransferenciaActivoFijoDetalle { get; set; }
    }
}
