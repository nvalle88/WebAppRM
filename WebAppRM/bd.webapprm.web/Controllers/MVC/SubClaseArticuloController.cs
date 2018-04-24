using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using bd.webapprm.servicios.Interfaces;
using bd.webapprm.entidades.Negocio;
using bd.webapprm.entidades.Utils;
using bd.log.guardar.Servicios;
using bd.log.guardar.ObjectTranfer;
using bd.log.guardar.Enumeradores;
using Newtonsoft.Json;
using bd.webapprm.entidades;
using bbd.webapprm.servicios.Enumeradores;
using bd.webapprm.servicios.Extensores;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace bd.webapprm.web.Controllers.MVC
{
    public class SubClaseArticuloController : Controller
    {
        private readonly IApiServicio apiServicio;

        public SubClaseArticuloController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        public async Task<IActionResult> Index()
        {
            var lista = new List<SubClaseArticulo>();
            try
            {
                lista = await apiServicio.Listar<SubClaseArticulo>(new Uri(WebApp.BaseAddressRM), "api/SubClaseArticulo/ListarSubClaseArticulos");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando SubClaseArticulo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                ViewData["TipoArticulo"] = new SelectList(await apiServicio.Listar<TipoArticulo>(new Uri(WebApp.BaseAddressRM), "api/TipoArticulo/ListarTipoArticulo"), "IdTipoArticulo", "Nombre");
                ViewData["ClaseArticulo"] = await ObtenerSelectListClaseArticulo((ViewData["TipoArticulo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["TipoArticulo"] as SelectList).FirstOrDefault().Value) : -1);
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubClaseArticulo subClaseArticulo)
        {
            try
            {
                subClaseArticulo.ClaseArticulo = JsonConvert.DeserializeObject<ClaseArticulo>((await apiServicio.SeleccionarAsync<Response>(subClaseArticulo.IdClaseArticulo.ToString(), new Uri(WebApp.BaseAddressRM), "api/ClaseArticulo")).Resultado.ToString());
                var response = await apiServicio.InsertarAsync(subClaseArticulo, new Uri(WebApp.BaseAddressRM), "api/SubClaseArticulo/InsertarSubClaseArticulo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado un SubClaseArticulo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "SubClaseArticulo:", subClaseArticulo.IdSubClaseArticulo) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}");
                }
                ViewData["Error"] = response.Message;
                ViewData["TipoArticulo"] = new SelectList(await apiServicio.Listar<TipoArticulo>(new Uri(WebApp.BaseAddressRM), "api/TipoArticulo/ListarTipoArticulo"), "IdTipoArticulo", "Nombre");
                ViewData["ClaseArticulo"] = await ObtenerSelectListClaseArticulo(subClaseArticulo?.ClaseArticulo?.IdTipoArticulo ?? -1);
                return View(subClaseArticulo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando SubClaseArticulo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}");
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM), "api/SubClaseArticulo");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");

                    var subClaseArticulo = JsonConvert.DeserializeObject<SubClaseArticulo>(respuesta.Resultado.ToString());
                    ViewData["TipoArticulo"] = new SelectList(await apiServicio.Listar<TipoArticulo>(new Uri(WebApp.BaseAddressRM), "api/TipoArticulo/ListarTipoArticulo"), "IdTipoArticulo", "Nombre");
                    ViewData["ClaseArticulo"] = await ObtenerSelectListClaseArticulo(subClaseArticulo.ClaseArticulo.IdTipoArticulo);
                    return View(subClaseArticulo);
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
        public async Task<IActionResult> Edit(string id, SubClaseArticulo subClaseArticulo)
        {
            Response response = new Response();
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    subClaseArticulo.ClaseArticulo = JsonConvert.DeserializeObject<ClaseArticulo>((await apiServicio.SeleccionarAsync<Response>(subClaseArticulo.IdClaseArticulo.ToString(), new Uri(WebApp.BaseAddressRM), "api/ClaseArticulo")).Resultado.ToString());
                    response = await apiServicio.EditarAsync(id, subClaseArticulo, new Uri(WebApp.BaseAddressRM), "api/SubClaseArticulo");
                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "SubClaseArticulo", id), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha actualizado un registro SubClaseArticulo", UserName = "Usuario 1" });
                        return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}");
                    }
                    ViewData["Error"] = response.Message;
                    ViewData["TipoArticulo"] = new SelectList(await apiServicio.Listar<TipoArticulo>(new Uri(WebApp.BaseAddressRM), "api/TipoArticulo/ListarTipoArticulo"), "IdTipoArticulo", "Nombre");
                    ViewData["ClaseArticulo"] = await ObtenerSelectListClaseArticulo(subClaseArticulo?.ClaseArticulo?.IdTipoArticulo ?? -1);
                    return View(subClaseArticulo);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Editando un SubClaseArticulo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorEditar}");
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddressRM), "api/SubClaseArticulo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "SubClaseArticulo", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webappth" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}");
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar SubClaseArticulo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}");
            }
        }

        public async Task<SelectList> ObtenerSelectListClaseArticulo(int idTipoArticulo)
        {
            try
            {
                var listaClaseArticulo = idTipoArticulo != -1 ? (await apiServicio.Listar<ClaseArticulo>(new Uri(WebApp.BaseAddressRM), "api/ClaseArticulo/ListarClaseArticulo")).Where(c => c.IdTipoArticulo == idTipoArticulo) : new List<ClaseArticulo>();
                return new SelectList(listaClaseArticulo, "IdClaseArticulo", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<ClaseArticulo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClaseArticulo_SelectResult(int idTipoArticulo)
        {
            ViewBag.ClaseArticulo = await ObtenerSelectListClaseArticulo(idTipoArticulo);
            return PartialView("_ClaseArticuloSelect", new SubClaseArticulo());
        }
    }
}