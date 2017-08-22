namespace bd.webapprm.entidades
{
    using System.ComponentModel.DataAnnotations;

    public partial class CapacitacionTemarioProveedor
    {
        [Key]
        public int IdCapacitacionTemarioProveedor { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Serie:")]
        [Range(1, 100, ErrorMessage = "El {0} debe ser mayor a {1} y menor de {2} ")]
        public int NumeroHoras { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Costo:")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Costo { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Temario:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdCapacitacionTemario { get; set; }
        public virtual CapacitacionTemario CapacitacionTemario { get; set; }

        [Display(Name = "Proveedor:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdCapacitacionProveedor { get; set; }
        public virtual CapacitacionProveedor CapacitacionProveedor { get; set; }

        [Display(Name = "Modalidad:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdCapacitacionModalidad { get; set; }
        public virtual CapacitacionModalidad CapacitacionModalidad { get; set; }

    }
}
