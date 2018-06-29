namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Estado
    {
        public Estado()
        {
            InventarioActivoFijo = new HashSet<InventarioActivoFijo>();
            OrdenCompra = new HashSet<OrdenCompra>();
            RecepcionActivoFijoDetalle = new HashSet<RecepcionActivoFijoDetalle>();
            RequerimientoArticulos = new HashSet<RequerimientoArticulos>();
            TransferenciaActivoFijo = new HashSet<TransferenciaActivoFijo>();
        }

        [Key]
        public int IdEstado { get; set; }

        [Required(ErrorMessage = "Debe introducir el {0}")]
        [Display(Name = "Estado:")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Nombre { get; set; }

        //Propiedades Virtuales Referencias a otras clases
        public virtual ICollection<InventarioActivoFijo> InventarioActivoFijo { get; set; }
        public virtual ICollection<OrdenCompra> OrdenCompra { get; set; }
        public virtual ICollection<RecepcionActivoFijoDetalle> RecepcionActivoFijoDetalle { get; set; }
        public virtual ICollection<RequerimientoArticulos> RequerimientoArticulos { get; set; }
        public virtual ICollection<TransferenciaActivoFijo> TransferenciaActivoFijo { get; set; }
    }
}
