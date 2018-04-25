using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using bd.webapprm.servicios.Interfaces;
using bd.webapprm.entidades.Utils;
using bd.log.guardar.Servicios;
using bd.log.guardar.ObjectTranfer;
using bd.log.guardar.Enumeradores;
using Newtonsoft.Json;
using bd.webapprm.entidades;
using bbd.webapprm.servicios.Enumeradores;
using bd.webapprm.servicios.Extensores;

namespace bd.webapprm.web.Controllers.MVC
{
    public class CatalogoCuentaController : Controller
    {
        private readonly IApiServicio apiServicio;

        public CatalogoCuentaController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        public async Task<IActionResult> Index()
        {
            var lista = new List<CatalogoCuenta>();
            try
            {
                lista = await apiServicio.Listar<CatalogoCuenta>(new Uri(WebApp.BaseAddressRM), "api/CatalogoCuenta/ListarCatalogosCuenta");
                foreach (var item in lista)
                    try { item.CatalogoCuentaHijo = lista.SingleOrDefault(c => c.IdCatalogoCuenta == item.IdCatalogoCuentaHijo); } catch (Exception) { }
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando catálogos de cuenta", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> Create()
        {
            var listaCatalogosCuenta = await apiServicio.Listar<CatalogoCuenta>(new Uri(WebApp.BaseAddressRM), "api/CatalogoCuenta/ListarCatalogosCuenta");
            ViewData["IdCatalogoCuentaHijoVisible"] = listaCatalogosCuenta.Count > 0;
            listaCatalogosCuenta.Insert(0, new CatalogoCuenta { IdCatalogoCuentaHijo = 0, Codigo = "<< Sin selección >>" });
            ViewData["IdCatalogoCuentaHijo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(listaCatalogosCuenta, "IdCatalogoCuenta", "Codigo");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CatalogoCuenta catalogoCuenta)
        {
            try
            {
                var listaCatalogosCuenta = await apiServicio.Listar<CatalogoCuenta>(new Uri(WebApp.BaseAddressRM), "api/CatalogoCuenta/ListarCatalogosCuenta");
                if (catalogoCuenta.IdCatalogoCuentaHijo == 0)
                    catalogoCuenta.IdCatalogoCuentaHijo = null;

                var response = await apiServicio.InsertarAsync(catalogoCuenta, new Uri(WebApp.BaseAddressRM), "api/CatalogoCuenta/InsertarCatalogoCuenta");
                if (response.IsSuccess)
                {
                    var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado un catálogo de cuenta", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Catálogo de Cuenta:", catalogoCuenta.IdCatalogoCuenta) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}");
                }
                ViewData["Error"] = response.Message;
                ViewData["IdCatalogoCuentaHijoVisible"] = listaCatalogosCuenta.Count > 0;
                listaCatalogosCuenta.Insert(0, new CatalogoCuenta { IdCatalogoCuentaHijo = 0, Codigo = "<< Sin selección >>" });
                ViewData["IdCatalogoCuentaHijo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(listaCatalogosCuenta, "IdCatalogoCuenta", "Codigo");
                return View(catalogoCuenta);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando Catálogo de Cuenta", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}");
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM), "api/CatalogoCuenta");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");

                    var listaCatalogosCuenta = await apiServicio.Listar<CatalogoCuenta>(new Uri(WebApp.BaseAddressRM), "api/CatalogoCuenta/ListarCatalogosCuenta");
                    ViewData["IdCatalogoCuentaHijoVisible"] = listaCatalogosCuenta.Count > 1;
                    listaCatalogosCuenta.Insert(0, new CatalogoCuenta { IdCatalogoCuentaHijo = 0, Codigo = "<< Sin selección >>" });
                    var catalogoCuenta = JsonConvert.DeserializeObject<CatalogoCuenta>(respuesta.Resultado.ToString());
                    ViewData["IdCatalogoCuentaHijo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(listaCatalogosCuenta, "IdCatalogoCuenta", "Codigo");
                    return View(catalogoCuenta);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CatalogoCuenta catalogoCuenta)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var response = await apiServicio.EditarAsync(id, catalogoCuenta, new Uri(WebApp.BaseAddressRM), "api/CatalogoCuenta");
                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Catálogo de Cuenta", id), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha actualizado un registro catálogo de cuenta", UserName = "Usuario 1" });
                        return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}");
                    }
                    ViewData["Error"] = response.Message;
                    var listaCatalogosCuenta = await apiServicio.Listar<CatalogoCuenta>(new Uri(WebApp.BaseAddressRM), "api/CatalogoCuenta/ListarCatalogosCuenta");
                    ViewData["IdCatalogoCuentaHijoVisible"] = listaCatalogosCuenta.Count > 1;
                    listaCatalogosCuenta.Insert(0, new CatalogoCuenta { IdCatalogoCuentaHijo = 0, Codigo = "<< Sin selección >>" });
                    ViewData["IdCatalogoCuentaHijo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(listaCatalogosCuenta, "IdCatalogoCuenta", "Codigo");
                    return View(catalogoCuenta);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Editando un catálogo de cuenta", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorEditar}");
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddressRM), "api/CatalogoCuenta");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Catálogo de Cuenta", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webappth" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}");
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar Catálogo de Cuenta", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}");
            }
        }
    }
}