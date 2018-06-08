using bd.webapprm.entidades.Utils.Seguridad;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace bd.webapprm.servicios.Interfaces
{
    public interface IMenuServicio
    {
        Task<List<Adscmenu>> Listar(string usuario, string url);
        string ObtenerControlador(string Controlador);
        string ObtenerAccion(string Controlador);
    }
}
