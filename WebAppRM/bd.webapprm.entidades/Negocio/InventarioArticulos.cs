using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class InventarioArticulos
    {
        [Key]
        public int IdInventarioArticulos { get; set; }

        [Display(Name = "Maestro de artículo de sucursal:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdMaestroArticuloSucursal { get; set; }
        public virtual MaestroArticuloSucursal MaestroArticuloSucursal { get; set; }

        [Display(Name = "Bodega:")]
        [Required(ErrorMessage = "Debe seleccionar la {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0} ")]
        public int IdBodega { get; set; }
        public virtual Bodega Bodega { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Cantidad:")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Fecha { get; set; }
    }
}
