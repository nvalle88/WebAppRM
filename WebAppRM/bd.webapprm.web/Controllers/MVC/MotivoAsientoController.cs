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
    public class MotivoAsientoController : Controller
    {
        private readonly IApiServicio apiServicio;

        public MotivoAsientoController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        public async Task<IActionResult> Index()
        {
            var lista = new List<MotivoAsiento>();
            try
            {
                lista = await apiServicio.Listar<MotivoAsiento>(new Uri(WebApp.BaseAddressRM), "api/MotivoAsiento/ListarMotivoAsiento");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando Motivos de Asientos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                ViewData["ConfiguracionContabilidad"] = new SelectList((await apiServicio.Listar<ConfiguracionContabilidad>(new Uri(WebApp.BaseAddressRM), "api/ConfiguracionContabilidad/ListarConfiguracionContabilidad")).Select(c=> new { c.IdConfiguracionContabilidad, Nombre = $"Valor Debe: {c.ValorD}, Valor Haber: {c.ValorH}" }), "IdConfiguracionContabilidad", "Nombre");
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MotivoAsiento motivoAsiento)
        {
            try
            {
                var response = await apiServicio.InsertarAsync(motivoAsiento, new Uri(WebApp.BaseAddressRM), "api/MotivoAsiento/InsertarMotivoAsiento");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado un Motivo de Asiento", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Motivo de Asiento:", motivoAsiento.IdMotivoAsiento) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}");
                }
                ViewData["Error"] = response.Message;
                ViewData["ConfiguracionContabilidad"] = new SelectList((await apiServicio.Listar<ConfiguracionContabilidad>(new Uri(WebApp.BaseAddressRM), "api/ConfiguracionContabilidad/ListarConfiguracionContabilidad")).Select(c => new { IdConfiguracionContabilidad = c.IdConfiguracionContabilidad, Nombre = $"Valor Debe: {c.ValorD}, Valor Haber: {c.ValorH}" }), "IdConfiguracionContabilidad", "Nombre");
                return View(motivoAsiento);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando Motivo de Asiento", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}");
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM), "api/MotivoAsiento");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");

                    respuesta.Resultado = JsonConvert.DeserializeObject<MotivoAsiento>(respuesta.Resultado.ToString());
                    ViewData["ConfiguracionContabilidad"] = new SelectList((await apiServicio.Listar<ConfiguracionContabilidad>(new Uri(WebApp.BaseAddressRM), "api/ConfiguracionContabilidad/ListarConfiguracionContabilidad")).Select(c => new { IdConfiguracionContabilidad = c.IdConfiguracionContabilidad, Nombre = $"Valor Debe: {c.ValorD}, Valor Haber: {c.ValorH}" }), "IdConfiguracionContabilidad", "Nombre");
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
        public async Task<IActionResult> Edit(string id, MotivoAsiento motivoAsiento)
        {
            Response response = new Response();
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    response = await apiServicio.EditarAsync(id, motivoAsiento, new Uri(WebApp.BaseAddressRM), "api/MotivoAsiento");
                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Motivo de Asiento", id), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha actualizado un registro Motivo de Asiento", UserName = "Usuario 1" });
                        return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}");
                    }
                    ViewData["Error"] = response.Message;
                    ViewData["ConfiguracionContabilidad"] = new SelectList((await apiServicio.Listar<ConfiguracionContabilidad>(new Uri(WebApp.BaseAddressRM), "api/ConfiguracionContabilidad/ListarConfiguracionContabilidad")).Select(c => new { IdConfiguracionContabilidad = c.IdConfiguracionContabilidad, Nombre = $"Valor Debe: {c.ValorD}, Valor Haber: {c.ValorH}" }), "IdConfiguracionContabilidad", "Nombre");
                    return View(motivoAsiento);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Editando un Motivo de Asiento", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorEditar}");
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddressRM), "api/MotivoAsiento");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Motivo de Asiento", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webappth" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}");
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar Motivo de Asiento", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}");
            }
        }
    }
}