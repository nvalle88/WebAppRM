namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
     
    public partial class SolicitudProveduria
    {
        [Key]
        public int IdSolicitudProveduria { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Empleado:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEmpleado { get; set; }
        public virtual Empleado Empleado { get; set; }

        public virtual ICollection<SolicitudProveduriaDetalle> SolicitudProveduriaDetalle { get; set; }
    }
}
