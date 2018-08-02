using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class TransferenciaActivoFijoDetalle
    {
        [Key]
        [Column(Order = 0)]
        [Display(Name = "Detalle de recepción de activo fijo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdRecepcionActivoFijoDetalle { get; set; }
        public virtual RecepcionActivoFijoDetalle RecepcionActivoFijoDetalle { get; set; }

        [Key]
        [Column(Order = 1)]
        [Display(Name = "Transferencia de activo fijo:")]
        [Required(ErrorMessage = "Debe seleccionar la {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0}")]
        public int IdTransferenciaActivoFijo { get; set; }
        public virtual TransferenciaActivoFijo TransferenciaActivoFijo { get; set; }

        [Display(Name = "Ubicación de origen:")]
        [Required(ErrorMessage = "Debe seleccionar la {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0}")]
        public int IdUbicacionActivoFijoOrigen { get; set; }
        public virtual UbicacionActivoFijo UbicacionActivoFijoOrigen { get; set; }

        [Display(Name = "Ubicación de destino:")]
        [Required(ErrorMessage = "Debe seleccionar la {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0}")]
        public int IdUbicacionActivoFijoDestino { get; set; }
        public virtual UbicacionActivoFijo UbicacionActivoFijoDestino { get; set; }

        [Display(Name = "Código de activo fijo:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int? IdCodigoActivoFijo { get; set; }
        public virtual CodigoActivoFijo CodigoActivoFijo { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "¿Es Componente?")]
        public bool IsComponente { get; set; }
    }
}
