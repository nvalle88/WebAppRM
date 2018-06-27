using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class CategoriaActivoFijo
    {
        public CategoriaActivoFijo()
        {
            ClaseActivoFijo = new HashSet<ClaseActivoFijo>();
        }

        [Key]
        public int IdCategoriaActivoFijo { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Nombre:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "La {0} no puede tener más de {1} y menos de {2}")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Porcentaje de depreciación anual:")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal PorCientoDepreciacionAnual { get; set; }

        [Required(ErrorMessage = "Debe introducir los {0}")]
        [Display(Name = "Años de vida útil:")]
        public int AnosVidaUtil { get; set; }

        public virtual ICollection<ClaseActivoFijo> ClaseActivoFijo { get; set; }
    }
}
