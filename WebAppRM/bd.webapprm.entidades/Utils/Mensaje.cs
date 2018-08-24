using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.entidades.Utils
{
    public static class Mensaje
    {
        public static string Satisfactorio { get { return "La acción se ha realizado satisfactoriamente."; } }
        public static string RegistroNoExiste { get { return "El registro que desea editar no existe."; } }
        public static string ErrorCrear { get { return "Ha ocurrido un error al crear el registro."; } }
        public static string ErrorEditar { get { return "Ha ocurrido un error al editar el registro."; } }
        public static string ErrorListado { get { return "Ha ocurrido un error al cargar el listado."; } }
        public static string Excepcion { get { return "Ha ocurrido una Excepción."; } }
        public static string ErrorCargarDatos { get { return "Ha ocurrido un error al cargar los datos."; } }
        public static string ErrorUploadFiles { get { return "Ha ocurrido un error al subir la documentación adicional."; } }
        public static string ModeloInvalido { get { return "El Módelo es inválido."; } }
        public static string Informacion { get { return "Información"; } }
        public static string Error { get { return "Error"; } }
        public static string Aviso { get { return "Aviso"; } }
        public static string ErrorRecursoSolicitado { get { return "No puede acceder al recurso solicitado."; } }
        public static string NoExisteModulo { get { return "No se ha encontrado el Módulo."; } }
        public static string ErrorDiasPedido { get { return "El plazo para realizar el requerimiento de proveeduría ha expirado."; } }
    }

    public static class Estados
    {
        public static string Recepcionado { get { return "RECEPCIONADO"; } }
        public static string ValidacionTecnica { get { return "VALIDACIÓN TÉCNICA"; } }
        public static string Desaprobado { get { return "DESAPROBADO"; } }
        public static string Alta { get { return "ALTA"; } }
        public static string Baja { get { return "BAJA"; } }
        public static string Creada { get { return "CREADO"; } }
        public static string Aceptada { get { return "ACEPTADO"; } }
        public static string Mantenimiento { get { return "MANTENIMIENTO"; } }
        public static string EnEjecucion { get { return "EN EJECUCIÓN"; } }
        public static string Concluido { get { return "CONCLUIDO"; } }
        public static string EnTramite { get { return "EN TRÁMITE"; } }
        public static string Procesada { get { return "PROCESADA"; } }
        public static string Solicitado { get { return "SOLICITADO"; } }
        public static string Despachado { get { return "DESPACHADO"; } }
    }

    public static class LineasServicio
    {
        public static string ActivosFijos { get { return "ACTIVOS FIJOS"; } }
        public static string Proveeduria { get { return "PROVEEDURÍA"; } }
    }

    public static class Categorias
    {
        public static string Edificio { get { return "EDIFICIOS"; } }
        public static string MueblesEnseres { get { return "MUEBLES Y ENSERES"; } }
        public static string EquiposOficina { get { return "EQUIPOS DE OFICINA"; } }
        public static string Vehiculo { get { return "VEHÍCULOS"; } }
        public static string EquiposComputoSoftware { get { return "EQUIPOS DE CÓMPUTO Y SOFTWARE"; } }
    }

    public static class MotivosTransferencia
    {
        public static string CambioCustodio { get { return "CAMBIO DE CUSTODIO"; } }
        public static string CambioUbicacion { get { return "CAMBIO DE UBICACIÓN"; } }
        public static string PrestamoUsoExterno { get { return "PRÉSTAMO DE USO EXTERNO"; } }
        public static string PrestamoUsoInterno { get { return "PRÉSTAMO DE USO INTERNO"; } }
        public static string TransferenciaBodegas { get { return "TRANSFERENCIA ENTRE BODEGAS"; } }
    }

    public static class ADMI_Grupos
    {
        public static string AdminAF { get { return "Administradores de Activos Fijos"; } }
        public static string EncargadoSeguros { get { return "Encargados de seguro"; } }
        public static string AdminNacionalProveeduria { get { return "Admin Nacional de Proveeduría"; } }
        public static string AdminZonalProveeduria { get { return "Admin Zonal de Proveeduría"; } }
        public static string FuncionarioSolicitante { get { return "Funcionario solicitante"; } }
    }

    public static class ThClassName
    {
        public static string seleccion { get { return "Seleccion"; } }
        public static string codigoSecuencial { get { return "Codigosecuencial"; } }
        public static string tipoActivoFijo { get { return "TipoActivoFijo"; } }
        public static string claseActivoFijo { get { return "ClaseActivoFijo"; } }
        public static string subClaseActivoFijo { get { return "SubclaseActivoFijo"; } }
        public static string nombreActivoFijo { get { return "NombreActivoFijo"; } }
        public static string marca { get { return "Marca"; } }
        public static string modelo { get { return "Modelo"; } }
        public static string valorCompra { get { return "ValorCompra"; } }
        public static string depreciacion { get { return "Depreciacion"; } }
        public static string validacionTecnica { get { return "ValidacionTecnica"; } }
        public static string serie { get { return "Serie"; } }
        public static string numeroChasis { get { return "NumeroChasis"; } }
        public static string numeroMotor { get { return "NumeroMotor"; } }
        public static string placa { get { return "Placa"; } }
        public static string numeroClaveCatastral { get { return "NumeroClaveCatastral"; } }
        public static string sucursal { get { return "Sucursal"; } }
        public static string sucursalOrigen { get { return "SucursalOrigen"; } }
        public static string sucursalDestino { get { return "SucursalDestino"; } }
        public static string dependencia { get { return "Dependencia"; } }
        public static string bodega { get { return "Bodega"; } }
        public static string empleado { get { return "Empleado"; } }
        public static string empleadoResponsableEnvio { get { return "EmpleadoResponsableEnvio"; } }
        public static string empleadoResponsableRecibo { get { return "EmpleadoResponsableRecibo"; } }
        public static string empleadoEntrega { get { return "EmpleadoEntrega"; } }
        public static string empleadoRecibe { get { return "EmpleadoRecibe"; } }
        public static string empleadoResponsable { get { return "EmpleadoResponsable"; } }
        public static string empleadoSolicita { get { return "EmpleadoSolicita"; } }
        public static string empleadoAutorizado { get { return "EmpleadoAutorizado"; } }
        public static string proveedor { get { return "Proveedor"; } }
        public static string motivoAlta { get { return "MotivoAlta"; } }
        public static string fechaRecepcion { get { return "FechaRecepcion"; } }
        public static string fechaTransferencia { get { return "FechaTransferencia"; } }
        public static string ordenCompra { get { return "OrdenCompra"; } }
        public static string fondoFinanciamiento { get { return "FondoFinanciamiento"; } }
        public static string fechaAlta { get { return "FechaAlta"; } }
        public static string numeroFactura { get { return "NumeroFactura"; } }
        public static string fechaBaja { get { return "FechaBaja"; } }
        public static string motivoBaja { get { return "MotivoBaja"; } }
        public static string componentes { get { return "Componentes"; } }
        public static string observaciones { get { return "Observaciones"; } }
        public static string opciones { get { return "Opciones"; } }
        public static string nombreArticulo { get { return "NombreArticulo"; } }
        public static string cantidad { get { return "Cantidad"; } }
        public static string cantidadBodega { get { return "CantidadBodega"; } }
        public static string unidadMedida { get { return "UnidadMedida"; } }
        public static string valorUnitario { get { return "ValorUnitario"; } }
        public static string valorTotal { get { return "ValorTotal"; } }
        public static string tipoArticulo { get { return "TipoArticulo"; } }
        public static string claseArticulo { get { return "ClaseArticulo"; } }
        public static string subclaseArticulo { get { return "SubclaseArticulo"; } }
        public static string ramo { get { return "Ramo"; } }
        public static string subramo { get { return "Subramo"; } }
        public static string numeroRecepcion { get { return "NumeroRecepcion"; } }
        public static string estado { get { return "Estado"; } }
        public static string companiaSeguro { get { return "CompaniaSeguro"; } }
        public static string numeroPolizaSeguro { get { return "NumeroPolizaSeguro"; } }
        public static string comunes { get { return "Comunes"; } }
        public static string funcionarioSolicitante { get { return "FuncionarioSolicitante"; } }
        public static string fechaSolicitud { get { return "FechaSolicitud"; } }
        public static string fechaAprobadoDenegado { get { return "FechaAprobadoDenegado"; } }
        public static string fechaFactura { get { return "FechaFactura"; } }
        public static string tipoUtilizacionAlta { get { return "TipoUtilizacionAlta"; } }
        public static string memoOficioResolucion { get { return "MemoOficioResolucion"; } }
        public static string numeroInforme { get { return "NumeroInforme"; } }
        public static string fechaCorteInventario { get { return "FechaCorteInventario"; } }
        public static string fechaInforme { get { return "FechaInforme"; } }
        public static string inventarioManual { get { return "InventarioManual"; } }
        public static string motivoTraslado { get { return "MotivoTraslado"; } }
        public static string fechaSalida { get { return "FechaSalida"; } }
        public static string fechaRetorno { get { return "FechaRetorno"; } }
        public static string fechaDesde { get { return "FechaDesde"; } }
        public static string fechaHasta { get { return "FechaHasta"; } }
        public static string fechaMantenimiento { get { return "FechaMantenimiento"; } }
        public static string numeroDenuncia { get { return "NumeroDenuncia"; } }
        public static string fechaRevalorizacion { get { return "FechaRevalorizacion"; } }
        public static string fechaDepreciacion { get { return "FechaDepreciacion"; } }
        public static string valorResidual { get { return "ValorResidual"; } }
        public static string motivoTransferencia { get { return "MotivoTransferencia"; } }
        public static string fechaUbicacion { get { return "FechaUbicacion"; } }
        public static string motivoUbicacion { get { return "MotivoUbicacion"; } }
    }
}