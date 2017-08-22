namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
     
     

    public partial class TransferenciaActivoFijoDetalle
    {
        [Key]
        public int IdTransferenciaActivoFijoDetalle { get; set; }

        public int IdTransferenciaActivoFijo { get; set; }

        public int? IdActivoFijo { get; set; }

        public virtual ActivoFijo ActivoFijo { get; set; }

        public virtual TransferenciaActivoFijo TransferenciaActivoFijo { get; set; }
    }
}
