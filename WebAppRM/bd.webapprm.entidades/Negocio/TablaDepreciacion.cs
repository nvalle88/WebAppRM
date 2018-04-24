namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
     
    public partial class TablaDepreciacion
    {
        [Key]
        public int IdTablaDepreciacion { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Índice de Depreciación:")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal IndiceDepreciacion { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        public virtual ICollection<ClaseActivoFijo> ClaseActivoFijo { get; set; }
    }
}
