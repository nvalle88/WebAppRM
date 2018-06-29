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
using bd.webapprm.entidades.ObjectTransfer;
using bd.webapprm.servicios.Extensores;

namespace bd.webapprm.web.Controllers.MVC
{
    public class ProveeduriaController : Controller
    {
        private readonly IApiServicio apiServicio;

        public ProveeduriaController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        #region Recepción de Proveeduría
        public IActionResult Index()
        {
            return RedirectToAction("ListadoRecepcion");
        }

        public async Task<IActionResult> ListadoRecepcion()
        {
            var lista = new List<RecepcionArticulos>();
            try
            {
                lista = await apiServicio.Listar<RecepcionArticulos>(new Uri(WebApp.BaseAddressRM), "api/RecepcionArticulo/ListarRecepcionArticulos");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando artículos recepcionados", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> Recepcion()
        {
            try
            {
                ViewData["TipoArticulo"] = new SelectList(await apiServicio.Listar<TipoArticulo>(new Uri(WebApp.BaseAddressRM), "api/TipoArticulo/ListarTipoArticulo"), "IdTipoArticulo", "Nombre");
                ViewData["ClaseArticulo"] = await ObtenerSelectListClaseArticulo((ViewData["TipoArticulo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["TipoArticulo"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["SubClaseArticulo"] = await ObtenerSelectListSubClaseArticulo((ViewData["ClaseArticulo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["ClaseArticulo"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["Articulo"] = await ObtenerSelectListArticulo((ViewData["SubClaseArticulo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["SubClaseArticulo"] as SelectList).FirstOrDefault().Value) : -1);
                
                ViewData["Sucursal"] = await ObtenerSelectListSucursal((ViewData["Ciudad"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Ciudad"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["MaestroArticuloSucursal"] = await ObtenerSelectListMaestroArticuloSucursal((ViewData["Sucursal"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Sucursal"] as SelectList).FirstOrDefault().Value) : -1);

                ViewData["Proveedor"] = new SelectList((await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedores")).Select(c => new { c.IdProveedor, NombreApellidos = String.Format("{0} {1}", c.Nombre, c.Apellidos) }), "IdProveedor", "NombreApellidos");
                ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoRecepcion));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recepcion(RecepcionArticulos recepcionArticulo) => await GestionarRecepcionArticulos(recepcionArticulo);

        public async Task<IActionResult> EditarRecepcion(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM), "api/RecepcionArticulo");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoRecepcion));
                    
                    var recepcionArticulos = JsonConvert.DeserializeObject<RecepcionArticulos>(respuesta.Resultado.ToString());
                    if (respuesta.IsSuccess)
                    {
                        //ViewData["TipoArticulo"] = new SelectList(await apiServicio.Listar<TipoArticulo>(new Uri(WebApp.BaseAddressRM), "api/TipoArticulo/ListarTipoArticulo"), "IdTipoArticulo", "Nombre");
                        //ViewData["ClaseArticulo"] = await ObtenerSelectListClaseArticulo(recepcionArticulos?.Articulo?.SubClaseArticulo?.ClaseArticulo?.TipoArticulo?.IdTipoArticulo ?? -1);
                        //ViewData["SubClaseArticulo"] = await ObtenerSelectListSubClaseArticulo(recepcionArticulos?.Articulo?.SubClaseArticulo?.ClaseArticulo?.IdClaseArticulo ?? -1);
                        //ViewData["Articulo"] = await ObtenerSelectListArticulo(recepcionArticulos?.Articulo?.SubClaseArticulo?.IdSubClaseArticulo ?? -1);
                        
                        //ViewData["Sucursal"] = await ObtenerSelectListSucursal(recepcionArticulos?.MaestroArticuloSucursal?.Sucursal?.Ciudad?.IdCiudad ?? -1);
                        //ViewData["MaestroArticuloSucursal"] = await ObtenerSelectListMaestroArticuloSucursal(recepcionArticulos?.MaestroArticuloSucursal?.Sucursal?.IdSucursal ?? -1);
                        
                        ViewData["Proveedor"] = new SelectList((await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedores")).Select(c => new { c.IdProveedor, NombreApellidos = String.Format("{0} {1}", c.Nombre, c.Apellidos) }), "IdProveedor", "NombreApellidos");
                        ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                        return View(recepcionArticulos);
                    }
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoRecepcion));
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoRecepcion));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRecepcion(RecepcionArticulos recepcionArticulo) => await GestionarRecepcionArticulos(recepcionArticulo);

        private async Task<IActionResult> GestionarRecepcionArticulos(RecepcionArticulos recepcionArticulo)
        {
            try
            {
                //recepcionArticulo.MaestroArticuloSucursal = JsonConvert.DeserializeObject<MaestroArticuloSucursal>((await apiServicio.SeleccionarAsync<Response>(recepcionArticulo.IdMaestroArticuloSucursal.ToString(), new Uri(WebApp.BaseAddressRM), "api/MaestroArticuloSucursal")).Resultado.ToString());
                //recepcionArticulo.IdMaestroArticuloSucursal = recepcionArticulo.MaestroArticuloSucursal.IdMaestroArticuloSucursal;
                //recepcionArticulo.Proveedor = JsonConvert.DeserializeObject<Proveedor>((await apiServicio.SeleccionarAsync<Response>(recepcionArticulo.IdProveedor.ToString(), new Uri(WebApp.BaseAddressRM), "api/Proveedor")).Resultado.ToString());
                //recepcionArticulo.IdProveedor = recepcionArticulo.Proveedor.IdProveedor;
                //recepcionArticulo.Articulo = JsonConvert.DeserializeObject<Articulo>((await apiServicio.SeleccionarAsync<Response>(recepcionArticulo.IdArticulo.ToString(), new Uri(WebApp.BaseAddressRM), "api/Articulo")).Resultado.ToString());
                //recepcionArticulo.IdArticulo = recepcionArticulo.Articulo.IdArticulo;

                var response = new Response();
                if (recepcionArticulo.IdRecepcionArticulos == 0)
                    response = await apiServicio.InsertarAsync(recepcionArticulo, new Uri(WebApp.BaseAddressRM), "api/RecepcionArticulo/InsertarRecepcionArticulo");
                else
                    response = await apiServicio.EditarAsync<RecepcionArticulos>(recepcionArticulo.IdRecepcionArticulos.ToString(), recepcionArticulo, new Uri(WebApp.BaseAddressRM), "api/RecepcionArticulo");

                if (response.IsSuccess)
                {
                    //await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = recepcionArticulo.IdRecepcionArticulos == 0 ? "Se ha recepcionado un artículo" : "Se ha editado la recepción de un artículo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Artículo:", recepcionArticulo.IdArticulo) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ListadoRecepcion));
                }

                //ViewData["TipoArticulo"] = new SelectList(await apiServicio.Listar<TipoArticulo>(new Uri(WebApp.BaseAddressRM), "api/TipoArticulo/ListarTipoArticulo"), "IdTipoArticulo", "Nombre");
                //ViewData["ClaseArticulo"] = await ObtenerSelectListClaseArticulo(recepcionArticulo?.Articulo?.SubClaseArticulo?.ClaseArticulo?.IdTipoArticulo ?? -1);
                //ViewData["SubClaseArticulo"] = await ObtenerSelectListSubClaseArticulo(recepcionArticulo?.Articulo?.SubClaseArticulo?.ClaseArticulo?.IdClaseArticulo ?? -1);
                //ViewData["Articulo"] = await ObtenerSelectListArticulo(recepcionArticulo?.Articulo?.SubClaseArticulo?.IdSubClaseArticulo ?? -1);
                                
                //ViewData["Sucursal"] = await ObtenerSelectListSucursal(recepcionArticulo?.MaestroArticuloSucursal?.Sucursal?.Ciudad?.IdCiudad ?? -1);
                //ViewData["MaestroArticuloSucursal"] = await ObtenerSelectListMaestroArticuloSucursal(recepcionArticulo?.MaestroArticuloSucursal?.Sucursal?.IdSucursal ?? -1);
                
                ViewData["Proveedor"] = new SelectList((await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedores")).Select(c => new { c.IdProveedor, NombreApellidos = String.Format("{0} {1}", c.Nombre, c.Apellidos) }), "IdProveedor", "NombreApellidos");
                ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                ViewData["Error"] = response.Message;
                return View(recepcionArticulo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando recepción Artículo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListadoRecepcion));
            }
        }
        #endregion

        #region AJAX_ClaseArticulo
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
            return PartialView("_ClaseArticuloSelect", new RecepcionArticulos());
        }
        #endregion

        #region AJAX_SubClaseArticulo
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
            return PartialView("_SubClaseArticuloSelect", new RecepcionArticulos());
        }
        #endregion

        #region AJAX_Articulo
        public async Task<SelectList> ObtenerSelectListArticulo(int idSubClaseArticulo)
        {
            try
            {
                var listaArticulo = idSubClaseArticulo != -1 ? await apiServicio.Listar<Articulo>(new Uri(WebApp.BaseAddressRM), $"api/Articulo/ListarArticulosPorSubClase/{idSubClaseArticulo}") : new List<Articulo>();
                return new SelectList(listaArticulo, "IdArticulo", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<Articulo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Articulo_SelectResult(int idSubClaseArticulo)
        {
            ViewBag.Articulo = await ObtenerSelectListArticulo(idSubClaseArticulo);
            return PartialView("_ArticuloSelect", new RecepcionArticulos());
        }
        #endregion

        #region AJAX_Sucursal
        public async Task<SelectList> ObtenerSelectListSucursal(int idCiudad)
        {
            try
            {
                var listaSucursal = idCiudad != -1 ? (await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal")).Where(c => c.IdCiudad == idCiudad).ToList() : new List<Sucursal>();
                return new SelectList(listaSucursal, "IdSucursal", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<Sucursal>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Sucursal_SelectResult(int idCiudad)
        {
            ViewBag.Sucursal = await ObtenerSelectListSucursal(idCiudad);
            return PartialView("_SucursalSelect", new RecepcionArticulos());
        }
        #endregion

        #region AJAX_MaestroArticuloSucursal
        public async Task<SelectList> ObtenerSelectListMaestroArticuloSucursal(int idSucursal)
        {
            try
            {
                var listaMaestroArticuloSucursal = idSucursal != -1 ? await apiServicio.Listar<MaestroArticuloSucursal>(new Uri(WebApp.BaseAddressRM), $"api/MaestroArticuloSucursal/ListarMaestroArticuloSucursalPorSucursal/{idSucursal}") : new List<MaestroArticuloSucursal>();
                return new SelectList(listaMaestroArticuloSucursal.Select(c => new { c.IdMaestroArticuloSucursal, Maestro = String.Format("Mínimo: {0} - Máximo: {1}", c.Minimo, c.Maximo) }), "IdMaestroArticuloSucursal", "Maestro");
            }
            catch (Exception)
            {
                return new SelectList(new List<MaestroArticuloSucursal>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> MaestroArticuloSucursal_SelectResult(int idSucursal)
        {
            ViewBag.MaestroArticuloSucursal = await ObtenerSelectListMaestroArticuloSucursal(idSucursal);
            return PartialView("_MaestroArticuloSucursalSelect", new RecepcionArticulos());
        }
        #endregion
    }
}
