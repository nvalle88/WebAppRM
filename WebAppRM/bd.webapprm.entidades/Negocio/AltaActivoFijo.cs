namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class AltaActivoFijo
    {
        public AltaActivoFijo()
        {
            DocumentoActivoFijo = new HashSet<DocumentoActivoFijo>();
        }

        [Key]
        public int IdAltaActivoFijo { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha de Alta:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime FechaAlta { get; set; }

        [Display(Name = "Motivo de alta:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdMotivoAlta { get; set; }
        public virtual MotivoAlta MotivoAlta { get; set; }

        [Display(Name = "Número de factura:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int? IdFacturaActivoFijo { get; set; }
        public virtual FacturaActivoFijo FacturaActivoFijo { get; set; }

        public virtual ICollection<RecepcionActivoFijoDetalleAltaActivoFijo> RecepcionActivoFijoDetalleAltaActivoFijo { get; set; }
        public virtual ICollection<DocumentoActivoFijo> DocumentoActivoFijo { get; set; }
    }
}
