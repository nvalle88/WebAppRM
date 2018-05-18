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
            RecepcionActivoFijoDetalleAltaActivoFijo = new HashSet<RecepcionActivoFijoDetalleAltaActivoFijo>();
        }

        [Key]
        public int IdRecepcionActivoFijoDetalle { get; set; }
        
        [Display(Name = "Serie:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string Serie { get; set; }

        [Display(Name = "Número de chasis:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string NumeroChasis { get; set; }

        [Display(Name = "Número de motor:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string NumeroMotor { get; set; }

        [Display(Name = "Placa:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "La {0} no puede tener más de {1} y menos de {2}")]
        public string Placa { get; set; }

        [Display(Name = "Número de clave catastral:")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string NumeroClaveCatastral { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Recepción de activo fijo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdRecepcionActivoFijo { get; set; }
        public virtual RecepcionActivoFijo RecepcionActivoFijo { get; set; }

        [Display(Name = "ActivoFijo:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdActivoFijo { get; set; }
        public virtual ActivoFijo ActivoFijo { get; set; }

        [Display(Name = "Estado:")]
        [Required(ErrorMessage = "Debe seleccionar el {0} ")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdEstado { get; set; }
        public virtual Estado Estado { get; set; }

        [NotMapped]
        public UbicacionActivoFijo UbicacionActivoFijoActual { get; set; }

        [NotMapped]
        public Sucursal SucursalActual { get; set; }

        [NotMapped]
        public AltaActivoFijo AltaActivoFijoActual { get; set; }

        public virtual BajaActivoFijo BajaActivoFijo { get; set; }
        public virtual ICollection<ComponenteActivoFijo> ComponentesActivoFijoComponente { get; set; }
        public virtual ICollection<ComponenteActivoFijo> ComponentesActivoFijoOrigen { get; set; }
        public virtual ICollection<DepreciacionActivoFijo> DepreciacionActivoFijo { get; set; }
        public virtual ICollection<DocumentoActivoFijo> DocumentoActivoFijo { get; set; }
        public virtual ICollection<MantenimientoActivoFijo> MantenimientoActivoFijo { get; set; }
        public virtual ICollection<UbicacionActivoFijo> UbicacionActivoFijo { get; set; }
        public virtual ICollection<RecepcionActivoFijoDetalleAltaActivoFijo> RecepcionActivoFijoDetalleAltaActivoFijo { get; set; }
    }
}
