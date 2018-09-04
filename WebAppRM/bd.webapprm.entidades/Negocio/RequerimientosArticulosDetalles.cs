using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class RequerimientosArticulosDetalles
    {
        [Key]
        [Column(Order = 0)]
        [Display(Name = "Requerimiento de artículo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdRequerimientosArticulos { get; set; }
        public virtual RequerimientoArticulos RequerimientoArticulos { get; set; }

        [Key]
        [Column(Order = 1)]
        [Display(Name = "Maestro de artículo de sucursal:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdMaestroArticuloSucursal { get; set; }
        public virtual MaestroArticuloSucursal MaestroArticuloSucursal { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Cantidad solicitada:")]
        public int CantidadSolicitada { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Cantidad aprobada:")]
        public int CantidadAprobada { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Cantidad entregada:")]
        public int CantidadEntregada { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Valor de artículo:")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal ValorActual { get; set; }

        [NotMapped]
        [Display(Name = "Cantidad en bodega:")]
        public int CantidadBodega { get; set; }
    }
}
