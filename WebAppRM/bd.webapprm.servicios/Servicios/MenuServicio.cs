using bd.webapprm.entidades.Utils;
using bd.webapprm.entidades.Utils.Seguridad;
using bd.webapprm.servicios.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace bd.webapprm.servicios.Servicios
{
    public class MenuServicio : IMenuServicio
    {
        private IDictionary<string, List<string>> DiccionarioAcciones = new Dictionary<string, List<string>>
        {
            { "/ActivoFijo/Recepcion", new List<string> { "EditarRecepcionAR" } },
            { "/ActivoFijo/ActivosFijosValidacionTecnica", new List<string> { "RevisionActivoFijo", "EditarRecepcionVT" } },
            { "/ActivoFijo/ActivosFijosRecepcionadosSinPoliza", new List<string> { "AsignarPolizaSeguro" } },
            { "/ActivoFijo/ListadoCambioCustodio", new List<string> { "GestionarCambioCustodio" } },
            { "/ActivoFijo/ListarMantenimientosActivosFijos", new List<string> { "ListadoMantenimientos", "CrearMantenimiento", "EditarMantenimiento" } },
            { "/ActivoFijo/ListarProcesosJudicialesActivosFijos", new List<string> { "ListadoProcesosJudiciales", "GestionarProcesoJudicial" } },
            { "/ActivoFijo/ListadoInventarioActivosFijos", new List<string> { "GestionarInventarioManual", "GestionarInventarioAutomatico" } },
            { "/ActivoFijo/ListadoMovilizacionActivosFijos", new List<string> { "GestionarMovilizacion" } },
            { "/ActivoFijo/ListarRevalorizacionesActivosFijos", new List<string> { "ListadoRevalorizaciones", "GestionarRevalorizacion" } },
            { "/Proveeduria/ListadoOrdenCompraEntramite", new List<string> { "DetallesOrdenCompraEnTramite" } },
            { "/Proveeduria/ListadoOrdenCompraProcesadas", new List<string> { "ProcesarOrdenCompraEnTramite" } },
            { "/Proveeduria/ListadoRequerimientosSolicitados", new List<string> { "DetallesRequerimientoArticulos" } },
            { "/Proveeduria/ListadoRequerimientosDespachados", new List<string> { "DespacharRequerimiento" } },
        };

        public List<string> ObtenerAccionesDiccionario(string admeControlador)
        {
            if (!String.IsNullOrEmpty(admeControlador))
            {
                var diccionario = DiccionarioAcciones.FirstOrDefault(c => c.Key == admeControlador);
                var listaAcciones = new List<string>();

                if (diccionario.Value != null)
                    listaAcciones = diccionario.Value;

                string accion = ObtenerAccion(admeControlador);
                if (accion != "Index" && accion != "Create" && accion != "Edit") //Agregar aquí las acciones para que se compare solo por el Controlador
                    listaAcciones.Insert(0, accion);
                return listaAcciones;
            }
            return new List<string>();
        }

        public string ObtenerControlador(string Controlador)
        {
            if (Controlador != null)
            {
                var matriz = Controlador.Split('/');
                var salida = matriz[1];
                return salida;
            }
            return "";
        }

        public string ObtenerAccion(string Controlador)
        {
            if (Controlador != null)
            {
                var matriz = Controlador.Split('/');
                var salida = matriz[2];
                return salida;
            }
            return null;
        }

        public async Task<List<Adscmenu>> Listar(string usuario, string url)
        {
            var usuarioSistema = new UsuarioSistema
            {
                Sistema = WebApp.NombreAplicacion,
                Usuario = usuario,
            };

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var request = JsonConvert.SerializeObject(usuarioSistema);
                    var content = new StringContent(request, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{WebApp.BaseAddressSeguridad}{url}", content);
                    var resultado = await response.Content.ReadAsStringAsync();
                    var respuesta = JsonConvert.DeserializeObject<List<Adscmenu>>(resultado);
                    return respuesta;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public IHtmlContent CrearMenu(IHtmlHelper helper, IUrlHelper url, List<Adscmenu> menuItems, int niveles)
        {
            var nav = new TagBuilder("nav");
            if (niveles > 0)
            {
                var ul = new TagBuilder("ul");
                foreach (var nivel1 in menuItems.Where(p => p.AdmePadre == null || p.AdmePadre == Convert.ToString(0)))
                {
                    var content = new HtmlContentBuilder()
                         .AppendHtml("<li>")
                         .AppendHtml($"<a href='#'><i class='fa fa-lg fa-desktop'></i> <span style='font-family:Arial' class='font-sm'>  {nivel1.AdmeAplicacion}</span></a>")
                         .AppendHtml("<ul>")
                         .AppendHtml(CrearMenuNivel(helper, url, nivel1.AdmeAplicacion, menuItems, (niveles - 1)))
                         .AppendHtml("</ul>")
                         .AppendHtml("</li>");
                    ul.InnerHtml.AppendHtml(content);
                }
                nav.InnerHtml.AppendHtml(ul);
            }
            return nav;
        }

        private IHtmlContent CrearMenuNivel(IHtmlHelper helper, IUrlHelper url, string admeAplicacion, List<Adscmenu> menuItems, int niveles)
        {
            var htmlContentBuilder = new HtmlContentBuilder();
            if (niveles > 0)
            {
                foreach (var item in menuItems.Where(p => p.AdmePadre == admeAplicacion && (p.AdmeTipo == "M" || p.AdmeTipo == "A")))
                {
                    htmlContentBuilder.AppendHtml($"<li class='{RouteIf(helper, ObtenerAccionesDiccionario(item.AdmeControlador), ObtenerControlador(item.AdmeControlador), "active")}'>");
                    if (item.AdmeTipo == "M")
                        htmlContentBuilder.AppendHtml($"<a href='#'><i class='fa fa-lg fa-folder-o'></i> <span style='font-family:Arial' class='font-sm'>  {item.AdmeAplicacion}</span></a>");
                    if (item.AdmeTipo == "A")
                        htmlContentBuilder.AppendHtml($"<a href='{url.Action(ObtenerAccion(item.AdmeControlador), ObtenerControlador(item.AdmeControlador))}'><i class='fa fa-lg fa-gear'></i> <span style='font-family:Arial' class='font-sm'>  {item.AdmeAplicacion}</span></a>");
                    else
                    {
                        if (niveles > 1)
                        {
                            htmlContentBuilder.AppendHtml("<ul>");
                            htmlContentBuilder.AppendHtml(CrearMenuNivel(helper, url, item.AdmeAplicacion, menuItems, (niveles - 1)));
                            htmlContentBuilder.AppendHtml("</ul>");
                        }
                    }
                    htmlContentBuilder.AppendHtml("</li>");
                }
            }
            return htmlContentBuilder;
        }

        private IHtmlContent RouteIf(IHtmlHelper helper, List<string> accion, string controlador, string attribute)
        {
            var currentController = (helper.ViewContext.RouteData.Values["controller"] ?? string.Empty).ToString().Replace("-", string.Empty);
            var currentAction = (helper.ViewContext.RouteData.Values["action"] ?? string.Empty).ToString().Replace("-", string.Empty);

            var hasController = controlador.Equals(currentController, StringComparison.OrdinalIgnoreCase);
            var hasAction = accion.Count > 0 ? accion.Contains(currentAction) : controlador.Equals(currentAction, StringComparison.OrdinalIgnoreCase);

            return accion.Count > 0 ? (hasAction && hasController) ? new HtmlString(attribute) : new HtmlString(string.Empty) : (hasAction || hasController) ? new HtmlString(attribute) : new HtmlString(string.Empty);
        }
    }
}
