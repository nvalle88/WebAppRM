using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.webapprm.entidades.ObjectTransfer
{
    public class IdRecepcionActivoFijoDetalleSeleccionado
    {
        public int idRecepcionActivoFijoDetalle { get; set; }
        public bool seleccionado { get; set; }
    }

    public class RecepcionActivoFijoDetalleSeleccionado
    {
        public RecepcionActivoFijoDetalle RecepcionActivoFijoDetalle { get; set; }
        public bool Seleccionado { get; set; }

        [Display(Name = "Observaciones:")]
        public string Observaciones { get; set; }

        [Display(Name = "Componentes:")]
        public string Componentes { get; set; }
    }

    public class MaestroArticuloSucursalSeleccionado
    {
        public MaestroArticuloSucursal MaestroArticuloSucursal { get; set; }
        public bool Seleccionado { get; set; }
        public int CantidadBodega { get; set; }
    }

    public class IdSucursalIdRecepcionActivoFijoDetalleSeleccionado
    {
        public int IdSucursal { get; set; }
        public List<IdRecepcionActivoFijoDetalleSeleccionado> ListaIdRecepcionActivoFijoDetalleSeleccionado { get; set; }
    }

    public class IdRecepcionActivoFijoDetalleSeleccionadoIdsComponentesExcluir
    {
        public List<IdRecepcionActivoFijoDetalleSeleccionado> ListaIdRecepcionActivoFijoDetalleSeleccionado { get; set; }
        public List<int> IdsComponentesExcluir { get; set; }
    }

    public class IdRecepcionActivoFijoDetalleSeleccionadoIdsInicialesAltaBaja
    {
        public List<IdRecepcionActivoFijoDetalleSeleccionado> ListaIdRecepcionActivoFijoDetalleSeleccionado { get; set; }
        public List<IdRecepcionActivoFijoDetalleSeleccionado> ListaIdRecepcionActivoFijoDetalleSeleccionadoInicialesAltaBaja { get; set; }
        public int? IdRecepcionActivoFijo { get; set; }
    }

    public class IdRecepcionActivoFijoDetalleSeleccionadoEstado
    {
        public List<string> Estados { get; set; }
        public List<IdRecepcionActivoFijoDetalleSeleccionado> ListaIdRecepcionActivoFijoDetalleSeleccionado { get; set; }
    }
}
