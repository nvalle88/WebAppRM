namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class ActividadesEsenciales
    {
        [Key]
        public int ActividadesEsencialesId { get; set; }

        [Required(ErrorMessage = "Debe introducir  {0}")]
        [Display(Name = "Actiividades eseneciales:")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Descripcion { get; set; }


        //Propiedades Virtuales
        public virtual ICollection<IndiceOcupacionalActividadesEsenciales> IndiceOcupacionalActividadesEsenciales { get; set; }
    }
}
