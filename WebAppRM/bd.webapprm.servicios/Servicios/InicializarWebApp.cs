using bd.webapprm.entidades.Utils;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace bd.webapprm.servicios.Servicios
{
    public class InicializarWebApp
    {
        #region Methods
        private static async Task<Adscsist> ObtenerHostSistema(string id, Uri baseAddreess)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = baseAddreess;
                var url = string.Format("{0}/{1}", "api/Adscsists", id);
                var respuesta = await client.GetAsync(url);

                var resultado = await respuesta.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<Response>(resultado);
                var sistema = JsonConvert.DeserializeObject<Adscsist>(response.Resultado.ToString());
                return sistema;
            }
        }

        public static async Task InicializarWebRecursosMateriales(string id, Uri baseAddreess)
        {
            try
            {
                var sistema = await ObtenerHostSistema(id, new Uri("http://carlos/swSeguridad/"));
                WebApp.BaseAddressRM = sistema.AdstHost;
            }
            catch (Exception)
            { }
        }

        public static async Task InicializarWebTalentoHumano(string id, Uri baseAddreess)
        {
            try
            {
                var sistema = await ObtenerHostSistema(id, new Uri("http://carlos/swSeguridad/"));
                WebApp.BaseAddressTH = sistema.AdstHost;
            }
            catch (Exception)
            { }
        }

        public static async Task InicializarSeguridad(string id, Uri baseAddreess)
        {
            try
            {
                var sistema = await ObtenerHostSistema(id, baseAddreess);
                WebApp.BaseAddressSeguridad = sistema.AdstHost;
            }
            catch (Exception)
            { }
        }

        public static async Task InicializarLogEntry(string id, Uri baseAddress)
        {
            try
            {
                var sistema = await ObtenerHostSistema(id, baseAddress);
                //AppGuardarLog.BaseAddress = sistema.AdstHost;
            }
            catch (Exception)
            { }
        }
        #endregion
    }
}
