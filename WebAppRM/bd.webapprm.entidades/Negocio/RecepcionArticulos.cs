namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class RecepcionArticulos
    {
        public RecepcionArticulos()
        {
            OrdenCompraRecepcionArticulos = new HashSet<OrdenCompraRecepcionArticulos>();
        }

        [Key]
        public int IdRecepcionArticulos { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha sin existencia:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime FechaRecepcion { get; set; }

        [Display(Name = "Bodega:")]
        [Required(ErrorMessage = "Debe seleccionar la {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0}")]
        public int IdBodega { get; set; }
        public virtual Bodega Bodega { get; set; }

        [Display(Name = "Motivo de recepción:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdMotivoRecepcionArticulos { get; set; }
        public virtual MotivoRecepcionArticulos MotivoRecepcionArticulos { get; set; }

        [Display(Name = "Empleado que devuelve:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int? IdEmpleadoDevolucion { get; set; }
        public virtual Empleado EmpleadoDevolucion { get; set; }

        public virtual ICollection<OrdenCompraRecepcionArticulos> OrdenCompraRecepcionArticulos { get; set; }
    }
}
