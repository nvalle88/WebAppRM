namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Estado
    {
        public Estado()
        {
            RecepcionActivoFijoDetalle = new HashSet<RecepcionActivoFijoDetalle>();
            SolicitudProveeduriaDetalle = new HashSet<SolicitudProveeduriaDetalle>();
            TransferenciaActivoFijo = new HashSet<TransferenciaActivoFijo>();
        }

        [Key]
        public int IdEstado { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Estado:")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Nombre { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        public virtual ICollection<RecepcionActivoFijoDetalle> RecepcionActivoFijoDetalle { get; set; }

        public virtual ICollection<SolicitudProveeduriaDetalle> SolicitudProveeduriaDetalle { get; set; }

        public virtual ICollection<TransferenciaActivoFijo> TransferenciaActivoFijo { get; set; }
    }
}
