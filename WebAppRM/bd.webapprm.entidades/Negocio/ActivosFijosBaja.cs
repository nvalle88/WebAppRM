namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using bd.webapprm.entidades;
    using System.ComponentModel.DataAnnotations;

    public partial class ActivosFijosBaja
    {
        [Key]
        public int IdBaja { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Fecha de Baja:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime FechaBaja { get; set; }


        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Motivo de Baja:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdMotivoBaja { get; set; }
        public virtual ActivoFijoMotivoBaja ActivoFijoMotivoBaja { get; set; }

        [Display(Name = "Activo Fijo:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdActivo { get; set; }
        public virtual ActivoFijo ActivoFijo { get; set; }

    }
}
