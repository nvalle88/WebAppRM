namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;


    public partial class CapacitacionTemario
    {
        [Key]
        public int IdCapacitacionTemario { get; set; }


        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Tema:")]
        [StringLength(250, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Tema { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Área de conocimiento:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdCapacitacionAreaConocimiento { get; set; }
        public virtual CapacitacionAreaConocimiento CapacitacionAreaConocimiento { get; set; }

        public virtual ICollection<CapacitacionPlanificacion> CapacitacionPlanificacion { get; set; }

        public virtual ICollection<CapacitacionRecibida> CapacitacionRecibida { get; set; }

        public virtual ICollection<CapacitacionTemarioProveedor> CapacitacionTemarioProveedor { get; set; }
    }
}
