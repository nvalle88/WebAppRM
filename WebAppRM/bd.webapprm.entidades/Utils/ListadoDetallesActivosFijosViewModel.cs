using bd.webapprm.entidades.ObjectTransfer;
using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.entidades.Utils
{
    public class ListadoDetallesActivosFijosViewModel
    {
        public bool IsConfiguracionSeleccion { get; set; }
        public bool IsConfiguracionSeleccionDisabled { get; set; }
        public bool IsConfiguracionDatosActivo { get; set; }
        public bool IsConfiguracionSmartForm { get; set; }
        public bool IsConfiguracionDatosAlta { get; set; }
        public bool IsConfiguracionDatosBaja { get; set; }
        public bool IsConfiguracionDatosMovilizaciones { get; set; }
        public bool IsConfiguracionOpciones { get; set; }
        public bool IsConfiguracionListadoComponentes { get; set; }
        public bool IsConfiguracionSeleccionComponentes { get; set; }
        public bool IsConfiguracionListadoAltasGestionar { get; set; }
        public bool IsConfiguracionAltasGestionarEditar { get; set; }
        public bool IsConfiguracionSeleccionAltas { get; set; }
        public bool IsConfiguracionSeleccionBajas { get; set; }
        public bool IsConfiguracionDetallesRecepcion { get; set; }
        public bool IsConfiguracionListadoMantenimientos { get; set; }
        public bool IsConfiguracionListadoProcesosJudiciales { get; set; }
        public bool IsConfiguracionListadoRevalorizaciones { get; set; }
        public bool IsConfiguracionListadoAltas { get; set; }
        public bool IsConfiguracionListadoBajasGestionar { get; set; }
        public bool IsConfiguracionBajasGestionarEditar { get; set; }
        public bool IsConfiguracionListadoBajas { get; set; }
        public bool IsConfiguracionGestionarInventarioAutomatico { get; set; }
        public bool IsConfiguracionSeleccionMovilizaciones { get; set; }
        public bool IsConfiguracionListadoMovilizacionesGestionar { get; set; }
        public bool IsConfiguracionListadoMovilizaciones { get; set; }
        public bool IsConfiguracionListadoGenerales { get; set; }
        public bool MostrarFiltradoUltimaColumna { get; set; }
        public string CallbackFunctionCheckBox { get; set; }
        public string CallbackFunctionRemoveTodos { get; set; } = "callBackFunctionEliminarDatoEspecifico";
        public string NombreTabla { get; set; } = "tableDetallesActivoFijo";
        public int Cantidad { get; set; } = 16;

        public ListadoDetallesActivosFijosViewModel(bool? IsConfiguracionSeleccion = null,
            bool? IsConfiguracionSeleccionDisabled = null,
            bool? IsConfiguracionDatosActivo = null,
            bool? IsConfiguracionSmartForm = null,
            bool? IsConfiguracionDatosAlta = null,
            bool? IsConfiguracionDatosBaja = null,
            bool? IsConfiguracionOpciones = null,
            bool? IsConfiguracionListadoComponentes = null,
            bool? IsConfiguracionSeleccionComponentes = null,
            bool? IsConfiguracionListadoAltasGestionar = null,
            bool? IsConfiguracionAltasGestionarEditar = null,
            bool? IsConfiguracionSeleccionAltas = null,
            bool? IsConfiguracionSeleccionBajas = null,
            bool? IsConfiguracionDetallesRecepcion = null,
            bool? IsConfiguracionListadoMantenimientos = null,
            bool? IsConfiguracionListadoProcesosJudiciales = null, 
            bool? IsConfiguracionListadoRevalorizaciones = null,
            bool? IsConfiguracionListadoAltas = null,
            bool? IsConfiguracionListadoBajasGestionar = null, 
            bool? IsConfiguracionBajasGestionarEditar = null,
            bool? IsConfiguracionListadoBajas = null,
            bool? IsConfiguracionGestionarInventarioAutomatico = null,
            bool? IsConfiguracionSeleccionMovilizaciones = null, 
            bool? IsConfiguracionListadoMovilizacionesGestionar = null,
            bool? IsConfiguracionListadoMovilizaciones = null,
            bool? IsConfiguracionListadoGenerales = null)
        {
            this.IsConfiguracionSeleccion = IsConfiguracionSeleccion ?? false;
            this.IsConfiguracionSeleccionDisabled = IsConfiguracionSeleccionDisabled ?? false;
            this.IsConfiguracionDatosActivo = IsConfiguracionDatosActivo ?? false;
            this.IsConfiguracionSmartForm = IsConfiguracionSmartForm ?? false;
            this.IsConfiguracionDatosAlta = IsConfiguracionDatosAlta ?? false;
            this.IsConfiguracionDatosBaja = IsConfiguracionDatosBaja ?? false;
            this.IsConfiguracionOpciones = IsConfiguracionOpciones ?? false;
            this.IsConfiguracionListadoComponentes = IsConfiguracionListadoComponentes ?? false;
            this.IsConfiguracionSeleccionComponentes = IsConfiguracionSeleccionComponentes ?? false;
            this.IsConfiguracionListadoAltasGestionar = IsConfiguracionListadoAltasGestionar ?? false;
            this.IsConfiguracionAltasGestionarEditar = IsConfiguracionAltasGestionarEditar ?? false;
            this.IsConfiguracionSeleccionAltas = IsConfiguracionSeleccionAltas ?? false;
            this.IsConfiguracionSeleccionBajas = IsConfiguracionSeleccionBajas ?? false;
            this.IsConfiguracionDetallesRecepcion = IsConfiguracionDetallesRecepcion ?? false;
            this.IsConfiguracionListadoMantenimientos = IsConfiguracionListadoMantenimientos ?? false;
            this.IsConfiguracionListadoProcesosJudiciales = IsConfiguracionListadoProcesosJudiciales ?? false;
            this.IsConfiguracionListadoRevalorizaciones = IsConfiguracionListadoRevalorizaciones ?? false;
            this.IsConfiguracionListadoAltas = IsConfiguracionListadoAltas ?? false;
            this.IsConfiguracionListadoBajasGestionar = IsConfiguracionListadoBajasGestionar ?? false;
            this.IsConfiguracionBajasGestionarEditar = IsConfiguracionBajasGestionarEditar ?? false;
            this.IsConfiguracionListadoBajas = IsConfiguracionListadoBajas ?? false;
            this.IsConfiguracionGestionarInventarioAutomatico = IsConfiguracionGestionarInventarioAutomatico ?? false;
            this.IsConfiguracionSeleccionMovilizaciones = IsConfiguracionSeleccionMovilizaciones ?? false;
            this.IsConfiguracionListadoMovilizacionesGestionar = IsConfiguracionListadoMovilizacionesGestionar ?? false;
            this.IsConfiguracionListadoMovilizaciones = IsConfiguracionListadoMovilizaciones ?? false;
            this.IsConfiguracionListadoGenerales = IsConfiguracionListadoGenerales ?? false;

            if (this.IsConfiguracionSeleccionComponentes || this.IsConfiguracionDetallesRecepcion || this.IsConfiguracionSeleccionAltas || this.IsConfiguracionListadoAltas || this.IsConfiguracionListadoBajas || (this.IsConfiguracionSeleccionBajas && !this.IsConfiguracionGestionarInventarioAutomatico) || (this.IsConfiguracionListadoBajasGestionar && this.IsConfiguracionBajasGestionarEditar) || this.IsConfiguracionListadoMovilizacionesGestionar || this.IsConfiguracionListadoMovilizaciones)
                MostrarFiltradoUltimaColumna = true;

            if (this.IsConfiguracionListadoComponentes || this.IsConfiguracionSeleccionComponentes || this.IsConfiguracionListadoAltas || this.IsConfiguracionListadoMantenimientos || this.IsConfiguracionListadoRevalorizaciones || this.IsConfiguracionListadoProcesosJudiciales || this.IsConfiguracionListadoBajas || this.IsConfiguracionListadoBajasGestionar || this.IsConfiguracionSeleccionBajas || this.IsConfiguracionSeleccionMovilizaciones || this.IsConfiguracionListadoMovilizacionesGestionar || this.IsConfiguracionListadoMovilizaciones)
                this.IsConfiguracionDatosAlta = true;

            if (this.IsConfiguracionListadoBajas)
                this.IsConfiguracionDatosBaja = true;

            if (this.IsConfiguracionListadoComponentes || this.IsConfiguracionListadoMantenimientos || this.IsConfiguracionListadoRevalorizaciones || this.IsConfiguracionListadoProcesosJudiciales || this.IsConfiguracionListadoAltasGestionar || (this.IsConfiguracionListadoBajasGestionar && !this.IsConfiguracionBajasGestionarEditar) || this.IsConfiguracionGestionarInventarioAutomatico || this.IsConfiguracionSeleccionMovilizaciones)
                this.IsConfiguracionOpciones = true;

            if (this.IsConfiguracionSeleccionMovilizaciones || this.IsConfiguracionListadoMovilizacionesGestionar || this.IsConfiguracionListadoMovilizaciones)
                IsConfiguracionDatosMovilizaciones = true;

            if (this.IsConfiguracionSeleccionComponentes)
            {
                NombreTabla += "Componentes";
                CallbackFunctionCheckBox = "callBackFunctionSeleccionComponente";
            }
            else if (this.IsConfiguracionListadoAltasGestionar)
                NombreTabla += "Seleccionados";
            else if (this.IsConfiguracionSeleccionAltas)
            {
                NombreTabla += "Altas";
                CallbackFunctionCheckBox = "callBackFunctionSeleccionAlta";
            }
            else if (this.IsConfiguracionListadoBajasGestionar || this.IsConfiguracionBajasGestionarEditar)
                NombreTabla += "Seleccionados";
            else if (this.IsConfiguracionSeleccionBajas)
            {
                NombreTabla += "Bajas";
                CallbackFunctionCheckBox = "callBackFunctionSeleccionBaja";
            }
            else if (this.IsConfiguracionSeleccionMovilizaciones)
                NombreTabla += "Seleccionados";
            else if (this.IsConfiguracionListadoMovilizacionesGestionar || this.IsConfiguracionListadoMovilizaciones)
            {
                NombreTabla += "Altas";
                CallbackFunctionCheckBox = "callBackFunctionSeleccionAlta";
            }
            else if (this.IsConfiguracionListadoGenerales)
                NombreTabla = "dt_basic";

            if (this.IsConfiguracionOpciones)
                Cantidad++;

            if (this.IsConfiguracionDatosAlta)
                Cantidad += 2;

            if (this.IsConfiguracionDatosBaja)
                Cantidad += 2;

            if (this.IsConfiguracionSeleccion)
                Cantidad++;

            if (this.IsConfiguracionDatosActivo)
                Cantidad += 4;

            if (IsConfiguracionDatosMovilizaciones)
            {
                Cantidad++;
                if (this.IsConfiguracionSeleccionMovilizaciones)
                    Cantidad++;

                if (this.IsConfiguracionListadoMovilizacionesGestionar || this.IsConfiguracionListadoMovilizaciones)
                    Cantidad++;
            }
        }
    }
}
