namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class CapacitacionRecibida
    {
        [Key]
        public int IdCapacitacionRecibida { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Capacitación temario:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdCapacitacionTemario { get; set; }
        public virtual CapacitacionTemario CapacitacionTemario { get; set; }

        [Display(Name = "Empleado:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleado { get; set; }
        public virtual Empleado Empleado { get; set; }

        public virtual ICollection<CapacitacionEncuesta> CapacitacionEncuesta { get; set; }

        public virtual ICollection<CapacitacionProveedor> CapacitacionProveedor { get; set; }

        
    }
}
