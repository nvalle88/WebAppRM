namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Proceso
    {
        [Key]
        public int IdProceso { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Proceso:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public string Nombre { get; set; }

        public virtual ICollection<ProcesoDetalle> ProcesoDetalle { get; set; }
    }
}
