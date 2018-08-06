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
using Microsoft.AspNetCore.Mvc.Rendering;
using bd.webapprm.servicios.Extensores;

namespace bd.webapprm.web.Controllers.MVC
{
    public class SubClaseActivoFijoController : Controller
    {
        private readonly IApiServicio apiServicio;

        public SubClaseActivoFijoController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        public async Task<IActionResult> Index()
        {
            var lista = new List<SubClaseActivoFijo>();
            try
            {
                lista = await apiServicio.Listar<SubClaseActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/SubClaseActivoFijo/ListarSubClasesActivoFijo");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando subclases de activo fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo((ViewData["TipoActivoFijo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["TipoActivoFijo"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["Ramo"] = new SelectList(await apiServicio.Listar<Ramo>(new Uri(WebApp.BaseAddressRM), "api/Ramo/ListarRamo"), "IdRamo", "Nombre");
                ViewData["Subramo"] = await ObtenerSelectListSubramo((ViewData["Ramo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Ramo"] as SelectList).FirstOrDefault().Value) : -1);
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubClaseActivoFijo subClaseActivoFijo)
        {
            try
            {
                var response = await apiServicio.InsertarAsync(subClaseActivoFijo, new Uri(WebApp.BaseAddressRM), "api/SubClaseActivoFijo/InsertarSubClaseActivoFijo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado una subclase de activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Sub Clase de Activo Fijo:", subClaseActivoFijo.IdClaseActivoFijo) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}");
                }
                ViewData["Error"] = response.Message;
                ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo(subClaseActivoFijo?.ClaseActivoFijo?.IdTipoActivoFijo ?? -1);
                ViewData["Ramo"] = new SelectList(await apiServicio.Listar<Ramo>(new Uri(WebApp.BaseAddressRM), "api/Ramo/ListarRamo"), "IdRamo", "Nombre");
                ViewData["Subramo"] = await ObtenerSelectListSubramo(subClaseActivoFijo?.Subramo?.IdRamo ?? -1);
                return View(subClaseActivoFijo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando una Subclase de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}");
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM), "api/SubClaseActivoFijo");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");

                    var subClaseActivoFijo = JsonConvert.DeserializeObject<SubClaseActivoFijo>(respuesta.Resultado.ToString());
                    ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                    ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo(subClaseActivoFijo.ClaseActivoFijo.IdTipoActivoFijo);
                    ViewData["Ramo"] = new SelectList(await apiServicio.Listar<Ramo>(new Uri(WebApp.BaseAddressRM), "api/Ramo/ListarRamo"), "IdRamo", "Nombre");
                    ViewData["Subramo"] = await ObtenerSelectListSubramo(subClaseActivoFijo?.Subramo?.IdRamo ?? -1);
                    return View(subClaseActivoFijo);
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
        public async Task<IActionResult> Edit(string id, SubClaseActivoFijo subClaseActivoFijo)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var response = await apiServicio.EditarAsync(id, subClaseActivoFijo, new Uri(WebApp.BaseAddressRM), "api/SubClaseActivoFijo");
                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Subclase de Activo Fijo", id), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha actualizado un registro sub clase de activo fijo", UserName = "Usuario 1" });
                        return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}");
                    }
                    ViewData["Error"] = response.Message;
                    ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                    ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo(subClaseActivoFijo?.ClaseActivoFijo?.IdTipoActivoFijo ?? -1);
                    ViewData["Ramo"] = new SelectList(await apiServicio.Listar<Ramo>(new Uri(WebApp.BaseAddressRM), "api/Ramo/ListarRamo"), "IdRamo", "Nombre");
                    ViewData["Subramo"] = await ObtenerSelectListSubramo(subClaseActivoFijo?.Subramo?.IdRamo ?? -1);
                    return View(subClaseActivoFijo);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Editando una subclase de activo fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorEditar}");
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddressRM), "api/SubClaseActivoFijo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Subclase de Activo Fijo", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webapprm" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}");
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar Subclase de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}");
            }
        }

        public async Task<SelectList> ObtenerSelectListClaseActivoFijo(int idTipoActivoFijo)
        {
            try
            {
                var listaClaseActivoFijo = idTipoActivoFijo != -1 ? await apiServicio.Listar<ClaseActivoFijo>(new Uri(WebApp.BaseAddressRM), $"api/ClaseActivoFijo/ListarClaseActivoFijoPorTipoActivoFijo/{idTipoActivoFijo}") : new List<ClaseActivoFijo>();
                return new SelectList(listaClaseActivoFijo, "IdClaseActivoFijo", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<ClaseActivoFijo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClaseActivoFijo_SelectResult(int idTipoActivoFijo)
        {
            ViewBag.ClaseActivoFijo = await ObtenerSelectListClaseActivoFijo(idTipoActivoFijo);
            return PartialView("_ClaseActivoFijoSelect", new SubClaseActivoFijo());
        }

        #region AJAX_Subramo
        public async Task<SelectList> ObtenerSelectListSubramo(int idRamo)
        {
            try
            {
                var listaSubramo = idRamo != -1 ? await apiServicio.Listar<Subramo>(new Uri(WebApp.BaseAddressRM), $"api/Subramo/ListarSubramoPorRamo/{idRamo}") : new List<Subramo>();
                return new SelectList(listaSubramo, "IdSubramo", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<Subramo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Subramo_SelectResult(int idRamo)
        {
            ViewBag.Subramo = await ObtenerSelectListSubramo(idRamo);
            return PartialView("_SubramoSelect", new SubClaseActivoFijo());
        }
        #endregion
    }
}