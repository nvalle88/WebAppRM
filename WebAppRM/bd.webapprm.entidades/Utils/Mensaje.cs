﻿using System;
using System.Collections.Generic;
using System.Text;

namespace bd.webapprm.entidades.Utils
{
    public static class Mensaje
    {
        public static string Satisfactorio { get { return "La acción se ha realizado satisfactoriamente"; } }
        public static string RegistroNoExiste { get { return "El registro que desea editar no existe."; } }
        public static string ErrorCrear { get { return "Ha ocurrido un error al crear el registro."; } }
        public static string ErrorEditar { get { return "Ha ocurrido un error al editar el registro."; } }
        public static string ErrorListado { get { return "Ha ocurrido un error al cargar el listado."; } }
        public static string Excepcion { get { return "Ha ocurrido una Excepción."; } }
        public static string ErrorCargarDatos { get { return "Ha ocurrido un error al cargar los datos."; } }
        public static string ErrorUploadFiles { get { return "Ha ocurrido un error al subir la documentación adicional."; } }
        public static string ModeloInvalido { get { return "El Módelo es inválido"; } }
        public static string Informacion { get { return "Información"; } }
        public static string Error { get { return "Error"; } }
        public static string Aviso { get { return "Aviso"; } }
        public static string ErrorRecursoSolicitado { get { return "No puede acceder al recurso solicitado."; } }
        public static string NoExisteModulo { get { return "No se ha encontrado el Módulo"; } }
        public static string ErrorDiasPedido { get { return "El plazo para realizar el requerimiento de proveeduría ha expirado."; } }
    }

    public static class Estados
    {
        public static string Recepcionado { get { return "Recepcionado"; } }
        public static string ValidacionTecnica { get { return "Validación Técnica"; } }
        public static string Desaprobado { get { return "Desaprobado"; } }
        public static string Alta { get { return "Alta"; } }
        public static string Baja { get { return "Baja"; } }
        public static string Creada { get { return "Creado"; } }
        public static string Aceptada { get { return "Aceptado"; } }
        public static string Mantenimiento { get { return "Mantenimiento"; } }
        public static string EnEjecucion { get { return "En Ejecución"; } }
        public static string Concluido { get { return "Concluido"; } }
        public static string EnTramite { get { return "En trámite"; } }
        public static string Procesada { get { return "Procesada"; } }
        public static string Solicitado { get { return "Solicitado"; } }
        public static string Despachado { get { return "Despachado"; } }
    }

    public static class LineasServicio
    {
        public static string ActivosFijos { get { return "Activos Fijos"; } }
        public static string Proveeduria { get { return "Proveeduría"; } }
    }

    public static class Categorias
    {
        public static string Edificio { get { return "Edificios"; } }
        public static string MueblesEnseres { get { return "Muebles y enseres"; } }
        public static string EquiposOficina { get { return "Equipos de oficina"; } }
        public static string Vehiculo { get { return "Vehículos"; } }
        public static string EquiposComputoSoftware { get { return "Equipos de cómputo y software"; } }
    }

    public static class MotivosTransferencia
    {
        public static string CambioCustodio { get { return "Cambio de Custodio"; } }
        public static string CambioUbicacion { get { return "Cambio de Ubicación"; } }
        public static string PrestamoUsoExterno { get { return "Préstamo de Uso Externo"; } }
        public static string PrestamoUsoInterno { get { return "Préstamo de Uso Interno"; } }
        public static string TransferenciaBodegas { get { return "Transferencia entre Bodegas"; } }
    }

    public static class ADMI_Grupos
    {
        public static string AdminNacionalProveeduria { get { return "Admin Nacional de Proveeduría"; } }
        public static string AdminZonalProveeduria { get { return "Admin Zonal de Proveeduría"; } }
        public static string FuncionarioSolicitante { get { return "Funcionario solicitante"; } }
    }
}