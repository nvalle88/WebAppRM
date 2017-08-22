namespace bd.webapprm.entidades
{
    using System.ComponentModel.DataAnnotations;

    public partial class RecepcionActivoFijoDetalle
    {
        [Key]
        public int IdRecepcionActivoFijoDetalle { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Número de póliza:")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string NumeroPoliza { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Recepción de activo fijo:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdRecepcionActivoFijo { get; set; }
        public virtual RecepcionActivoFijo RecepcionActivoFijo { get; set; }

        [Display(Name = "ActivoFijo:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdActivoFijo { get; set; }
        public virtual ActivoFijo ActivoFijo { get; set; }

        [Display(Name = "Estado:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEstado { get; set; }
        public virtual Estado Estado { get; set; }
    }
}
