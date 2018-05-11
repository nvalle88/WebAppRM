namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class CatalogoCuenta
    {
        [Key]
        public int IdCatalogoCuenta { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Código:")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        [RegularExpression(@"^[-A-Z0-9a-z-]*$", ErrorMessage = "El {0} tiene que ser alfanumérico.")]
        public string Codigo { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Catálogo de cuenta:")]
        [Range(0, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int? IdCatalogoCuentaHijo { get; set; }

        public virtual CatalogoCuenta CatalogoCuentaHijo { get; set; }

        public virtual ICollection<CatalogoCuenta> CatalogosCuenta { get; set; }

        public virtual ICollection<ConfiguracionContabilidad> ConfiguracionContabilidad { get; set; }

        public virtual ICollection<ConfiguracionContabilidad> ConfiguracionContabilidad1 { get; set; }
    }
}
