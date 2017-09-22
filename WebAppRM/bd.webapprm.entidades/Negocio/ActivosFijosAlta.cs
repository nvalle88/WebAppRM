namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class ActivosFijosAlta
    {
        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Fecha de Alta:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime FechaAlta { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Número de Factura:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdFactura { get; set; }
        public virtual Factura Factura { get; set; }

        [Key]
        public int IdActivoFijo { get; set; }
        public virtual ActivoFijo ActivoFijo { get; set; }

    }
}
