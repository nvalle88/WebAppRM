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
                var url = string.Format("{0}/{1}", "/api/Adscsists", id);
                var respuesta = await client.GetAsync(url);

                var resultado = await respuesta.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<Response>(resultado);
                var sistema = JsonConvert.DeserializeObject<Adscsist>(response.Resultado.ToString());
                return sistema;
            }
        }

        public static async Task InicializarWebRecursosMateriales(string id)
        {
            try
            {
                var sistema = await ObtenerHostSistema(id, new Uri("http://localhost:53317"));
                WebApp.BaseAddressRM = sistema.AdstHost;
            }
            catch (Exception)
            { }
        }

        public static async Task InicializarWebTalentoHumano(string id)
        {
            try
            {
                var sistema = await ObtenerHostSistema(id, new Uri("http://localhost:53317"));
                WebApp.BaseAddressTH = sistema.AdstHost;
            }
            catch (Exception)
            { }
        }
        #endregion
    }
}
