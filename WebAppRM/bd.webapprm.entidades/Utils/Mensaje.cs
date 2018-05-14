using System;
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
    }

    public static class Estados
    {
        public static string Recepcionado { get { return "Recepcionado"; } }
        public static string ValidacionTecnica { get { return "Validación Técnica"; } }
        public static string Desaprobado { get { return "Desaprobado"; } }
        public static string Alta { get { return "Alta"; } }
        public static string Baja { get { return "Baja"; } }
        public static string Creada { get { return "Creado"; } }
        public static string Aceptada { get { return "Aceptada"; } }
        public static string Rechazada { get { return "Rechazada"; } }
    }
}