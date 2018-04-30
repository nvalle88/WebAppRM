namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Ciudad
    {
        [Key]
        public int IdCiudad { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Ciudad:")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Nombre { get; set; }

        [NotMapped]
        public int IdPais { get; set; }
        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Provincia:")]
        [Required(ErrorMessage = "Debe seleccionar la {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar la {0} ")]
        public int IdProvincia { get; set; }
        public virtual Provincia Provincia { get; set; }

        public virtual ICollection<ActivoFijo> ActivoFijo { get; set; }

        public virtual ICollection<Sucursal> Sucursal { get; set; }

        public virtual ICollection<Parroquia> Parroquia { get; set; }

        public virtual ICollection<ItinerarioViatico> ItinerarioViatico { get; set; }

        public virtual ICollection<Empleado> Empleado { get; set; }
    }
}
