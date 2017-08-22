namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class CapacitacionModalidad
    {
        [Key]
        public int IdCapacitacionModalidad { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Modalidad de capacitación:")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Descripcion { get; set; }


        //Propiedades Virtuales Referencias a otras clases

        public virtual ICollection<CapacitacionTemarioProveedor> CapacitacionTemarioProveedor { get; set; }

        public virtual ICollection<CapacitacionPlanificacion> CapacitacionPlanificacion { get; set; }
    }
}
