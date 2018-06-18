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
    public class MaestroArticuloSucursalController : Controller
    {
        private readonly IApiServicio apiServicio;

        public MaestroArticuloSucursalController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        public async Task<IActionResult> Index()
        {
            var lista = new List<MaestroArticuloSucursal>();
            try
            {
                lista = await apiServicio.Listar<MaestroArticuloSucursal>(new Uri(WebApp.BaseAddressRM), "api/MaestroArticuloSucursal/ListarMaestroArticuloSucursal");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando maestros de artículos de sucursal", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                ViewData["Sucursal"] = new SelectList(await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal"), "IdSucursal", "Nombre");
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}");
            }
        }

        private bool ValidacionMinMax(MaestroArticuloSucursal maestroArticuloSucursal, Response response)
        {
            if (maestroArticuloSucursal.Minimo > 0 && maestroArticuloSucursal.Maximo > 0)
            {
                if (maestroArticuloSucursal.Minimo > maestroArticuloSucursal.Maximo)
                {
                    ModelState.AddModelError("Minimo", "El Mínimo no puede ser mayor que el Máximo");
                    response.Message = "El Módelo es inválido";
                    return false;
                }
                return true;
            }
            return false;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaestroArticuloSucursal maestroArticuloSucursal)
        {
            try
            {
                var response = new Response();
                if (ValidacionMinMax(maestroArticuloSucursal, response))
                {
                    maestroArticuloSucursal.Sucursal = JsonConvert.DeserializeObject<Sucursal>((await apiServicio.SeleccionarAsync<Response>(maestroArticuloSucursal.IdSucursal.ToString(), new Uri(WebApp.BaseAddressTH), "api/Sucursal")).Resultado.ToString());
                    response = await apiServicio.InsertarAsync(maestroArticuloSucursal, new Uri(WebApp.BaseAddressRM), "api/MaestroArticuloSucursal/InsertarMaestroArticuloSucursal");
                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado un maestro artículo de sucursal", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Maestro Artículo de Sucursal:", maestroArticuloSucursal.IdMaestroArticuloSucursal) });
                        return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}");
                    }
                }
                ViewData["Error"] = response.Message;
                ViewData["Sucursal"] = new SelectList(await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal"), "IdSucursal", "Nombre");
                return View(maestroArticuloSucursal);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando Maestro Artículo de Sucursal", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}");
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM), "api/MaestroArticuloSucursal");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");

                    var maestroArticuloSucursal = JsonConvert.DeserializeObject<MaestroArticuloSucursal>(respuesta.Resultado.ToString());
                    ViewData["Sucursal"] = new SelectList(await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal"), "IdSucursal", "Nombre");
                    return View(maestroArticuloSucursal);
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
        public async Task<IActionResult> Edit(string id, MaestroArticuloSucursal maestroArticuloSucursal)
        {
            var response = new Response();
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    if (ValidacionMinMax(maestroArticuloSucursal, response))
                    {
                        maestroArticuloSucursal.Sucursal = JsonConvert.DeserializeObject<Sucursal>((await apiServicio.SeleccionarAsync<Response>(maestroArticuloSucursal.IdSucursal.ToString(), new Uri(WebApp.BaseAddressTH), "api/Sucursal")).Resultado.ToString());
                        response = await apiServicio.EditarAsync(id, maestroArticuloSucursal, new Uri(WebApp.BaseAddressRM), "api/MaestroArticuloSucursal");
                        if (response.IsSuccess)
                        {
                            await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Maestro Artículo de Sucursal", id), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha actualizado un registro maestro artículo de sucursal", UserName = "Usuario 1" });
                            return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}");
                        }
                    }
                    ViewData["Error"] = response.Message;
                    ViewData["Sucursal"] = new SelectList(await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal"), "IdSucursal", "Nombre");
                    return View(maestroArticuloSucursal);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Editando un maestro artículo de sucursal", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorEditar}");
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddressRM), "api/MaestroArticuloSucursal");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Maestro Artículo de Sucursal", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webapprm" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}");
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar Maestro Artículo de Sucursal", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}");
            }
        }
    }
}