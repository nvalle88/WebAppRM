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
using bd.webapprm.entidades.ObjectTransfer;

namespace bd.webapprm.web.Controllers.MVC
{
    public class BodegaController : Controller
    {
        private readonly IApiServicio apiServicio;
        private readonly IClaimsTransfer claimsTransfer;

        public BodegaController(IApiServicio apiServicio, IClaimsTransfer claimsTransfer)
        {
            this.apiServicio = apiServicio;
            this.claimsTransfer = claimsTransfer;
        }

        public async Task<IActionResult> Index()
        {
            var lista = new List<Bodega>();
            try
            {
                lista = await apiServicio.Listar<Bodega>(new Uri(WebApp.BaseAddressRM), "api/Bodega/ListarBodega");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando bodegas", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var claimTransfer = claimsTransfer.ObtenerClaimsTransferHttpContext();
                ViewData["Sucursal"] = new SelectList(new List<Sucursal> { new Sucursal { IdSucursal = (int)claimTransfer.IdSucursal, Nombre = claimTransfer.NombreSucursal } }, "IdSucursal", "Nombre");
                ViewData["Empleado"] = new SelectList((await apiServicio.ObtenerElementoAsync<List<DatosBasicosEmpleadoViewModel>>(new EmpleadosPorSucursalViewModel { IdSucursal = claimTransfer.IdSucursal, EmpleadosActivos = true }, new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleadosPorSucursal")).Select(c => new ListaEmpleadoViewModel { IdEmpleado = c.IdEmpleado, NombreApellido = $"{c.Nombres} {c.Apellidos}" }), "IdEmpleado", "NombreApellido");
                ViewData["Dependencia"] = new MultiSelectList(await apiServicio.ObtenerElementoAsync<List<Dependencia>>(new Sucursal { IdSucursal = claimTransfer.IdSucursal }, new Uri(WebApp.BaseAddressTH), "api/Dependencias/ListarDependenciaporSucursal"), "IdDependencia", "Nombre");
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Bodega bodega, List<int> dependencias)
        {
            try
            {
                var claimTransfer = claimsTransfer.ObtenerClaimsTransferHttpContext();
                bodega.IdSucursal = claimTransfer.IdSucursal;
                dependencias.ForEach(c => bodega.Dependencia.Add(new Dependencia { IdDependencia = c }));

                var response = await apiServicio.InsertarAsync(bodega, new Uri(WebApp.BaseAddressRM), "api/Bodega/InsertarBodega");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado una Bodega", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Bodega:", bodega.IdBodega) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}");
                }
                ViewData["Error"] = response.Message;
                ViewData["Sucursal"] = new SelectList(new List<Sucursal> { new Sucursal { IdSucursal = (int)claimTransfer.IdSucursal, Nombre = claimTransfer.NombreSucursal } }, "IdSucursal", "Nombre");
                ViewData["Empleado"] = new SelectList((await apiServicio.ObtenerElementoAsync<List<DatosBasicosEmpleadoViewModel>>(new EmpleadosPorSucursalViewModel { IdSucursal = claimTransfer.IdSucursal, EmpleadosActivos = true }, new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleadosPorSucursal")).Select(c => new ListaEmpleadoViewModel { IdEmpleado = c.IdEmpleado, NombreApellido = $"{c.Nombres} {c.Apellidos}" }), "IdEmpleado", "NombreApellido");
                ViewData["Dependencia"] = new MultiSelectList(await apiServicio.ObtenerElementoAsync<List<Dependencia>>(new Sucursal { IdSucursal = claimTransfer.IdSucursal }, new Uri(WebApp.BaseAddressTH), "api/Dependencias/ListarDependenciaporSucursal"), "IdDependencia", "Nombre", dependencias);
                return View(bodega);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando una Bodega", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}");
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM), "api/Bodega");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");

                    var bodega = JsonConvert.DeserializeObject<Bodega>(respuesta.Resultado.ToString());
                    var claimTransfer = claimsTransfer.ObtenerClaimsTransferHttpContext();
                    ViewData["Sucursal"] = new SelectList(new List<Sucursal> { new Sucursal { IdSucursal = (int)claimTransfer.IdSucursal, Nombre = claimTransfer.NombreSucursal } }, "IdSucursal", "Nombre");
                    ViewData["Empleado"] = new SelectList((await apiServicio.ObtenerElementoAsync<List<DatosBasicosEmpleadoViewModel>>(new EmpleadosPorSucursalViewModel { IdSucursal = claimTransfer.IdSucursal, EmpleadosActivos = true }, new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleadosPorSucursal")).Select(c => new ListaEmpleadoViewModel { IdEmpleado = c.IdEmpleado, NombreApellido = $"{c.Nombres} {c.Apellidos}" }), "IdEmpleado", "NombreApellido");
                    ViewData["Dependencia"] = new MultiSelectList(await apiServicio.ObtenerElementoAsync<List<Dependencia>>(new Sucursal { IdSucursal = claimTransfer.IdSucursal }, new Uri(WebApp.BaseAddressTH), "api/Dependencias/ListarDependenciaporSucursal"), "IdDependencia", "Nombre", (await apiServicio.ObtenerElementoAsync<List<Dependencia>>(bodega.IdBodega, new Uri(WebApp.BaseAddressRM), "api/Bodega/ListadoDependenciasPorBodega")).Select(c=> c.IdDependencia));
                    return View(bodega);
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
        public async Task<IActionResult> Edit(string id, Bodega bodega, List<int> dependencias)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var claimTransfer = claimsTransfer.ObtenerClaimsTransferHttpContext();
                    bodega.IdSucursal = claimTransfer.IdSucursal;
                    dependencias.ForEach(c => bodega.Dependencia.Add(new Dependencia { IdDependencia = c }));

                    var response = await apiServicio.EditarAsync(id, bodega, new Uri(WebApp.BaseAddressRM), "api/Bodega");
                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Bodega", id), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha actualizado un registro Bodega", UserName = "Usuario 1" });
                        return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}");
                    }
                    ViewData["Error"] = response.Message;
                    ViewData["Sucursal"] = new SelectList(new List<Sucursal> { new Sucursal { IdSucursal = (int)claimTransfer.IdSucursal, Nombre = claimTransfer.NombreSucursal } }, "IdSucursal", "Nombre");
                    ViewData["Empleado"] = new SelectList((await apiServicio.ObtenerElementoAsync<List<DatosBasicosEmpleadoViewModel>>(new EmpleadosPorSucursalViewModel { IdSucursal = claimTransfer.IdSucursal, EmpleadosActivos = true }, new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleadosPorSucursal")).Select(c => new ListaEmpleadoViewModel { IdEmpleado = c.IdEmpleado, NombreApellido = $"{c.Nombres} {c.Apellidos}" }), "IdEmpleado", "NombreApellido");
                    ViewData["Dependencia"] = new MultiSelectList(await apiServicio.ObtenerElementoAsync<List<Dependencia>>(new Sucursal { IdSucursal = claimTransfer.IdSucursal }, new Uri(WebApp.BaseAddressTH), "api/Dependencias/ListarDependenciaporSucursal"), "IdDependencia", "Nombre", dependencias);
                    return View(bodega);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Editando una Bodega", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorEditar}");
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddressRM), "api/Bodega");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Bodega", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webapprm" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}");
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar Bodega", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}");
            }
        }
    }
}