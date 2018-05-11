using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class Bodega
    {
        public Bodega()
        {
            UbicacionActivoFijo = new HashSet<UbicacionActivoFijo>();
        }

        [Key]
        public int IdBodega { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Bodega:")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "La {0} no puede tener más de {1} y menos de {2}")]
        public string Nombre { get; set; }

        [Display(Name = "Sucursal:")]
        [Required(ErrorMessage = "Debe seleccionar la {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0} ")]
        public int IdSucursal { get; set; }
        public virtual Sucursal Sucursal { get; set; }

        public virtual ICollection<UbicacionActivoFijo> UbicacionActivoFijo { get; set; }
    }
}
