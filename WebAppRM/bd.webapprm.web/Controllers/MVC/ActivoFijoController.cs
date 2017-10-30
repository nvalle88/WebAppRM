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
using Microsoft.AspNetCore.Http;

namespace bd.webapprm.web.Controllers.MVC
{
    public class ActivoFijoController : Controller
    {
        private readonly IApiServicio apiServicio;

        public ActivoFijoController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        #region Recepción de Activos
        public IActionResult Index()
        {
            return RedirectToAction("Recepcion");
        }

        public async Task<IActionResult> Recepcion()
        {
            ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddress), "/api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
            ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo((ViewData["TipoActivoFijo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["TipoActivoFijo"] as SelectList).FirstOrDefault().Value) : -1);
            ViewData["SubClaseActivoFijo"] = await ObtenerSelectListSubClaseActivoFijo((ViewData["ClaseActivoFijo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["ClaseActivoFijo"] as SelectList).FirstOrDefault().Value) : -1);
            ViewData["MotivoRecepcion"] = new SelectList(await apiServicio.Listar<MotivoRecepcion>(new Uri(WebApp.BaseAddress), "/api/MotivoRecepcion/ListarMotivoRecepcion"), "IdMotivoRecepcion", "Descripcion");

            ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddress), "/api/Pais/ListarPaises"), "IdPais", "Nombre");
            ViewData["Provincia"] = await ObtenerSelectListProvincia((ViewData["Pais"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Pais"] as SelectList).FirstOrDefault().Value) : -1);
            ViewData["Ciudad"] = await ObtenerSelectListCiudad((ViewData["Provincia"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Provincia"] as SelectList).FirstOrDefault().Value) : -1);
            ViewData["Sucursal"] = await ObtenerSelectListSucursal((ViewData["Ciudad"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Ciudad"] as SelectList).FirstOrDefault().Value) : -1);
            ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo((ViewData["Sucursal"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Sucursal"] as SelectList).FirstOrDefault().Value) : -1);

            var listaProveedor = await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddress), "/api/Proveedor/ListarProveedores");
            var tlistaProveedor = listaProveedor.Select(c => new { IdProveedor = c.IdProveedor, NombreApellidos = String.Format("{0} {1}", c.Nombre, c.Apellidos) });
            ViewData["Proveedor"] = new SelectList(tlistaProveedor, "IdProveedor", "NombreApellidos");

            var listaEmpleado = await apiServicio.Listar<Empleado>(new Uri(WebApp.BaseAddress), "/api/Empleado/ListarEmpleados");
            var tlistaEmpleado = listaEmpleado.Select(c => new { IdEmpleado = c.IdEmpleado, NombreApellidos = String.Format("{0} {1}", c.Persona.Nombres, c.Persona.Apellidos) });
            ViewData["Empleado"] = new SelectList(tlistaEmpleado, "IdEmpleado", "NombreApellidos");

            ViewData["Marca"] = new SelectList(await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddress), "/api/Marca/ListarMarca"), "IdMarca", "Nombre");
            ViewData["Modelo"] = await ObtenerSelectListModelo((ViewData["Marca"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Marca"] as SelectList).FirstOrDefault().Value) : -1);
            ViewData["UnidadMedida"] = new SelectList(await apiServicio.Listar<UnidadMedida>(new Uri(WebApp.BaseAddress), "/api/UnidadMedida/ListarUnidadMedida"), "IdUnidadMedida", "Nombre");
            return View();
        }

        public async Task<IActionResult> EditarRecepcion(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddress),
                                                                  "/api/RecepcionActivoFijo");


                    respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionActivoFijoDetalle>(respuesta.Resultado.ToString());
                    var recepcionActivoFijoDetalle = respuesta.Resultado as RecepcionActivoFijoDetalle;
                    if (respuesta.IsSuccess)
                    {
                        if (recepcionActivoFijoDetalle.Estado != null)
                        {
                            if (recepcionActivoFijoDetalle.Estado.Nombre == "Recepcionado" || recepcionActivoFijoDetalle.Estado.Nombre == "Validación Técnica")
                            {
                                ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddress), "/api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                                ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo(recepcionActivoFijoDetalle?.RecepcionActivoFijo?.SubClaseActivoFijo?.ClaseActivoFijo?.TipoActivoFijo?.IdTipoActivoFijo ?? -1);
                                ViewData["SubClaseActivoFijo"] = await ObtenerSelectListSubClaseActivoFijo(recepcionActivoFijoDetalle?.RecepcionActivoFijo?.SubClaseActivoFijo?.ClaseActivoFijo?.IdClaseActivoFijo ?? -1);
                                ViewData["MotivoRecepcion"] = new SelectList(await apiServicio.Listar<MotivoRecepcion>(new Uri(WebApp.BaseAddress), "/api/MotivoRecepcion/ListarMotivoRecepcion"), "IdMotivoRecepcion", "Descripcion");

                                ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddress), "/api/Pais/ListarPaises"), "IdPais", "Nombre");
                                ViewData["Provincia"] = await ObtenerSelectListProvincia(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.Pais?.IdPais ?? -1);
                                ViewData["Ciudad"] = await ObtenerSelectListCiudad(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.IdProvincia ?? -1);
                                ViewData["Sucursal"] = await ObtenerSelectListSucursal(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.IdCiudad ?? -1);
                                ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.IdSucursal ?? -1);

                                var listaProveedor = await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddress), "/api/Proveedor/ListarProveedores");
                                var tlistaProveedor = listaProveedor.Select(c => new { IdProveedor = c.IdProveedor, NombreApellidos = String.Format("{0} {1}", c.Nombre, c.Apellidos) });
                                ViewData["Proveedor"] = new SelectList(tlistaProveedor, "IdProveedor", "NombreApellidos");

                                var listaEmpleado = await apiServicio.Listar<Empleado>(new Uri(WebApp.BaseAddress), "/api/Empleado/ListarEmpleados");
                                var tlistaEmpleado = listaEmpleado.Select(c => new { IdEmpleado = c.IdEmpleado, NombreApellidos = String.Format("{0} {1}", c.Persona.Nombres, c.Persona.Apellidos) });
                                ViewData["Empleado"] = new SelectList(tlistaEmpleado, "IdEmpleado", "NombreApellidos");

                                ViewData["Marca"] = new SelectList(await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddress), "/api/Marca/ListarMarca"), "IdMarca", "Nombre");
                                ViewData["Modelo"] = await ObtenerSelectListModelo(recepcionActivoFijoDetalle?.ActivoFijo?.Modelo?.Marca?.IdMarca ?? -1);
                                ViewData["UnidadMedida"] = new SelectList(await apiServicio.Listar<UnidadMedida>(new Uri(WebApp.BaseAddress), "/api/UnidadMedida/ListarUnidadMedida"), "IdUnidadMedida", "Nombre");
                                return View(respuesta.Resultado);
                            }
                        }
                    }
                }
                return BadRequest();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        public async Task<Response> InsertarRecepcionActivoFijoDetalle(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle)
        {
            Response response = new Response();
            try
            {
                response = await apiServicio.InsertarAsync(recepcionActivoFijoDetalle,
                                                             new Uri(WebApp.BaseAddress),
                                                             "/api/RecepcionActivoFijo/InsertarRecepcionActivoFijo");
                if (response.IsSuccess)
                {
                    var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        ExceptionTrace = null,
                        Message = "Se ha recepcionado un activo fijo",
                        UserName = "Usuario 1",
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        EntityID = string.Format("{0} {1}", "Activo Fijo:", recepcionActivoFijoDetalle.ActivoFijo.IdActivoFijo),
                    });

                    recepcionActivoFijoDetalle = JsonConvert.DeserializeObject<RecepcionActivoFijoDetalle>(response.Resultado.ToString());
                    EmpleadoActivoFijo nuevoEmpleadoActivoFijo = new EmpleadoActivoFijo { IdActivoFijo = recepcionActivoFijoDetalle.IdActivoFijo, IdEmpleado = recepcionActivoFijoDetalle.RecepcionActivoFijo.IdEmpleado, FechaAsignacion = DateTime.Now };
                    response = await apiServicio.InsertarAsync(nuevoEmpleadoActivoFijo,
                                                                 new Uri(WebApp.BaseAddress),
                                                                 "/api/EmpleadoActivoFijo/InsertarEmpleadoActivoFijo");

                    if (response.IsSuccess)
                    {
                        responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                        {
                            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                            ExceptionTrace = null,
                            Message = "Se ha adicionado un empleado a un activo fijo",
                            UserName = "Usuario 1",
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                            EntityID = string.Format("{0} {1}", "Empleado de Activo Fijo:", recepcionActivoFijoDetalle.RecepcionActivoFijo.IdEmpleado),
                        });
                    }
                }
            }
            catch (Exception)
            {
                response.IsSuccess = false;
            }
            return response;
        }

        public async Task<Response> EditarRecepcionActivoFijoDetalle(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle)
        {
            Response response = new Response();
            try
            {
                response = await apiServicio.EditarAsync<RecepcionActivoFijoDetalle>(recepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle.ToString(), recepcionActivoFijoDetalle, new Uri(WebApp.BaseAddress), "/api/RecepcionActivoFijo");
                if (response.IsSuccess)
                {
                    var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        ExceptionTrace = null,
                        Message = "Se ha editado una recepción de activo fijo",
                        UserName = "Usuario 1",
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        EntityID = string.Format("{0} {1}", "Activo Fijo:", recepcionActivoFijoDetalle.ActivoFijo.IdActivoFijo)
                    });
                }

