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
        [Column(Order = 0)]
        [Display(Name = "Artículo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdArticulo { get; set; }
        public virtual Articulo Articulo { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Cantidad solicitada:")]
        public int CantidadSolicitada { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Cantidad aprobada:")]
        public int CantidadAprobada { get; set; }
    }
}
