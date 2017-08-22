namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class CapacitacionEncuesta
    {
        [Key]
        public string IdCapacitacionEncuesta { get; set; }



        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Capacitación encuesta:")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Descripcion { get; set; }


        
        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Capacitación recibida")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdCapacitacionRecibida { get; set; }
        public virtual CapacitacionRecibida CapacitacionRecibida { get; set; }

        [Display(Name = "Servidor público")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleado { get; set; }
        public virtual Empleado Empleado { get; set; }

        public virtual ICollection<CapacitacionPregunta> CapacitacionPregunta { get; set; }

       

    }
}
