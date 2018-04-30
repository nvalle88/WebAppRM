namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class LibroActivoFijo
    {
        [Key]
        public int IdLibroActivoFijo { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Sucursal:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0} ")]
        public int? IdSucursal { get; set; }
        public virtual Sucursal Sucursal { get; set; }

        public virtual ICollection<ActivoFijo> ActivoFijo { get; set; }

        public virtual ICollection<RecepcionActivoFijo> RecepcionActivoFijo { get; set; }
    }
}
