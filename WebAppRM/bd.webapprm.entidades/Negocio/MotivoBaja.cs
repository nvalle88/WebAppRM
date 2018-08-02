namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class MotivoBaja
    {
        [Key]
        public int IdMotivoBaja { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Motivo de baja:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Nombre { get; set; }

        public virtual ICollection<BajaActivoFijo> BajaActivosFijos { get; set; }
    }
}
