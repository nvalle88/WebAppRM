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
            AjusteInventarioArticulos = new HashSet<AjusteInventarioArticulos>();
            Dependencia = new HashSet<Dependencia>();
            InventarioArticulos = new HashSet<InventarioArticulos>();
            OrdenCompra = new HashSet<OrdenCompra>();
            UbicacionActivoFijo = new HashSet<UbicacionActivoFijo>();
        }

        [Key]
        public int IdBodega { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Bodega:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "La {0} no puede tener más de {1} y menos de {2}")]
        public string Nombre { get; set; }

        [Display(Name = "Sucursal:")]
        [Required(ErrorMessage = "Debe seleccionar la {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0}")]
        public int IdSucursal { get; set; }
        public virtual Sucursal Sucursal { get; set; }

        [Display(Name = "Custodio:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdEmpleadoResponsable { get; set; }
        public virtual Empleado EmpleadoResponsable { get; set; }

        public virtual ICollection<AjusteInventarioArticulos> AjusteInventarioArticulos { get; set; }
        public virtual ICollection<Dependencia> Dependencia { get; set; }
        public virtual ICollection<InventarioArticulos> InventarioArticulos { get; set; }
        public virtual ICollection<OrdenCompra> OrdenCompra { get; set; }
        public virtual ICollection<UbicacionActivoFijo> UbicacionActivoFijo { get; set; }
    }
}
