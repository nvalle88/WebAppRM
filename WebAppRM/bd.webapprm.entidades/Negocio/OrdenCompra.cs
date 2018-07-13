using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class OrdenCompra
    {
        public OrdenCompra()
        {
            OrdenCompraDetalles = new HashSet<OrdenCompraDetalles>();
        }

        [Key]
        public int IdOrdenCompra { get; set; }

        [Display(Name = "Motivo de recepción:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdMotivoRecepcionArticulos { get; set; }
        public virtual MotivoRecepcionArticulos MotivoRecepcionArticulos { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Fecha:")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Fecha { get; set; }

        [Display(Name = "Estado:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdEstado { get; set; }
        public virtual Estado Estado { get; set; }

        [Display(Name = "Factura:")]
        [Required(ErrorMessage = "Debe seleccionar la {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0}")]
        public int IdFacturaActivoFijo { get; set; }
        public virtual FacturaActivoFijo Factura { get; set; }

        [Display(Name = "Empleado responsable:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdEmpleadoResponsable { get; set; }
        public virtual Empleado EmpleadoResponsable { get; set; }

        [Display(Name = "Bodega:")]
        [Required(ErrorMessage = "Debe seleccionar la {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0}")]
        public int IdBodega { get; set; }
        public virtual Bodega Bodega { get; set; }

        [Display(Name = "Proveedor:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int? IdProveedor { get; set; }
        public virtual Proveedor Proveedor { get; set; }

        [Display(Name = "Empleado que devuelve:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int? IdEmpleadoDevolucion { get; set; }
        public virtual Empleado EmpleadoDevolucion { get; set; }

        [Display(Name = "Código:")]
        [Required(ErrorMessage = "Debe introducir el {0}")]
        [RegularExpression(@"^\d*$", ErrorMessage = "El {0} solo puede contener números.")]
        public string Codigo { get; set; }

        public virtual ICollection<OrdenCompraDetalles> OrdenCompraDetalles { get; set; }
    }
}
