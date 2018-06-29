using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class OrdenCompraDetalles
    {
        [Key]
        [Column(Order = 0)]
        [Display(Name = "Orden de compra:")]
        [Required(ErrorMessage = "Debe seleccionar la {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0}")]
        public int IdOrdenCompra { get; set; }
        public virtual OrdenCompra OrdenCompra { get; set; }

        [Key]
        [Column(Order = 1)]
        [Display(Name = "Maestro de artículo de sucursal:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdMaestroArticuloSucursal { get; set; }
        public virtual MaestroArticuloSucursal MaestroArticuloSucursal { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Valor unitario:")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal ValorUnitario { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Cantidad:")]
        public int Cantidad { get; set; }
    }
}
