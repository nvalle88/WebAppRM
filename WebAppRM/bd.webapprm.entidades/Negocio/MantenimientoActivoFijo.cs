namespace bd.webapprm.entidades
{
    using System;
    using System.ComponentModel.DataAnnotations;



    public partial class MantenimientoActivoFijo
    {
        [Key]
        public int IdMantenimientoActivoFijo { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Fecha de mantenimiento:")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaMantenimiento { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Fecha inicial:")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaDesde { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Fecha final:")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaHasta { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Valor del mantenimiento:")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Observaciones:")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Observaciones { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Empleado:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleado { get; set; }
        public virtual Empleado Empleado { get; set; }

        [Display(Name = "Activo fijo:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdActivoFijo { get; set; }
        public virtual ActivoFijo ActivoFijo { get; set; }
    }
}
