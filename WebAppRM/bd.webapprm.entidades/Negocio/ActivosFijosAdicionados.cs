namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class ActivosFijosAdicionados
    {
        [Key]
        public int idAdicion { get; set; }

        public int idActivoFijoOrigen { get; set; }
        public virtual ActivoFijo ActivoFijo { get; set; }

        public int idActivoFijoDestino { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Fecha de Adición:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime fechaAdicion { get; set; }
    }
}
