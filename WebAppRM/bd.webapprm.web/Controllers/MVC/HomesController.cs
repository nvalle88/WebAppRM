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

       
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}