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
using MoreLinq;

namespace bd.webapprm.web.Controllers.MVC
{
    public class ActivoFijoController : Controller
    {
        private readonly IApiServicio apiServicio;

        public ActivoFijoController(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }

        #region Recepción
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

                    rafd.Serie = !String.IsNullOrEmpty(Request.Form[$"hSerie_{posFormDatoEspecifico}"].ToString()) ? Request.Form[$"hSerie_{posFormDatoEspecifico}"].ToString() : null;
                    if (recepcionActivoFijoDetalle.ActivoFijo.SubClaseActivoFijo.ClaseActivoFijo.CategoriaActivoFijo.Nombre == Categorias.Edificio)
                        rafd.NumeroClaveCatastral = !String.IsNullOrEmpty(Request.Form[$"hNumeroClaveCatastral_{posFormDatoEspecifico}"].ToString()) ? Request.Form[$"hNumeroClaveCatastral_{posFormDatoEspecifico}"].ToString() : null;
                    else if (recepcionActivoFijoDetalle.ActivoFijo.SubClaseActivoFijo.ClaseActivoFijo.CategoriaActivoFijo.Nombre == Categorias.Vehiculo)
                    {
                        rafd.NumeroChasis = !String.IsNullOrEmpty(Request.Form[$"hNumeroChasis_{posFormDatoEspecifico}"].ToString()) ? Request.Form[$"hNumeroChasis_{posFormDatoEspecifico}"].ToString() : null;
                        rafd.NumeroMotor = !String.IsNullOrEmpty(Request.Form[$"hNumeroMotor_{posFormDatoEspecifico}"].ToString()) ? Request.Form[$"hNumeroMotor_{posFormDatoEspecifico}"].ToString() : null;
                        rafd.Placa = !String.IsNullOrEmpty(Request.Form[$"hPlaca_{posFormDatoEspecifico}"].ToString()) ? Request.Form[$"hPlaca_{posFormDatoEspecifico}"].ToString() : null;
                    }

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
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando recepción Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
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
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Sistema", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webapprm" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}", activoFijoRecepcionado ? nameof(ActivosFijosRecepcionados) : nameof(ActivosFijosValidacionTecnica));
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}", activoFijoRecepcionado ? nameof(ActivosFijosRecepcionados) : nameof(ActivosFijosValidacionTecnica));
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar Marca", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
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
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<ActivoFijo>>(new List<string> { Estados.Recepcionado }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivoFijoPorEstado");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos recepcionados", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
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
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<ActivoFijo>>(new List<string> { Estados.ValidacionTecnica }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivoFijoPorEstado");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos que requieren validación técnica", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
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
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando aprobación Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
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
        public async Task<IActionResult> DetallesActivoFijoResult(List<IdRecepcionActivoFijoDetalleSeleccionado> listadoRecepcionActivoFijoDetalleSeleccionado, List<PropiedadValor> arrConfiguraciones, bool mostrarNoSeleccionados)
        {
            var lista = new List<RecepcionActivoFijoDetalleSeleccionado>();
            ViewData["Configuraciones"] = arrConfiguraciones;
            try
            {
                if (!mostrarNoSeleccionados)
                    listadoRecepcionActivoFijoDetalleSeleccionado.RemoveAll(c => !c.seleccionado);

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
            ViewData["Configuraciones"] = new List<PropiedadValor>() { new PropiedadValor { Propiedad = "IsConfiguracionListadoComponentes", Valor = "true" }, new PropiedadValor { Propiedad = "IsConfiguracionDatosActivo", Valor = "true" } };
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
            var lista = new List<ActivoFijo>();
            ViewData["IsConfiguracionEditarPolizaSeguro"] = true;
            try
            {
                lista = await apiServicio.Listar<ActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarRecepcionActivoFijoSinPoliza");
                ViewData["Titulo"] = "Activos Fijos sin Póliza de Seguro";
                ViewData["UrlEditar"] = nameof(AsignarPolizaSeguro);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con estado Recepcionado sin número de póliza asignado", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoActivoFijo", lista);
        }

        public async Task<IActionResult> ActivosFijosRecepcionadosConPoliza()
        {
            var lista = new List<ActivoFijo>();
            ViewData["IsConfiguracionMostrarPolizaSeguro"] = true;
            try
            {
                lista = await apiServicio.Listar<ActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarRecepcionActivoFijoConPoliza");
                ViewData["Titulo"] = "Activos Fijos con Póliza de Seguro";
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con estado Recepcionado con número de póliza asignado", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoActivoFijo", lista);
        }

        public async Task<IActionResult> AsignarPolizaSeguro(string id) => await ObtenerRecepcionActivoFijo(id, new List<string>(), nameof(ActivosFijosRecepcionadosSinPoliza));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarPolizaSeguro(ActivoFijo activoFijo)
        {
            try
            {
                var response = await apiServicio.InsertarAsync(activoFijo, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/AsignarPolizaSeguro");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha asignado el número de póliza", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Póliza de seguro Activo Fijo Detalle:", activoFijo.IdActivoFijo) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ActivosFijosRecepcionadosConPoliza));
                }
                ViewData["Error"] = response.Message;
                return View(activoFijo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Asignando Póliza", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ActivosFijosRecepcionadosSinPoliza));
            }
        }
        #endregion

        #region Altas
        public async Task<IActionResult> ListadoActivosFijosAlta()
        {
            var lista = new List<ActivoFijo>();
            ViewData["IsConfiguracionListadoAltas"] = true;
            ViewData["Titulo"] = "Activos Fijos en Alta";
            ViewData["UrlEditar"] = nameof(GestionarAlta);
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<ActivoFijo>>(Estados.Alta, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivosFijosPorAgrupacionPorEstado");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos en alta", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
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
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando altas de activos fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
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
                    ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = altaActivoFijo.AltaActivoFijoDetalle.Select(c => new RecepcionActivoFijoDetalleSeleccionado
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
                altaActivoFijo.AltaActivoFijoDetalle = new List<AltaActivoFijoDetalle>();
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
                    altaActivoFijo.AltaActivoFijoDetalle.Add(new AltaActivoFijoDetalle {
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
                var listaRecepcionActivoFijoDetalleSeleccionado = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(altaActivoFijo.AltaActivoFijoDetalle.Select(c=> new IdRecepcionActivoFijoDetalleSeleccionado { idRecepcionActivoFijoDetalle = c.IdRecepcionActivoFijoDetalle, seleccionado = true }), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijo");
                foreach (var item in listaRecepcionActivoFijoDetalleSeleccionado)
                {
                    var rafdAltaActivoFijoActual = altaActivoFijo.AltaActivoFijoDetalle.FirstOrDefault(x => x.IdRecepcionActivoFijoDetalle == item.RecepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle).RecepcionActivoFijoDetalle;
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
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando alta de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
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
        #region Cambio de Custodio
        public async Task<IActionResult> ListadoCambioCustodio()
        {
            var lista = new List<TransferenciaActivoFijo>();
            ViewData["IsCambioCustodio"] = true;
            try
            {
                lista = await apiServicio.Listar<TransferenciaActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarTransferenciasCambiosCustodioActivosFijos");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando cambios de custodio de activos fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoTransferenciasSucursales", lista);
        }

        public async Task<IActionResult> GestionarCambioCustodio()
        {
            try
            {
                ViewData["Configuraciones"] = new List<PropiedadValor>()
                {
                    new PropiedadValor { Propiedad = "IsConfiguracionSeleccion", Valor = "true" },
                    new PropiedadValor { Propiedad = "IsConfiguracionDatosActivo", Valor = "true" },
                    new PropiedadValor { Propiedad = "IsConfiguracionSeleccionBajas", Valor = "true" }
                };
                ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(new CambioCustodioViewModel { IdEmpleadoEntrega = (ViewData["Empleado"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Empleado"] as SelectList).FirstOrDefault().Value) : -1, ListadoIdRecepcionActivoFijoDetalle = new List<int>() }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijoSeleccionadoPorEmpleado");
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoCambioCustodio));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GestionarCambioCustodio(CambioCustodioViewModel cambioCustodioModel)
        {
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Aceptada }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                var arrIdsRecepcionActivoFijoDetalle = Request.Form["idsRecepcionActivoFijoDetalle"].ToString().Split(',');
                cambioCustodioModel.ListadoIdRecepcionActivoFijoDetalle = arrIdsRecepcionActivoFijoDetalle.Select(c => int.Parse(c)).ToList();
                var response = await apiServicio.InsertarAsync(cambioCustodioModel, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/InsertarCambioCustodioActivoFijo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado un cambio de custodio de activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Custodio de Activo Fijo que recibe:", cambioCustodioModel.IdEmpleadoRecibe) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ListadoCambioCustodio));
                }
                var listaRecepcionActivoFijoDetalleSeleccionado = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(cambioCustodioModel, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijoSeleccionadoPorEmpleado");
                ViewData["Error"] = response.Message;
                ViewData["Configuraciones"] = new List<PropiedadValor>()
                {
                    new PropiedadValor { Propiedad = "IsConfiguracionSeleccion", Valor = "true" },
                    new PropiedadValor { Propiedad = "IsConfiguracionDatosActivo", Valor = "true" },
                    new PropiedadValor { Propiedad = "IsConfiguracionSeleccionBajas", Valor = "true" }
                };
                ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = listaRecepcionActivoFijoDetalleSeleccionado;
                return View(cambioCustodioModel);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando cambio de custodio de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListadoCambioCustodio));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ListadoActivosFijosCustodioResult(int idEmpleado)
        {
            var lista = new List<RecepcionActivoFijoDetalleSeleccionado>();
            try
            {
                ViewData["Configuraciones"] = new List<PropiedadValor>()
                {
                    new PropiedadValor { Propiedad = "IsConfiguracionSeleccion", Valor = "true" },
                    new PropiedadValor { Propiedad = "IsConfiguracionDatosActivo", Valor = "true" },
                    new PropiedadValor { Propiedad = "IsConfiguracionSeleccionBajas", Valor = "true" }
                };
                lista = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(new CambioCustodioViewModel { IdEmpleadoEntrega = idEmpleado, ListadoIdRecepcionActivoFijoDetalle = new List<int>() }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijoSeleccionadoPorEmpleado");
            }
            catch (Exception)
            { }
            return PartialView("_ListadoDetallesActivosFijos", lista);
        }
        #endregion

        #region Cambio de Ubicación entre Sucursales
        public async Task<IActionResult> ListadoSolicitudesTransferencia()
        {
            var lista = new List<TransferenciaActivoFijo>();
            ViewData["IsSolicitudesTransferencia"] = true;
            try
            {
                lista = await apiServicio.Listar<TransferenciaActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarTransferenciasCambiosUbicacionCreadasSolicitudActivosFijos");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando solicitudes de transferencia de activos fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoTransferenciasSucursales", lista);
        }

        public async Task<IActionResult> ListadoTransferenciasCreadas()
        {
            var lista = new List<TransferenciaActivoFijo>();
            ViewData["IsTransferenciasCreadas"] = true;
            try
            {
                lista = await apiServicio.Listar<TransferenciaActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarTransferenciasCambiosUbicacionCreadasSolicitudActivosFijos");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando transferencias creadas de activos fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoTransferenciasSucursales", lista);
        }

        public async Task<IActionResult> ListadoTransferenciasAceptadas()
        {
            var lista = new List<TransferenciaActivoFijo>();
            ViewData["IsTransferenciasAceptadas"] = true;
            try
            {
                lista = await apiServicio.Listar<TransferenciaActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarTransferenciasCambiosUbicacionAceptadasActivosFijos");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando transferencias aceptadas de activos fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoTransferenciasSucursales", lista);
        }

        public async Task<IActionResult> GestionarTransferenciaSucursal(int? id)
        {
            try
            {
                var listaSucursales = await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal");
                if (id != null)
                {
                    var response = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ObtenerTransferenciaActivoFijo");
                    if (!response.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoTransferenciasCreadas));

                    var transferenciaActivoFijo = JsonConvert.DeserializeObject<TransferenciaActivoFijo>(response.Resultado.ToString());
                    if (transferenciaActivoFijo.Estado.Nombre == Estados.Creada && transferenciaActivoFijo.MotivoTransferencia.Motivo_Transferencia == MotivosTransferencia.CambioUbicacion)
                    {
                        var cambioUbicacionSucursalViewModel = new CambioUbicacionSucursalViewModel
                        {
                            IdTransferenciaActivoFijo = (int)id,
                            IdSucursalOrigen = (int)transferenciaActivoFijo.TransferenciaActivoFijoDetalle.FirstOrDefault().UbicacionActivoFijoOrigen.LibroActivoFijo.IdSucursal,
                            IdEmpleadoEntrega = (int)transferenciaActivoFijo.TransferenciaActivoFijoDetalle.FirstOrDefault().UbicacionActivoFijoOrigen.IdEmpleado,
                            IdEmpleadoResponsableEnvio = (int)transferenciaActivoFijo.IdEmpleadoResponsableEnvio,
                            IdSucursalDestino = (int)transferenciaActivoFijo.TransferenciaActivoFijoDetalle.FirstOrDefault().UbicacionActivoFijoDestino.LibroActivoFijo.IdSucursal,
                            IdEmpleadoRecibe = (int)transferenciaActivoFijo.TransferenciaActivoFijoDetalle.FirstOrDefault().UbicacionActivoFijoDestino.IdEmpleado,
                            IdEmpleadoResponsableRecibo = (int)transferenciaActivoFijo.IdEmpleadoResponsableRecibo,
                            IdLibroActivoFijoDestino = transferenciaActivoFijo.TransferenciaActivoFijoDetalle.FirstOrDefault().UbicacionActivoFijoDestino.IdLibroActivoFijo,
                            FechaTransferencia = transferenciaActivoFijo.FechaTransferencia,
                            Observaciones = transferenciaActivoFijo.Observaciones
                        };
                        ViewData["SucursalOrigen"] = new SelectList(listaSucursales, "IdSucursal", "Nombre", cambioUbicacionSucursalViewModel.IdSucursalOrigen);
                        ViewData["SucursalDestino"] = new SelectList(listaSucursales.Exclude(listaSucursales.FindIndex(c => c.IdSucursal == cambioUbicacionSucursalViewModel.IdSucursalOrigen), 1), "IdSucursal", "Nombre", cambioUbicacionSucursalViewModel.IdSucursalDestino);
                        ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo(cambioUbicacionSucursalViewModel.IdSucursalDestino, cambioUbicacionSucursalViewModel.IdLibroActivoFijoDestino);

                        var listadoEmpleadoSucursalOrigen = (await apiServicio.ObtenerElementoAsync<List<DatosBasicosEmpleadoViewModel>>(new EmpleadosPorSucursalViewModel { IdSucursal = cambioUbicacionSucursalViewModel.IdSucursalOrigen, EmpleadosActivos = true }, new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleadosPorSucursal")).Select(c => new ListaEmpleadoViewModel { IdEmpleado = c.IdEmpleado, NombreApellido = $"{c.Nombres} {c.Apellidos}" });
                        ViewData["EmpleadoEntrega"] = new SelectList(listadoEmpleadoSucursalOrigen, "IdEmpleado", "NombreApellido", cambioUbicacionSucursalViewModel.IdEmpleadoEntrega);
                        ViewData["EmpleadoResponsableEnvio"] = new SelectList(listadoEmpleadoSucursalOrigen, "IdEmpleado", "NombreApellido", cambioUbicacionSucursalViewModel.IdEmpleadoResponsableEnvio);

                        var listadoEmpleadoSucursalDestino = (await apiServicio.ObtenerElementoAsync<List<DatosBasicosEmpleadoViewModel>>(new EmpleadosPorSucursalViewModel { IdSucursal = cambioUbicacionSucursalViewModel.IdSucursalDestino, EmpleadosActivos = true }, new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleadosPorSucursal")).Select(c => new ListaEmpleadoViewModel { IdEmpleado = c.IdEmpleado, NombreApellido = $"{c.Nombres} {c.Apellidos}" });
                        ViewData["EmpleadoRecibe"] = new SelectList(listadoEmpleadoSucursalDestino, "IdEmpleado", "NombreApellido", cambioUbicacionSucursalViewModel.IdEmpleadoRecibe);
                        ViewData["EmpleadoResponsableRecibo"] = new SelectList(listadoEmpleadoSucursalDestino, "IdEmpleado", "NombreApellido", cambioUbicacionSucursalViewModel.IdEmpleadoResponsableRecibo);

                        ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = transferenciaActivoFijo.TransferenciaActivoFijoDetalle.Select(c => new RecepcionActivoFijoDetalleSeleccionado { RecepcionActivoFijoDetalle = c.RecepcionActivoFijoDetalle, Seleccionado = true }).ToList();
                        ViewData["Configuraciones"] = new List<PropiedadValor>() { new PropiedadValor { Propiedad = "IsConfiguracionDatosActivo", Valor = "true" }, new PropiedadValor { Propiedad = "IsConfiguracionSeleccionBajas", Valor = "true" } };
                        return View(cambioUbicacionSucursalViewModel);
                    }
                    else
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorRecursoSolicitado}", nameof(ListadoTransferenciasCreadas));
                }
                ViewData["SucursalOrigen"] = new SelectList(listaSucursales, "IdSucursal", "Nombre");
                int idSucursalOrigen = (ViewData["SucursalOrigen"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["SucursalOrigen"] as SelectList).FirstOrDefault().Value) : -1;
                
                ViewData["SucursalDestino"] = new SelectList(listaSucursales.Exclude(0, 1), "IdSucursal", "Nombre");
                int idSucursalDestino = (ViewData["SucursalDestino"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["SucursalDestino"] as SelectList).FirstOrDefault().Value) : -1;
                ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo((ViewData["SucursalDestino"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["SucursalDestino"] as SelectList).FirstOrDefault().Value) : -1);

                var listaEmpleadoSucursalOrigen = idSucursalOrigen != -1 ? await apiServicio.ObtenerElementoAsync<List<DatosBasicosEmpleadoViewModel>>(new EmpleadosPorSucursalViewModel { IdSucursal = idSucursalOrigen, EmpleadosActivos = true }, new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleadosPorSucursal") : new List<DatosBasicosEmpleadoViewModel>();
                ViewData["EmpleadoEntrega"] = new SelectList(listaEmpleadoSucursalOrigen.Select(c => new ListaEmpleadoViewModel { IdEmpleado = c.IdEmpleado, NombreApellido = $"{c.Nombres} {c.Apellidos}" }), "IdEmpleado", "NombreApellido");
                ViewData["EmpleadoResponsableEnvio"] = ViewData["EmpleadoEntrega"];

                var listaEmpleadoSucursalDestino = idSucursalDestino != -1 ? await apiServicio.ObtenerElementoAsync<List<DatosBasicosEmpleadoViewModel>>(new EmpleadosPorSucursalViewModel { IdSucursal = idSucursalDestino, EmpleadosActivos = true }, new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleadosPorSucursal") : new List<DatosBasicosEmpleadoViewModel>();
                ViewData["EmpleadoRecibe"] = new SelectList(listaEmpleadoSucursalDestino.Select(c => new ListaEmpleadoViewModel { IdEmpleado = c.IdEmpleado, NombreApellido = $"{c.Nombres} {c.Apellidos}" }), "IdEmpleado", "NombreApellido");
                ViewData["EmpleadoResponsableRecibo"] = ViewData["EmpleadoRecibe"];

                int idEmpleadoEntrega = (ViewData["EmpleadoEntrega"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["EmpleadoEntrega"] as SelectList).FirstOrDefault().Value) : -1;
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = idEmpleadoEntrega != -1 ? await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(new CambioCustodioViewModel { IdEmpleadoEntrega = idEmpleadoEntrega, ListadoIdRecepcionActivoFijoDetalle = new List<int>() }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijoSeleccionadoPorEmpleado") : new List<RecepcionActivoFijoDetalleSeleccionado>();
                ViewData["Configuraciones"] = new List<PropiedadValor>() { new PropiedadValor { Propiedad = "IsConfiguracionSeleccion", Valor = "true" }, new PropiedadValor { Propiedad = "IsConfiguracionDatosActivo", Valor = "true" }, new PropiedadValor { Propiedad = "IsConfiguracionSeleccionBajas", Valor = "true" } };
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoTransferenciasCreadas));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GestionarTransferenciaSucursal(CambioUbicacionSucursalViewModel cambioUbicacionSucursalViewModel)
        {
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Creada }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                var arrIdsRecepcionActivoFijoDetalle = Request.Form["idsRecepcionActivoFijoDetalle"].ToString().Split(',');
                cambioUbicacionSucursalViewModel.ListadoIdRecepcionActivoFijoDetalle = arrIdsRecepcionActivoFijoDetalle.Select(c => int.Parse(c)).ToList();

                var response = new Response();
                if (cambioUbicacionSucursalViewModel.IdTransferenciaActivoFijo == 0)
                {
                    response = await apiServicio.InsertarAsync(cambioUbicacionSucursalViewModel, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/InsertarCambioUbicacionSucursalActivoFijo");
                    if (response.IsSuccess)
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado un cambio de ubicación entre sucursales de activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Custodio de Activo Fijo que recibe:", cambioUbicacionSucursalViewModel.IdEmpleadoRecibe) });
                }
                else
                {
                    response = await apiServicio.EditarAsync(cambioUbicacionSucursalViewModel.IdTransferenciaActivoFijo.ToString(), cambioUbicacionSucursalViewModel, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/EditarCambioUbicacionSucursalActivoFijo");
                    if (response.IsSuccess)
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha editado un cambio de ubicación entre sucursales de activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Cambio de ubicación entre sucursales de Activo Fijo:", cambioUbicacionSucursalViewModel.IdTransferenciaActivoFijo) });
                }

                if (response.IsSuccess)
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ListadoTransferenciasCreadas));

                ViewData["Configuraciones"] = new List<PropiedadValor>()
                {
                    new PropiedadValor { Propiedad = "IsConfiguracionSeleccion", Valor = "true" },
                    new PropiedadValor { Propiedad = "IsConfiguracionDatosActivo", Valor = "true" },
                    new PropiedadValor { Propiedad = "IsConfiguracionSeleccionBajas", Valor = "true" }
                };
                var listaSucursales = await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal");
                ViewData["SucursalOrigen"] = new SelectList(listaSucursales, "IdSucursal", "Nombre", cambioUbicacionSucursalViewModel.IdSucursalOrigen);
                ViewData["SucursalDestino"] = new SelectList(listaSucursales.Exclude(listaSucursales.FindIndex(c => c.IdSucursal == cambioUbicacionSucursalViewModel.IdSucursalOrigen), 1), "IdSucursal", "Nombre", cambioUbicacionSucursalViewModel.IdSucursalDestino);
                ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo(cambioUbicacionSucursalViewModel.IdSucursalDestino, cambioUbicacionSucursalViewModel.IdLibroActivoFijoDestino);

                var listadoEmpleadoSucursalOrigen = (await apiServicio.ObtenerElementoAsync<List<DatosBasicosEmpleadoViewModel>>(new EmpleadosPorSucursalViewModel { IdSucursal = cambioUbicacionSucursalViewModel.IdSucursalOrigen, EmpleadosActivos = true }, new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleadosPorSucursal")).Select(c => new ListaEmpleadoViewModel { IdEmpleado = c.IdEmpleado, NombreApellido = $"{c.Nombres} {c.Apellidos}" });
                ViewData["EmpleadoEntrega"] = new SelectList(listadoEmpleadoSucursalOrigen, "IdEmpleado", "NombreApellido", cambioUbicacionSucursalViewModel.IdEmpleadoEntrega);
                ViewData["EmpleadoResponsableEnvio"] = new SelectList(listadoEmpleadoSucursalOrigen, "IdEmpleado", "NombreApellido", cambioUbicacionSucursalViewModel.IdEmpleadoResponsableEnvio);

                var listadoEmpleadoSucursalDestino = (await apiServicio.ObtenerElementoAsync<List<DatosBasicosEmpleadoViewModel>>(new EmpleadosPorSucursalViewModel { IdSucursal = cambioUbicacionSucursalViewModel.IdSucursalDestino, EmpleadosActivos = true }, new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleadosPorSucursal")).Select(c => new ListaEmpleadoViewModel { IdEmpleado = c.IdEmpleado, NombreApellido = $"{c.Nombres} {c.Apellidos}" });
                ViewData["EmpleadoRecibe"] = new SelectList(listadoEmpleadoSucursalDestino, "IdEmpleado", "NombreApellido", cambioUbicacionSucursalViewModel.IdEmpleadoRecibe);
                ViewData["EmpleadoResponsableRecibo"] = new SelectList(listadoEmpleadoSucursalDestino, "IdEmpleado", "NombreApellido", cambioUbicacionSucursalViewModel.IdEmpleadoResponsableRecibo);

                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(new CambioCustodioViewModel {
                    IdEmpleadoEntrega = cambioUbicacionSucursalViewModel.IdEmpleadoEntrega,
                    ListadoIdRecepcionActivoFijoDetalle = cambioUbicacionSucursalViewModel.ListadoIdRecepcionActivoFijoDetalle
                }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijoSeleccionadoPorEmpleado");
                ViewData["Error"] = response.Message;
                return View(cambioUbicacionSucursalViewModel);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando cambio de ubicación de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListadoTransferenciasCreadas));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EmpleadoTransferenciaResult(int idSucursal, string namePartialView)
        {
            if (namePartialView == "_EmpleadosTransferenciaSucursalOrigen")
            {
                ViewData["EmpleadoEntrega"] = await ObtenerSelectListEmpleado(idSucursal);
                ViewData["EmpleadoResponsableEnvio"] = ViewData["EmpleadoEntrega"];
            }
            else
            {
                ViewData["EmpleadoRecibe"] = await ObtenerSelectListEmpleado(idSucursal);
                ViewData["EmpleadoResponsableRecibo"] = ViewData["EmpleadoRecibe"];
                ViewData["LibroActivoFijo"] = await ObtenerSelectListLibroActivoFijo(idSucursal);
            }
            return PartialView(namePartialView, new CambioUbicacionSucursalViewModel());
        }

        public async Task<IActionResult> GestionarAprobacionTransferenciaSucursal(int id, bool id2)
        {
            try
            {
                var response = new Response();
                if (id2)
                    await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Aceptada }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                else
                    await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Desaprobado }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");

                response = await apiServicio.InsertarAsync(new TransferenciaActivoFijoTransfer { IdTransferenciaActivoFijo = id, Aprobado = id2 }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/AprobacionTransferenciaCambioUbicacionActivoFijo");
                return this.Redireccionar(response.IsSuccess ? $"{Mensaje.Informacion}|{Mensaje.Satisfactorio}" : $"{Mensaje.Error}|{Mensaje.Excepcion}", nameof(ListadoTransferenciasAceptadas));
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Aceptando transferencia creada de activos fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}", nameof(ListadoTransferenciasCreadas));
            }
        }
        #endregion
        #endregion

        #region Bajas
        public async Task<IActionResult> ListadoActivosFijosBaja()
        {
            var lista = new List<ActivoFijo>();
            ViewData["IsConfiguracionListadoBajas"] = true;
            ViewData["Titulo"] = "Activos Fijos en Baja";
            ViewData["UrlEditar"] = nameof(GestionarBaja);
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<ActivoFijo>>(Estados.Baja, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivosFijosPorAgrupacionPorEstado");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos en baja", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
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
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando bajas de activos fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
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
                    ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = bajaActivoFijo.BajaActivoFijoDetalle.Select(c => new RecepcionActivoFijoDetalleSeleccionado
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
                bajaActivoFijo.BajaActivoFijoDetalle = new List<BajaActivoFijoDetalle>();
                var listaFormDatosEspecificos = Request.Form.Where(c => c.Key.StartsWith("hIdRecepcionActivoFijoDetalle_"));
                foreach (var item in listaFormDatosEspecificos)
                {
                    int posFormDatoEspecifico = int.Parse(item.Key.ToString().Split('_')[1]);
                    int idEmpleado = int.Parse(Request.Form[$"hEmpleado_{posFormDatoEspecifico}"]);
                    int idRecepcionActivoFijoDetalle = int.Parse(Request.Form[$"hIdRecepcionActivoFijoDetalle_{posFormDatoEspecifico}"]);

                    var rafd = new RecepcionActivoFijoDetalle { IdRecepcionActivoFijoDetalle = idRecepcionActivoFijoDetalle, UbicacionActivoFijoActual = new UbicacionActivoFijo { IdEmpleado = idEmpleado } };
                    bajaActivoFijo.BajaActivoFijoDetalle.Add(new BajaActivoFijoDetalle
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
                var listaRecepcionActivoFijoDetalleSeleccionado = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(bajaActivoFijo.BajaActivoFijoDetalle.Select(c => new IdRecepcionActivoFijoDetalleSeleccionado { idRecepcionActivoFijoDetalle = c.IdRecepcionActivoFijoDetalle, seleccionado = true }), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijo");
                foreach (var item in listaRecepcionActivoFijoDetalleSeleccionado)
                {
                    var rafdBajaActivoFijoActual = bajaActivoFijo.BajaActivoFijoDetalle.FirstOrDefault(x => x.IdRecepcionActivoFijoDetalle == item.RecepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle).RecepcionActivoFijoDetalle;
                    item.RecepcionActivoFijoDetalle.UbicacionActivoFijoActual.IdEmpleado = rafdBajaActivoFijoActual.UbicacionActivoFijoActual.IdEmpleado;
                    item.RecepcionActivoFijoDetalle.UbicacionActivoFijoActual.Empleado = JsonConvert.DeserializeObject<Empleado>((await apiServicio.SeleccionarAsync<Response>(rafdBajaActivoFijoActual.UbicacionActivoFijoActual.IdEmpleado.ToString(), new Uri(WebApp.BaseAddressTH), "api/Empleados")).Resultado.ToString());
                }
                ViewData["Error"] = response.Message;
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = listaRecepcionActivoFijoDetalleSeleccionado;
                return View(bajaActivoFijo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando baja de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
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

        #region Mantenimientos
        public async Task<IActionResult> ListarMantenimientosActivosFijos()
        {
            var lista = new List<ActivoFijo>();
            try
            {
                ViewData["IsConfiguracionListadoMantenimientos"] = true;
                ViewData["Titulo"] = "Activos Fijos en Alta";
                ViewData["UrlEditar"] = nameof(EditarMantenimiento);
                lista = await apiServicio.ObtenerElementoAsync<List<ActivoFijo>>(Estados.Alta, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivosFijosPorAgrupacionPorEstado");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando Mantenimientos de Activos Fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoActivoFijo", lista);
        }

        public async Task<IActionResult> ListadoMantenimientos(string id)
        {
            var lista = new List<MantenimientoActivoFijo>();
            ViewData["IdRecepcionActivoFijoDetalle"] = id;
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<MantenimientoActivoFijo>>(id, new Uri(WebApp.BaseAddressRM), "api/MantenimientoActivoFijo/ListarMantenimientosActivoFijoPorIdDetalleActivoFijo");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando Mantenimientos de Activos Fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
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
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoMantenimientos), routeValues: new { id });
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
                        return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ListadoMantenimientos), routeValues: new { id = mantenimientoActivoFijo.IdRecepcionActivoFijoDetalle });
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
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando Mantenimiento Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListadoMantenimientos), routeValues: new { id = mantenimientoActivoFijo.IdRecepcionActivoFijoDetalle });
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
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoMantenimientos), routeValues: new { id });

                    respuesta.Resultado = JsonConvert.DeserializeObject<MantenimientoActivoFijo>(respuesta.Resultado.ToString());
                    ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                    return View(respuesta.Resultado);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoMantenimientos), routeValues: new { id });
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoMantenimientos), routeValues: new { id });
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
                            return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ListadoMantenimientos), routeValues: new { id = mantenimientoActivoFijo.IdRecepcionActivoFijoDetalle });
                        }
                    }
                    else
                        response.Message = Mensaje.ModeloInvalido;

                    ViewData["Error"] = response.Message;
                    ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                    return View(mantenimientoActivoFijo);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoMantenimientos), routeValues: new { id = mantenimientoActivoFijo.IdRecepcionActivoFijoDetalle });
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Editando un Mantenimiento Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorEditar}", nameof(ListadoMantenimientos), routeValues: new { id = mantenimientoActivoFijo.IdRecepcionActivoFijoDetalle });
            }
        }

        public async Task<IActionResult> EliminarMantenimiento(string id, string id2)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id2, new Uri(WebApp.BaseAddressRM), "api/MantenimientoActivoFijo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Mantenimiento Activo Fijo", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webapprm" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}", nameof(ListadoMantenimientos), routeValues: new { id });
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}", nameof(ListadoMantenimientos), routeValues: new { id });
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar Mantenimiento Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}", nameof(ListadoMantenimientos), routeValues: new { id });
            }
        }
        #endregion

        #region Procesos Judiciales
        public async Task<IActionResult> ListarProcesosJudicialesActivosFijos()
        {
            var lista = new List<ActivoFijo>();
            try
            {
                ViewData["IsConfiguracionListadoProcesosJudiciales"] = true;
                ViewData["Titulo"] = "Activos Fijos en Alta";
                ViewData["UrlEditar"] = nameof(EditarMantenimiento);
                lista = await apiServicio.ObtenerElementoAsync<List<ActivoFijo>>(Estados.Alta, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarActivosFijosPorAgrupacionPorEstado");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando procesos judiciales de Activos Fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoActivoFijo", lista);
        }

        public async Task<IActionResult> ListadoProcesosJudiciales(string id)
        {
            var lista = new List<ProcesoJudicialActivoFijo>();
            ViewData["IdRecepcionActivoFijoDetalle"] = id;
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<ProcesoJudicialActivoFijo>>(id, new Uri(WebApp.BaseAddressRM), "api/ProcesoJudicialActivoFijo/ListarProcesoJudicialActivoFijoPorIdDetalleActivoFijo");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando procesos judiciales de Activos Fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> GestionarProcesoJudicial(string id, string id2)
        {
            try
            {
                ViewData["IdRecepcionActivoFijoDetalle"] = id;
                if (!string.IsNullOrEmpty(id2))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id2, new Uri(WebApp.BaseAddressRM), "api/ProcesoJudicialActivoFijo");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoProcesosJudiciales), routeValues: new { id });

                    respuesta.Resultado = JsonConvert.DeserializeObject<ProcesoJudicialActivoFijo>(respuesta.Resultado.ToString());
                    return View(respuesta.Resultado);
                }
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoProcesosJudiciales), routeValues: new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GestionarProcesoJudicial(ProcesoJudicialActivoFijo procesoJudicialActivoFijo, List<IFormFile> file)
        {
            try
            {
                ViewData["IdRecepcionActivoFijoDetalle"] = procesoJudicialActivoFijo.IdRecepcionActivoFijoDetalle;
                if (!string.IsNullOrEmpty(procesoJudicialActivoFijo.IdProcesoJudicialActivoFijo.ToString()))
                {
                    var response = new Response();
                    if (ModelState.IsValid)
                    {
                        int idProcesoJudicialActivoFijo = 0;
                        if (procesoJudicialActivoFijo.IdProcesoJudicialActivoFijo == 0)
                        {
                            response = await apiServicio.InsertarAsync(procesoJudicialActivoFijo, new Uri(WebApp.BaseAddressRM), "api/ProcesoJudicialActivoFijo/InsertarProcesoJudicialActivoFijo");
                            if (response.IsSuccess)
                            {
                                var procesoJudicialActivoFijoAux = JsonConvert.DeserializeObject<ProcesoJudicialActivoFijo>(response.Resultado.ToString());
                                idProcesoJudicialActivoFijo = procesoJudicialActivoFijoAux.IdProcesoJudicialActivoFijo;
                                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Proceso Judicial", procesoJudicialActivoFijo.IdProcesoJudicialActivoFijo.ToString()), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha creado un registro Proceso judicial de Activo Fijo", UserName = "Usuario 1" });
                            }
                        }
                        else
                        {
                            idProcesoJudicialActivoFijo = procesoJudicialActivoFijo.IdProcesoJudicialActivoFijo;
                            response = await apiServicio.EditarAsync(procesoJudicialActivoFijo.IdProcesoJudicialActivoFijo.ToString(), procesoJudicialActivoFijo, new Uri(WebApp.BaseAddressRM), "api/ProcesoJudicialActivoFijo");
                            if (response.IsSuccess)
                                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Proceso Judicial", procesoJudicialActivoFijo.IdProcesoJudicialActivoFijo.ToString()), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha actualizado un registro Proceso judicial de Activo Fijo", UserName = "Usuario 1" });
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
                                        var activoFijoDocumentoTransfer = new DocumentoActivoFijoTransfer { Nombre = item.FileName, Fichero = data, IdProcesoJudicialActivoFijo = idProcesoJudicialActivoFijo };
                                        responseFile = await apiServicio.InsertarAsync(activoFijoDocumentoTransfer, new Uri(WebApp.BaseAddressRM), "api/DocumentoActivoFijo/UploadFiles");
                                        if (responseFile != null && responseFile.IsSuccess)
                                            await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha subido un archivo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Documento de Activo Fijo:", activoFijoDocumentoTransfer.Nombre) });
                                    }
                                }
                            }
                        }
                        if (response.IsSuccess)
                            return this.Redireccionar(responseFile.IsSuccess ? $"{Mensaje.Informacion}|{Mensaje.Satisfactorio}" : $"{Mensaje.Aviso}|{Mensaje.ErrorUploadFiles}", nameof(ListadoProcesosJudiciales), routeValues: new { id = procesoJudicialActivoFijo.IdRecepcionActivoFijoDetalle });
                    }
                    else
                        response.Message = Mensaje.ModeloInvalido;

                    ViewData["Error"] = response.Message;
                    return View(procesoJudicialActivoFijo);
                }
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoProcesosJudiciales), routeValues: new { id = procesoJudicialActivoFijo.IdRecepcionActivoFijoDetalle });
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Editando un Proceso judicial de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorEditar}", nameof(ListadoProcesosJudiciales), routeValues: new { id = procesoJudicialActivoFijo.IdRecepcionActivoFijoDetalle });
            }
        }

        public async Task<IActionResult> EliminarProcesoJudicial(string id, string id2)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id2, new Uri(WebApp.BaseAddressRM), "api/ProcesoJudicialActivoFijo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Proceso judicial de Activo Fijo", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webapprm" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}", nameof(ListadoProcesosJudiciales), routeValues: new { id });
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}", nameof(ListadoProcesosJudiciales), routeValues: new { id });
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar Proceso judicial de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}", nameof(ListadoProcesosJudiciales), routeValues: new { id });
            }
        }
        #endregion

        #region Inventarios
        public async Task<IActionResult> ListadoInventarioActivosFijos()
        {
            var lista = new List<InventarioActivoFijo>();
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<InventarioActivoFijo>>(new RangoFechaTransfer { FechaInicial = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0), FechaFinal = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59) }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarInventariosActivosFijosPorRangoFecha");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando inventarios de activos fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        [HttpPost]
        public async Task<IActionResult> ListadoInventarioResult(string fechaInicial, string fechaFinal)
        {
            var lista = new List<InventarioActivoFijo>();
            try
            {
                if (!String.IsNullOrEmpty(fechaInicial) && !String.IsNullOrEmpty(fechaFinal))
                {
                    var arrFechaInicial = fechaInicial.Split('/');
                    var arrFechaFinal = fechaFinal.Split('/');

                    DateTime fInicial = new DateTime(int.Parse(arrFechaInicial[2]), int.Parse(arrFechaInicial[0]), int.Parse(arrFechaInicial[1]), 0, 0, 0);
                    DateTime fFinal = new DateTime(int.Parse(arrFechaFinal[2]), int.Parse(arrFechaFinal[0]), int.Parse(arrFechaFinal[1]), 23, 59, 59);
                    lista = await apiServicio.ObtenerElementoAsync<List<InventarioActivoFijo>>(new RangoFechaTransfer { FechaInicial = fInicial, FechaFinal = fFinal }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarInventariosActivosFijosPorRangoFecha");
                }
                else
                    lista = await apiServicio.Listar<InventarioActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarInventariosActivosFijos");
            }
            catch (Exception)
            { }
            return PartialView("_ListadoInventarioActivosFijos", lista);
        }

        public async Task<IActionResult> GestionarInventarioManual(string id)
        {
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.EnEjecucion }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Concluido }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                var listaEstado = await apiServicio.Listar<Estado>(new Uri(WebApp.BaseAddressTH), "api/Estados/ListarEstados");

                ViewData["Configuraciones"] = new List<PropiedadValor>() { new PropiedadValor { Propiedad = "IsConfiguracionSeleccion", Valor = "true" }, new PropiedadValor { Propiedad = "IsConfiguracionDatosActivo", Valor = "true" }, new PropiedadValor { Propiedad = "IsConfiguracionSeleccionBajas", Valor = "true" } };
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ObtenerInventarioActivosFijos");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoInventarioActivosFijos));

                    var inventarioActivoFijo = JsonConvert.DeserializeObject<InventarioActivoFijo>(respuesta.Resultado.ToString());
                    ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(new IdRecepcionActivoFijoDetalleSeleccionadoEstado { Estados = new List<string> { Estados.Alta }, ListaIdRecepcionActivoFijoDetalleSeleccionado = inventarioActivoFijo.InventarioActivoFijoDetalle.Select(c=> new IdRecepcionActivoFijoDetalleSeleccionado { idRecepcionActivoFijoDetalle = c.IdRecepcionActivoFijoDetalle, seleccionado = c.Constatado }).ToList() }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijoSeleccionadoPorEstado");
                    ViewData["Estado"] = new SelectList(listaEstado.Where(c => c.Nombre == Estados.EnEjecucion || c.Nombre == Estados.Concluido).OrderByDescending(c=> c.Nombre), "IdEstado", "Nombre", inventarioActivoFijo.IdEstado);
                    return View(inventarioActivoFijo);
                }
                ViewData["Estado"] = new SelectList(listaEstado.Where(c => c.Nombre == Estados.EnEjecucion || c.Nombre == Estados.Concluido).OrderByDescending(c => c.Nombre), "IdEstado", "Nombre");
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(new IdRecepcionActivoFijoDetalleSeleccionadoEstado { Estados = new List<string> { Estados.Alta }, ListaIdRecepcionActivoFijoDetalleSeleccionado = new List<IdRecepcionActivoFijoDetalleSeleccionado>() }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijoSeleccionadoPorEstado");
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoInventarioActivosFijos));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GestionarInventarioManual(InventarioActivoFijo inventarioActivoFijo)
        {
            try
            {
                var arrIdsRecepcionActivoFijoDetalle = Request.Form["idsRecepcionActivoFijoDetalleSeleccionado"].ToString().Split(',');
                inventarioActivoFijo.InventarioActivoFijoDetalle = new List<InventarioActivoFijoDetalle>();
                foreach (var idRafdSeleccionado in arrIdsRecepcionActivoFijoDetalle)
                {
                    var arrIdsSeleccionado = idRafdSeleccionado.Split('_');
                    inventarioActivoFijo.InventarioActivoFijoDetalle.Add(new InventarioActivoFijoDetalle
                    {
                        IdRecepcionActivoFijoDetalle = int.Parse(arrIdsSeleccionado[0]),
                        IdInventarioActivoFijo = inventarioActivoFijo.IdInventarioActivoFijo,
                        Constatado = bool.Parse(arrIdsSeleccionado[1])
                    });
                }

                var response = new Response();
                if (inventarioActivoFijo.IdInventarioActivoFijo == 0)
                {
                    response = await apiServicio.InsertarAsync(inventarioActivoFijo, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/InsertarInventarioActivoFijo");
                    if (response.IsSuccess)
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado un inventario de activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Inventario de Activo Fijo:", inventarioActivoFijo.IdInventarioActivoFijo) });
                }
                else
                {
                    response = await apiServicio.EditarAsync(inventarioActivoFijo.IdInventarioActivoFijo.ToString(), inventarioActivoFijo, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/EditarInventarioActivoFijo");
                    if (response.IsSuccess)
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha editado un inventario de activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Inventario de Activo Fijo:", inventarioActivoFijo.IdInventarioActivoFijo) });
                }

                if (response.IsSuccess)
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ListadoInventarioActivosFijos));

                ViewData["Configuraciones"] = new List<PropiedadValor>() { new PropiedadValor { Propiedad = "IsConfiguracionSeleccion", Valor = "true" }, new PropiedadValor { Propiedad = "IsConfiguracionDatosActivo", Valor = "true" }, new PropiedadValor { Propiedad = "IsConfiguracionSeleccionBajas", Valor = "true" } };
                ViewData["Estado"] = new SelectList((await apiServicio.Listar<Estado>(new Uri(WebApp.BaseAddressTH), "api/Estados/ListarEstados")).Where(c => c.Nombre == Estados.EnEjecucion || c.Nombre == Estados.Concluido).OrderByDescending(c => c.Nombre), "IdEstado", "Nombre");
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(inventarioActivoFijo.InventarioActivoFijoDetalle.Select(c => new IdRecepcionActivoFijoDetalleSeleccionado { idRecepcionActivoFijoDetalle = c.IdRecepcionActivoFijoDetalle, seleccionado = c.Constatado }), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijo");
                return View(inventarioActivoFijo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando inventario de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListadoInventarioActivosFijos));
            }
        }

        public async Task<IActionResult> GestionarInventarioAutomatico(string id)
        {
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.EnEjecucion }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Concluido }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                var listaEstado = await apiServicio.Listar<Estado>(new Uri(WebApp.BaseAddressTH), "api/Estados/ListarEstados");

                ViewData["Configuraciones"] = new List<PropiedadValor>() { new PropiedadValor { Propiedad = "IsConfiguracionSeleccion", Valor = "true" }, new PropiedadValor { Propiedad = "IsConfiguracionDatosActivo", Valor = "true" }, new PropiedadValor { Propiedad = "IsConfiguracionSeleccionBajas", Valor = "true" }, new PropiedadValor { Propiedad = "IsConfiguracionGestionarInventarioAutomatico", Valor = "true" } };
                if (!string.IsNullOrEmpty(id))
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ObtenerInventarioActivosFijos");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoInventarioActivosFijos));

                    var inventarioActivoFijo = JsonConvert.DeserializeObject<InventarioActivoFijo>(respuesta.Resultado.ToString());
                    ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(inventarioActivoFijo.InventarioActivoFijoDetalle.Select(c => new IdRecepcionActivoFijoDetalleSeleccionado { idRecepcionActivoFijoDetalle = c.IdRecepcionActivoFijoDetalle, seleccionado = c.Constatado }), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijo");
                    ViewData["Estado"] = new SelectList(listaEstado.Where(c => c.Nombre == Estados.EnEjecucion || c.Nombre == Estados.Concluido).OrderByDescending(c => c.Nombre), "IdEstado", "Nombre", inventarioActivoFijo.IdEstado);
                    return View(inventarioActivoFijo);
                }
                ViewData["Estado"] = new SelectList(listaEstado.Where(c => c.Nombre == Estados.EnEjecucion || c.Nombre == Estados.Concluido).OrderByDescending(c => c.Nombre), "IdEstado", "Nombre");
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = new List<RecepcionActivoFijoDetalleSeleccionado>();
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoInventarioActivosFijos));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GestionarInventarioAutomatico(InventarioActivoFijo inventarioActivoFijo)
        {
            try
            {
                var arrIdsRecepcionActivoFijoDetalle = Request.Form["idsRecepcionActivoFijoDetalleSeleccionado"].ToString().Split(',');
                inventarioActivoFijo.InventarioActivoFijoDetalle = new List<InventarioActivoFijoDetalle>();
                foreach (var idRafdSeleccionado in arrIdsRecepcionActivoFijoDetalle)
                {
                    var arrIdsSeleccionado = idRafdSeleccionado.Split('_');
                    inventarioActivoFijo.InventarioActivoFijoDetalle.Add(new InventarioActivoFijoDetalle
                    {
                        IdRecepcionActivoFijoDetalle = int.Parse(arrIdsSeleccionado[0]),
                        IdInventarioActivoFijo = inventarioActivoFijo.IdInventarioActivoFijo,
                        Constatado = bool.Parse(arrIdsSeleccionado[1])
                    });
                }

                var response = new Response();
                if (inventarioActivoFijo.IdInventarioActivoFijo == 0)
                {
                    response = await apiServicio.InsertarAsync(inventarioActivoFijo, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/InsertarInventarioActivoFijo");
                    if (response.IsSuccess)
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado un inventario automático de activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Inventario de Activo Fijo:", inventarioActivoFijo.IdInventarioActivoFijo) });
                }
                else
                {
                    response = await apiServicio.EditarAsync(inventarioActivoFijo.IdInventarioActivoFijo.ToString(), inventarioActivoFijo, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/EditarInventarioActivoFijo");
                    if (response.IsSuccess)
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha editado un inventario automático de activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Inventario de Activo Fijo:", inventarioActivoFijo.IdInventarioActivoFijo) });
                }

                if (response.IsSuccess)
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ListadoInventarioActivosFijos));

                ViewData["Configuraciones"] = new List<PropiedadValor>() { new PropiedadValor { Propiedad = "IsConfiguracionSeleccion", Valor = "true" }, new PropiedadValor { Propiedad = "IsConfiguracionDatosActivo", Valor = "true" }, new PropiedadValor { Propiedad = "IsConfiguracionSeleccionBajas", Valor = "true" }, new PropiedadValor { Propiedad = "IsConfiguracionGestionarInventarioAutomatico", Valor = "true" } };
                ViewData["Estado"] = new SelectList((await apiServicio.Listar<Estado>(new Uri(WebApp.BaseAddressTH), "api/Estados/ListarEstados")).Where(c => c.Nombre == Estados.EnEjecucion || c.Nombre == Estados.Concluido).OrderByDescending(c => c.Nombre), "IdEstado", "Nombre");
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(inventarioActivoFijo.InventarioActivoFijoDetalle.Select(c => new IdRecepcionActivoFijoDetalleSeleccionado { idRecepcionActivoFijoDetalle = c.IdRecepcionActivoFijoDetalle, seleccionado = c.Constatado }), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijo");
                return View(inventarioActivoFijo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando inventario automático de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListadoInventarioActivosFijos));
            }
        }

        [HttpPost]
        public async Task<IActionResult> DatosInventarioActivoFijoResult(string codigoSecuencial)
        {
            try
            {
                var response = await apiServicio.ObtenerElementoAsync<Response>(codigoSecuencial, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ObtenerDetalleActivoFijoParaInventario");
                if (response.IsSuccess)
                {
                    var recepcionActivoFijoDetalle = JsonConvert.DeserializeObject<RecepcionActivoFijoDetalle>(response.Resultado.ToString());
                    if (recepcionActivoFijoDetalle.Estado.Nombre != Estados.Alta)
                        return StatusCode(500, $"El activo fijo con el código secuencial {codigoSecuencial} no se encuentra en estado {Estados.Alta}.");

                    return PartialView("_DatosInventarioActivoFijo", recepcionActivoFijoDetalle);
                }

                return StatusCode(500, $"No se encontró ningún activo fijo con el código secuencial {codigoSecuencial}.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocurrió un error al cargar los datos del activo fijo.");
            }
        }
        #endregion

        #region Movilizaciones
        public async Task<IActionResult> ListadoMovilizacionActivosFijos()
        {
            var lista = new List<MovilizacionActivoFijo>();
            try
            {
                lista = await apiServicio.Listar<MovilizacionActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarMovilizacionesActivosFijos");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando movilizaciones de activos fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> GestionarMovilizacion(int? id)
        {
            try
            {
                ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                ViewData["MotivoTraslado"] = new SelectList(await apiServicio.Listar<MotivoTraslado>(new Uri(WebApp.BaseAddressRM), "api/MotivoTraslado/ListarMotivoTraslado"), "IdMotivoTraslado", "Descripcion");
                ViewData["Configuraciones"] = new List<PropiedadValor>()
                {
                    //new PropiedadValor { Propiedad = "IsConfiguracionListadoBajasGestionar", Valor = "true" },
                    //new PropiedadValor { Propiedad = "IsConfiguracionBajasGestionarEditar", Valor = (id != null).ToString() },
                    new PropiedadValor { Propiedad = "IsConfiguracionListadoMovilizaciones", Valor = "true" },
                    new PropiedadValor { Propiedad = "IsConfiguracionDatosActivo", Valor = "true" }
                };
                if (id != null)
                {
                    var response = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ObtenerMovilizacionActivosFijos");
                    if (!response.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoActivosFijosBaja));

                    var movilizacionActivoFijo = JsonConvert.DeserializeObject<MovilizacionActivoFijo>(response.Resultado.ToString());
                    ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = movilizacionActivoFijo.MovilizacionActivoFijoDetalle.Select(c => new RecepcionActivoFijoDetalleSeleccionado
                    {
                        RecepcionActivoFijoDetalle = c.RecepcionActivoFijoDetalle,
                        Seleccionado = true
                    }).ToList();
                }
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = new List<RecepcionActivoFijoDetalleSeleccionado>();
                return View();
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoMovilizacionActivosFijos));
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
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con estado Recepcionado", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
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
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos por área usuaria y clasificado por funcionario", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
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
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando mantenimientos de activos fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
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
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con estado Recepcionado con número de póliza asignado", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
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
        public async Task<SelectList> ObtenerSelectListLibroActivoFijo(int idSucursal, int? idLibroActivoFijo = null)
        {
            try
            {
                var listaLibroActivoFijo = idSucursal != -1 ? await apiServicio.Listar<LibroActivoFijo>(new Uri(WebApp.BaseAddressRM), $"api/LibroActivoFijo/ListarLibrosActivoFijoPorSucursal/{idSucursal}") : new List<LibroActivoFijo>();
                return new SelectList(listaLibroActivoFijo, "IdLibroActivoFijo", "IdLibroActivoFijo", idLibroActivoFijo);
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