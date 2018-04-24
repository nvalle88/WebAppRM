using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using bd.webapprm.servicios.Interfaces;
using bd.webapprm.entidades;
using bd.webapprm.entidades.Utils;
using bd.log.guardar.Servicios;
using bd.log.guardar.ObjectTranfer;
using bbd.webapprm.servicios.Enumeradores;
using bd.log.guardar.Enumeradores;
using Newtonsoft.Json;
using bd.webapprm.servicios.Extensores;

namespace bd.webapprm.web.Controllers.MVC
{
    public class MaestroDetalleArticuloController : Controller
    {
        private readonly IApiServicio apiServicio;


        public MaestroDetalleArticuloController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                ViewData["IdArticulo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddressRM), "api/Articulo/ListarArticulos"), "IdArticulo", "Nombre");
                ViewData["MaestroArticuloSucursal"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList((await apiServicio.Listar<MaestroArticuloSucursal>(new Uri(WebApp.BaseAddressRM), "api/MaestroArticuloSucursal/ListarMaestroArticuloSucursal")).Select(c=> new { c.IdMaestroArticuloSucursal, Nombre = $"{c.Sucursal.Ciudad.Provincia.Pais.Nombre} - {c.Sucursal.Ciudad.Provincia.Nombre} - {c.Sucursal.Ciudad.Nombre} - {c.Sucursal.Nombre}" }), "IdMaestroArticuloSucursal", "Nombre");
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaestroDetalleArticulo maestroDetalleArticulo)
        {
            try
            {
                var response = await apiServicio.InsertarAsync(maestroDetalleArticulo, new Uri(WebApp.BaseAddressRM), "api/MaestroDetalleArticulo/InsertarMaestroDetalleArticulo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado un MaestroDetalleArticulo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "MaestroDetalleArticulo:", maestroDetalleArticulo.IdMaestroDetalleArticulo) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}");
                }
                ViewData["Error"] = response.Message;
                ViewData["IdArticulo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddressRM), "api/Articulo/ListarArticulos"), "IdArticulo", "Nombre");
                ViewData["MaestroArticuloSucursal"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList((await apiServicio.Listar<MaestroArticuloSucursal>(new Uri(WebApp.BaseAddressRM), "api/MaestroArticuloSucursal/ListarMaestroArticuloSucursal")).Select(c => new { c.IdMaestroArticuloSucursal, Nombre = $"{c.Sucursal.Ciudad.Provincia.Pais.Nombre} - {c.Sucursal.Ciudad.Provincia.Nombre} - {c.Sucursal.Ciudad.Nombre} - {c.Sucursal.Nombre}" }), "IdMaestroArticuloSucursal", "Nombre");
                return View(maestroDetalleArticulo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando MaestroDetalleArticulo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}");
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM), "api/MaestroDetalleArticulo");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");

                    respuesta.Resultado = JsonConvert.DeserializeObject<MaestroDetalleArticulo>(respuesta.Resultado.ToString());
                    ViewData["IdArticulo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddressRM), "api/Articulo/ListarArticulos"), "IdArticulo", "Nombre");
                    ViewData["MaestroArticuloSucursal"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList((await apiServicio.Listar<MaestroArticuloSucursal>(new Uri(WebApp.BaseAddressRM), "api/MaestroArticuloSucursal/ListarMaestroArticuloSucursal")).Select(c => new { c.IdMaestroArticuloSucursal, Nombre = $"{c.Sucursal.Ciudad.Provincia.Pais.Nombre} - {c.Sucursal.Ciudad.Provincia.Nombre} - {c.Sucursal.Ciudad.Nombre} - {c.Sucursal.Nombre}" }), "IdMaestroArticuloSucursal", "Nombre");
                    return View(respuesta.Resultado);
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
        public async Task<IActionResult> Edit(string id, MaestroDetalleArticulo maestroDetalleArticulo)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var response = await apiServicio.EditarAsync(id, maestroDetalleArticulo, new Uri(WebApp.BaseAddressRM), "api/MaestroDetalleArticulo");
                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Sistema", id), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha actualizado un registro sistema", UserName = "Usuario 1" });
                        return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}");
                    }
                    ViewData["Error"] = response.Message;
                    ViewData["IdArticulo"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddressRM), "api/Articulo/ListarArticulos"), "IdArticulo", "Nombre");
                    ViewData["MaestroArticuloSucursal"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList((await apiServicio.Listar<MaestroArticuloSucursal>(new Uri(WebApp.BaseAddressRM), "api/MaestroArticuloSucursal/ListarMaestroArticuloSucursal")).Select(c => new { c.IdMaestroArticuloSucursal, Nombre = $"{c.Sucursal.Ciudad.Provincia.Pais.Nombre} - {c.Sucursal.Ciudad.Provincia.Nombre} - {c.Sucursal.Ciudad.Nombre} - {c.Sucursal.Nombre}" }), "IdMaestroArticuloSucursal", "Nombre");
                    return View(maestroDetalleArticulo);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Editando una MaestroDetalleArticulo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorEditar}");
            }
        }

        public async Task<IActionResult> Index()
        {
            var lista = new List<MaestroDetalleArticulo>();
            try
            {
                lista = await apiServicio.Listar<MaestroDetalleArticulo>(new Uri(WebApp.BaseAddressRM), "api/MaestroDetalleArticulo/ListarMaestroDetalleArticulo");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando MaestroDetalleArticulo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddressRM), "api/MaestroDetalleArticulo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Sistema", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webappth" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}");
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar MaestroDetalleArticulo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}");
            }
        }
    }
}