namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Marca
    {
        [Key]
        public int IdMarca { get; set; }
        [Required(ErrorMessage = "Debe introducir  {0}")]
        [Display(Name ="Marca:")]
        [StringLength(50,MinimumLength =2,ErrorMessage ="El {0} no puede tener menos de {2} y más de {1}")]
        public string Nombre { get; set; }

        public virtual ICollection<Modelo> Modelo { get; set; }
    }
}
