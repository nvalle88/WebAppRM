using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bbd.webapprm.servicios.Enumeradores;
using bd.log.guardar.Enumeradores;
using bd.log.guardar.ObjectTranfer;
using bd.log.guardar.Servicios;
using bd.webapprm.entidades;
using bd.webapprm.entidades.Utils;
using bd.webapprm.entidades.Utils.Seguridad;
using bd.webapprm.servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bd.webapprm.web.Controllers.MVC
{
    public class HomesController : Controller
    {
        private readonly IApiServicio apiServicio;

        public HomesController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        [Authorize(Policy = PoliticasSeguridad.TienePermiso)]
        public async Task<IActionResult> Index()
        {
            var lista = new List<Noticia>();
            try
            {
                lista = await apiServicio.Listar<Noticia>(new Uri(WebApp.BaseAddress), "api/Homes/ListarNoticias");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando noticias", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                return BadRequest();
            }
        }
    }
}