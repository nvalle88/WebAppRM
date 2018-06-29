namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Empleado
    {
        public Empleado()
        {
            AjusteInventarioArticulos = new HashSet<AjusteInventarioArticulos>();
            Bodega = new HashSet<Bodega>();
            TransferenciasActivoFijoEmpleadoResponsableEnvio = new HashSet<TransferenciaActivoFijo>();
            TransferenciasActivoFijoEmpleadoResponsableRecibo = new HashSet<TransferenciaActivoFijo>();
            UbicacionActivoFijo = new HashSet<UbicacionActivoFijo>();
            MantenimientoActivoFijo = new HashSet<MantenimientoActivoFijo>();
            MovilizacionesActivoFijoEmpleadoAutorizado = new HashSet<MovilizacionActivoFijo>();
            MovilizacionesActivoFijoEmpleadoResponsable = new HashSet<MovilizacionActivoFijo>();
            MovilizacionesActivoFijoEmpleadoSolicita = new HashSet<MovilizacionActivoFijo>();
            OrdenCompra = new HashSet<OrdenCompra>();
            RecepcionArticulos = new HashSet<RecepcionArticulos>();
            RequerimientoArticulos = new HashSet<RequerimientoArticulos>();
            SalidaArticulosEmpleadosDespacho = new HashSet<SalidaArticulos>();
            SalidaArticulosEmpleadosRealizanBaja = new HashSet<SalidaArticulos>();
        }

        [Key]
        public int IdEmpleado { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Fecha de ingreso:")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime FechaIngreso { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Fecha de ingreso al sector público:")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? FechaIngresoSectorPublico { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "¿Trabajó en la Superintendencia de InstitucionesFinancieras?")]
        public bool TrabajoSuperintendenciaBanco { get; set; }

        public bool FondosReservas { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "¿Declaración jurada?")]
        public bool DeclaracionJurada { get; set; }

        [Display(Name = "Ingreso por otra actividad:")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "El {0} no puede tener más de {1} y menos de {2}")]
        public string IngresosOtraActividad { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Mesees de imposición:")]
        [Range(0, 600, ErrorMessage = "El {0} debe estar entre {1} y {2} ")]
        public int MesesImposiciones { get; set; }

        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Días de imposición:")]
        [Range(0, 31, ErrorMessage = "El {0} debe estar entre {1} y {2} ")]
        public int DiasImposiciones { get; set; }

        //Propiedades Virtuales Referencias a otras clases

        [Display(Name = "Persona:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdPersona { get; set; }
        public virtual Persona Persona { get; set; }

        [Display(Name = "Ciudad de nacimiento:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdCiudadLugarNacimiento { get; set; }
        public virtual Ciudad CiudadNacimiento { get; set; }

        [Display(Name = "Provincia de sufragio:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int IdProvinciaLugarSufragio { get; set; }
        public virtual Provincia ProvinciaSufragio { get; set; }

        [Display(Name = "Dependencia:")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe seleccionar el {0} ")]
        public int? IdDependencia { get; set; }
        public virtual Dependencia Dependencia { get; set; }

        public virtual ICollection<AjusteInventarioArticulos> AjusteInventarioArticulos { get; set; }
        public virtual ICollection<Bodega> Bodega { get; set; }
        public virtual ICollection<TransferenciaActivoFijo> TransferenciasActivoFijoEmpleadoResponsableEnvio { get; set; }
        public virtual ICollection<TransferenciaActivoFijo> TransferenciasActivoFijoEmpleadoResponsableRecibo { get; set; }
        public virtual ICollection<UbicacionActivoFijo> UbicacionActivoFijo { get; set; }
        public virtual ICollection<MantenimientoActivoFijo> MantenimientoActivoFijo { get; set; }
        public virtual ICollection<MovilizacionActivoFijo> MovilizacionesActivoFijoEmpleadoAutorizado { get; set; }
        public virtual ICollection<MovilizacionActivoFijo> MovilizacionesActivoFijoEmpleadoResponsable { get; set; }
        public virtual ICollection<MovilizacionActivoFijo> MovilizacionesActivoFijoEmpleadoSolicita { get; set; }
        public virtual ICollection<OrdenCompra> OrdenCompra { get; set; }
        public virtual ICollection<RecepcionArticulos> RecepcionArticulos { get; set; }
        public virtual ICollection<RequerimientoArticulos> RequerimientoArticulos { get; set; }
        public virtual ICollection<SalidaArticulos> SalidaArticulosEmpleadosDespacho { get; set; }
        public virtual ICollection<SalidaArticulos> SalidaArticulosEmpleadosRealizanBaja { get; set; }
    }
}
