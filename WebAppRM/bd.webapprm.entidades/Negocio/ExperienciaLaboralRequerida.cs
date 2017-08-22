namespace bd.webapprm.entidades
{
    using System.ComponentModel.DataAnnotations;

    public partial class ExperienciaLaboralRequerida
    {
        [Key]
        public int IdExperienciaLaboralRequerida { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Índice Ocupacional Capacitaciones:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdIndiceOcupacionalCapacitaciones { get; set; }
        public virtual IndiceOcupacionalCapacitaciones IndiceOcupacionalCapacitaciones { get; set; }

        [Display(Name = "Especificidad de experiencia:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEspecificidadExperiencia { get; set; }
        public virtual EspecificidadExperiencia EspecificidadExperiencia { get; set; }

        [Display(Name = "Año de experiencia:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdAnoExperiencia { get; set; }
        public virtual AnoExperiencia AnoExperiencia { get; set; }

        [Display(Name = "Estudio:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEstudio { get; set; }
        public virtual Estudio Estudio { get; set; }

    }
}
