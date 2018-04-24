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
    public class DetalleFacturaController : Controller
    {
        private readonly IApiServicio apiServicio;

        public DetalleFacturaController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        public async Task<IActionResult> Index()
        {
            var lista = new List<DetalleFactura>();
            try
            {
                lista = await apiServicio.Listar<DetalleFactura>(new Uri(WebApp.BaseAddressRM), "api/DetalleFactura/ListarDetallesFactura");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando detalles de factura", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                ViewData["IdFactura"] = new SelectList(await apiServicio.Listar<Factura>(new Uri(WebApp.BaseAddressRM), "api/Factura/ListarFacturas"), "IdFactura", "Numero");
                ViewData["IdArticulo"] = new SelectList(await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddressRM), "api/Articulo/ListarArticulos"), "IdArticulo", "Nombre");
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DetalleFactura detalleFactura)
        {
            try
            {
                var response = await apiServicio.InsertarAsync(detalleFactura, new Uri(WebApp.BaseAddressRM), "api/DetalleFactura/InsertarDetalleFactura");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado un detalle de factura", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Detalle de Factura:", detalleFactura.IdDetalleFactura) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}");
                }
                ViewData["Error"] = response.Message;
                ViewData["IdFactura"] = new SelectList(await apiServicio.Listar<Factura>(new Uri(WebApp.BaseAddressRM), "api/Factura/ListarFacturas"), "IdFactura", "Numero");
                ViewData["IdArticulo"] = new SelectList(await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddressRM), "api/Articulo/ListarArticulos"), "IdArticulo", "Nombre");
                return View(detalleFactura);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando Detalle de factura", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}");
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM), "api/DetalleFactura");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");

                    respuesta.Resultado = JsonConvert.DeserializeObject<DetalleFactura>(respuesta.Resultado.ToString());
                    ViewData["IdFactura"] = new SelectList(await apiServicio.Listar<Factura>(new Uri(WebApp.BaseAddressRM), "api/Factura/ListarFacturas"), "IdFactura", "Numero");
                    ViewData["IdArticulo"] = new SelectList(await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddressRM), "api/Articulo/ListarArticulos"), "IdArticulo", "Nombre");
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
        public async Task<IActionResult> Edit(string id, DetalleFactura detalleFactura)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var response = await apiServicio.EditarAsync(id, detalleFactura, new Uri(WebApp.BaseAddressRM), "api/DetalleFactura");
                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Detalle de Factura", id), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha actualizado un detalle de factura", UserName = "Usuario 1" });
                        return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}");
                    }
                    ViewData["Error"] = response.Message;
                    ViewData["IdFactura"] = new SelectList(await apiServicio.Listar<Factura>(new Uri(WebApp.BaseAddressRM), "api/Factura/ListarFacturas"), "IdFactura", "Numero");
                    ViewData["IdArticulo"] = new SelectList(await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddressRM), "api/Articulo/ListarArticulos"), "IdArticulo", "Nombre");
                    return View(detalleFactura);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Editando un detalle de factura", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorEditar}");
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddressRM), "api/DetalleFactura");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Detalle de Factura", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webappth" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}");
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar Detalle de Factura", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}");
            }
        }
    }
}