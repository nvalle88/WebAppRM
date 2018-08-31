namespace bd.webapprm.entidades
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class DepreciacionActivoFijo
    {
        [Key]
        public int IdDepreciacionActivoFijo { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha de depreciación:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime FechaDepreciacion { get; set; }

        [NotMapped]
        [Display(Name = "Fecha de revalorización:")]
        public DateTime FechaRevalorizacion { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Valor de compra:")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal ValorCompra { get; set; }

        [NotMapped]
        [Display(Name = "Valor de compra anterior:")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal ValorCompraAnterior { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Depreciación acumulada:")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal DepreciacionAcumulada { get; set; }
        
        [Display(Name = "Valor Residual:")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        [NotMapped]
        public decimal ValorResidual { get; set; }

        //Propiedades Virtuales Referencias a otras clases
        [Display(Name = "Detalle de recepción de activo fijo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdRecepcionActivoFijoDetalle { get; set; }
        public virtual RecepcionActivoFijoDetalle RecepcionActivoFijoDetalle { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "¿Es Revalorización?")]
        public bool IsRevalorizacion { get; set; }
    }
}
