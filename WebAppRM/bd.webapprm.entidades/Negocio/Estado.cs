namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Estado
    {
        [Key]
        public int IdEstado { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Estado:")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Nombre { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        public virtual ICollection<SolicitudPermiso> SolicitudPermiso { get; set; }

        public virtual ICollection<SolicitudVacaciones> SolicitudVacaciones { get; set; }

        public virtual ICollection<SolicitudViatico> SolicitudViatico { get; set; }

        public virtual ICollection<SolicitudModificacionFichaEmpleado> SolicitudModificacionFichaEmpleado { get; set; }

        public virtual ICollection<SolicitudAnticipo> SolicitudAnticipo { get; set; }

        public virtual ICollection<RecepcionActivoFijoDetalle> RecepcionActivoFijoDetalle { get; set; }

        public virtual ICollection<SolicitudCertificadoPersonal> SolicitudCertificadoPersonal { get; set; }

        public virtual ICollection<SolicitudProveeduriaDetalle> SolicitudProveeduriaDetalle { get; set; }

        public virtual ICollection<SolicitudPlanificacionVacaciones> SolicitudPlanificacionVacaciones { get; set; }
    }
}
