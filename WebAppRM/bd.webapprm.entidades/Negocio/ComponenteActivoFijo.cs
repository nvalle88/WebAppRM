using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace bd.webapprm.entidades
{
    public partial class ComponenteActivoFijo
    {
        [Key]
        public int IdComponenteActivoFijo { get; set; }

        [Display(Name = "Activo Fijo de origen:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdRecepcionActivoFijoDetalleOrigen { get; set; }
        public virtual RecepcionActivoFijoDetalle RecepcionActivoFijoDetalleOrigen { get; set; }

        [Display(Name = "Componente de Activo Fijo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdRecepcionActivoFijoDetalleComponente { get; set; }
        public virtual RecepcionActivoFijoDetalle RecepcionActivoFijoDetalleComponente { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Fecha de adición:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime FechaAdicion { get; set; }
    }
}
