namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    public partial class TranferenciaArticulo
    {
        [Key]
        public int IdTranferenciaArticulo { get; set; }

        [Display(Name = "Maestro de artículo de sucursal que envía:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdMaestroArticuloEnvia { get; set; }
        public virtual MaestroArticuloSucursal MaestroArticuloSucursalEnvia { get; set; }

        [Display(Name = "Maestro de artículo de sucursal que recibe:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdMaestroArticuloRecibe { get; set; }
        public virtual MaestroArticuloSucursal MaestroArticuloSucursalRecibe { get; set; }

        [Display(Name = "Artículo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdArticulo { get; set; }
        public virtual Articulo Articulo { get; set; }

        [Display(Name = "Empleado que envía:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleadoEnvia { get; set; }
        public virtual Empleado EmpleadoEnvia { get; set; }

        [Display(Name = "Empleado que recibe:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleadoRecibe { get; set; }
        public virtual Empleado EmpleadoRecibe { get; set; }

        [Display(Name = "Cantidad:")]
        public int? Cantidad { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha de transferencia:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? Fecha { get; set; }
    }
}
