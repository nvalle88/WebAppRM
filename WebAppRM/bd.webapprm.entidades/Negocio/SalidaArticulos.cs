using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace bd.webapprm.entidades
{
    public partial class SalidaArticulos
    {
        [Key]
        public int IdSalidaArticulos { get; set; }

        [Display(Name = "Motivo de salida:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdMotivoSalidaArticulos { get; set; }
        public virtual MotivoSalidaArticulos MotivoSalidaArticulos { get; set; }

        [Display(Name = "Descripción del motivo:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "La {0} no puede tener más de {1} y menos de {2}")]
        public string DescripcionMotivo { get; set; }

        [Display(Name = "Empleado que realiza la baja:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int? IdEmpleadoRealizaBaja { get; set; }
        public virtual Empleado EmpleadoRealizaBaja { get; set; }

        [Display(Name = "Proveedor al que se realiza la devolución:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int? IdProveedorDevolucion { get; set; }
        public virtual Proveedor ProveedorDevolucion { get; set; }

        [Display(Name = "Empleado que realiza el despacho:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int? IdEmpleadoDespacho { get; set; }
        public virtual Empleado EmpleadoDespacho { get; set; }

        [Display(Name = "Requerimiento de artículo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdRequerimientoArticulos { get; set; }
        public virtual RequerimientoArticulos RequerimientoArticulos { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Fecha de salida:")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime FechaSalida { get; set; }
    }
}