                var listaEmpleadoActivoFijo = await apiServicio.Listar<EmpleadoActivoFijo>(new Uri(WebApp.BaseAddress), "/api/EmpleadoActivoFijo/ListarEmpleadosActivoFijo");
                var empleadoActivoFijo = listaEmpleadoActivoFijo.FirstOrDefault(c => c.IdActivoFijo == recepcionActivoFijoDetalle.IdActivoFijo);
                if (empleadoActivoFijo != null)
                {
                    empleadoActivoFijo.IdEmpleado = recepcionActivoFijoDetalle.RecepcionActivoFijo.IdEmpleado;
                    response = await apiServicio.EditarAsync<EmpleadoActivoFijo>(empleadoActivoFijo.IdEmpleadoActivoFijo.ToString(), empleadoActivoFijo, new Uri(WebApp.BaseAddress), "/api/EmpleadoActivoFijo");
                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                        {
                            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                            ExceptionTrace = null,
                            Message = "Se ha editado un empleado de un activo fijo",
                            UserName = "Usuario 1",
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                            EntityID = string.Format("{0} {1}", "Empleado de Activo Fijo:", recepcionActivoFijoDetalle.RecepcionActivoFijo.IdEmpleado),
                        });
                    }
                }
            }
            catch (Exception)
            {
                response.IsSuccess = false;
            }
            return response;
        }

        public async Task<IActionResult> GestionRecepcionActivoFijoDetalle(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle, int valor_tab)
        {
            Response response = new Response();
            try
            {
                var listaTipoActivoFijo = await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddress), "/api/TipoActivoFijo/ListarTipoActivoFijos");
                var listaMarca = await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddress), "/api/Marca/ListarMarca");

                var listaSubClaseActivoFijo = await apiServicio.Listar<SubClaseActivoFijo>(new Uri(WebApp.BaseAddress), "/api/SubClaseActivoFijo/ListarSubClasesActivoFijo");
                recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo = listaSubClaseActivoFijo.SingleOrDefault(c => c.IdSubClaseActivoFijo == recepcionActivoFijoDetalle?.RecepcionActivoFijo?.IdSubClaseActivoFijo);

                var listaLibroActivoFijo = await apiServicio.Listar<LibroActivoFijo>(new Uri(WebApp.BaseAddress), "/api/LibroActivoFijo/ListarLibrosActivoFijo");
                recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo = listaLibroActivoFijo.SingleOrDefault(c => c.IdLibroActivoFijo == recepcionActivoFijoDetalle?.ActivoFijo?.IdLibroActivoFijo);

                if (recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo == null)
                {
                    try
                    {
                        recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo = new SubClaseActivoFijo();
                        int IdClaseActivoFijo = int.Parse(Request.Form["RecepcionActivoFijo.SubClaseActivoFijo.IdClaseActivoFijo"].ToString());

                        var listaClaseActivoFijo = await apiServicio.Listar<ClaseActivoFijo>(new Uri(WebApp.BaseAddress), "/api/ClaseActivoFijo/ListarClaseActivoFijo");
                        recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo.ClaseActivoFijo = listaClaseActivoFijo.SingleOrDefault(c => c.IdClaseActivoFijo == IdClaseActivoFijo);
                        recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo.IdClaseActivoFijo = IdClaseActivoFijo;
                    }
                    catch (Exception)
                    {
                        recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo.ClaseActivoFijo = new ClaseActivoFijo();
                        try
                        {
                            int IdTipoActivoFijo = int.Parse(Request.Form["RecepcionActivoFijo.SubClaseActivoFijo.ClaseActivoFijo.IdTipoActivoFijo"].ToString());
                            recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo.ClaseActivoFijo.TipoActivoFijo = listaTipoActivoFijo.SingleOrDefault(c => c.IdTipoActivoFijo == IdTipoActivoFijo);
                            recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo.ClaseActivoFijo.IdTipoActivoFijo = IdTipoActivoFijo;
                        }
                        catch (Exception)
                        { }
                    }
                }

                if (recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo == null)
                {
                    try
                    {
                        recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo = new LibroActivoFijo();
                        int IdSucursal = int.Parse(Request.Form["ActivoFijo.LibroActivoFijo.IdSucursal"].ToString());

                        var listaSucursal = await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddress), "/api/Sucursal/ListarSucursales");
                        recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.Sucursal = listaSucursal.SingleOrDefault(c => c.IdSucursal == IdSucursal);
                        recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.IdSucursal = IdSucursal;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.Sucursal = new Sucursal();
                            int IdCiudad = int.Parse(Request.Form["ActivoFijo.LibroActivoFijo.Sucursal.IdCiudad"].ToString());

                            var listaCiudad = await apiServicio.Listar<Ciudad>(new Uri(WebApp.BaseAddress), "/api/Ciudad/ListarCiudades");
                            recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.Sucursal.Ciudad = listaCiudad.SingleOrDefault(c => c.IdCiudad == IdCiudad);
                            recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.Sucursal.IdCiudad = IdCiudad;
                        }
                        catch (Exception)
                        {
                            try
                            {
                                recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.Sucursal.Ciudad = new Ciudad();
                                int IdProvincia = int.Parse(Request.Form["ActivoFijo.LibroActivoFijo.Sucursal.Ciudad.IdProvincia"].ToString());

                                var listaProvincia = await apiServicio.Listar<Provincia>(new Uri(WebApp.BaseAddress), "/api/Provincia/ListarProvincias");
                                recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.Sucursal.Ciudad.Provincia = listaProvincia.SingleOrDefault(c => c.IdProvincia == IdProvincia);
                                recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.Sucursal.Ciudad.IdProvincia = IdProvincia;
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.Sucursal.Ciudad.Provincia = new Provincia();
                                    int IdPais = int.Parse(Request.Form["ActivoFijo.LibroActivoFijo.Sucursal.Ciudad.Provincia.IdPais"].ToString());

                                    var listaPais = await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddress), "/api/Pais/ListarPaises");
                                    recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.Sucursal.Ciudad.Provincia.Pais = listaPais.SingleOrDefault(c => c.IdPais == IdPais);
                                    recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.Sucursal.Ciudad.Provincia.IdPais = IdPais;
                                }
                                catch (Exception)
                                { }
                            }
                        }
                    }
                }

                var listaModelo = await apiServicio.Listar<Modelo>(new Uri(WebApp.BaseAddress), "/api/Modelo/ListarModelos");
                recepcionActivoFijoDetalle.ActivoFijo.Modelo = listaModelo.SingleOrDefault(c => c.IdModelo == recepcionActivoFijoDetalle?.ActivoFijo?.IdModelo);

                if (recepcionActivoFijoDetalle.ActivoFijo.Modelo == null)
                {
                    try
                    {
                        recepcionActivoFijoDetalle.ActivoFijo.Modelo = new Modelo();
                        int IdMarca = int.Parse(Request.Form["ActivoFijo.Modelo.IdMarca"].ToString());
                        recepcionActivoFijoDetalle.ActivoFijo.Modelo.Marca = listaMarca.SingleOrDefault(c => c.IdMarca == IdMarca);
                        recepcionActivoFijoDetalle.ActivoFijo.Modelo.IdMarca = IdMarca;
                    }
                    catch (Exception)
                    { }
                }

                var listaUnidadMedida = await apiServicio.Listar<UnidadMedida>(new Uri(WebApp.BaseAddress), "/api/UnidadMedida/ListarUnidadMedida");
                recepcionActivoFijoDetalle.ActivoFijo.UnidadMedida = listaUnidadMedida.SingleOrDefault(c => c.IdUnidadMedida == recepcionActivoFijoDetalle.ActivoFijo.IdUnidadMedida);

                recepcionActivoFijoDetalle.ActivoFijo.Ciudad = recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo?.Sucursal?.Ciudad;
                recepcionActivoFijoDetalle.ActivoFijo.IdCiudad = recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo?.Sucursal?.Ciudad?.IdCiudad ?? 0;

                recepcionActivoFijoDetalle.RecepcionActivoFijo.LibroActivoFijo = recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo;
                recepcionActivoFijoDetalle.RecepcionActivoFijo.IdLibroActivoFijo = recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo?.IdLibroActivoFijo ?? 0;

                recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.Codigosecuencial = recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo?.Codigosecuencial ?? "N/A";
                recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.CodigoBarras = recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo?.CodigoBarras ?? "N/A";

                recepcionActivoFijoDetalle.NumeroPoliza = "N/A";

                await apiServicio.InsertarAsync(new Estado { Nombre = "Recepcionado" },
                                                             new Uri(WebApp.BaseAddress),
                                                             "/api/Estado/InsertarEstado");

                await apiServicio.InsertarAsync(new Estado { Nombre = "Validación Técnica" },
                                                             new Uri(WebApp.BaseAddress),
                                                             "/api/Estado/InsertarEstado");

                var listaEstado = await apiServicio.Listar<Estado>(new Uri(WebApp.BaseAddress), "/api/Estado/ListarEstados");
                var nombreEstado = !recepcionActivoFijoDetalle.RecepcionActivoFijo.ValidacionTecnica ? "Recepcionado" : "Validación Técnica";
                var estado = listaEstado.SingleOrDefault(c => c.Nombre == nombreEstado);
                recepcionActivoFijoDetalle.Estado = estado;
                recepcionActivoFijoDetalle.IdEstado = estado.IdEstado;

                recepcionActivoFijoDetalle.ActivoFijo.SubClaseActivoFijo = recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo;
                recepcionActivoFijoDetalle.ActivoFijo.IdSubClaseActivoFijo = recepcionActivoFijoDetalle.RecepcionActivoFijo.IdSubClaseActivoFijo;

                TryValidateModel(recepcionActivoFijoDetalle);

                Func<Task<Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary>> llenarModelo = async () =>
                {
                    ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddress), "/api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                    ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo(recepcionActivoFijoDetalle?.RecepcionActivoFijo?.SubClaseActivoFijo?.ClaseActivoFijo?.TipoActivoFijo?.IdTipoActivoFijo ?? -1);
                    ViewData["SubClaseActivoFijo"] = await ObtenerSelectListSubClaseActivoFijo(recepcionActivoFijoDetalle?.RecepcionActivoFijo?.SubClaseActivoFijo?.ClaseActivoFijo?.IdClaseActivoFijo ?? -1);
                    ViewData["MotivoRecepcion"] = new SelectList(await apiServicio.Listar<MotivoRecepcion>(new Uri(WebApp.BaseAddress), "/api/MotivoRecepcion/ListarMotivoRecepcion"), "IdMotivoRecepcion", "Descripcion");

                    ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddress), "/api/Pais/ListarPaises"), "IdPais", "Nombre");
                    ViewData["Provincia"] = await ObtenerSelectListProvincia(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.Pais?.IdPais ?? -1);
                    ViewData["Ciudad"] = await ObtenerSelectListCiudad(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.IdProvincia ?? -1);
                    ViewData["Sucursal"] = await ObtenerSelectListSucursal(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.IdCiudad ?? -1);
                    ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.IdSucursal ?? -1);

                    var listaProveedor = await apiServicio.Listar<Proveedor>(new Uri(WebApp.BaseAddress), "/api/Proveedor/ListarProveedores");
                    var tlistaProveedor = listaProveedor.Select(c => new { IdProveedor = c.IdProveedor, NombreApellidos = String.Format("{0} {1}", c.Nombre, c.Apellidos) });
                    ViewData["Proveedor"] = new SelectList(tlistaProveedor, "IdProveedor", "NombreApellidos");

                    var listaEmpleado = await apiServicio.Listar<Empleado>(new Uri(WebApp.BaseAddress), "/api/Empleado/ListarEmpleados");
                    var tlistaEmpleado = listaEmpleado.Select(c => new { IdEmpleado = c.IdEmpleado, NombreApellidos = String.Format("{0} {1}", c.Persona.Nombres, c.Persona.Apellidos) });
                    ViewData["Empleado"] = new SelectList(tlistaEmpleado, "IdEmpleado", "NombreApellidos");

                    ViewData["Marca"] = new SelectList(await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddress), "/api/Marca/ListarMarca"), "IdMarca", "Nombre");
                    ViewData["Modelo"] = await ObtenerSelectListModelo(recepcionActivoFijoDetalle?.ActivoFijo?.Modelo?.Marca?.IdMarca ?? -1);
                    ViewData["UnidadMedida"] = new SelectList(await apiServicio.Listar<UnidadMedida>(new Uri(WebApp.BaseAddress), "/api/UnidadMedida/ListarUnidadMedida"), "IdUnidadMedida", "Nombre");
                    return ViewData;
                };

                if (valor_tab == 1) //Datos Generales
                {
                    response = await apiServicio.InsertarAsync(recepcionActivoFijoDetalle,
                                                             new Uri(WebApp.BaseAddress),
                                                             "/api/RecepcionActivoFijo/ValidarModeloRecepcionActivoFijo");

                    if (response.IsSuccess)
                    {
                        if (!recepcionActivoFijoDetalle.RecepcionActivoFijo.ValidacionTecnica)
                        {
                            ViewBag.Codificacion = true;
                            await llenarModelo();
                            return View(recepcionActivoFijoDetalle);
                        }

                        if (recepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle == 0)
                            response = await InsertarRecepcionActivoFijoDetalle(recepcionActivoFijoDetalle);
                        else
                        {
                            recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.Codigosecuencial = "N/A";
                            recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.CodigoBarras = "N/A";
                            response = await EditarRecepcionActivoFijoDetalle(recepcionActivoFijoDetalle);
                        }

                        if (response.IsSuccess)
                            return RedirectToAction("ActivoValidacionTecnica");
                    }
                }
                else //Codificación
                {
                    try
                    {
                        int numeroConsecutivo = int.Parse(Request.Form["numeroConsecutivo"].ToString());
                        string codigoSecuencial = String.Format("{0}{1}{2}", recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo.ClaseActivoFijo.Nombre, recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo.ClaseActivoFijo.TipoActivoFijo.Nombre, numeroConsecutivo);
                        ViewBag.numeroConsecutivo = numeroConsecutivo;

                        var listaCodigoActivoFijo = await apiServicio.Listar<CodigoActivoFijo>(new Uri(WebApp.BaseAddress), "/api/CodigoActivoFijo/ListarCodigosActivoFijo");
                        if (listaCodigoActivoFijo.Count(c => c.Codigosecuencial == codigoSecuencial) == 0)
                        {
                            if (listaCodigoActivoFijo.Count(c => c.CodigoBarras == recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.CodigoBarras) == 0)
                            {
                                recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.Codigosecuencial = codigoSecuencial;

                                if (recepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle == 0)
                                    response = await InsertarRecepcionActivoFijoDetalle(recepcionActivoFijoDetalle);
                                else
                                    response = await EditarRecepcionActivoFijoDetalle(recepcionActivoFijoDetalle);

                                if (response.IsSuccess)
                                    return RedirectToAction("ActivosRecepcionados");
                            }
                            else
                            {
                                ViewBag.Codificacion = true;
                                ViewData["ErrorCodificacion"] = "Ya existe un Activo Fijo con el mismo Código de Barras";
                            }
                        }
                        else
                        {
                            ViewBag.Codificacion = true;
                            ViewData["ErrorCodificacion"] = "Ya existe un Activo Fijo con el mismo Código Único";
                        }
                    }
                    catch (Exception)
                    {
                        ViewBag.Codificacion = true;
                        ViewData["ErrorCodificacion"] = "Modelo inválido";
                        ViewData["errorNumeroConsecutivo"] = "Tiene que escribir un número";
                    }
                }
                await llenarModelo();
                ViewData["Error"] = valor_tab == 1 ? response.Message : ViewData["ErrorCodificacion"];
                return View(recepcionActivoFijoDetalle);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Creando recepción Activo Fijo",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });

                return BadRequest();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recepcion(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle) => await GestionRecepcionActivoFijoDetalle(recepcionActivoFijoDetalle, int.Parse(Request.Form["tab"].ToString()));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRecepcion(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle) => await GestionRecepcionActivoFijoDetalle(recepcionActivoFijoDetalle, int.Parse(Request.Form["tab"].ToString()));

        public async Task<IActionResult> ObtenerRecepcionActivoFijo(string id, string estado)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddress),
                                                                  "/api/RecepcionActivoFijo");


                    respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionActivoFijoDetalle>(respuesta.Resultado.ToString());
                    if (respuesta.IsSuccess)
                    {
                        if (estado != null)
                        {
                            if ((respuesta.Resultado as RecepcionActivoFijoDetalle).Estado.Nombre == estado)
                                return View(respuesta.Resultado);
                        }
                        else
                            return View(respuesta.Resultado);
                    }
                }
                return BadRequest();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        public async Task<IActionResult> ActivosRecepcionados()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/RecepcionActivoFijo/ListarRecepcionActivoFijo");

                var listaActivosFijosRecepcionados = lista.Where(c => c.Estado.Nombre == "Recepcionado").ToList();
                ViewData["titulo"] = "Activos Fijos Recepcionados";
                ViewData["textoColumna"] = "Editar";
                ViewData["url"] = "EditarRecepcion";
                return View("ListadoActivoFijo", listaActivosFijosRecepcionados);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando activos fijos recepcionados",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }
        #endregion

        #region Codificación de Activos
        public async Task<IActionResult> ActivoValidacionTecnica()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/RecepcionActivoFijo/ListarRecepcionActivoFijo");

                var listaActivosFijosValidacionTecnica = lista.Where(c => c.Estado.Nombre == "Validación Técnica").ToList();
                ViewData["titulo"] = "Activos Fijos que requieren Validación Técnica";
                ViewData["textoColumna"] = "Revisar";
                ViewData["url"] = "RevisionActivoFijo";
                ViewData["urlEditar"] = "EditarRecepcion";
                return View("ListadoActivoFijo", listaActivosFijosValidacionTecnica);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando activos fijos que requieren validación técnica",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        public async Task<IActionResult> DesaprobarActivoFijo(string id)
        {
            Response response = new Response();
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = "Desaprobado" },
                                                             new Uri(WebApp.BaseAddress),
                                                             "/api/Estado/InsertarEstado");

                response = await apiServicio.InsertarAsync(int.Parse(id),
                                                             new Uri(WebApp.BaseAddress),
                                                             "/api/RecepcionActivoFijo/DesaprobarActivoFijo");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Creando desaprobación Activo Fijo",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });
                return BadRequest();
            }
            return RedirectToAction("ActivoValidacionTecnica");
        }

        public async Task<IActionResult> CodificacionActivoFijo(string id) => await ObtenerRecepcionActivoFijo(id, "Validación Técnica");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CodificacionActivoFijo(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle)
        {
            try
            {
                int numeroConsecutivo = int.Parse(Request.Form["numeroConsecutivo"].ToString());
                string codigoSecuencial = String.Format("{0}{1}{2}", recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo.ClaseActivoFijo.Nombre, recepcionActivoFijoDetalle.RecepcionActivoFijo.SubClaseActivoFijo.ClaseActivoFijo.TipoActivoFijo.Nombre, numeroConsecutivo);
                ViewBag.numeroConsecutivo = numeroConsecutivo;

                var listaCodigoActivoFijo = await apiServicio.Listar<CodigoActivoFijo>(new Uri(WebApp.BaseAddress), "/api/CodigoActivoFijo/ListarCodigosActivoFijo");
                if (listaCodigoActivoFijo.Count(c => c.Codigosecuencial == codigoSecuencial) == 0)
                {
                    if (listaCodigoActivoFijo.Count(c => c.CodigoBarras == recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.CodigoBarras) == 0)
                    {
                        recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.Codigosecuencial = codigoSecuencial;
                        Response response = new Response();
                        try
                        {
                            string id = recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.IdCodigoActivoFijo.ToString();
                            if (!string.IsNullOrEmpty(id))
                            {
                                response = await apiServicio.EditarAsync(id, recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo, new Uri(WebApp.BaseAddress),
                                                                             "/api/CodigoActivoFijo");

                                if (response.IsSuccess)
                                {
                                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                                    {
                                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                                        EntityID = string.Format("{0} : {1}", "Código de Activo Fijo", id),
                                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit),
                                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                                        Message = "Se ha actualizado un registro clase de código de activo fijo",
                                        UserName = "Usuario 1"
                                    });
                                }

                                recepcionActivoFijoDetalle.Estado = new Estado { Nombre = "Recepcionado" };
                                response = await apiServicio.EditarAsync(recepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle.ToString(), recepcionActivoFijoDetalle, new Uri(WebApp.BaseAddress), "/api/RecepcionActivoFijo/EstadoActivoFijo");
                                if (response.IsSuccess)
                                {
                                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                                    {
                                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                                        EntityID = string.Format("{0} : {1}", "Estado de Activo Fijo", id),
                                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit),
                                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                                        Message = "Se ha actualizado un registro estado de activo fijo",
                                        UserName = "Usuario 1"
                                    });
                                    return RedirectToAction("ActivosFijosRecepcionados");
                                }
                            }
                            ViewData["Error"] = response.Message;
                            return View(recepcionActivoFijoDetalle);
                        }
                        catch (Exception ex)
                        {
                            await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                            {
                                ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                                Message = "Editando un clase de código de activo fijo",
                                ExceptionTrace = ex,
                                LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit),
                                LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                                UserName = "Usuario APP webappth"
                            });

                            return BadRequest();
                        }
                    }
                    else
                    {
                        ViewData["Error"] = "Ya existe un Activo Fijo con el mismo Código de Barras";
                    }
                }
                else
                {
                    ViewData["Error"] = "Ya existe un Activo Fijo con el mismo Código Único";
                }
            }
            catch (Exception)
            {
                ViewData["Error"] = "Modelo inválido";
                ViewData["errorNumeroConsecutivo"] = "Tiene que escribir un número";
            }
            return View(recepcionActivoFijoDetalle);
        }

        public async Task<IActionResult> RevisionActivoFijo(string id) => await ObtenerRecepcionActivoFijo(id, "Validación Técnica");
        #endregion

        #region Póliza de Seguro
        public async Task<IActionResult> ActivosFijosRecepcionadosSinPoliza()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/RecepcionActivoFijo/ListarRecepcionActivoFijo");

                var listaActivosFijosRecepcionados = lista.Where(c => c.Estado.Nombre == "Recepcionado" && c.NumeroPoliza == "N/A").ToList();
                ViewData["titulo"] = "Activos Fijos sin Póliza de Seguro";
                ViewData["textoColumna"] = "Asignar póliza";
                ViewData["url"] = "AsignarPoliza";
                return View("ListadoActivoFijo", listaActivosFijosRecepcionados);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando activos fijos con estado Recepcionado sin número de póliza asignado",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        public async Task<IActionResult> ActivosFijosRecepcionadosConPoliza()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/RecepcionActivoFijo/ListarRecepcionActivoFijo");

                var listaActivosFijosRecepcionados = lista.Where(c => c.Estado.Nombre == "Recepcionado" && c.NumeroPoliza != "N/A").ToList();
                ViewData["titulo"] = "Activos Fijos con Póliza de Seguro";
                ViewData["textoColumna"] = "Editar";
                ViewData["url"] = "AsignarPoliza";
                ViewData["poliza"] = 1;
                return View("ListadoActivoFijo", listaActivosFijosRecepcionados);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando activos fijos con estado Recepcionado con número de póliza asignado",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        public async Task<IActionResult> AsignarPoliza(string id) => await ObtenerRecepcionActivoFijo(id, "Recepcionado");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarPoliza(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle)
        {
            Response response = new entidades.Utils.Response();
            try
            {
                response = await apiServicio.InsertarAsync(recepcionActivoFijoDetalle,
                                                             new Uri(WebApp.BaseAddress),
                                                             "/api/RecepcionActivoFijo/AsignarPoliza");

                if (response.IsSuccess)
                {
                    var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        ExceptionTrace = null,
                        Message = "Se ha asignado el número de póliza",
                        UserName = "Usuario 1",
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        EntityID = string.Format("{0} {1}", "Recepción Activo Fijo Detalle:", recepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle),
                    });
                    ViewData["mensaje"] = "Póliza de seguro asignada correctamente";
                    return View(recepcionActivoFijoDetalle);
                }
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Asignando Póliza",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });

                return BadRequest();
            }
            return View();
        }
        #endregion

        #region Alta de Activos
        public async Task<IActionResult> AsignarNumeroFactura(string id) => await ObtenerRecepcionActivoFijo(id, "Recepcionado");

        public async Task<IActionResult> ActivosFijosRecepcionados()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/RecepcionActivoFijo/ListarRecepcionActivoFijo");

                var listaActivosFijosRecepcionados = lista.Where(c => c.Estado.Nombre == "Recepcionado").ToList();
                ViewData["titulo"] = "Activos Fijos Recepcionados";
                ViewData["textoColumna"] = "Dar Alta";
                ViewData["url"] = "AsignarNumeroFactura"; //Url de la ventana para gestionar el Alta
                ViewData["RequerirAlta"] = true;
                return View("ListadoActivoFijo", listaActivosFijosRecepcionados);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando activos fijos con estado Recepcionado",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        public async Task<IActionResult> ActivosFijosAltas()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();

            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/RecepcionActivoFijo/ListarRecepcionActivoFijo");

                var listaActivosFijosAltas = lista.Where(c => c.Estado.Nombre == "Alta").ToList();
                ViewData["titulo"] = "Activos Fijos con Estado Alta";
                return View("ListadoActivoFijoAlta", listaActivosFijosAltas);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando activos fijos con estado Alta",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        public async Task<IActionResult> AprobarAlta(int id)
        {
            Response response = new Response();

            try
            {
                RecepcionActivoFijoDetalle RecepcionActivoFijoDetalle = new RecepcionActivoFijoDetalle();

                var respuesta = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddress), "/api/RecepcionActivoFijo");

                respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionActivoFijoDetalle>(respuesta.Resultado.ToString());

                RecepcionActivoFijoDetalle = respuesta.Resultado as RecepcionActivoFijoDetalle;
                
                if (respuesta.IsSuccess)
                {
                    try
                    {
                        ActivosFijosAlta AFA = new ActivosFijosAlta { IdActivoFijo = RecepcionActivoFijoDetalle.IdActivoFijo, FechaAlta = DateTime.Now };

                        response = await apiServicio.InsertarAsync(AFA,
                                                                 new Uri(WebApp.BaseAddress),
                                                                 "/api/ActivosFijosAlta/InsertarActivosFijosAlta");

                        if (response.IsSuccess)
                        {
                            var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                            {
                                ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                                ExceptionTrace = null,
                                Message = "Se ha insertado un Activo Fijo con el estado ALTA",
                                UserName = "Usuario 1",
                                LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                                LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                                EntityID = string.Format("{0} {1}", "Activo Fijo con estado Alta:", AFA.IdFactura),
                            });

                            ViewData["mensaje"] = "Activo Fijo Alta insertado correctamente";

                            try
                            {
                                RecepcionActivoFijoDetalle.Estado = new Estado { Nombre = "Alta" };

                                response = await apiServicio.EditarAsync(RecepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle.ToString(), RecepcionActivoFijoDetalle, new Uri(WebApp.BaseAddress), "/api/RecepcionActivoFijo/EstadoActivoFijo");

                                if (response.IsSuccess)
                                {
                                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                                    {
                                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                                        EntityID = string.Format("{0} : {1}", "Estado de Activo Fijo", RecepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle),
                                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit),
                                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                                        Message = "Se ha actualizado un registro estado (Alta) de activo fijo",
                                        UserName = "Usuario 1"
                                    });

                                    return RedirectToAction("ActivosFijosRecepcionados");
                                }
                            }
                            catch (Exception ex)
                            {
                                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                                {
                                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                                    Message = "Cambiando Estado de un Activo Fijo a Alta",
                                    ExceptionTrace = ex,
                                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                                    UserName = "Usuario APP WebAppTh"
                                });

                                return BadRequest();
                            }
                        }
                    }catch (Exception ex)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                        {
                            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                            Message = "Insertando Alta de Activo Fijo",
                            ExceptionTrace = ex,
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                            UserName = "Usuario APP WebAppTh"
                        });

                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Seleccionando un Recepción Activo Fijo Detalle",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });

                return BadRequest();
            }
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarNumeroFactura(RecepcionActivoFijoDetalle recepcionAFD)
        {
            Response response = new entidades.Utils.Response();

            int fact = int.Parse(Request.Form["facturas"]);

            ActivosFijosAlta AFA = new ActivosFijosAlta { IdActivoFijo = recepcionAFD.IdActivoFijo, IdFactura = fact, FechaAlta = DateTime.Now };

            try
            {
                response = await apiServicio.InsertarAsync(AFA,
                                                                 new Uri(WebApp.BaseAddress),
                                                                 "/api/ActivosFijosAlta/InsertarActivosFijosAlta");

                if (response.IsSuccess)
                {
                    var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        ExceptionTrace = null,
                        Message = "Se ha insertado un Activo Fijo con el estado ALTA",
                        UserName = "Usuario 1",
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        EntityID = string.Format("{0} {1}", "Activo Fijo con estado Alta:", AFA.IdFactura),
                    });

                    ViewData["mensaje"] = "Activo Fijo Alta insertado correctamente";

                    try
                    {
                        int IdRecepcionActivoFijoDetalle = int.Parse(Request.Form["IdRecepcionActivoFijoDetalle"]);

                        RecepcionActivoFijoDetalle RecepcionActivoFijoDetalle = new RecepcionActivoFijoDetalle();

                        var respuesta = await apiServicio.SeleccionarAsync<Response>(IdRecepcionActivoFijoDetalle.ToString(), new Uri(WebApp.BaseAddress), "/api/RecepcionActivoFijo");

                        respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionActivoFijoDetalle>(respuesta.Resultado.ToString());

                        RecepcionActivoFijoDetalle = respuesta.Resultado as RecepcionActivoFijoDetalle;
                        RecepcionActivoFijoDetalle.Estado = new Estado { Nombre = "Alta" };

                        if (respuesta.IsSuccess)
                        {
                            try
                            {
                                response = await apiServicio.EditarAsync(RecepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle.ToString(), RecepcionActivoFijoDetalle, new Uri(WebApp.BaseAddress), "/api/RecepcionActivoFijo/EstadoActivoFijo");

                                if (response.IsSuccess)
                                {
                                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                                    {
                                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                                        EntityID = string.Format("{0} : {1}", "Estado de Activo Fijo", IdRecepcionActivoFijoDetalle),
                                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit),
                                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                                        Message = "Se ha actualizado un registro estado (Alta) de activo fijo",
                                        UserName = "Usuario 1"
                                    });

                                    return RedirectToAction("ActivosFijosRecepcionados");
                                }

                            }
                            catch (Exception ex)
                            {
                                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                                {
                                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                                    Message = "Cambiando Estado de un Activo Fijo a Alta",
                                    ExceptionTrace = ex,
                                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                                    UserName = "Usuario APP WebAppTh"
                                });

                                return BadRequest();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                        {
                            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                            Message = "Seleccionando un Activo Fijo Recepcion Detalle",
                            ExceptionTrace = ex,
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                            UserName = "Usuario APP WebAppTh"
                        });

                        return BadRequest();
                    }
                }

            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Insertando Alta de Activo Fijo",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });

                return BadRequest();
            }
                        
            return View();
        }

        public async Task<IActionResult> AddComponentes(string id)
        {
            Response response = new Response();

            try
            {
                RecepcionActivoFijoDetalle RecepcionActivoFijoDetalle = new RecepcionActivoFijoDetalle();

                var respuesta = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddress), "/api/RecepcionActivoFijo");

                respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionActivoFijoDetalle>(respuesta.Resultado.ToString());

                RecepcionActivoFijoDetalle = respuesta.Resultado as RecepcionActivoFijoDetalle;

                if (respuesta.IsSuccess)
                {
                    var lista = new List<RecepcionActivoFijoDetalle>();

                    try
                    {
                        lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddress)
                                                                   , "/api/RecepcionActivoFijo/ListarRecepcionActivoFijo");

                        var listaActivosFijosRecepcionados = lista.Where(c => c.Estado.Nombre == "Recepcionado" && c.RecepcionActivoFijo.IdMotivoRecepcion == 2).ToList();

                        ViewBag.objeto = RecepcionActivoFijoDetalle;

                        try
                        {
                            List<ActivosFijosAdicionados> listaActivosFijosAdicionados = new List<ActivosFijosAdicionados>();

                            listaActivosFijosAdicionados = await apiServicio.Listar<ActivosFijosAdicionados>(new Uri(WebApp.BaseAddress),
                                                                             "/api/ActivosFijosAdicionados/ListarActivosFijosAdicionados");

                            ViewBag.listaActivosFijosAdicionados = listaActivosFijosAdicionados;
                        }
                        catch (Exception ex)
                        {

                            throw;
                        }

                        return View("AdicionarComponentes", listaActivosFijosRecepcionados);

                    } catch (Exception ex)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                        {
                            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                            Message = "Listando activos fijos con estado Recepcionado para adicionar componentes",
                            ExceptionTrace = ex,
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                            UserName = "Usuario APP webappth"
                        });
                        return BadRequest();
                    }                    
                }
                
            } catch (Exception)
            {

            }

            return View("AdicionarComponentes");
        }

        public async Task<IActionResult> AdicionarActivo(string id, string id1, string id2)
        {
            Response response = new Response();
            
            ActivosFijosAdicionados ActivosFijosAdicionados = new ActivosFijosAdicionados { idActivoFijoOrigen = int.Parse(id1), idActivoFijoDestino = int.Parse(id), fechaAdicion = DateTime.Now };

            response = await apiServicio.InsertarAsync(ActivosFijosAdicionados,
                                                                 new Uri(WebApp.BaseAddress),
                                                                 "/api/ActivosFijosAdicionados/InsertarActivosFijosAdicionados");
            if (response.IsSuccess)
            {
                List<ActivosFijosAdicionados> listaActivosFijosAdicionados = new List<ActivosFijosAdicionados>();

                listaActivosFijosAdicionados = await apiServicio.Listar<ActivosFijosAdicionados>(new Uri(WebApp.BaseAddress),
                                                                 "/api/ActivosFijosAdicionados/ListarActivosFijosAdicionados");

                ViewBag.listaActivosFijosAdicionados = listaActivosFijosAdicionados;
            }

            return RedirectToAction("AddComponentes", new { id = id2});
        }

        public async Task<IActionResult> EliminarActivoAdicionado(string idAdicion, string id2)
        {
            Response response = new Response();

            ActivosFijosAdicionados ActivosFijosAdicionados = new ActivosFijosAdicionados();

            var respuesta = await apiServicio.SeleccionarAsync<Response>(idAdicion, new Uri(WebApp.BaseAddress),
                                                                 "/api/ActivosFijosAdicionados");

            if (respuesta.IsSuccess)
            {
                response = await apiServicio.EliminarAsync(idAdicion,
                                                                 new Uri(WebApp.BaseAddress),
                                                                 "/api/ActivosFijosAdicionados");
                if (response.IsSuccess)
                {
                    return RedirectToAction("AddComponentes", new { id = id2});
                }
            }

            return RedirectToAction("AddComponentes", id2);
        }

        public async Task<IActionResult> ActivosFijosReporteAltas()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            var listaAFA = new List<ActivosFijosAlta>();

            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/RecepcionActivoFijo/ListarRecepcionActivoFijo");
                
                try
                {
                    listaAFA = await apiServicio.Listar<ActivosFijosAlta>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/ActivosFijosAlta/ListarAltasActivosFijos");

                    ViewData["listaAFA"] = listaAFA;

                    return View("AltaReporte", lista);
                }
                catch (Exception ex)
                {

                    throw;
                }                
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando activos fijos que han estado en Alta",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }
        #endregion

        #region Transferencias
        //Implementar aquí
        public async Task<IActionResult> ActivosFijosATransferir()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/RecepcionActivoFijo/ListarRecepcionActivoFijo");

                var listaActivosFijosRecepcionados = lista.Select(c => c).ToList();

                return View("ActivosFijosATransferir", listaActivosFijosRecepcionados);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando activos fijos con estado Recepcionado",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        public async Task<IActionResult> TransferirActivoFijo(string id)
        {
            Response response = new Response();
            try
            {
                RecepcionActivoFijoDetalle recepcionActivoFijoDetalle = new RecepcionActivoFijoDetalle();

                var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddress),
                                                                  "/api/RecepcionActivoFijo");


                respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionActivoFijoDetalle>(respuesta.Resultado.ToString());

                if (respuesta.IsSuccess)
                {
                    recepcionActivoFijoDetalle = respuesta.Resultado as RecepcionActivoFijoDetalle;
                }

                var listaMotivoTransferencia = await apiServicio.Listar<MotivoTransferencia>(new Uri(WebApp.BaseAddress), "/api/MotivoTransferencia/ListarMotivoTransferencia");

                List<SelectListItem> listaMT = new List<SelectListItem>();

                foreach (var item in listaMotivoTransferencia)
                {
                    SelectListItem _item = new SelectListItem();
                    _item.Text = item.Motivo_Transferencia;
                    _item.Value = item.IdMotivoTransferencia.ToString();

                    listaMT.Add(_item);
                }

                ViewBag.listaMT = listaMT;

                var listaPais = await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddress), "/api/Pais/ListarPaises");

                List<SelectListItem> listaP = new List<SelectListItem>();

                foreach (var item in listaPais)
                {
                    SelectListItem _item = new SelectListItem();
                    _item.Text = item.Nombre;
                    _item.Value = item.IdPais.ToString();

                    listaP.Add(_item);
                }

                ViewBag.listaP = listaP;
                                
                ViewData["Pais"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddress), "/api/Pais/ListarPaises"), "IdPais", "Nombre");
                ViewData["Provincia"] = await ObtenerSelectListProvincia((ViewData["Pais"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Pais"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["Ciudad"] = await ObtenerSelectListCiudad((ViewData["Provincia"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Provincia"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["Sucursal"] = await ObtenerSelectListSucursal((ViewData["Ciudad"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Ciudad"] as SelectList).FirstOrDefault().Value) : -1);

                ViewData["PaisO"] = new SelectList(await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddress), "/api/Pais/ListarPaises"), "IdPais", "Nombre");
                ViewData["ProvinciaO"] = await ObtenerSelectListProvincia((ViewData["PaisO"] as SelectList).Where(x=>x.Value == recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.Sucursal.Ciudad.Provincia.IdPais.ToString()).FirstOrDefault() != null ? int.Parse((ViewData["PaisO"] as SelectList).Where(x => x.Value == recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.Sucursal.Ciudad.Provincia.IdPais.ToString()).FirstOrDefault().Value) : -1);
                ViewData["CiudadO"] = await ObtenerSelectListCiudad((ViewData["ProvinciaO"] as SelectList).Where(x => x.Value == recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.Sucursal.Ciudad.Provincia.IdProvincia.ToString()).FirstOrDefault() != null ? int.Parse((ViewData["ProvinciaO"] as SelectList).Where(x => x.Value == recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.Sucursal.Ciudad.Provincia.IdProvincia.ToString()).FirstOrDefault().Value) : -1);
                ViewData["SucursalO"] = await ObtenerSelectListSucursal((ViewData["CiudadO"] as SelectList).Where(x => x.Value == recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.Sucursal.Ciudad.IdCiudad.ToString()).FirstOrDefault() != null ? int.Parse((ViewData["CiudadO"] as SelectList).Where(x => x.Value == recepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.Sucursal.Ciudad.IdCiudad.ToString()).FirstOrDefault().Value) : -1);


                var listaEmpleado = await apiServicio.Listar<Empleado>(new Uri(WebApp.BaseAddress), "/api/Empleado/ListarEmpleados");
                var tlistaEmpleado = listaEmpleado.Select(c => new { IdEmpleado = c.IdEmpleado, NombreApellidos = String.Format("{0} {1}", c.Persona.Nombres, c.Persona.Apellidos) });
                ViewData["Empleado"] = new SelectList(tlistaEmpleado, "IdEmpleado", "NombreApellidos");
                                
                return View("TransferirActivoFijo", recepcionActivoFijoDetalle);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Creando Transferencia de Activo Fijo",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });

                return BadRequest();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarTransferencia()
        {
            Response response = new Response();

            var lista = new List<RecepcionActivoFijoDetalle>();

            try
            {
                int IdRecepcionActivoFijoDetalle = int.Parse(Request.Form["IdRecepcionActivoFijoDetalle"]);

                RecepcionActivoFijoDetalle RecepcionActivoFijoDetalle = new RecepcionActivoFijoDetalle();

                var respuesta = await apiServicio.SeleccionarAsync<Response>(IdRecepcionActivoFijoDetalle.ToString(), new Uri(WebApp.BaseAddress), "/api/RecepcionActivoFijo");

                if (respuesta.IsSuccess)
                {
                    respuesta.Resultado = JsonConvert.DeserializeObject<RecepcionActivoFijoDetalle>(respuesta.Resultado.ToString());

                    RecepcionActivoFijoDetalle = respuesta.Resultado as RecepcionActivoFijoDetalle;

                    int motivoTransferencia = int.Parse(Request.Form["motivoTransferencia"].ToString());
                    int empleadoEnvia = int.Parse(Request.Form["empleadoEnvia"].ToString());
                    int empleadoRecibe = int.Parse(Request.Form["empleadoRecibe"].ToString());
                    string origen = RecepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.Sucursal.Nombre; //Request.Form["ActivoFijo.LibroActivoFijo.Sucursal.Nombre"].ToString();
                    string observaciones = Request.Form["observaciones"].ToString();
                    DateTime fechaTransferencia = DateTime.Now;

                    int IdSucursalD = int.Parse(Request.Form["ActivoFijo.LibroActivoFijo.IdSucursal"].ToString());
                    int IdCiudadD = int.Parse(Request.Form["ActivoFijo.LibroActivoFijo.Sucursal.IdCiudad"].ToString());
                    int IdProvinciaD = int.Parse(Request.Form["ActivoFijo.LibroActivoFijo.Sucursal.Ciudad.IdProvincia"].ToString());
                    int IdPaisD = int.Parse(Request.Form["ActivoFijo.LibroActivoFijo.Sucursal.Ciudad.Provincia.IdPais"].ToString());

                    try
                    {
                        Sucursal sucursal = new Sucursal();

                        var otraRespuesta = await apiServicio.SeleccionarAsync<Response>(IdSucursalD.ToString(), new Uri(WebApp.BaseAddress), "/api/Sucursal");

                        otraRespuesta.Resultado = JsonConvert.DeserializeObject<Sucursal>(otraRespuesta.Resultado.ToString());

                        if (otraRespuesta.IsSuccess)
                        {
                            sucursal = otraRespuesta.Resultado as Sucursal;

                            string destinoS = sucursal.Nombre;

                            LibroActivoFijo libroAF = new LibroActivoFijo { IdLibroActivoFijo = RecepcionActivoFijoDetalle.ActivoFijo.LibroActivoFijo.IdLibroActivoFijo, IdSucursal = sucursal.IdSucursal };

                            try
                            {
                                var respuestaActualizar = await apiServicio.EditarAsync<LibroActivoFijo>(libroAF.IdLibroActivoFijo.ToString(), libroAF, new Uri(WebApp.BaseAddress), "/api/LibroActivoFijo");

                                if (respuestaActualizar.IsSuccess)
                                {
                                    TransferenciaActivoFijo TransferenciaActivoFijo = new TransferenciaActivoFijo();
                                    TransferenciaActivoFijo.IdMotivoTransferencia = motivoTransferencia;
                                    TransferenciaActivoFijo.IdEmpleado = empleadoEnvia;
                                    TransferenciaActivoFijo.IdEmpleadoRecibo = empleadoRecibe;
                                    TransferenciaActivoFijo.Origen = origen;
                                    TransferenciaActivoFijo.Destino = destinoS;
                                    TransferenciaActivoFijo.Observaciones = observaciones;
                                    TransferenciaActivoFijo.FechaTransferencia = fechaTransferencia;

                                    try
                                    {
                                        response = await apiServicio.InsertarAsync(TransferenciaActivoFijo,
                                                                     new Uri(WebApp.BaseAddress),
                                                                     "/api/TransferenciaActivoFijo/InsertarTransferenciaActivoFijo");

                                        if (response.IsSuccess)
                                        {
                                            var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                                            {
                                                ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                                                ExceptionTrace = null,
                                                Message = "Se ha insertado una Transferencia de Activo Fijo",
                                                UserName = "Usuario 1",
                                                LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                                                LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                                                EntityID = string.Format("{0} {1}", "Transferencia de Activo Fijo:", TransferenciaActivoFijo.IdTransferenciaActivoFijo),
                                            });

                                            var respuestaTransfInsertada = await apiServicio.Listar<TransferenciaActivoFijo>(new Uri(WebApp.BaseAddress), "/api/TransferenciaActivoFijo/ListarTransferenciaActivoFijo");
                                            
                                            TransferenciaActivoFijoDetalle TransferenciaActivoFijoDetalle = new TransferenciaActivoFijoDetalle();
                                            TransferenciaActivoFijoDetalle.IdTransferenciaActivoFijo = respuestaTransfInsertada.Where(x => x.FechaTransferencia == fechaTransferencia).Select(x => x).FirstOrDefault().IdTransferenciaActivoFijo;
                                            TransferenciaActivoFijoDetalle.IdActivoFijo = RecepcionActivoFijoDetalle.IdActivoFijo;

                                            var respuestaDetalle = await apiServicio.InsertarAsync(TransferenciaActivoFijoDetalle,
                                                                         new Uri(WebApp.BaseAddress),
                                                                         "/api/TransferenciaActivoFijoDetalle/InsertarTransferenciaActivoFijoDetalle");

                                            if (respuestaDetalle.IsSuccess)
                                            {
                                                try
                                                {
                                                    lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddress)
                                                                                                        , "/api/RecepcionActivoFijo/ListarRecepcionActivoFijo");

                                                    var listaActivosFijosRecepcionados = lista.Select(c => c).ToList();

                                                    return View("ActivosFijosATransferir", listaActivosFijosRecepcionados);
                                                }
                                                catch (Exception ex)
                                                {
                                                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                                                    {
                                                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                                                        Message = "Listando activos fijos",
                                                        ExceptionTrace = ex,
                                                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                                                        LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                                                        UserName = "Usuario APP webappth"
                                                    });
                                                    return BadRequest();
                                                }
                                            }
                                            
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                        throw;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                                throw;
                            }                            
                        }

                    }
                    catch (Exception ex)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                        {
                            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                            Message = "Seleccionando una Sucursal",
                            ExceptionTrace = ex,
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                            UserName = "Usuario APP WebAppTh"
                        });

                        return BadRequest();
                    }
                }

                //try
                //{
                //    response = await apiServicio.InsertarAsync(TransferenciaActivoFijo,
                //                                                     new Uri(WebApp.BaseAddress),
                //                                                     "/api/TransferenciaActivoFijo/InsertarTransferenciaActivoFijo");

                //    if (response.IsSuccess)
                //    {
                //        var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                //        {
                //            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                //            ExceptionTrace = null,
                //            Message = "Se ha insertado una Transferencia de Activo Fijo",
                //            UserName = "Usuario 1",
                //            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                //            LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                //            EntityID = string.Format("{0} {1}", "Transferencia de Activo Fijo:", TransferenciaActivoFijo.IdTransferenciaActivoFijo),
                //        });

                //        try
                //        {
                //            lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddress)
                //                                                                , "/api/RecepcionActivoFijo/ListarRecepcionActivoFijo");

                //            var listaActivosFijosRecepcionados = lista.Select(c => c).ToList();

                //            return View("ActivosFijosATransferir", listaActivosFijosRecepcionados);
                //        }
                //        catch (Exception ex)
                //        {
                //            await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                //            {
                //                ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                //                Message = "Listando activos fijos",
                //                ExceptionTrace = ex,
                //                LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                //                LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                //                UserName = "Usuario APP webappth"
                //            });
                //            return BadRequest();
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                //    {
                //        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                //        Message = "Insertando una Transferencia de Activo Fijo",
                //        ExceptionTrace = ex,
                //        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                //        LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                //        UserName = "Usuario APP WebAppTh"
                //    });

                //    return BadRequest();
                //}
            }
            catch (Exception ex)
            {

                throw;
            }
                        
            return View("ActivosFijosATransferir", lista);
        }

        #endregion

        #region Depreciación
        //Implementar aquí
        #endregion

        #region Baja de Activos
        public async Task<IActionResult> ActivoFijoBaja(string id)
        {
            ViewData["MotivoActivoFijoBaja"] = new SelectList(await apiServicio.Listar<ActivoFijoMotivoBaja>(new Uri(WebApp.BaseAddress), "/api/ActivoFijoMotivoBaja/ListarActivoFijoMotivoBaja"), "IdActivoFijoMotivoBaja", "Nombre");
            return await ObtenerRecepcionActivoFijo(id, "Alta");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivoFijoBaja(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle)
        {
            Response response = new Response();
            try
            {
                ActivosFijosBaja activosFijosBaja = new ActivosFijosBaja {FechaBaja = DateTime.Now, IdMotivoBaja = recepcionActivoFijoDetalle.ActivoFijo.ActivosFijosBaja.IdMotivoBaja, IdActivo = recepcionActivoFijoDetalle.IdActivoFijo };
                response = await apiServicio.InsertarAsync(activosFijosBaja,
                                                             new Uri(WebApp.BaseAddress),
                                                             "/api/ActivosFijosBaja/InsertarActivosFijosBaja");
                if (response.IsSuccess)
                {

                    var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        ExceptionTrace = null,
                        Message = "Se ha creado una Baja de Activo Fijo",
                        UserName = "Usuario 1",
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        EntityID = string.Format("{0} {1}", "ActivosFijosBaja:", activosFijosBaja.IdMotivoBaja),
                    });

                    
                }

                await apiServicio.InsertarAsync(new Estado { Nombre = "Baja" },
                                                             new Uri(WebApp.BaseAddress),
                                                             "/api/Estado/InsertarEstado");

                recepcionActivoFijoDetalle.Estado = new Estado { Nombre = "Baja" };
                response = await apiServicio.EditarAsync(recepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle.ToString(), recepcionActivoFijoDetalle, new Uri(WebApp.BaseAddress), "/api/RecepcionActivoFijo/EstadoActivoFijo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        EntityID = string.Format("{0} : {1}", "Estado de Activo Fijo", recepcionActivoFijoDetalle.ActivoFijo.CodigoActivoFijo.IdCodigoActivoFijo.ToString()),
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        Message = "Se ha actualizado un registro estado de activo fijo",
                        UserName = "Usuario 1"
                    });
                    return RedirectToAction("ActivosFijosRecepcionadosBaja");
                }

                ViewData["Error"] = response.Message;
                ViewData["MotivoActivoFijoBaja"] = new SelectList(await apiServicio.Listar<ActivoFijoMotivoBaja>(new Uri(WebApp.BaseAddress), "/api/ActivoFijoMotivoBaja/ListarActivoFijoMotivoBaja"), "IdActivoFijoMotivoBaja", "Nombre");
                return View(recepcionActivoFijoDetalle);

            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Creando Tipo Activo Fijo",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppRM"
                });

                return BadRequest();
            }
        }

        public async Task<IActionResult> ActivosFijosBajas()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();

            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/RecepcionActivoFijo/ListarRecepcionActivoFijo");

                var listaActivosFijosBajas = lista.Where(c => c.Estado.Nombre == "Baja").ToList();
                ViewData["titulo"] = "Activos Fijos con Estado Baja";
                return View("ListadoActivoFijoBaja", listaActivosFijosBajas);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando activos fijos con estado Baja",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        public async Task<IActionResult> ActivosFijosRecepcionadosBaja()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/RecepcionActivoFijo/ListarRecepcionActivoFijo");

                var listaActivosFijosRecepcionados = lista.Where(c => c.Estado.Nombre == "Alta").ToList();
                ViewData["titulo"] = "Activos Fijos de Alta";
                ViewData["textoColumna"] = "Dar Baja";
                ViewData["url"] = "ActivoFijoBaja"; //Url de la ventana para gestionar el Alta
                return View("ListadoActivoFijo", listaActivosFijosRecepcionados);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando activos fijos con estado Recepcionado",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }
        #endregion

        #region Mantenimiento de Activos
        public async Task<IActionResult> ListarMantenimientos()
        {
            var lista = new List<MantenimientoActivoFijo>();
            try
            {
                lista = await apiServicio.Listar<MantenimientoActivoFijo>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/MantenimientoActivoFijo/ListarMantenimientosActivoFijo");
                
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando Mantenimientos de Activos Fijos",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        public async Task<IActionResult> CrearMantenimiento()
        {
            ViewData["ActivoFijo"] = new SelectList(await apiServicio.Listar<ActivoFijo>(new Uri(WebApp.BaseAddress), "/api/ActivosFijos/ListarActivosFijos"), "IdActivoFijo", "Nombre");
            var listaEmpleado = await apiServicio.Listar<Empleado>(new Uri(WebApp.BaseAddress), "/api/Empleado/ListarEmpleados");
            var tlistaEmpleado = listaEmpleado.Select(c => new { IdEmpleado = c.IdEmpleado, NombreApellidos = String.Format("{0} {1}", c.Persona.Nombres, c.Persona.Apellidos) });
            ViewData["Empleado"] = new SelectList(tlistaEmpleado, "IdEmpleado", "NombreApellidos");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearMantenimiento(MantenimientoActivoFijo mantenimientoActivoFijo)
        {
            Response response = new Response();
            try
            {
                response = await apiServicio.InsertarAsync(mantenimientoActivoFijo,
                                                             new Uri(WebApp.BaseAddress),
                                                             "/api/MantenimientoActivoFijo/InsertarMantenimientoActivoFijo");
                if (response.IsSuccess)
                {

                    var responseLog = await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        ExceptionTrace = null,
                        Message = "Se ha creado un Mantenimiento Activo Fijo",
                        UserName = "Usuario 1",
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        EntityID = string.Format("{0} {1}", "Mantenimiento Activo Fijo:", mantenimientoActivoFijo.IdMantenimientoActivoFijo),
                    });

                    return RedirectToAction("ListarMantenimientos");
                }

                ViewData["Error"] = response.Message;
                ViewData["ActivoFijo"] = new SelectList(await apiServicio.Listar<ActivoFijo>(new Uri(WebApp.BaseAddress), "/api/ActivosFijos/ListarActivosFijos"), "IdActivoFijo", "Nombre");
                var listaEmpleado = await apiServicio.Listar<Empleado>(new Uri(WebApp.BaseAddress), "/api/Empleado/ListarEmpleados");
                var tlistaEmpleado = listaEmpleado.Select(c => new { IdEmpleado = c.IdEmpleado, NombreApellidos = String.Format("{0} {1}", c.Persona.Nombres, c.Persona.Apellidos) });
                ViewData["Empleado"] = new SelectList(tlistaEmpleado, "IdEmpleado", "NombreApellidos");
                return View(mantenimientoActivoFijo);

            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Creando Mantenimiento Activo Fijo",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP WebAppTh"
                });

                return BadRequest();
            }
        }

        public async Task<IActionResult> EditarMantenimiento(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddress),
                                                                  "/api/MantenimientoActivoFijo");


                    respuesta.Resultado = JsonConvert.DeserializeObject<MantenimientoActivoFijo>(respuesta.Resultado.ToString());
                    ViewData["ActivoFijo"] = new SelectList(await apiServicio.Listar<ActivoFijo>(new Uri(WebApp.BaseAddress), "/api/ActivosFijos/ListarActivosFijos"), "IdActivoFijo", "Nombre");
                    var listaEmpleado = await apiServicio.Listar<Empleado>(new Uri(WebApp.BaseAddress), "/api/Empleado/ListarEmpleados");
                    var tlistaEmpleado = listaEmpleado.Select(c => new { IdEmpleado = c.IdEmpleado, NombreApellidos = String.Format("{0} {1}", c.Persona.Nombres, c.Persona.Apellidos) });
                    ViewData["Empleado"] = new SelectList(tlistaEmpleado, "IdEmpleado", "NombreApellidos");
                    if (respuesta.IsSuccess)
                    {
                        return View(respuesta.Resultado);
                    }

                }

                return BadRequest();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarMantenimiento(string id, MantenimientoActivoFijo mantenimientoActivoFijo)
        {
            Response response = new Response();
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    response = await apiServicio.EditarAsync(id, mantenimientoActivoFijo, new Uri(WebApp.BaseAddress),
                                                                 "/api/MantenimientoActivoFijo");

                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                        {
                            ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                            EntityID = string.Format("{0} : {1}", "Motivo de Asiento", id),
                            LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit),
                            LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                            Message = "Se ha actualizado un registro Mantenimiento Activo Fijo",
                            UserName = "Usuario 1"
                        });

                        return RedirectToAction("ListarMantenimientos");
                    }

                }
                return View(mantenimientoActivoFijo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Editando un Mantenimiento Activo Fijo",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });

                return BadRequest();
            }
        }

        public async Task<IActionResult> EliminarMantenimiento(string id)
        {

            try
            {
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddress)
                                                               , "/api/MantenimientoActivoFijo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                        EntityID = string.Format("{0} : {1}", "Mantenimiento Activo Fijo", id),
                        Message = "Registro eliminado",
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ADV),
                        UserName = "Usuario APP webappth"
                    });
                    return RedirectToAction("ListarMantenimientos");
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Eliminar Mantenimiento Activo Fijo",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });

                return BadRequest();
            }
        }
        #endregion

        #region Reportes
        public async Task<IActionResult> HojaVidaActivoFijo(string id) => await ObtenerRecepcionActivoFijo(id, null);

        public async Task<IActionResult> HojaVidaReporte()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/RecepcionActivoFijo/ListarRecepcionActivoFijo");

                var listaActivosFijosRecepcionados = lista.Where(c => c.Estado.Nombre == "Alta").ToList();//LIstar por algun argumento por definir??
                ViewData["titulo"] = "Activos Fijos";
                ViewData["textoColumna"] = "Ver Hoja de Vida";
                ViewData["url"] = "HojaVidaActivoFijo"; //Url de la ventana para gestionar el Alta
                return View("ListadoActivoFijo", listaActivosFijosRecepcionados);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando activos fijos con estado Recepcionado",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        public async Task<IActionResult> BienesReporte()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/RecepcionActivoFijo/ListarRecepcionActivoFijo");

                var listaActivosFijos = lista.Where(c => c.Estado.Nombre != "Validación Técnica" && c.Estado.Nombre != "Desaprobado").OrderBy(c=> c.ActivoFijo.LibroActivoFijo.Sucursal.Ciudad.Provincia.Pais.Nombre).ThenBy(c=> c.ActivoFijo.LibroActivoFijo.Sucursal.Ciudad.Provincia.Nombre).ThenBy(c => c.ActivoFijo.LibroActivoFijo.Sucursal.Ciudad.Nombre).ThenBy(c => c.ActivoFijo.LibroActivoFijo.Sucursal.Nombre).ThenBy(c => c.ActivoFijo.LibroActivoFijo.IdSucursal).ThenBy(c=> c.RecepcionActivoFijo.Empleado.Persona.Nombres).ThenBy(c => c.RecepcionActivoFijo.Empleado.Persona.Apellidos).ToList();
                return View(listaActivosFijos);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando activos fijos por área usuaria y clasificado por funcionario",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        public async Task<IActionResult> MantenimientosReporte()
        {
            var lista = new List<MantenimientoActivoFijo>();
            try
            {
                lista = await apiServicio.Listar<MantenimientoActivoFijo>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/MantenimientoActivoFijo/ListarMantenimientosActivoFijo");                
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando mantenimientos de activos fijos",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        public async Task<IActionResult> PolizasReporte()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddress)
                                                                    , "/api/RecepcionActivoFijo/ListarRecepcionActivoFijo");

                var listaActivosFijosRecepcionados = lista.Where(c => c.Estado.Nombre == "Recepcionado" && c.NumeroPoliza != "N/A").ToList();               
                return View(listaActivosFijosRecepcionados);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.WebAppRM),
                    Message = "Listando activos fijos con estado Recepcionado con número de póliza asignado",
                    ExceptionTrace = ex,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "Usuario APP webappth"
                });
                return BadRequest();
            }
        }

        #endregion

        #region AJAX_ClaseActivoFijo
        public async Task<SelectList> ObtenerSelectListClaseActivoFijo(int idTipoActivoFijo)
        {
            try
            {
                var listaClaseActivoFijo = await apiServicio.Listar<ClaseActivoFijo>(new Uri(WebApp.BaseAddress), "/api/ClaseActivoFijo/ListarClaseActivoFijo");
                listaClaseActivoFijo = idTipoActivoFijo != -1 ? listaClaseActivoFijo.Where(c => c.IdTipoActivoFijo == idTipoActivoFijo).ToList() : new List<ClaseActivoFijo>();
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
            return PartialView("_ClaseActivoFijoSelect", new RecepcionActivoFijoDetalle());
        }
        #endregion

        #region AJAX_SubClaseActivoFijo
        public async Task<SelectList> ObtenerSelectListSubClaseActivoFijo(int idClaseActivoFijo)
        {
            try
            {
                var listaSubClaseActivoFijo = await apiServicio.Listar<SubClaseActivoFijo>(new Uri(WebApp.BaseAddress), "/api/SubClaseActivoFijo/ListarSubClasesActivoFijo");
                listaSubClaseActivoFijo = idClaseActivoFijo != -1 ? listaSubClaseActivoFijo.Where(c => c.IdClaseActivoFijo == idClaseActivoFijo).ToList() : new List<SubClaseActivoFijo>();
                return new SelectList(listaSubClaseActivoFijo, "IdSubClaseActivoFijo", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<SubClaseActivoFijo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubClaseActivoFijo_SelectResult(int idClaseActivoFijo)
        {
            ViewBag.SubClaseActivoFijo = await ObtenerSelectListSubClaseActivoFijo(idClaseActivoFijo);
            return PartialView("_SubClaseActivoFijoSelect", new RecepcionActivoFijoDetalle());
        }
        #endregion

        #region AJAX_Modelo
        public async Task<SelectList> ObtenerSelectListModelo(int idMarca)
        {
            try
            {
                var listaModelo = await apiServicio.Listar<Modelo>(new Uri(WebApp.BaseAddress), "/api/Modelo/ListarModelos");
                listaModelo = idMarca != -1 ? listaModelo.Where(c => c.IdMarca == idMarca).ToList() : new List<Modelo>();
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
            ViewBag.Modelo = await ObtenerSelectListModelo(idMarca);
            return PartialView("_ModeloSelect", new RecepcionActivoFijoDetalle());
        }
        #endregion

        #region AJAX_Provincia
        public async Task<SelectList> ObtenerSelectListProvincia(int idPais)
        {
            try
            {
                var listaProvincia = await apiServicio.Listar<Provincia>(new Uri(WebApp.BaseAddress), "/api/Provincia/ListarProvincias");
                listaProvincia = idPais != -1 ? listaProvincia.Where(c => c.IdPais == idPais).ToList() : new List<Provincia>();
                return new SelectList(listaProvincia, "IdProvincia", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<Provincia>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Provincia_SelectResult(int idPais)
        {
            ViewBag.Provincia = await ObtenerSelectListProvincia(idPais);
            return PartialView("_ProvinciaSelect", new RecepcionActivoFijoDetalle());
        }
        #endregion

        #region AJAX_Ciudad
        public async Task<SelectList> ObtenerSelectListCiudad(int idProvincia)
        {
            try
            {
                var listaCiudad = await apiServicio.Listar<Ciudad>(new Uri(WebApp.BaseAddress), "/api/Ciudad/ListarCiudades");
                listaCiudad = idProvincia != -1 ? listaCiudad.Where(c => c.IdProvincia == idProvincia).ToList() : new List<Ciudad>();
                return new SelectList(listaCiudad, "IdCiudad", "Nombre");
            }
            catch (Exception)
            {
                return new SelectList(new List<Ciudad>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Ciudad_SelectResult(int idProvincia)
        {
            ViewBag.Ciudad = await ObtenerSelectListCiudad(idProvincia);
            return PartialView("_CiudadSelect", new RecepcionActivoFijoDetalle());
        }
        #endregion

        #region AJAX_Sucursal
        public async Task<SelectList> ObtenerSelectListSucursal(int idCiudad)
        {
            try
            {
                var listaSucursal = await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddress), "/api/Sucursal/ListarSucursales");
                listaSucursal = idCiudad != -1 ? listaSucursal.Where(c => c.IdCiudad == idCiudad).ToList() : new List<Sucursal>();
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
            return PartialView("_SucursalSelect", new RecepcionActivoFijoDetalle());
        }
        #endregion

        #region AJAX_LibroActivoFijo
        public async Task<SelectList> ObtenerSelectListLibroActivoFijo(int idSucursal)
        {
            try
            {
                var listaLibroActivoFijo = await apiServicio.Listar<LibroActivoFijo>(new Uri(WebApp.BaseAddress), "/api/LibroActivoFijo/ListarLibrosActivoFijo");
                listaLibroActivoFijo = idSucursal != -1 ? listaLibroActivoFijo.Where(c => c.IdSucursal == idSucursal).ToList() : new List<LibroActivoFijo>();
                return new SelectList(listaLibroActivoFijo, "IdLibroActivoFijo", "IdLibroActivoFijo");
            }
            catch (Exception)
            {
                return new SelectList(new List<LibroActivoFijo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> LibroActivoFijo_SelectResult(int idSucursal)
        {
            ViewBag.LibroActivoFijo = await ObtenerSelectListLibroActivoFijo(idSucursal);
            return PartialView("_LibroActivoFijoSelect", new RecepcionActivoFijoDetalle());
        }
        #endregion
    }
}