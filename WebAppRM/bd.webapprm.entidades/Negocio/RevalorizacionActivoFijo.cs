using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class RevalorizacionActivoFijo
    {
        [Key]
        public int IdRevalorizacionActivoFijo { get; set; }

        [NotMapped]
        [Display(Name = "Valor de compra anterior:")]
        public decimal ValorCompraAnterior { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Valor de compra:")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal ValorCompra { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha de revalorización:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime FechaRevalorizacion { get; set; }

        [Display(Name = "Detalle de recepción de activo fijo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdRecepcionActivoFijoDetalle { get; set; }
        public virtual RecepcionActivoFijoDetalle RecepcionActivoFijoDetalle { get; set; }
    }
}
