using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using bd.webapprm.servicios.Interfaces;
using bd.webapprm.entidades.Utils;
using Microsoft.AspNetCore.Http;
using bd.log.guardar.ObjectTranfer;
using bd.log.guardar.Servicios;
using System.Linq;
using System.Security.Claims;
using bd.webapprm.entidades.Utils.Seguridad;

namespace bd.webapprm.servicios.Servicios
{
    public class ApiServicio : IApiServicio
    {
        private readonly IHttpContextAccessor httpContext;

        public ApiServicio(IHttpContextAccessor httpContext)
        {
            this.httpContext = httpContext;
        }

        private async Task<bool> SalvarLog(LogEntryTranfer logEntryTranfer)
        {
            var responseLog = await GuardarLogService.SaveLogEntry(logEntryTranfer);
            return responseLog.IsSuccess;
        }
        public async Task<Response> SalvarLog<T>(HttpContext context, EntradaLog model)
        {
            var NombreUsuario = "";
            try
            {
                var claim = context.User.Identities.Where(x => x.NameClaimType == ClaimTypes.Name).FirstOrDefault();
                NombreUsuario = claim.Claims.Where(c => c.Type == ClaimTypes.Name).FirstOrDefault().Value;

                var menuRespuesta = await ObtenerElementoAsync<log.guardar.Utiles.Response>(new ModuloAplicacion { Path = context.Request.Path, NombreAplicacion = WebApp.NombreAplicacion }, new Uri(WebApp.BaseAddressSeguridad), "api/Adscmenus/GetMenuPadre");
                var menu = JsonConvert.DeserializeObject<Adscmenu>(menuRespuesta.Resultado.ToString());

                var Log = new LogEntryTranfer
                {
                    ApplicationName = WebApp.NombreAplicacion,
                    EntityID = menu.AdmeAplicacion,
                    ExceptionTrace = model.ExceptionTrace,
                    LogCategoryParametre = model.LogCategoryParametre,
                    LogLevelShortName = model.LogLevelShortName,
                    Message = context.Request.Path,
                    ObjectNext = model.ObjectNext,
                    ObjectPrevious = model.ObjectPrevious,
                    UserName = NombreUsuario
                };
                var responseLog = await GuardarLogService.SaveLogEntry(Log);
                return new Response { IsSuccess = responseLog.IsSuccess };
            }
            catch (Exception ex)
            {
                var Log = new LogEntryTranfer { ApplicationName = WebApp.NombreAplicacion, EntityID = Mensaje.NoExisteModulo, ExceptionTrace = ex.Message, LogCategoryParametre = model.LogCategoryParametre, LogLevelShortName = model.LogLevelShortName, Message = context.Request.Path, ObjectNext = model.ObjectNext, ObjectPrevious = model.ObjectPrevious, UserName = NombreUsuario };
                var resultado = await SalvarLog(Log);
                return new Response { IsSuccess = resultado };
            }
        }
        public async Task<Response> InsertarAsync<T>(T model, Uri baseAddress, string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var request = JsonConvert.SerializeObject(model);
                    var content = new StringContent(request, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{baseAddress}{url}", content);
                    var resultado = await response.Content.ReadAsStringAsync();
                    var respuesta = JsonConvert.DeserializeObject<Response>(resultado);
                    return respuesta;
                }
            }
            catch (Exception ex)
            {
                return new Response { IsSuccess = true, Message = ex.Message };
            }
        }
        public async Task<Response> EliminarAsync(string id, Uri baseAddress, string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.DeleteAsync($"{baseAddress}{url}/{id}");
                    var resultado = await response.Content.ReadAsStringAsync();
                    var respuesta = JsonConvert.DeserializeObject<Response>(resultado);
                    return respuesta;
                }
            }
            catch (Exception ex)
            {
                return new Response { IsSuccess = false, Message = ex.Message };
            }
        }
        public async Task<Response> EditarAsync<T>(string id,T model, Uri baseAddress, string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var request = JsonConvert.SerializeObject(model);
                    var content = new StringContent(request, Encoding.UTF8, "application/json");
                    var response = await client.PutAsync($"{baseAddress}{url}/{id}", content);
                    var resultado = await response.Content.ReadAsStringAsync();
                    var respuesta = JsonConvert.DeserializeObject<Response>(resultado);
                    return respuesta;
                }
            }
            catch (Exception ex)
            {
                return new Response { IsSuccess = true, Message = ex.Message };
            }
        }
        public async Task<Response> EditarAsync<T>(object model, Uri baseAddress, string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var request = JsonConvert.SerializeObject(model);
                    var content = new StringContent(request, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{baseAddress}{url}", content);
                    var resultado = await response.Content.ReadAsStringAsync();
                    var respuesta = JsonConvert.DeserializeObject<Response>(resultado);
                    return respuesta;
                }
            }
            catch (Exception ex)
            {
                return new Response { IsSuccess = true, Message = ex.Message };
            }
        }
        public async Task<List<T>> Listar<T>(Uri baseAddress, string url) where T : class
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    //AsignarClientHeaders(client, new List<string> { "IdSucursal" });
                    var respuesta = await client.GetAsync($"{baseAddress}{url}");
                    var resultado = await respuesta.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<List<T>>(resultado);
                    return response ?? new List<T>();
                }
            }
            catch (Exception )
            {
                return new List<T>();
            }
        }
        public async Task<T> SeleccionarAsync<T>(string id,Uri baseAddress, string url) where T : class
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var respuesta = await client.GetAsync($"{baseAddress}{url}/{id}");
                    var resultado = await respuesta.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<T>(resultado);
                    return response;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<T> ObtenerElementoAsync<T>(object model, Uri baseAddress, string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var request = JsonConvert.SerializeObject(model);
                    var content = new StringContent(request, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{baseAddress}{url}", content);
                    var resultado = await response.Content.ReadAsStringAsync();
                    var respuesta = JsonConvert.DeserializeObject<T>(resultado);
                    return respuesta;
                }
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        private void AsignarClientHeaders(HttpClient client, IEnumerable<string> claimTypes)
        {
            foreach (var item in claimTypes)
                client.DefaultRequestHeaders.Add(item, ObtenerIdSucursalUsuario(item));
        }
        private string ObtenerIdSucursalUsuario(string claimType)
        {
            try
            {
                var claim = httpContext.HttpContext.User.Identities.Where(x => x.NameClaimType == ClaimTypes.Name).FirstOrDefault();
                var claimValue = claim.Claims.Where(c => c.Type == claimType).FirstOrDefault();
                return claimValue?.Value ?? String.Empty;
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }
    }
}
