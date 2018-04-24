using bd.log.guardar.Inicializar;
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
                var respuesta = await client.GetAsync(new Uri($"{baseAddreess}/api/Adscsists/{id}"));
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
                WebApp.BaseAddressRM = "http://localhost:5000/";//(await ObtenerHostSistema(id, baseAddreess)).AdstHost;
            }
            catch (Exception)
            { }
        }

        public static async Task InicializarWebTalentoHumano(string id, Uri baseAddreess)
        {
            try
            {
                WebApp.BaseAddressTH = "http://localhost:5001/";//(await ObtenerHostSistema(id, baseAddreess)).AdstHost;
            }
            catch (Exception)
            { }
        }

        public static async Task InicializarLogEntry(string id, Uri baseAddress)
        {
            try
            {
                AppGuardarLog.BaseAddress = (await ObtenerHostSistema(id, baseAddress)).AdstHost;
            }
            catch (Exception)
            { }
        }

        public static async Task InicializarSeguridad(string id, Uri baseAddreess)
        {
            try
            {
                WebApp.BaseAddressSeguridad = (await ObtenerHostSistema(id, baseAddreess)).AdstHost;
            }
            catch (Exception)
            { }
        }
        #endregion
    }
}
