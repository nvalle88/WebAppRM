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
using System.IO;
using bd.webapprm.entidades.ObjectTransfer;
using bd.webapprm.servicios.Extensores;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Filters;

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
            try
            {
                ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo((ViewData["TipoActivoFijo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["TipoActivoFijo"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["SubClaseActivoFijo"] = await ObtenerSelectListSubClaseActivoFijo((ViewData["ClaseActivoFijo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["ClaseActivoFijo"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["MotivoRecepcion"] = new SelectList(await apiServicio.Listar<MotivoRecepcion>(new Uri(WebApp.BaseAddressRM), "api/MotivoRecepcion/ListarMotivoRecepcion"), "IdMotivoRecepcion", "Descripcion");
                ViewData["FondoFinanciamiento"] = new SelectList(await apiServicio.Listar<FondoFinanciamiento>(new Uri(WebApp.BaseAddressRM), "api/FondoFinanciamiento/ListarFondoFinanciamiento"), "IdFondoFinanciamiento", "Nombre");
                ViewData["Proveedor"] = new SelectList((await apiServicio.ObtenerElementoAsync<List<Proveedor>> (new ProveedorTransfer { LineaServicio = LineasServicio.ActivosFijos, Activo = true }, new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedoresPorLineaServicioEstado")).Select(c => new { c.IdProveedor, NombreApellidos = $"{c.Nombre} {c.Apellidos}" }), "IdProveedor", "NombreApellidos");

                ViewData["Sucursal"] = new SelectList(await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal"), "IdSucursal", "Nombre");
                ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo((ViewData["Sucursal"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Sucursal"] as SelectList).FirstOrDefault().Value) : -1);

                ViewData["Marca"] = new SelectList(await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddressRM), "api/Marca/ListarMarca"), "IdMarca", "Nombre");
                ViewData["Modelo"] = await ObtenerSelectListModelo((ViewData["Marca"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Marca"] as SelectList).FirstOrDefault().Value) : -1);

                ViewData["Ramo"] = new SelectList(await apiServicio.Listar<Ramo>(new Uri(WebApp.BaseAddressRM), "api/Ramo/ListarRamo"), "IdRamo", "Nombre");
                ViewData["Subramo"] = await ObtenerSelectListSubramo((ViewData["Ramo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Ramo"] as SelectList).FirstOrDefault().Value) : -1);
                ViewData["CompaniaSeguro"] = new SelectList(await apiServicio.Listar<CompaniaSeguro>(new Uri(WebApp.BaseAddressRM), "api/CompaniaSeguro/ListarCompaniaSeguro"), "IdCompaniaSeguro", "Nombre");
                
                ViewData["ListadoRecepcionActivoFijoDetalle"] = new List<RecepcionActivoFijoDetalle>();

                var primeraCategoria = await apiServicio.ObtenerElementoAsync<CategoriaActivoFijo>(null, new Uri(WebApp.BaseAddressRM), "api/CategoriaActivoFijo/ObtenerPrimeraCategoria");
                ViewData["Categoria"] = primeraCategoria?.Nombre ?? Categorias.Edificio;
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ActivosFijosRecepcionados));
            }
        }

        public async Task<IActionResult> EditarRecepcionAR(string id) => await EditarRecepcion(id, nameof(ActivosFijosRecepcionados), nameof(EditarRecepcionAR), Estados.Recepcionado);
        public async Task<IActionResult> EditarRecepcionVT(string id) => await EditarRecepcion(id, nameof(ActivosFijosValidacionTecnica), nameof(EditarRecepcionVT), Estados.ValidacionTecnica);
        public async Task<IActionResult> EditarRecepcion(string id, string nombreVistaError, string accionVistaEditar, string estado)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.ObtenerElementoAsync<Response>(new IdEstadosTransfer { Id = int.Parse(id), Estados = new List<string> { estado } }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ObtenerActivoFijoPorEstado");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nombreVistaError);

                    var activoFijo = JsonConvert.DeserializeObject<ActivoFijo>(respuesta.Resultado.ToString());
                    var recepcionActivoFijoDetalle = activoFijo.RecepcionActivoFijoDetalle.FirstOrDefault();
                    recepcionActivoFijoDetalle.ActivoFijo = activoFijo;

                    ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                    ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo(activoFijo?.SubClaseActivoFijo?.ClaseActivoFijo?.IdTipoActivoFijo ?? -1);
                    ViewData["SubClaseActivoFijo"] = await ObtenerSelectListSubClaseActivoFijo(activoFijo?.SubClaseActivoFijo?.IdClaseActivoFijo ?? -1);
                    ViewData["MotivoRecepcion"] = new SelectList(await apiServicio.Listar<MotivoRecepcion>(new Uri(WebApp.BaseAddressRM), "api/MotivoRecepcion/ListarMotivoRecepcion"), "IdMotivoRecepcion", "Descripcion");
                    ViewData["FondoFinanciamiento"] = new SelectList(await apiServicio.Listar<FondoFinanciamiento>(new Uri(WebApp.BaseAddressRM), "api/FondoFinanciamiento/ListarFondoFinanciamiento"), "IdFondoFinanciamiento", "Nombre");

                    ViewData["Sucursal"] = new SelectList(await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal"), "IdSucursal", "Nombre");
                    ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo(recepcionActivoFijoDetalle?.UbicacionActivoFijoActual?.LibroActivoFijo?.IdSucursal ?? -1);

                    ViewData["Proveedor"] = new SelectList((await apiServicio.ObtenerElementoAsync<List<Proveedor>>(new ProveedorTransfer { LineaServicio = LineasServicio.ActivosFijos, Activo = true }, new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedoresPorLineaServicioEstado")).Select(c => new { c.IdProveedor, NombreApellidos = $"{c.Nombre} {c.Apellidos}" }), "IdProveedor", "NombreApellidos");
                    ViewData["Marca"] = new SelectList(await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddressRM), "api/Marca/ListarMarca"), "IdMarca", "Nombre");
                    ViewData["Modelo"] = await ObtenerSelectListModelo(activoFijo?.Modelo?.IdMarca ?? -1);

                    ViewData["Ramo"] = new SelectList(await apiServicio.Listar<Ramo>(new Uri(WebApp.BaseAddressRM), "api/Ramo/ListarRamo"), "IdRamo", "Nombre");
                    ViewData["Subramo"] = await ObtenerSelectListSubramo(activoFijo?.PolizaSeguroActivoFijo?.Subramo?.IdRamo ?? -1);
                    ViewData["CompaniaSeguro"] = new SelectList(await apiServicio.Listar<CompaniaSeguro>(new Uri(WebApp.BaseAddressRM), "api/CompaniaSeguro/ListarCompaniaSeguro"), "IdCompaniaSeguro", "Nombre");
                    
                    ViewData["ListadoRecepcionActivoFijoDetalle"] = activoFijo.RecepcionActivoFijoDetalle.ToList();
                    ViewData["UbicacionActivoFijo"] = recepcionActivoFijoDetalle?.UbicacionActivoFijoActual;
                    ViewData["Categoria"] = activoFijo.SubClaseActivoFijo.ClaseActivoFijo.CategoriaActivoFijo.Nombre;
                    ViewData["AccionVistaEditar"] = accionVistaEditar;
                    return View("EditarRecepcion", recepcionActivoFijoDetalle);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nombreVistaError);
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nombreVistaError);
            }
        }

        public async Task<IActionResult> GestionRecepcionActivoFijoDetalle(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle, LibroActivoFijo libroActivoFijo)
        {
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Recepcionado }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.ValidacionTecnica }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                var listaEstado = await apiServicio.Listar<Estado>(new Uri(WebApp.BaseAddressTH), "api/Estados/ListarEstados");
                recepcionActivoFijoDetalle.Estado = listaEstado.SingleOrDefault(c => c.Nombre == (!recepcionActivoFijoDetalle.RecepcionActivoFijo.ValidacionTecnica ? Estados.Recepcionado : Estados.ValidacionTecnica));
                recepcionActivoFijoDetalle.IdEstado = recepcionActivoFijoDetalle.Estado.IdEstado;

                var response = new Response();
                int idActivoFijo = 0;
                var listaRecepcionActivoFijoUbicacionTransfer = new List<RecepcionActivoFijoDetalle>();
                var listaFormDatosEspecificos = Request.Form.Where(c => c.Key.StartsWith("hBodega_"));
                foreach (var item in listaFormDatosEspecificos)
                {
                    int posFormDatoEspecifico = int.Parse(item.Key.ToString().Split('_')[1]);
                    int? idBodega = Utiles.TryParseInt(Request.Form[$"hBodega_{posFormDatoEspecifico}"]);
                    int? idEmpleado = Utiles.TryParseInt(Request.Form[$"hEmpleado_{posFormDatoEspecifico}"]);
                    int? idRecepcionActivoFijoDetalle = Utiles.TryParseInt(Request.Form[$"hIdRecepcionActivoFijoDetalle_{posFormDatoEspecifico}"]);
                    int? idUbicacionActivoFijo = Utiles.TryParseInt(Request.Form[$"hUbicacion_{posFormDatoEspecifico}"]);
                    string codigoSecuencial = Request.Form[$"hCodigoSecuencial_{posFormDatoEspecifico}"].ToString();
                    int? idCodigoActivoFijo = Utiles.TryParseInt(Request.Form[$"hIdCodigoActivoFijo_{posFormDatoEspecifico}"]);
                    string[] arrComponentes = Request.Form[$"hComponentes_{posFormDatoEspecifico}"].ToString().Trim().Split(',');

                    var rafd = new RecepcionActivoFijoDetalle
                    {
                        IdRecepcionActivoFijoDetalle = idRecepcionActivoFijoDetalle ?? 0,
                        IdEstado = recepcionActivoFijoDetalle.IdEstado,
                        IdRecepcionActivoFijo = recepcionActivoFijoDetalle.IdRecepcionActivoFijo,
                        IdActivoFijo = recepcionActivoFijoDetalle.IdActivoFijo,
                        ActivoFijo = recepcionActivoFijoDetalle.ActivoFijo,
                        RecepcionActivoFijo = recepcionActivoFijoDetalle.RecepcionActivoFijo,
                        Estado = recepcionActivoFijoDetalle.Estado,
                        IdCodigoActivoFijo = idCodigoActivoFijo ?? 0,
                        CodigoActivoFijo = new CodigoActivoFijo { Codigosecuencial = codigoSecuencial },
                        UbicacionActivoFijoActual = new UbicacionActivoFijo
                        {
                            IdUbicacionActivoFijo = idUbicacionActivoFijo ?? 0,
                            IdEmpleado = idEmpleado,
                            IdBodega = idBodega,
                            Bodega = idBodega != null ? JsonConvert.DeserializeObject<Bodega>((await apiServicio.SeleccionarAsync<Response>(idBodega.ToString(), new Uri(WebApp.BaseAddressRM), "api/Bodega")).Resultado.ToString()) : null,
                            Empleado = idEmpleado != null ? JsonConvert.DeserializeObject<Empleado>((await apiServicio.SeleccionarAsync<Response>(idEmpleado.ToString(), new Uri(WebApp.BaseAddressTH), "api/Empleados")).Resultado.ToString()) : null,
                            FechaUbicacion = recepcionActivoFijoDetalle.RecepcionActivoFijo.FechaRecepcion,
                            IdLibroActivoFijo = int.Parse(Request.Form["IdLibroActivoFijo"].ToString()),
                            LibroActivoFijo = JsonConvert.DeserializeObject<LibroActivoFijo>((await apiServicio.SeleccionarAsync<Response>(Request.Form["IdLibroActivoFijo"].ToString(), new Uri(WebApp.BaseAddressRM), "api/LibroActivoFijo")).Resultado.ToString())
                        }
                    };
                    if (recepcionActivoFijoDetalle.ActivoFijo.SubClaseActivoFijo.ClaseActivoFijo.CategoriaActivoFijo.Nombre == Categorias.Edificio)
                        rafd.NumeroClaveCatastral = !String.IsNullOrEmpty(Request.Form[$"hNumeroClaveCatastral_{posFormDatoEspecifico}"].ToString()) ? Request.Form[$"hNumeroClaveCatastral_{posFormDatoEspecifico}"].ToString() : null;
                    else if (recepcionActivoFijoDetalle.ActivoFijo.SubClaseActivoFijo.ClaseActivoFijo.CategoriaActivoFijo.Nombre == Categorias.Vehiculo)
                    {
                        rafd.NumeroChasis = !String.IsNullOrEmpty(Request.Form[$"hNumeroChasis_{posFormDatoEspecifico}"].ToString()) ? Request.Form[$"hNumeroChasis_{posFormDatoEspecifico}"].ToString() : null;
                        rafd.NumeroMotor = !String.IsNullOrEmpty(Request.Form[$"hNumeroMotor_{posFormDatoEspecifico}"].ToString()) ? Request.Form[$"hNumeroMotor_{posFormDatoEspecifico}"].ToString() : null;
                        rafd.Placa = !String.IsNullOrEmpty(Request.Form[$"hPlaca_{posFormDatoEspecifico}"].ToString()) ? Request.Form[$"hPlaca_{posFormDatoEspecifico}"].ToString() : null;
                    }
                    else if (recepcionActivoFijoDetalle.ActivoFijo.SubClaseActivoFijo.ClaseActivoFijo.CategoriaActivoFijo.Nombre == Categorias.EquiposComputoSoftware)
                        rafd.Serie = !String.IsNullOrEmpty(Request.Form[$"hSerie_{posFormDatoEspecifico}"].ToString()) ? Request.Form[$"hSerie_{posFormDatoEspecifico}"].ToString() : null;

                    foreach (var comp in arrComponentes)
                    {
                        if (!String.IsNullOrEmpty(comp))
                            rafd.ComponentesActivoFijoOrigen.Add(new ComponenteActivoFijo { IdRecepcionActivoFijoDetalleOrigen = rafd.IdRecepcionActivoFijoDetalle, IdRecepcionActivoFijoDetalleComponente = int.Parse(comp.Trim()), FechaAdicion = recepcionActivoFijoDetalle.RecepcionActivoFijo.FechaRecepcion });
                    }
                    listaRecepcionActivoFijoUbicacionTransfer.Add(rafd);
                }

                if (recepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle == 0)
                {
                    response = await apiServicio.InsertarAsync(listaRecepcionActivoFijoUbicacionTransfer, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/InsertarRecepcionActivoFijo");
                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha recepcionado un activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Activo Fijo:", recepcionActivoFijoDetalle.ActivoFijo.IdActivoFijo) });
                        idActivoFijo = JsonConvert.DeserializeObject<List<RecepcionActivoFijoDetalle>>(response.Resultado.ToString()).FirstOrDefault().IdActivoFijo;
                    }
                }
                else
                {
                    idActivoFijo = recepcionActivoFijoDetalle.IdActivoFijo;
                    response = await apiServicio.EditarAsync(recepcionActivoFijoDetalle.IdActivoFijo.ToString(), listaRecepcionActivoFijoUbicacionTransfer, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos");
                    if (response.IsSuccess)
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha editado una recepción de activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Activo Fijo:", recepcionActivoFijoDetalle.ActivoFijo.IdActivoFijo) });
                }

                var responseFile = new Response { IsSuccess = true };
                if (response.IsSuccess)
                {
                    if (Request.Form.Files.Count > 0)
                    {
                        foreach (var item in Request.Form.Files)
                        {
                            byte[] data;
                            using (var br = new BinaryReader(item.OpenReadStream()))
                                data = br.ReadBytes((int)item.OpenReadStream().Length);

                            if (data.Length > 0)
                            {
                                var activoFijoDocumentoTransfer = new DocumentoActivoFijoTransfer { Nombre = item.FileName, Fichero = data, IdActivoFijo = idActivoFijo };
                                responseFile = await apiServicio.InsertarAsync(activoFijoDocumentoTransfer, new Uri(WebApp.BaseAddressRM), "api/DocumentoActivoFijo/UploadFiles");
                                if (responseFile != null && responseFile.IsSuccess)
                                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha subido un archivo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Documento de Activo Fijo:", activoFijoDocumentoTransfer.Nombre) });
                            }
                        }
                    }
                }

                if (response.IsSuccess)
                    return this.Redireccionar(responseFile.IsSuccess ? $"{Mensaje.Informacion}|{Mensaje.Satisfactorio}" : $"{Mensaje.Aviso}|{Mensaje.ErrorUploadFiles}", !recepcionActivoFijoDetalle.RecepcionActivoFijo.ValidacionTecnica ? nameof(ActivosFijosRecepcionados) : nameof(ActivosFijosValidacionTecnica));

                ViewData["Error"] = response.Message;
                ViewData["UbicacionActivoFijo"] = new UbicacionActivoFijo { LibroActivoFijo = libroActivoFijo };
                ViewData["ListadoRecepcionActivoFijoDetalle"] = listaRecepcionActivoFijoUbicacionTransfer;
                ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo(recepcionActivoFijoDetalle?.ActivoFijo?.SubClaseActivoFijo?.ClaseActivoFijo?.IdTipoActivoFijo ?? -1);
                ViewData["SubClaseActivoFijo"] = await ObtenerSelectListSubClaseActivoFijo(recepcionActivoFijoDetalle?.ActivoFijo?.SubClaseActivoFijo?.IdClaseActivoFijo ?? -1);
                ViewData["MotivoRecepcion"] = new SelectList(await apiServicio.Listar<MotivoRecepcion>(new Uri(WebApp.BaseAddressRM), "api/MotivoRecepcion/ListarMotivoRecepcion"), "IdMotivoRecepcion", "Descripcion");
                ViewData["FondoFinanciamiento"] = new SelectList(await apiServicio.Listar<FondoFinanciamiento>(new Uri(WebApp.BaseAddressRM), "api/FondoFinanciamiento/ListarFondoFinanciamiento"), "IdFondoFinanciamiento", "Nombre");

                ViewData["Sucursal"] = new SelectList(await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal"), "IdSucursal", "Nombre");
                ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo(libroActivoFijo?.IdSucursal ?? -1);

                ViewData["Proveedor"] = new SelectList((await apiServicio.ObtenerElementoAsync<List<Proveedor>>(new ProveedorTransfer { LineaServicio = LineasServicio.ActivosFijos, Activo = true }, new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedoresPorLineaServicioEstado")).Select(c => new { c.IdProveedor, NombreApellidos = $"{c.Nombre} {c.Apellidos}" }), "IdProveedor", "NombreApellidos");
                ViewData["Marca"] = new SelectList(await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddressRM), "api/Marca/ListarMarca"), "IdMarca", "Nombre");
                ViewData["Modelo"] = await ObtenerSelectListModelo(recepcionActivoFijoDetalle?.ActivoFijo?.Modelo?.IdMarca ?? -1);

                ViewData["Ramo"] = new SelectList(await apiServicio.Listar<Ramo>(new Uri(WebApp.BaseAddressRM), "api/Ramo/ListarRamo"), "IdRamo", "Nombre");
                ViewData["Subramo"] = await ObtenerSelectListSubramo(recepcionActivoFijoDetalle?.ActivoFijo?.PolizaSeguroActivoFijo?.Subramo?.IdRamo ?? -1);
                ViewData["CompaniaSeguro"] = new SelectList(await apiServicio.Listar<CompaniaSeguro>(new Uri(WebApp.BaseAddressRM), "api/CompaniaSeguro/ListarCompaniaSeguro"), "IdCompaniaSeguro", "Nombre");
                ViewData["Categoria"] = recepcionActivoFijoDetalle.ActivoFijo.SubClaseActivoFijo.ClaseActivoFijo.CategoriaActivoFijo.Nombre;
                return recepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle == 0 ? View(recepcionActivoFijoDetalle) : View("EditarRecepcion", recepcionActivoFijoDetalle);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando recepción Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(Recepcion));
            }
        }

        public async Task<IActionResult> DeleteRecepcion(string id, bool activoFijoRecepcionado)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Sistema", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webappth" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}", activoFijoRecepcionado ? nameof(ActivosFijosRecepcionados) : nameof(ActivosFijosValidacionTecnica));
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}", activoFijoRecepcionado ? nameof(ActivosFijosRecepcionados) : nameof(ActivosFijosValidacionTecnica));
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar Marca", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}", activoFijoRecepcionado ? nameof(ActivosFijosRecepcionados) : nameof(ActivosFijosValidacionTecnica));
            }
        }

        private string AgregarCeros(string valor, int cantidadCeros)
        {
            if (valor.Length < cantidadCeros)
            {
                var longitud = cantidadCeros - valor.Length;
                for (var i = 0; i < longitud; i++)
                    valor = "0" + valor;
            }
            return valor;
        }

        [HttpPost]
        public async Task<JsonResult> ValidarCodigoUnico(int idCodigoActivoFijo, string codigoSecuencial)
        {
            try
            {
                var listaCodigoActivoFijo = await apiServicio.Listar<CodigoActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/CodigoActivoFijo/ListarCodigosActivoFijo");
                return Json((idCodigoActivoFijo == 0 ? listaCodigoActivoFijo.Any(c => c.Codigosecuencial == codigoSecuencial.Trim()) : listaCodigoActivoFijo.Where(c => c.Codigosecuencial == codigoSecuencial.Trim()).Any(c => c.IdCodigoActivoFijo != idCodigoActivoFijo)));
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recepcion(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle, LibroActivoFijo libroActivoFijo) => await GestionRecepcionActivoFijoDetalle(recepcionActivoFijoDetalle, libroActivoFijo);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRecepcionAR(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle, LibroActivoFijo libroActivoFijo) => await GestionRecepcionActivoFijoDetalle(recepcionActivoFijoDetalle, libroActivoFijo);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRecepcionVT(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle, LibroActivoFijo libroActivoFijo) => await GestionRecepcionActivoFijoDetalle(recepcionActivoFijoDetalle, libroActivoFijo);

        public async Task<IActionResult> ObtenerRecepcionActivoFijo(string id, List<string> estados, string nombreVistaError)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.ObtenerElementoAsync<Response>(new IdEstadosTransfer { Id = int.Parse(id), Estados = estados }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ObtenerActivoFijoPorEstado");
                    if (respuesta.IsSuccess)
                    {
                        var activoFijo = JsonConvert.DeserializeObject<ActivoFijo>(respuesta.Resultado.ToString());
                        return View(activoFijo);
                    }
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nombreVistaError);
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nombreVistaError);
            }
        }

        public async Task<IActionResult> ActivosFijosRecepcionados()
        {
            var lista = new List<ActivoFijo>();
            ViewData["IsConfiguracionDetallesRecepcion"] = true;
            ViewData["Titulo"] = "Activos Fijos Recepcionados";
            ViewData["UrlEditar"] = nameof(EditarRecepcionAR);
            ViewData["EstadoFiltrado"] = Estados.Recepcionado;
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<ActivoFijo>>(new List<string> { Estados.Recepcionado }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivoFijoPorEstado");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos recepcionados", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoActivoFijo", lista);
        }

        public async Task<IActionResult> ActivosFijosValidacionTecnica()
        {
            var lista = new List<ActivoFijo>();
            ViewData["IsConfiguracionDetallesRecepcion"] = true;
            ViewData["Titulo"] = "Activos Fijos que requieren Validación Técnica";
            ViewData["UrlEditar"] = nameof(EditarRecepcionVT);
            ViewData["UrlRevision"] = nameof(RevisionActivoFijo);
            ViewData["EstadoFiltrado"] = Estados.ValidacionTecnica;
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<ActivoFijo>>(new List<string> { Estados.ValidacionTecnica }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivoFijoPorEstado");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos que requieren validación técnica", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoActivoFijo", lista);
        }

        public async Task<IActionResult> GestionarAprobacionActivoFijo(int id, bool id2)
        {
            try
            {
                if (id2)
                {
                    await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Recepcionado }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                    await apiServicio.InsertarAsync(new AprobacionActivoFijoTransfer { IdActivoFijo = id, NuevoEstadoActivoFijo = Estados.Recepcionado, ValidacionTecnica = false }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/AprobacionActivoFijo");
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ActivosFijosRecepcionados));
                }
                else
                {
                    await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Desaprobado }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                    await apiServicio.InsertarAsync(new AprobacionActivoFijoTransfer { IdActivoFijo = id, NuevoEstadoActivoFijo = Estados.Desaprobado, ValidacionTecnica = true }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/AprobacionActivoFijo");
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ActivosFijosValidacionTecnica));
                }
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando aprobación Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ActivosFijosValidacionTecnica));
            }
        }

        public async Task<IActionResult> RevisionActivoFijo(string id) => await ObtenerRecepcionActivoFijo(id, new List<string> { Estados.ValidacionTecnica }, nameof(ActivosFijosValidacionTecnica));

        [HttpPost]
        public async Task<IActionResult> ModalDatosEspecificosResult(RecepcionActivoFijoDetalle rafd, string categoria, int idSucursal, int? idBodega, int? idEmpleado)
        {
            ViewData["Categoria"] = categoria;
            ViewData["Empleado"] = await ObtenerSelectListEmpleado(idSucursal, idEmpleado);
            ViewData["Bodega"] = await ObtenerSelectListBodega(idSucursal, idBodega);
            ViewData["IdTipoUbicacion"] = idBodega != null ? 1 : idEmpleado != null ? 2 : 1;
            ViewData["TipoUbicacion"] = new SelectList(new[] { new { Id = 1, Valor = "Bodega" }, new { Id = 2, Valor = "Custodio" } }, "Id", "Valor", ViewData["IdTipoUbicacion"]);
            return PartialView("_ModalDatosEspecificos", rafd);
        }

        [HttpPost]
        public async Task<IActionResult> ValidacionDatosEspecificosResult(RecepcionActivoFijoDetalleDatosEspecificosViewModel rafd)
        {
            var listaPropiedadValorErrores = rafd.Validar();
            if (!ModelState.IsValid)
            {
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var value = ViewData.ModelState[modelStateKey];
                    foreach (var error in value.Errors)
                    {
                        listaPropiedadValorErrores.Add(new PropiedadValor
                        {
                            Propiedad = modelStateKey.Replace("rafd.", ""),
                            Valor = error.ErrorMessage
                        });
                    }
                }
                listaPropiedadValorErrores.RemoveAll(c => c.Propiedad == "IdRecepcionActivoFijoDetalle");
            }
            try
            {
                if (listaPropiedadValorErrores.Count == 0)
                {
                    var response = await apiServicio.InsertarAsync(new RecepcionActivoFijoDetalle { IdRecepcionActivoFijoDetalle = rafd.IdRecepcionActivoFijoDetalle, Serie = rafd.Serie, NumeroChasis = rafd.NumeroChasis, NumeroMotor = rafd.NumeroMotor, Placa = rafd.Placa, NumeroClaveCatastral = rafd.NumeroClaveCatastral }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ValidacionRecepcionActivoFijoDetalleDatosEspecificos");
                    if (response.IsSuccess)
                        listaPropiedadValorErrores = JsonConvert.DeserializeObject<List<PropiedadValor>>(response.Resultado.ToString());
                    else
                        listaPropiedadValorErrores.Add(new PropiedadValor { Propiedad = "IdBodega", Valor = "Ha ocurrido un error al intentar validar los datos." });
                }
            }
            catch (Exception)
            { }
            return Json(listaPropiedadValorErrores);
        }

        [HttpPost]
        public async Task<IActionResult> DetallesActivoFijoResult(List<IdRecepcionActivoFijoDetalleSeleccionado> listadoRecepcionActivoFijoDetalleSeleccionado, List<PropiedadValor> arrConfiguraciones)
        {
            var lista = new List<RecepcionActivoFijoDetalleSeleccionado>();
            ViewData["Configuraciones"] = arrConfiguraciones;
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(listadoRecepcionActivoFijoDetalleSeleccionado, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijo");
            }
            catch (Exception)
            { }
            return PartialView("_ListadoDetallesActivosFijos", lista);
        }

        [HttpPost]
        public async Task<IActionResult> ModalComponentesResult(RecepcionActivoFijoDetalleComponentes componentesActivo)
        {
            var lista = new List<RecepcionActivoFijoDetalleSeleccionado>();
            ViewData["Configuraciones"] = new List<PropiedadValor>() { new PropiedadValor { Propiedad = "IsConfiguracionListadoComponentes", Valor = "true" } };
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(ObtenerListaRecepcionActivoFijoDetalleSeleccionado(componentesActivo), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijo");
            }
            catch (Exception)
            { }
            return PartialView("_ListadoDetallesActivosFijos", lista);
        }

        [HttpPost]
        public async Task<IActionResult> ComponentesActivosFijosResult(RecepcionActivoFijoDetalleComponentes componentesActivo, List<int> idsComponentesExcluir)
        {
            var lista = new List<RecepcionActivoFijoDetalleSeleccionado>();
            ViewData["Configuraciones"] = new List<PropiedadValor>()
            {
                new PropiedadValor { Propiedad = "IsConfiguracionSeleccion", Valor = "true" },
                new PropiedadValor { Propiedad = "IsConfiguracionSeleccionComponentes", Valor = "true" },
                new PropiedadValor { Propiedad = "IsConfiguracionDatosActivo", Valor = "true" }
            };
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(new IdRecepcionActivoFijoDetalleSeleccionadoIdsComponentesExcluir
                {
                    ListaIdRecepcionActivoFijoDetalleSeleccionado = ObtenerListaRecepcionActivoFijoDetalleSeleccionado(componentesActivo),
                    IdsComponentesExcluir = idsComponentesExcluir
                }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarComponentesDisponiblesActivoFijo");
            }
            catch (Exception)
            { }
            return PartialView("_ListadoDetallesActivosFijos", lista);
        }

        private List<IdRecepcionActivoFijoDetalleSeleccionado> ObtenerListaRecepcionActivoFijoDetalleSeleccionado(RecepcionActivoFijoDetalleComponentes componentesActivo)
        {
            var listaIdRecepcionActivoFijoDetalleSeleccionado = new List<IdRecepcionActivoFijoDetalleSeleccionado>();
            try
            {
                componentesActivo.arrIdsComponentes.ForEach(c => listaIdRecepcionActivoFijoDetalleSeleccionado.Add(new IdRecepcionActivoFijoDetalleSeleccionado { idRecepcionActivoFijoDetalle = c, seleccionado = true }));
            }
            catch (Exception)
            { }
            return listaIdRecepcionActivoFijoDetalleSeleccionado;
        }

        [HttpPost]
        public async Task<IActionResult> CategoriaResult(int? idClaseActivoFijo)
        {
            try
            {
                if (idClaseActivoFijo == null)
                    return Json("");

                var categoria = await apiServicio.ObtenerElementoAsync<CategoriaActivoFijo>(idClaseActivoFijo, new Uri(WebApp.BaseAddressRM), "api/CategoriaActivoFijo/ObtenerCategoriaPorClaseActivoFijo");
                return Json(categoria?.Nombre ?? "");
            }
            catch (Exception)
            {
                return Json("");
            }
        }

        [HttpPost]
        public IActionResult CodificacionResult(string Codigosecuencial)
        {
            ViewData["IsSmartAdmin"] = true;
            return PartialView("_Codificacion", new RecepcionActivoFijoDetalle { CodigoActivoFijo = new CodigoActivoFijo { Codigosecuencial = Codigosecuencial } });
        }
        #endregion

        #region Póliza de Seguro
        public async Task<IActionResult> ActivosFijosRecepcionadosSinPoliza()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/ListarRecepcionActivoFijoSinPoliza");
                ViewData["Titulo"] = "Activos Fijos sin Póliza de Seguro";
                ViewData["textoColumna"] = "Asignar Póliza";
                ViewData["url"] = "AsignarPoliza";
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con estado Recepcionado sin número de póliza asignado", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoActivoFijo", lista);
        }

        public async Task<IActionResult> ActivosFijosRecepcionadosConPoliza()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/ListarRecepcionActivoFijoConPoliza");
                ViewData["Titulo"] = "Activos Fijos con Póliza de Seguro";
                ViewData["textoColumna"] = "Editar Póliza";
                ViewData["url"] = "AsignarPoliza";
                ViewData["poliza"] = true;
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con estado Recepcionado con número de póliza asignado", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoActivoFijo", lista);
        }

        public async Task<IActionResult> AsignarPoliza(string id) => await ObtenerRecepcionActivoFijo(id, new List<string> { Estados.Recepcionado }, nameof(ActivosFijosRecepcionadosSinPoliza));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarPoliza(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle)
        {
            try
            {
                var response = await apiServicio.InsertarAsync(recepcionActivoFijoDetalle, new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/AsignarPoliza");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha asignado el número de póliza", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Recepción Activo Fijo Detalle:", recepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ActivosFijosRecepcionadosConPoliza));
                }
                ViewData["Error"] = response.Message;
                return View(recepcionActivoFijoDetalle);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Asignando Póliza", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ActivosFijosRecepcionadosSinPoliza));
            }
        }
        #endregion

        #region Alta de Activos
        public async Task<IActionResult> ListadoActivosFijosAlta()
        {
            var lista = new List<ActivoFijo>();
            ViewData["IsConfiguracionListadoAltas"] = true;
            ViewData["Titulo"] = "Activos Fijos en Alta";
            ViewData["UrlEditar"] = nameof(GestionarAlta);
            ViewData["EstadoFiltrado"] = Estados.Alta;
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<ActivoFijo>>(Estados.Alta, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivosFijosPorAgrupacionPorEstado");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos en alta", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoActivoFijo", lista);
        }

        public async Task<IActionResult> ListadoAltaActivosFijos()
        {
            var lista = new List<AltaActivoFijo>();
            try
            {
                lista = await apiServicio.Listar<AltaActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarAltasActivosFijos");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando altas de activos fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> GestionarAlta(int? id)
        {
            try
            {
                ViewData["MotivoAlta"] = new SelectList(await apiServicio.Listar<MotivoAlta>(new Uri(WebApp.BaseAddressRM), "api/MotivoAlta/ListarMotivoAlta"), "IdMotivoAlta", "Descripcion");
                ViewData["TipoUtilizacionAlta"] = new SelectList(await apiServicio.Listar<TipoUtilizacionAlta>(new Uri(WebApp.BaseAddressRM), "api/TipoUtilizacionAlta/ListarTipoUtilizacionAlta"), "IdTipoUtilizacionAlta", "Nombre");
                ViewData["Configuraciones"] = new List<PropiedadValor>()
                {
                    new PropiedadValor { Propiedad = "IsConfiguracionListadoAltasGestionar", Valor = "true" },
                    new PropiedadValor { Propiedad = "IsConfiguracionAltasGestionarEditar", Valor = (id != null).ToString() },
                    new PropiedadValor { Propiedad = "IsConfiguracionDatosActivo", Valor = "true" }
                };
                if (id != null)
                {
                    var response = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ObtenerAltaActivosFijos");
                    if (!response.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoActivosFijosAlta));

                    var altaActivoFijo = JsonConvert.DeserializeObject<AltaActivoFijo>(response.Resultado.ToString());
                    ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = altaActivoFijo.RecepcionActivoFijoDetalleAltaActivoFijo.Select(c => new RecepcionActivoFijoDetalleSeleccionado
                    {
                        RecepcionActivoFijoDetalle = c.RecepcionActivoFijoDetalle,
                        Seleccionado = true
                    }).ToList();
                    return View(altaActivoFijo);
                }
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = new List<RecepcionActivoFijoDetalleSeleccionado>();
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoActivosFijosAlta));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GestionarAlta(AltaActivoFijo altaActivoFijo, IFormFile fileFactura, List<IFormFile> file)
        {
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Alta }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                var listaFormDatosEspecificos = Request.Form.Where(c => c.Key.StartsWith("hIdRecepcionActivoFijoDetalle_"));
                int idTipoUtilizacionAlta = int.Parse(Request.Form["IdTipoUtilizacionAlta"].ToString());
                altaActivoFijo.RecepcionActivoFijoDetalleAltaActivoFijo = new List<RecepcionActivoFijoDetalleAltaActivoFijo>();
                foreach (var item in listaFormDatosEspecificos)
                {
                    int posFormDatoEspecifico = int.Parse(item.Key.ToString().Split('_')[1]);
                    int idEmpleado = int.Parse(Request.Form[$"hEmpleado_{posFormDatoEspecifico}"]);
                    int idSucursal = int.Parse(Request.Form[$"hSucursal_{posFormDatoEspecifico}"]);
                    int idLibroActivoFijo = int.Parse(Request.Form[$"hLibroActivoFijo_{posFormDatoEspecifico}"]);
                    int idRecepcionActivoFijoDetalle = int.Parse(Request.Form[$"hIdRecepcionActivoFijoDetalle_{posFormDatoEspecifico}"]);
                    string[] arrComponentes = Request.Form[$"hComponentes_{posFormDatoEspecifico}"].ToString().Trim().Split(',');

                    var rafd = new RecepcionActivoFijoDetalle { IdRecepcionActivoFijoDetalle = idRecepcionActivoFijoDetalle, UbicacionActivoFijoActual = new UbicacionActivoFijo { IdEmpleado = idEmpleado, IdLibroActivoFijo = idLibroActivoFijo, LibroActivoFijo = new LibroActivoFijo { IdSucursal = idSucursal } } };
                    foreach (var comp in arrComponentes)
                    {
                        if (!String.IsNullOrEmpty(comp))
                            rafd.ComponentesActivoFijoOrigen.Add(new ComponenteActivoFijo { IdRecepcionActivoFijoDetalleOrigen = rafd.IdRecepcionActivoFijoDetalle, IdRecepcionActivoFijoDetalleComponente = int.Parse(comp.Trim()), FechaAdicion = altaActivoFijo.FechaAlta });
                    }
                    altaActivoFijo.RecepcionActivoFijoDetalleAltaActivoFijo.Add(new RecepcionActivoFijoDetalleAltaActivoFijo {
                        IdRecepcionActivoFijoDetalle = rafd.IdRecepcionActivoFijoDetalle,
                        IdAltaActivoFijo = altaActivoFijo.IdAltaActivoFijo,
                        IdTipoUtilizacionAlta = idTipoUtilizacionAlta,
                        RecepcionActivoFijoDetalle = rafd
                    });
                }

                var response = new Response();
                int idAltaActivoFijo = 0;
                int? idFacturaActivoFijo = 0;
                if (altaActivoFijo.IdAltaActivoFijo == 0)
                {
                    response = await apiServicio.InsertarAsync(altaActivoFijo, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/InsertarAltaActivoFijo");
                    if (response.IsSuccess)
                    {
                        var altaActivoFijoAux = JsonConvert.DeserializeObject<AltaActivoFijo>(response.Resultado.ToString());
                        idAltaActivoFijo = altaActivoFijoAux.IdAltaActivoFijo;
                        idFacturaActivoFijo = altaActivoFijo.IdFacturaActivoFijo;
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha realizado un alta de activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Alta de Activo Fijo:", altaActivoFijo.IdAltaActivoFijo) });
                    }
                }
                else
                {
                    idAltaActivoFijo = altaActivoFijo.IdAltaActivoFijo;
                    idFacturaActivoFijo = altaActivoFijo.IdFacturaActivoFijo;
                    response = await apiServicio.EditarAsync(altaActivoFijo.IdAltaActivoFijo.ToString(), altaActivoFijo, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/EditarAltaActivoFijo");
                    if (response.IsSuccess)
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha editado un alta de activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Alta de Activo Fijo:", altaActivoFijo.IdAltaActivoFijo) });
                }

                var responseFile = new Response { IsSuccess = true };
                if (response.IsSuccess)
                {
                    if (file.Count > 0)
                    {
                        foreach (var item in file)
                        {
                            byte[] data;
                            using (var br = new BinaryReader(item.OpenReadStream()))
                                data = br.ReadBytes((int)item.OpenReadStream().Length);

                            if (data.Length > 0)
                            {
                                var activoFijoDocumentoTransfer = new DocumentoActivoFijoTransfer { Nombre = item.FileName, Fichero = data, IdAltaActivoFijo = idAltaActivoFijo };
                                responseFile = await apiServicio.InsertarAsync(activoFijoDocumentoTransfer, new Uri(WebApp.BaseAddressRM), "api/DocumentoActivoFijo/UploadFiles");
                                if (responseFile != null && responseFile.IsSuccess)
                                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha subido un archivo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Documento de Alta de Activo Fijo:", activoFijoDocumentoTransfer.Nombre) });
                            }
                        }
                    }

                    if (fileFactura != null)
                    {
                        byte[] data;
                        using (var br = new BinaryReader(fileFactura.OpenReadStream()))
                            data = br.ReadBytes((int)fileFactura.OpenReadStream().Length);

                        if (data.Length > 0)
                        {
                            var activoFijoDocumentoTransfer = new DocumentoActivoFijoTransfer { Nombre = fileFactura.FileName, Fichero = data, IdFacturaActivoFijo = idFacturaActivoFijo };
                            responseFile = await apiServicio.InsertarAsync(activoFijoDocumentoTransfer, new Uri(WebApp.BaseAddressRM), "api/DocumentoActivoFijo/UploadFiles");
                            if (responseFile != null && responseFile.IsSuccess)
                                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha subido un archivo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Documento de Factura de Activo Fijo:", activoFijoDocumentoTransfer.Nombre) });
                        }
                    }
                }

                if (response.IsSuccess)
                    return this.Redireccionar(responseFile.IsSuccess ? $"{Mensaje.Informacion}|{Mensaje.Satisfactorio}" : $"{Mensaje.Aviso}|{Mensaje.ErrorUploadFiles}", nameof(ListadoActivosFijosAlta));

                ViewData["MotivoAlta"] = new SelectList(await apiServicio.Listar<MotivoAlta>(new Uri(WebApp.BaseAddressRM), "api/MotivoAlta/ListarMotivoAlta"), "IdMotivoAlta", "Descripcion");
                ViewData["TipoUtilizacionAlta"] = new SelectList(await apiServicio.Listar<TipoUtilizacionAlta>(new Uri(WebApp.BaseAddressRM), "api/TipoUtilizacionAlta/ListarTipoUtilizacionAlta"), "IdTipoUtilizacionAlta", "Nombre");
                ViewData["Configuraciones"] = new List<PropiedadValor>()
                {
                    new PropiedadValor { Propiedad = "IsConfiguracionListadoAltasGestionar", Valor = "true" },
                    new PropiedadValor { Propiedad = "IsConfiguracionAltasGestionarEditar", Valor = (altaActivoFijo.IdAltaActivoFijo > 0).ToString() },
                    new PropiedadValor { Propiedad = "IsConfiguracionDatosActivo", Valor = "true" }
                };
                var listaRecepcionActivoFijoDetalleSeleccionado = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(altaActivoFijo.RecepcionActivoFijoDetalleAltaActivoFijo.Select(c=> new IdRecepcionActivoFijoDetalleSeleccionado { idRecepcionActivoFijoDetalle = c.IdRecepcionActivoFijoDetalle, seleccionado = true }), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijo");
                foreach (var item in listaRecepcionActivoFijoDetalleSeleccionado)
                {
                    var rafdAltaActivoFijoActual = altaActivoFijo.RecepcionActivoFijoDetalleAltaActivoFijo.FirstOrDefault(x => x.IdRecepcionActivoFijoDetalle == item.RecepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle).RecepcionActivoFijoDetalle;
                    item.RecepcionActivoFijoDetalle.ComponentesActivoFijoOrigen = rafdAltaActivoFijoActual.ComponentesActivoFijoOrigen;
                    item.RecepcionActivoFijoDetalle.UbicacionActivoFijoActual.IdEmpleado = rafdAltaActivoFijoActual.UbicacionActivoFijoActual.IdEmpleado;
                    item.RecepcionActivoFijoDetalle.UbicacionActivoFijoActual.Empleado = JsonConvert.DeserializeObject<Empleado>((await apiServicio.SeleccionarAsync<Response>(rafdAltaActivoFijoActual.UbicacionActivoFijoActual.IdEmpleado.ToString(), new Uri(WebApp.BaseAddressTH), "api/Empleados")).Resultado.ToString());
                }
                ViewData["Error"] = response.Message;
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = listaRecepcionActivoFijoDetalleSeleccionado;
                return View(altaActivoFijo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando alta de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListadoActivosFijosAlta));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ListadoActivosFijosSeleccionAltaResult(List<IdRecepcionActivoFijoDetalleSeleccionado> listadoRecepcionActivoFijoDetalleSeleccionado, List<IdRecepcionActivoFijoDetalleSeleccionado> objAdicional)
        {
            var lista = new List<RecepcionActivoFijoDetalleSeleccionado>();
            ViewData["Configuraciones"] = new List<PropiedadValor>()
            {
                new PropiedadValor { Propiedad = "IsConfiguracionSeleccion", Valor = "true" },
                new PropiedadValor { Propiedad = "IsConfiguracionDatosActivo", Valor = "true" },
                new PropiedadValor { Propiedad = "IsConfiguracionSeleccionAltas", Valor = "true" }
            };
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(new IdRecepcionActivoFijoDetalleSeleccionadoIdsInicialesAltaBaja
                {
                    ListaIdRecepcionActivoFijoDetalleSeleccionado = listadoRecepcionActivoFijoDetalleSeleccionado,
                    ListaIdRecepcionActivoFijoDetalleSeleccionadoInicialesAltaBaja = objAdicional
                }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijoSeleccionadoPorEstadoAlta");
            }
            catch (Exception)
            { }
            return PartialView("_ListadoDetallesActivosFijos", lista);
        }

        [HttpPost]
        public async Task<IActionResult> ModalEmpleadosResult(int idSucursal, int? idEmpleado, int idRecepcionActivoFijoDetalle)
        {
            ViewData["Empleado"] = await ObtenerSelectListEmpleado(idSucursal, idEmpleado);
            ViewData["IdRecepcionActivoFijoDetalle"] = idRecepcionActivoFijoDetalle;
            return PartialView("_EmpleadoModalResult");
        }

        [HttpPost]
        public async Task<IActionResult> DetalleFacturaAltaActivosResult(int? idFacturaActivoFijo)
        {
            var altaActivoFijo = new AltaActivoFijo();
            if (idFacturaActivoFijo != null)
            {
                var response = await apiServicio.SeleccionarAsync<Response>(idFacturaActivoFijo.ToString(), new Uri(WebApp.BaseAddressRM), "api/FacturaActivoFijo");
                if (response.IsSuccess)
                {
                    var facturaActivoFijo = JsonConvert.DeserializeObject<FacturaActivoFijo>(response.Resultado.ToString());
                    altaActivoFijo.FacturaActivoFijo = facturaActivoFijo;
                }
            }
            return PartialView("_DetalleFacturaAltaActivos", altaActivoFijo);
        }
        #endregion

        #region Transferencias
        public async Task<IActionResult> ActivosFijosATransferir()
        {
            var lista = new List<RecepcionActivoFijoDetalle>();
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/ListarRecepcionActivoFijo");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con estado Recepcionado", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> TransferirActivoFijo(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ActivosFijosATransferir));

                    var recepcionActivoFijoDetalle = JsonConvert.DeserializeObject<RecepcionActivoFijoDetalle>(respuesta.Resultado.ToString());
                    ViewData["MotivoTransferencia"] = new SelectList(await apiServicio.Listar<MotivoTransferencia>(new Uri(WebApp.BaseAddressRM), "api/MotivoTransferencia/ListarMotivoTransferencia"), "IdMotivoTransferencia", "Motivo_Transferencia");

                    var listaPais = await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "api/Pais/ListarPais");
                    ViewData["Pais"] = new SelectList(listaPais, "IdPais", "Nombre");
                    ViewData["Provincia"] = await ObtenerSelectListProvincia((ViewData["Pais"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Pais"] as SelectList).FirstOrDefault().Value) : -1);
                    ViewData["Ciudad"] = await ObtenerSelectListCiudad((ViewData["Provincia"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Provincia"] as SelectList).FirstOrDefault().Value) : -1);
                    ViewData["Sucursal"] = await ObtenerSelectListSucursal((ViewData["Ciudad"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Ciudad"] as SelectList).FirstOrDefault().Value) : -1);
                    ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo((ViewData["Sucursal"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Sucursal"] as SelectList).FirstOrDefault().Value) : -1);

                    //ViewData["PaisOrigen"] = new SelectList(listaPais, "IdPais", "Nombre", recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.IdPais ?? -1);
                    //ViewData["ProvinciaOrigen"] = await ObtenerSelectListProvincia(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.IdPais ?? -1);
                    //ViewData["CiudadOrigen"] = await ObtenerSelectListCiudad(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.IdProvincia ?? -1);
                    //ViewData["SucursalOrigen"] = await ObtenerSelectListSucursal(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.Sucursal?.IdCiudad ?? -1);
                    //ViewData["LibroActivoFijoOrigen"] = await ObtenerSelectListLibroActivoFijo(recepcionActivoFijoDetalle?.ActivoFijo?.LibroActivoFijo?.IdSucursal ?? -1);

                    ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                    ViewData["IdActivoFijo"] = recepcionActivoFijoDetalle.IdActivoFijo;
                    return View();
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ActivosFijosATransferir));
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando Transferencia de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ActivosFijosATransferir));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferirActivoFijo(int IdActivoFijo, TransferenciaActivoFijo transferenciaActivoFijo)
        {
            try
            {
                var activoFijoOrigen = JsonConvert.DeserializeObject<ActivoFijo>((await apiServicio.SeleccionarAsync<Response>(IdActivoFijo.ToString(), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos")).Resultado.ToString());
                //transferenciaActivoFijoDetalle.TransferenciaActivoFijo.Origen = activoFijoOrigen?.LibroActivoFijo?.Sucursal?.Nombre ?? activoFijoOrigen?.Ciudad?.Nombre;

                //var libroActivoFijoDestino = JsonConvert.DeserializeObject<LibroActivoFijo>((await apiServicio.SeleccionarAsync<Response>(transferenciaActivoFijoDetalle.ActivoFijo.IdLibroActivoFijo.ToString(), new Uri(WebApp.BaseAddressRM), "api/LibroActivoFijo")).Resultado.ToString());
                //transferenciaActivoFijoDetalle.TransferenciaActivoFijo.Destino = libroActivoFijoDestino.Sucursal.Nombre;
                
                //int idPaisDestino = transferenciaActivoFijoDetalle.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.IdPais ?? -1;
                //int idProvinciaDestino = transferenciaActivoFijoDetalle.ActivoFijo?.LibroActivoFijo?.Sucursal?.Ciudad?.IdProvincia ?? -1;
                //int idCiudadDestino = transferenciaActivoFijoDetalle.ActivoFijo?.LibroActivoFijo?.Sucursal?.IdCiudad ?? -1;
                //int idSucursalDestino = transferenciaActivoFijoDetalle.ActivoFijo?.LibroActivoFijo?.IdSucursal ?? -1;

                //transferenciaActivoFijo.ActivoFijo.IdActivoFijo = IdActivoFijo;
                //transferenciaActivoFijoDetalle.ActivoFijo.LibroActivoFijo = libroActivoFijoDestino;
                var response = await apiServicio.InsertarAsync(transferenciaActivoFijo, new Uri(WebApp.BaseAddressRM), "api/TransferenciaActivoFijoDetalle/InsertarTransferenciaActivoFijoDetalle");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado una transferencia de Activo Fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Detalle de Transferencia:", transferenciaActivoFijo.IdTransferenciaActivoFijo) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ActivosFijosATransferir));
                }
                ViewData["Error"] = response.Message;
                ViewData["MotivoTransferencia"] = new SelectList(await apiServicio.Listar<MotivoTransferencia>(new Uri(WebApp.BaseAddressRM), "api/MotivoTransferencia/ListarMotivoTransferencia"), "IdMotivoTransferencia", "Motivo_Transferencia");

                var listaPais = await apiServicio.Listar<Pais>(new Uri(WebApp.BaseAddressTH), "api/Pais/ListarPais");
                //ViewData["Pais"] = new SelectList(listaPais, "IdPais", "Nombre", idPaisDestino);
                //ViewData["Provincia"] = await ObtenerSelectListProvincia(idPaisDestino);
                //ViewData["Ciudad"] = await ObtenerSelectListCiudad(idProvinciaDestino);
                //ViewData["Sucursal"] = await ObtenerSelectListSucursal(idCiudadDestino);
                //ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo(idSucursalDestino);

                //ViewData["PaisOrigen"] = new SelectList(listaPais, "IdPais", "Nombre", activoFijoOrigen?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.IdPais ?? -1);
                //ViewData["ProvinciaOrigen"] = await ObtenerSelectListProvincia(activoFijoOrigen?.LibroActivoFijo?.Sucursal?.Ciudad?.Provincia?.IdPais ?? -1);
                //ViewData["CiudadOrigen"] = await ObtenerSelectListCiudad(activoFijoOrigen?.LibroActivoFijo?.Sucursal?.Ciudad?.IdProvincia ?? -1);
                //ViewData["SucursalOrigen"] = await ObtenerSelectListSucursal(activoFijoOrigen?.LibroActivoFijo?.Sucursal?.IdCiudad ?? -1);
                //ViewData["LibroActivoFijoOrigen"] = await ObtenerSelectListLibroActivoFijo(activoFijoOrigen?.LibroActivoFijo?.IdSucursal ?? -1);

                ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                ViewData["IdActivoFijo"] = IdActivoFijo;
                return View(transferenciaActivoFijo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando un Modelo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ActivosFijosATransferir));
            }
        }
        #endregion

        #region Baja de Activos
        public async Task<IActionResult> ListadoActivosFijosBaja()
        {
            var lista = new List<ActivoFijo>();
            ViewData["IsConfiguracionListadoBajas"] = true;
            ViewData["Titulo"] = "Activos Fijos en Baja";
            ViewData["UrlEditar"] = nameof(GestionarBaja);
            ViewData["EstadoFiltrado"] = Estados.Baja;
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<ActivoFijo>>(Estados.Baja, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivosFijosPorAgrupacionPorEstado");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos en baja", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoActivoFijo", lista);
        }

        public async Task<IActionResult> ListadoBajaActivosFijos()
        {
            var lista = new List<BajaActivoFijo>();
            try
            {
                lista = await apiServicio.Listar<BajaActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarBajasActivosFijos");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando bajas de activos fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> GestionarBaja(int? id)
        {
            try
            {
                ViewData["MotivoBaja"] = new SelectList(await apiServicio.Listar<MotivoBaja>(new Uri(WebApp.BaseAddressRM), "api/MotivoBaja/ListarMotivoBaja"), "IdMotivoBaja", "Nombre");
                ViewData["Configuraciones"] = new List<PropiedadValor>()
                {
                    new PropiedadValor { Propiedad = "IsConfiguracionListadoBajasGestionar", Valor = "true" },
                    new PropiedadValor { Propiedad = "IsConfiguracionBajasGestionarEditar", Valor = (id != null).ToString() },
                    new PropiedadValor { Propiedad = "IsConfiguracionDatosActivo", Valor = "true" }
                };
                if (id != null)
                {
                    var response = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ObtenerBajaActivosFijos");
                    if (!response.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoActivosFijosBaja));

                    var bajaActivoFijo = JsonConvert.DeserializeObject<BajaActivoFijo>(response.Resultado.ToString());
                    ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = bajaActivoFijo.RecepcionActivoFijoDetalleBajaActivoFijo.Select(c => new RecepcionActivoFijoDetalleSeleccionado
                    {
                        RecepcionActivoFijoDetalle = c.RecepcionActivoFijoDetalle,
                        Seleccionado = true
                    }).ToList();
                    return View(bajaActivoFijo);
                }
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = new List<RecepcionActivoFijoDetalleSeleccionado>();
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoActivosFijosBaja));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GestionarBaja(BajaActivoFijo bajaActivoFijo)
        {
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Baja }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                bajaActivoFijo.RecepcionActivoFijoDetalleBajaActivoFijo = new List<RecepcionActivoFijoDetalleBajaActivoFijo>();
                var listaFormDatosEspecificos = Request.Form.Where(c => c.Key.StartsWith("hIdRecepcionActivoFijoDetalle_"));
                foreach (var item in listaFormDatosEspecificos)
                {
                    int posFormDatoEspecifico = int.Parse(item.Key.ToString().Split('_')[1]);
                    int idEmpleado = int.Parse(Request.Form[$"hEmpleado_{posFormDatoEspecifico}"]);
                    int idRecepcionActivoFijoDetalle = int.Parse(Request.Form[$"hIdRecepcionActivoFijoDetalle_{posFormDatoEspecifico}"]);

                    var rafd = new RecepcionActivoFijoDetalle { IdRecepcionActivoFijoDetalle = idRecepcionActivoFijoDetalle, UbicacionActivoFijoActual = new UbicacionActivoFijo { IdEmpleado = idEmpleado } };
                    bajaActivoFijo.RecepcionActivoFijoDetalleBajaActivoFijo.Add(new RecepcionActivoFijoDetalleBajaActivoFijo
                    {
                        IdRecepcionActivoFijoDetalle = rafd.IdRecepcionActivoFijoDetalle,
                        IdBajaActivoFijo = bajaActivoFijo.IdBajaActivoFijo,
                        RecepcionActivoFijoDetalle = rafd
                    });
                }
                var response = new Response();
                if (bajaActivoFijo.IdBajaActivoFijo == 0)
                {
                    response = await apiServicio.InsertarAsync(bajaActivoFijo, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/InsertarBajaActivoFijo");
                    if (response.IsSuccess)
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha realizado una baja de activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Baja de Activo Fijo:", bajaActivoFijo.IdBajaActivoFijo) });
                }
                else
                {
                    response = await apiServicio.EditarAsync(bajaActivoFijo.IdBajaActivoFijo.ToString(), bajaActivoFijo, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/EditarBajaActivoFijo");
                    if (response.IsSuccess)
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha editado una baja de activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Baja de Activo Fijo:", bajaActivoFijo.IdBajaActivoFijo) });
                }

                if (response.IsSuccess)
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ListadoActivosFijosBaja));

                ViewData["MotivoBaja"] = new SelectList(await apiServicio.Listar<MotivoBaja>(new Uri(WebApp.BaseAddressRM), "api/MotivoBaja/ListarMotivoBaja"), "IdMotivoBaja", "Nombre");
                ViewData["Configuraciones"] = new List<PropiedadValor>()
                {
                    new PropiedadValor { Propiedad = "IsConfiguracionListadoBajasGestionar", Valor = "true" },
                    new PropiedadValor { Propiedad = "IsConfiguracionBajasGestionarEditar", Valor = (bajaActivoFijo.IdBajaActivoFijo > 0).ToString() },
                    new PropiedadValor { Propiedad = "IsConfiguracionDatosActivo", Valor = "true" }
                };
                var listaRecepcionActivoFijoDetalleSeleccionado = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(bajaActivoFijo.RecepcionActivoFijoDetalleBajaActivoFijo.Select(c => new IdRecepcionActivoFijoDetalleSeleccionado { idRecepcionActivoFijoDetalle = c.IdRecepcionActivoFijoDetalle, seleccionado = true }), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijo");
                foreach (var item in listaRecepcionActivoFijoDetalleSeleccionado)
                {
                    var rafdBajaActivoFijoActual = bajaActivoFijo.RecepcionActivoFijoDetalleBajaActivoFijo.FirstOrDefault(x => x.IdRecepcionActivoFijoDetalle == item.RecepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle).RecepcionActivoFijoDetalle;
                    item.RecepcionActivoFijoDetalle.UbicacionActivoFijoActual.IdEmpleado = rafdBajaActivoFijoActual.UbicacionActivoFijoActual.IdEmpleado;
                    item.RecepcionActivoFijoDetalle.UbicacionActivoFijoActual.Empleado = JsonConvert.DeserializeObject<Empleado>((await apiServicio.SeleccionarAsync<Response>(rafdBajaActivoFijoActual.UbicacionActivoFijoActual.IdEmpleado.ToString(), new Uri(WebApp.BaseAddressTH), "api/Empleados")).Resultado.ToString());
                }
                ViewData["Error"] = response.Message;
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = listaRecepcionActivoFijoDetalleSeleccionado;
                return View(bajaActivoFijo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando baja de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListadoActivosFijosBaja));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ListadoActivosFijosSeleccionBajaResult(List<IdRecepcionActivoFijoDetalleSeleccionado> listadoRecepcionActivoFijoDetalleSeleccionado, List<IdRecepcionActivoFijoDetalleSeleccionado> objAdicional)
        {
            var lista = new List<RecepcionActivoFijoDetalleSeleccionado>();
            ViewData["Configuraciones"] = new List<PropiedadValor>()
            {
                new PropiedadValor { Propiedad = "IsConfiguracionSeleccion", Valor = "true" },
                new PropiedadValor { Propiedad = "IsConfiguracionDatosActivo", Valor = "true" },
                new PropiedadValor { Propiedad = "IsConfiguracionSeleccionBajas", Valor = "true" }
            };
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(new IdRecepcionActivoFijoDetalleSeleccionadoIdsInicialesAltaBaja
                {
                    ListaIdRecepcionActivoFijoDetalleSeleccionado = listadoRecepcionActivoFijoDetalleSeleccionado,
                    ListaIdRecepcionActivoFijoDetalleSeleccionadoInicialesAltaBaja = objAdicional
                }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijoSeleccionadoPorEstadoBaja");
            }
            catch (Exception)
            { }
            return PartialView("_ListadoDetallesActivosFijos", lista);
        }
        #endregion

        #region Mantenimiento de Activos
        public async Task<IActionResult> ListarMantenimientosActivosFijos()
        {
            var lista = new List<ActivoFijo>();
            try
            {
                ViewData["IsConfiguracionListadoMantenimientos"] = true;
                ViewData["Titulo"] = "Activos Fijos en Alta";
                ViewData["UrlEditar"] = nameof(EditarMantenimiento);
                ViewData["EstadoFiltrado"] = Estados.Mantenimiento;
                lista = await apiServicio.ObtenerElementoAsync<List<ActivoFijo>>(Estados.Alta, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivosFijosPorAgrupacionPorEstado");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando Mantenimientos de Activos Fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoActivoFijo", lista);
        }

        public async Task<IActionResult> ListarMantenimientos(string id)
        {
            var lista = new List<MantenimientoActivoFijo>();
            ViewData["IdRecepcionActivoFijoDetalle"] = id;
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<MantenimientoActivoFijo>>(id, new Uri(WebApp.BaseAddressRM), "api/MantenimientoActivoFijo/ListarMantenimientosActivoFijoPorIdDetalleActivoFijo");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando Mantenimientos de Activos Fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> CrearMantenimiento(int id)
        {
            try
            {
                ViewData["IdRecepcionActivoFijoDetalle"] = id;
                ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListarMantenimientos), routeValues: new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearMantenimiento(MantenimientoActivoFijo mantenimientoActivoFijo)
        {
            try
            {
                var response = new Response();
                if (ModelState.IsValid)
                {
                    response = await apiServicio.InsertarAsync(mantenimientoActivoFijo, new Uri(WebApp.BaseAddressRM), "api/MantenimientoActivoFijo/InsertarMantenimientoActivoFijo");
                    if (response.IsSuccess)
                    {
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado un Mantenimiento Activo Fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Mantenimiento Activo Fijo:", mantenimientoActivoFijo.IdMantenimientoActivoFijo) });
                        return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ListarMantenimientos), routeValues: new { id = mantenimientoActivoFijo.IdRecepcionActivoFijoDetalle });
                    }
                }
                else
                    response.Message = Mensaje.ModeloInvalido;

                ViewData["Error"] = response.Message;
                ViewData["IdRecepcionActivoFijoDetalle"] = mantenimientoActivoFijo.IdRecepcionActivoFijoDetalle;
                ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                return View(response.Resultado);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando Mantenimiento Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppTh" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListarMantenimientos), routeValues: new { id = mantenimientoActivoFijo.IdRecepcionActivoFijoDetalle });
            }
        }

        public async Task<IActionResult> EditarMantenimiento(string id, string id2)
        {
            try
            {
                if (!string.IsNullOrEmpty(id2))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id2, new Uri(WebApp.BaseAddressRM), "api/MantenimientoActivoFijo");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListarMantenimientos), routeValues: new { id });

                    respuesta.Resultado = JsonConvert.DeserializeObject<MantenimientoActivoFijo>(respuesta.Resultado.ToString());
                    ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                    return View(respuesta.Resultado);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListarMantenimientos), routeValues: new { id });
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListarMantenimientos), routeValues: new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarMantenimiento(MantenimientoActivoFijo mantenimientoActivoFijo)
        {
            try
            {
                if (!string.IsNullOrEmpty(mantenimientoActivoFijo.IdMantenimientoActivoFijo.ToString()))
                {
                    var response = new Response();
                    if (ModelState.IsValid)
                    {
                        response = await apiServicio.EditarAsync(mantenimientoActivoFijo.IdMantenimientoActivoFijo.ToString(), mantenimientoActivoFijo, new Uri(WebApp.BaseAddressRM), "api/MantenimientoActivoFijo");
                        if (response.IsSuccess)
                        {
                            await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Mantenimiento", mantenimientoActivoFijo.IdMantenimientoActivoFijo.ToString()), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha actualizado un registro Mantenimiento Activo Fijo", UserName = "Usuario 1" });
                            return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ListarMantenimientos), routeValues: new { id = mantenimientoActivoFijo.IdRecepcionActivoFijoDetalle });
                        }
                    }
                    else
                        response.Message = Mensaje.ModeloInvalido;

                    ViewData["Error"] = response.Message;
                    ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                    return View(mantenimientoActivoFijo);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListarMantenimientos), routeValues: new { id = mantenimientoActivoFijo.IdRecepcionActivoFijoDetalle });
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Editando un Mantenimiento Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorEditar}", nameof(ListarMantenimientos), routeValues: new { id = mantenimientoActivoFijo.IdRecepcionActivoFijoDetalle });
            }
        }

        public async Task<IActionResult> EliminarMantenimiento(string id, string id2)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id2, new Uri(WebApp.BaseAddressRM), "api/MantenimientoActivoFijo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Mantenimiento Activo Fijo", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webappth" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}", nameof(ListarMantenimientos), routeValues: new { id });
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}", nameof(ListarMantenimientos), routeValues: new { id });
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar Mantenimiento Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}", nameof(ListarMantenimientos), routeValues: new { id });
            }
        }
        #endregion

        #region Reportes
        public async Task<IActionResult> HojaVidaActivoFijo(string id) => await ObtenerRecepcionActivoFijo(id, null, nameof(HojaVidaReporte));

        public async Task<IActionResult> HojaVidaReporte()
        {
            try
            {
                var lista = await apiServicio.ObtenerElementoAsync<List<ActivoFijo>>(new List<string> { Estados.Alta }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivoFijoPorEstado");
                ViewData["Titulo"] = "Activos Fijos";
                ViewData["textoColumna"] = "Ver Hoja de Vida";
                ViewData["url"] = "HojaVidaActivoFijo";
                return View("ListadoActivoFijo", lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con estado Recepcionado", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> BienesReporte()
        {
            try
            {
                var lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/BienesReporte");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos por área usuaria y clasificado por funcionario", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> MantenimientosReporte()
        {
            try
            {
                var lista = await apiServicio.Listar<MantenimientoActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/MantenimientoActivoFijo/ListarMantenimientosActivoFijo");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando mantenimientos de activos fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }

        public async Task<IActionResult> PolizasReporte()
        {
            try
            {
                var lista = await apiServicio.Listar<RecepcionActivoFijoDetalle>(new Uri(WebApp.BaseAddressRM), "api/RecepcionActivoFijo/ListarRecepcionActivoFijoConPoliza");
                return View(lista);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con estado Recepcionado con número de póliza asignado", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webappth" });
                return BadRequest();
            }
        }
        #endregion

        #region AJAX_ClaseActivoFijo
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
            return PartialView("_ClaseActivoFijoSelect", new RecepcionActivoFijoDetalle());
        }
        #endregion

        #region AJAX_SubClaseActivoFijo
        public async Task<SelectList> ObtenerSelectListSubClaseActivoFijo(int idClaseActivoFijo)
        {
            try
            {
                var listaSubClaseActivoFijo = idClaseActivoFijo != -1 ? await apiServicio.Listar<SubClaseActivoFijo>(new Uri(WebApp.BaseAddressRM), $"api/SubClaseActivoFijo/ListarSubClasesActivoFijoPorClase/{idClaseActivoFijo}") : new List<SubClaseActivoFijo>();
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
            ViewBag.Modelo = await ObtenerSelectListModelo(idMarca);
            return PartialView("_ModeloSelect", new RecepcionActivoFijoDetalle());
        }
        #endregion

        #region AJAX_Provincia
        public async Task<SelectList> ObtenerSelectListProvincia(int idPais)
        {
            try
            {
                var listaProvincia = idPais != -1 ? (await apiServicio.Listar<Provincia>(new Uri(WebApp.BaseAddressTH), "api/Provincia/ListarProvincia")).Where(c => c.IdPais == idPais).ToList() : new List<Provincia>();
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
            return PartialView("_ProvinciaSelect", new UbicacionActivoFijo());
        }
        #endregion

        #region AJAX_Ciudad
        public async Task<SelectList> ObtenerSelectListCiudad(int idProvincia)
        {
            try
            {
                var listaCiudad = idProvincia != -1 ? (await apiServicio.Listar<Ciudad>(new Uri(WebApp.BaseAddressTH), "api/Ciudad/ListarCiudad")).Where(c => c.IdProvincia == idProvincia).ToList() : new List<Ciudad>();
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
            return PartialView("_CiudadSelect", new UbicacionActivoFijo());
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
            return PartialView("_SucursalSelect", new UbicacionActivoFijo());
        }
        #endregion

        #region AJAX_LibroActivoFijo
        public async Task<SelectList> ObtenerSelectListLibroActivoFijo(int idSucursal)
        {
            try
            {
                var listaLibroActivoFijo = idSucursal != -1 ? await apiServicio.Listar<LibroActivoFijo>(new Uri(WebApp.BaseAddressRM), $"api/LibroActivoFijo/ListarLibrosActivoFijoPorSucursal/{idSucursal}") : new List<LibroActivoFijo>();
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
            return PartialView("_LibroActivoFijoSelect", new UbicacionActivoFijo());
        }
        #endregion

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
                return new SelectList(new List<ClaseActivoFijo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Subramo_SelectResult(int idRamo)
        {
            ViewBag.Subramo = await ObtenerSelectListSubramo(idRamo);
            return PartialView("_SubramoSelect", new RecepcionActivoFijoDetalle());
        }
        #endregion

        #region AJAX_Bodega
        public async Task<SelectList> ObtenerSelectListBodega(int idSucursal, int? idBodega = null)
        {
            try
            {
                var listaBodega = idSucursal != -1 ? await apiServicio.Listar<Bodega>(new Uri(WebApp.BaseAddressRM), $"api/Bodega/ListarBodegaPorSucursal/{idSucursal}") : new List<Bodega>();
                return new SelectList(listaBodega, "IdBodega", "Nombre", idBodega);
            }
            catch (Exception)
            {
                return new SelectList(new List<Bodega>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Bodega_SelectResult(int idSucursal)
        {
            ViewBag.Bodega = await ObtenerSelectListBodega(idSucursal);
            return PartialView("_BodegaSelect", new UbicacionActivoFijo());
        }
        #endregion

        #region AJAX_Empleado
        public async Task<SelectList> ObtenerSelectListEmpleado(int idSucursal, int? idEmpleado = null)
        {
            try
            {
                var listaEmpleado = idSucursal != -1 ? await apiServicio.ObtenerElementoAsync<List<DatosBasicosEmpleadoViewModel>>(new EmpleadosPorSucursalViewModel { IdSucursal = idSucursal, EmpleadosActivos = true }, new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleadosPorSucursal") : new List<DatosBasicosEmpleadoViewModel>();
                return new SelectList(listaEmpleado.Select(c=> new ListaEmpleadoViewModel { IdEmpleado = c.IdEmpleado, NombreApellido = $"{c.Nombres} {c.Apellidos}" }), "IdEmpleado", "NombreApellido", idEmpleado);
            }
            catch (Exception)
            {
                return new SelectList(new List<ListaEmpleadoViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Empleado_SelectResult(int idSucursal)
        {
            ViewBag.Empleado = await ObtenerSelectListEmpleado(idSucursal);
            return PartialView("_EmpleadoSelect", new UbicacionActivoFijo());
        }
        #endregion
    }
}