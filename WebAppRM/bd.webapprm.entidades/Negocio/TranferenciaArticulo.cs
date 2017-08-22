namespace bd.webapprm.entidades
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
     
     

    
    public partial class TranferenciaArticulo
    {
        [Key]
        public int IdTranferenciaArticulo { get; set; }

        public int IdMaestroArticuloEnvia { get; set; }

        public int IdMaestroArticuloRecibe { get; set; }

        public int IdArticulo { get; set; }


        public int IdEmpleadoEnvia { get; set; }

     

        public int IdEmpleadoRecibe { get; set; }

        public int? Cantidad { get; set; }

        [StringLength(10)]
        public string Fecha { get; set; }

        public virtual Articulo Articulo { get; set; }

        public virtual Empleado EmpleadoRecibe { get; set; }

        public virtual Empleado EmpleadoEnvia { get; set; }

        public virtual MaestroArticuloSucursal MaestroArticuloSucursalRecibe { get; set; }

        public virtual MaestroArticuloSucursal MaestroArticuloSucursalEnvia { get; set; }
    }
}
