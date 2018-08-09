using bd.log.guardar.ObjectTranfer;
using bd.webapprm.entidades.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace bd.webapprm.servicios.Interfaces
{
    public interface IApiServicio
    {
        Task<Response> SalvarLog<T>(HttpContext context, EntradaLog model);
        Task<Response> InsertarAsync<T>(T model,Uri baseAddress, string url );
        Task<Response> EliminarAsync(string id, Uri baseAddress, string url);
        Task<Response> EditarAsync<T>(string id, T model, Uri baseAddress, string url);
        Task<Response> EditarAsync<T>(object model, Uri baseAddress, string url);
        Task<T> ObtenerElementoAsync<T>(object model, Uri baseAddress, string url);
        Task<T> SeleccionarAsync<T>(string id, Uri baseAddress, string url) where T : class;
        Task<List<T>> Listar<T>(Uri baseAddress, string url);
    }
}
