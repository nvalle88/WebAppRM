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
using Microsoft.AspNetCore.Mvc.Rendering;

namespace bd.webapprm.web.Controllers.MVC
{
    public class ArticuloController : Controller
    {
        private readonly IApiServicio apiServicio;

        public ArticuloController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        public async Task<IActionResult> Index()
        {
            var lista = new List<Articulo>();
            try
            {
                lista = await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddressRM), "api/Articulo/ListarArticulos");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando artículos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
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
                ViewData["SubClaseArticulo"] = await ObtenerSelectListSubClaseArticulo((ViewData["ClaseArticulo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["ClaseArticulo"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["IdMarca"] = new SelectList(await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddressRM), "api/Marca/ListarMarca"), "IdMarca", "Nombre");
                ViewData["IdModelo"] = await ObtenerSelectListModelo((ViewData["IdMarca"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["IdMarca"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["IdUnidadMedida"] = new SelectList(await apiServicio.Listar<UnidadMedida>(new Uri(WebApp.BaseAddressRM), "api/UnidadMedida/ListarUnidadMedida"), "IdUnidadMedida", "Nombre");
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Articulo articulo)
        {
            try
            {
                articulo.SubClaseArticulo = JsonConvert.DeserializeObject<SubClaseArticulo>((await apiServicio.SeleccionarAsync<Response>(articulo.IdSubClaseArticulo.ToString(), new Uri(WebApp.BaseAddressRM), "api/SubClaseArticulo")).Resultado.ToString());
                articulo.Modelo = JsonConvert.DeserializeObject<Modelo>((await apiServicio.SeleccionarAsync<Response>(articulo.IdModelo.ToString(), new Uri(WebApp.BaseAddressRM), "api/Modelo")).Resultado.ToString());
                var response = await apiServicio.InsertarAsync(articulo, new Uri(WebApp.BaseAddressRM), "api/Articulo/InsertarArticulo");
                if (response.IsSuccess)
                {
                    var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado un artículo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Artículo:", articulo.IdArticulo) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}");
                }
                ViewData["Error"] = response.Message;
                ViewData["TipoArticulo"] = new SelectList(await apiServicio.Listar<TipoArticulo>(new Uri(WebApp.BaseAddressRM), "api/TipoArticulo/ListarTipoArticulo"), "IdTipoArticulo", "Nombre");
                ViewData["ClaseArticulo"] = await ObtenerSelectListClaseArticulo(articulo?.SubClaseArticulo?.ClaseArticulo?.IdTipoArticulo ?? -1);
                ViewData["SubClaseArticulo"] = await ObtenerSelectListSubClaseArticulo(articulo?.SubClaseArticulo?.IdClaseArticulo ?? -1);
                ViewData["IdMarca"] = new SelectList(await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddressRM), "api/Marca/ListarMarca"), "IdMarca", "Nombre");
                ViewData["IdModelo"] = await ObtenerSelectListModelo(articulo?.Modelo?.IdMarca ?? -1);
                ViewData["IdUnidadMedida"] = new SelectList(await apiServicio.Listar<UnidadMedida>(new Uri(WebApp.BaseAddressRM), "api/UnidadMedida/ListarUnidadMedida"), "IdUnidadMedida", "Nombre");
                return View(articulo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando Artículo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}");
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM), "api/Articulo");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");
                    
                    var articulo = JsonConvert.DeserializeObject<Articulo>(respuesta.Resultado.ToString());
                    ViewData["TipoArticulo"] = new SelectList(await apiServicio.Listar<TipoArticulo>(new Uri(WebApp.BaseAddressRM), "api/TipoArticulo/ListarTipoArticulo"), "IdTipoArticulo", "Nombre");
                    ViewData["ClaseArticulo"] = await ObtenerSelectListClaseArticulo(articulo?.SubClaseArticulo?.ClaseArticulo?.IdTipoArticulo ?? -1);
                    ViewData["SubClaseArticulo"] = await ObtenerSelectListSubClaseArticulo(articulo?.SubClaseArticulo?.IdClaseArticulo ?? -1);
                    ViewData["IdMarca"] = new SelectList(await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddressRM), "api/Marca/ListarMarca"), "IdMarca", "Nombre");
                    ViewData["IdModelo"] = await ObtenerSelectListModelo(articulo?.Modelo?.IdMarca ?? -1);
                    ViewData["IdUnidadMedida"] = new SelectList(await apiServicio.Listar<UnidadMedida>(new Uri(WebApp.BaseAddressRM), "api/UnidadMedida/ListarUnidadMedida"), "IdUnidadMedida", "Nombre");
                    return View(articulo);
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
        public async Task<IActionResult> Edit(string id, Articulo articulo)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    articulo.SubClaseArticulo = JsonConvert.DeserializeObject<SubClaseArticulo>((await apiServicio.SeleccionarAsync<Response>(articulo.IdSubClaseArticulo.ToString(), new Uri(WebApp.BaseAddressRM), "api/SubClaseArticulo")).Resultado.ToString());
                    articulo.Modelo = JsonConvert.DeserializeObject<Modelo>((await apiServicio.SeleccionarAsync<Response>(articulo.IdModelo.ToString(), new Uri(WebApp.BaseAddressRM), "api/Modelo")).Resultado.ToString());
                    var response = await apiServicio.EditarAsync(id, articulo, new Uri(WebApp.BaseAddressRM), "api/Articulo");
                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Artículo", id), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha actualizado un registro artículo", UserName = "Usuario 1" });
                        return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}");
                    }
                    ViewData["Error"] = response.Message;
                    ViewData["TipoArticulo"] = new SelectList(await apiServicio.Listar<TipoArticulo>(new Uri(WebApp.BaseAddressRM), "api/TipoArticulo/ListarTipoArticulo"), "IdTipoArticulo", "Nombre");
                    ViewData["ClaseArticulo"] = await ObtenerSelectListClaseArticulo(articulo?.SubClaseArticulo?.ClaseArticulo?.IdTipoArticulo ?? -1);
                    ViewData["SubClaseArticulo"] = await ObtenerSelectListSubClaseArticulo(articulo?.SubClaseArticulo?.IdClaseArticulo ?? -1);
                    ViewData["IdMarca"] = new SelectList(await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddressRM), "api/Marca/ListarMarca"), "IdMarca", "Nombre");
                    ViewData["IdModelo"] = await ObtenerSelectListModelo(articulo?.Modelo?.IdMarca ?? -1);
                    ViewData["IdUnidadMedida"] = new SelectList(await apiServicio.Listar<UnidadMedida>(new Uri(WebApp.BaseAddressRM), "api/UnidadMedida/ListarUnidadMedida"), "IdUnidadMedida", "Nombre");
                    return View(articulo);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Editando un artículo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorEditar}");
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddressRM), "api/Articulo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Artículo", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webappth" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}");
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar Artículo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}");
            }
        }

        public async Task<SelectList> ObtenerSelectListClaseArticulo(int idTipoArticulo)
        {
            try
            {
                var listaClaseArticulo = idTipoArticulo != -1 ? await apiServicio.Listar<ClaseArticulo>(new Uri(WebApp.BaseAddressRM), $"api/ClaseArticulo/ListarClaseArticuloPorTipoArticulo/{idTipoArticulo}") : new List<ClaseArticulo>();
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
            return PartialView("_ClaseArticuloSelect", new Articulo());
        }

        public async Task<SelectList> ObtenerSelectListSubClaseArticulo(int idClaseArticulo)
        {
            try
            {
                var listaSubClaseArticulo = idClaseArticulo != -1 ? await apiServicio.Listar<SubClaseArticulo>(new Uri(WebApp.BaseAddressRM), $"api/SubClaseArticulo/ListarSubClaseArticulosPorClase/{idClaseArticulo}") : new List<SubClaseArticulo>();
                return new SelectList(listaSubClaseArticulo, "IdSubClaseArticulo", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<SubClaseArticulo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubClaseArticulo_SelectResult(int idClaseArticulo)
        {
            ViewBag.SubClaseArticulo = await ObtenerSelectListSubClaseArticulo(idClaseArticulo);
            return PartialView("_SubClaseArticuloSelect", new Articulo());
        }

        public async Task<SelectList> ObtenerSelectListModelo(int idMarca)
        {
            try
            {
                var listaModelo = idMarca != -1 ? await apiServicio.Listar<Modelo>(new Uri(WebApp.BaseAddressRM), $"api/Modelo/ListarModelosPorMarca/{idMarca}") : new List<Modelo>();
                return new SelectList(listaModelo, "IdModelo", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<Modelo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Modelo_SelectResult(int idMarca)
        {
            ViewBag.IdModelo = await ObtenerSelectListModelo(idMarca);
            return PartialView("_ModeloSelect", new Articulo());
        }
    }
}