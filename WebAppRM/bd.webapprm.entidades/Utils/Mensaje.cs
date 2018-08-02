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
}