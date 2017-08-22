namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
     
     

    public partial class TransferenciaActivoFijo
    {
        [Key]
        public int IdTransferenciaActivoFijo { get; set; }

        public int IdEmpleado { get; set; }

        //public int IdEmpleadoResponsableEnvio { get; set; }

        //public int IdEmpleadoResponsableRecibo { get; set; }

        public DateTime FechaTransferencia { get; set; }

        [StringLength(50)]
        public string Origen { get; set; }

        [Required(ErrorMessage = "Debe introducir  {0}")]
        [StringLength(50)]
        public string Destino { get; set; }

        public string Observaciones { get; set; }

        public virtual Empleado Empleado { get; set; }

        //public virtual Empleado Empleado1 { get; set; }

        //public virtual Empleado Empleado2 { get; set; }

        public virtual ICollection<TransferenciaActivoFijoDetalle> TransferenciaActivoFijoDetalle { get; set; }
    }
}
