namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class RecepcionArticulos
    {
        [Key]
        public int IdRecepcionArticulos { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Cantidad:")]
        [Range(1, double.MaxValue, ErrorMessage = "La {0} tiene que estar entre {1} y {2}.")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha de recepción:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime Fecha { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Servidor público:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdEmpleado { get; set; }
        public virtual Empleado Empleado { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Artículo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdArticulo { get; set; }
        public virtual Articulo Articulo { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Maestro de artículo de la sucursal:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdMaestroArticuloSucursal { get; set; }
        public virtual MaestroArticuloSucursal MaestroArticuloSucursal { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Proveedor:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdProveedor { get; set; }
        public virtual Proveedor Proveedor { get; set; }

        public virtual ICollection<AltaProveeduria> AltaProveeduria { get; set; }
    }
}
