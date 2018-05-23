using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades
{
    public partial class LineaServicio
    {
        public LineaServicio()
        {
            Proveedor = new HashSet<Proveedor>();
        }

        [Key]
        public int IdLineaServicio { get; set; }

        [Required(ErrorMessage = "Debe introducir la {0}")]
        [Display(Name = "Linea de servicio:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "La {0} no puede tener más de {1} y menos de {2}")]
        public string Nombre { get; set; }

        public virtual ICollection<Proveedor> Proveedor { get; set; }
    }
}
