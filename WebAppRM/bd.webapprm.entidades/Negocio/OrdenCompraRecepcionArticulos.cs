using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class OrdenCompraRecepcionArticulos
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
        [Display(Name = "Recepción de artículos:")]
        [Required(ErrorMessage = "Debe seleccionar la {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0}")]
        public int IdRecepcionArticulos { get; set; }
        public virtual RecepcionArticulos RecepcionArticulos { get; set; }
    }
}
