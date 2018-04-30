namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using bd.webapprm.entidades;
    using System.ComponentModel.DataAnnotations;

    public partial class BajaActivoFijoDetalle
    {
        [Key]
        public int IdActivoFijoBaja { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha de Baja:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime FechaBaja { get; set; }

        [Display(Name = "Número de memo, oficio o resolución:")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string MemoOficioResolucion { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Motivo de Baja:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdMotivoBaja { get; set; }
        public virtual MotivoBaja MotivoBaja { get; set; }

        [Display(Name = "Activo Fijo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdActivo { get; set; }
        public virtual ActivoFijo ActivoFijo { get; set; }
    }
}
