using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace bd.webapprm.web.Controllers.MVC
{
    public class ActivoFijoController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("RecepcionActivo");
        }

        public IActionResult RecepcionActivo()
        {
            return View();
        }
    }
}