namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class ActivoFijoMotivoBaja
    {
        [Key]
        public int IdActivoFijoMotivoBaja { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Motivo de Baja:")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Nombre { get; set; }

        public virtual ICollection<ActivosFijosBaja> ActivosFijosBaja { get; set; }
    }
}
