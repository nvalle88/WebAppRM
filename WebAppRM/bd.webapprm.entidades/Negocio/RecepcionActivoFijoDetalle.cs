namespace bd.webapprm.entidades
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class RecepcionActivoFijoDetalle
    {
        public RecepcionActivoFijoDetalle()
        {
            ComponentesActivoFijoComponente = new HashSet<ComponenteActivoFijo>();
            ComponentesActivoFijoOrigen = new HashSet<ComponenteActivoFijo>();
            DepreciacionActivoFijo = new HashSet<DepreciacionActivoFijo>();
            DocumentoActivoFijo = new HashSet<DocumentoActivoFijo>();
            MantenimientoActivoFijo = new HashSet<MantenimientoActivoFijo>();
            UbicacionActivoFijo = new HashSet<UbicacionActivoFijo>();
            AltaActivoFijoDetalle = new HashSet<AltaActivoFijoDetalle>();
            BajaActivoFijoDetalle = new HashSet<BajaActivoFijoDetalle>();
            TransferenciaActivoFijoDetalle = new HashSet<TransferenciaActivoFijoDetalle>();
            ProcesoJudicialActivoFijo = new HashSet<ProcesoJudicialActivoFijo>();
            InventarioActivoFijoDetalle = new HashSet<InventarioActivoFijoDetalle>();
            MovilizacionActivoFijoDetalle = new HashSet<MovilizacionActivoFijoDetalle>();
        }

        [Key]
        public int IdRecepcionActivoFijoDetalle { get; set; }
        
        [Display(Name = "Serie:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "La {0} no puede tener más de {1} y menos de {2}")]
        [RegularExpression(@"^[-A-Z0-9a-z-]*$", ErrorMessage = "La {0} tiene que ser alfanumérica.")]
        public string Serie { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Recepción de activo fijo:")]
        [Required(ErrorMessage = "Debe seleccionar la {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdRecepcionActivoFijo { get; set; }
        public virtual RecepcionActivoFijo RecepcionActivoFijo { get; set; }

        [Display(Name = "Descripción de activo fijo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdActivoFijo { get; set; }
        public virtual ActivoFijo ActivoFijo { get; set; }

        [Display(Name = "Estado:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdEstado { get; set; }
        public virtual Estado Estado { get; set; }

        [Display(Name = "Código de activo fijo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0}")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0}")]
        public int IdCodigoActivoFijo { get; set; }
        public virtual CodigoActivoFijo CodigoActivoFijo { get; set; }

        [NotMapped]
        public UbicacionActivoFijo UbicacionActivoFijoActual { get; set; }

        [NotMapped]
        public Sucursal SucursalActual { get; set; }

        [NotMapped]
        public AltaActivoFijo AltaActivoFijoActual { get; set; }

        [NotMapped]
        public BajaActivoFijo BajaActivoFijoActual { get; set; }

        public virtual RecepcionActivoFijoDetalleEdificio RecepcionActivoFijoDetalleEdificio { get; set; }
        public virtual RecepcionActivoFijoDetalleVehiculo RecepcionActivoFijoDetalleVehiculo { get; set; }
        public virtual ICollection<ComponenteActivoFijo> ComponentesActivoFijoComponente { get; set; }
        public virtual ICollection<ComponenteActivoFijo> ComponentesActivoFijoOrigen { get; set; }
        public virtual ICollection<DepreciacionActivoFijo> DepreciacionActivoFijo { get; set; }
        public virtual ICollection<DocumentoActivoFijo> DocumentoActivoFijo { get; set; }
        public virtual ICollection<MantenimientoActivoFijo> MantenimientoActivoFijo { get; set; }
        public virtual ICollection<UbicacionActivoFijo> UbicacionActivoFijo { get; set; }
        public virtual ICollection<AltaActivoFijoDetalle> AltaActivoFijoDetalle { get; set; }
        public virtual ICollection<BajaActivoFijoDetalle> BajaActivoFijoDetalle { get; set; }
        public virtual ICollection<TransferenciaActivoFijoDetalle> TransferenciaActivoFijoDetalle { get; set; }
        public virtual ICollection<ProcesoJudicialActivoFijo> ProcesoJudicialActivoFijo { get; set; }
        public virtual ICollection<InventarioActivoFijoDetalle> InventarioActivoFijoDetalle { get; set; }
        public virtual ICollection<MovilizacionActivoFijoDetalle> MovilizacionActivoFijoDetalle { get; set; }
    }
}
