namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class IndiceOcupacional
    {
        [Key]
        public int IdIndiceOcupacional { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Dependencia:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int? IdDependencia { get; set; }
        public virtual Dependencia Dependencia { get; set; }

        [Display(Name = "Manual del puesto:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int? IdManualPuesto { get; set; }
        //public virtual ManualPuesto ManualPuesto { get; set; }

        [Display(Name = "Rol del puesto:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int? IdRolPuesto { get; set; }

        [Display(Name = "Escala de grados:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int? IdEscalaGrados { get; set; }

        public virtual ICollection<IndiceOcupacionalModalidadPartida> IndiceOcupacionalModalidadPartida { get; set; }
    }
}
