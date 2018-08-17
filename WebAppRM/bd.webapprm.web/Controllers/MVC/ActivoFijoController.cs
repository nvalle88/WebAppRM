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

namespace bd.webapprm.web.Controllers.MVC
{
    public class ActivoFijoController : Controller
    {
        private readonly IApiServicio apiServicio;
        private readonly IClaimsTransfer claimsTransfer;
        private readonly IUtiles utilesServicio;

        public ActivoFijoController(IApiServicio apiServicio, IClaimsTransfer claimsTransfer, IUtiles utilesServicio)
        {
            this.apiServicio = apiServicio;
            this.claimsTransfer = claimsTransfer;
            this.utilesServicio = utilesServicio;
        }

        #region Recepción
        public IActionResult Index()
        {
            return RedirectToAction("Recepcion");
        }

        public async Task<IActionResult> Recepcion(int? id)
        {
            try
            {
                ViewData["MotivoAlta"] = new SelectList(await apiServicio.Listar<MotivoAlta>(new Uri(WebApp.BaseAddressRM), "api/MotivoAlta/ListarMotivoAlta"), "IdMotivoAlta", "Descripcion");
                ViewData["FondoFinanciamiento"] = new SelectList(await apiServicio.Listar<FondoFinanciamiento>(new Uri(WebApp.BaseAddressRM), "api/FondoFinanciamiento/ListarFondoFinanciamiento"), "IdFondoFinanciamiento", "Nombre");
                ViewData["Proveedor"] = new SelectList(await apiServicio.ObtenerElementoAsync<List<Proveedor>>(new ProveedorTransfer { LineaServicio = LineasServicio.ActivosFijos, Activo = true }, new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedoresPorLineaServicioEstado"), "IdProveedor", "RazonSocial");

                var claimTransfer = claimsTransfer.ObtenerClaimsTransferHttpContext();
                ViewData["Sucursal"] = new SelectList(new List<Sucursal> { new Sucursal { IdSucursal = claimTransfer.IdSucursal, Nombre = claimTransfer.NombreSucursal } }, "IdSucursal", "Nombre");
                
                ViewData["CompaniaSeguro"] = new SelectList(await apiServicio.Listar<CompaniaSeguro>(new Uri(WebApp.BaseAddressRM), "api/CompaniaSeguro/ListarCompaniaSeguro"), "IdCompaniaSeguro", "Nombre");

                if (id != null)
                {
                    var response = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), ViewData["IsRevisionActivoFijo"] != null ? "api/ActivosFijos/ObtenerRecepcionActivoFijoValidacionTecnica" : ViewData["IsPolizaSeguro"] != null || ViewData["IsVistaDetalles"] != null ? "api/ActivosFijos/ObtenerRecepcionPolizaSeguroActivoFijo" : "api/ActivosFijos/ObtenerRecepcionActivoFijo");
                    if (!response.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ActivosFijosRecepcionados));

                    var recepcionActivoFijo = JsonConvert.DeserializeObject<RecepcionActivoFijo>(response.Resultado.ToString());
                    var listadoRecepcionActivoFijoDetalle = recepcionActivoFijo.RecepcionActivoFijoDetalle.ToList();
                    ViewData["ListadoRecepcionActivoFijoDetalle"] = listadoRecepcionActivoFijoDetalle.Select(c=> new RecepcionActivoFijoDetalleSeleccionado { RecepcionActivoFijoDetalle = c, Seleccionado = false }).ToList();
                    ViewData["ListadoDocumentoActivoFijo"] = recepcionActivoFijo?.DocumentoActivoFijo?.ToList() ?? new List<DocumentoActivoFijo>();
                    return View(nameof(Recepcion), listadoRecepcionActivoFijoDetalle[0]);
                }
                return View(nameof(Recepcion));
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", claimsTransfer.IsADMIGrupo(ADMI_Grupos.AdminAF) ? nameof(ActivosFijosRecepcionados) : nameof(ActivosFijosRecepcionadosSinPoliza));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recepcion(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle)
        {
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Recepcionado }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.ValidacionTecnica }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                var listaEstado = await apiServicio.Listar<Estado>(new Uri(WebApp.BaseAddressTH), "api/Estados/ListarEstados");

                var response = new Response();
                int idRecepcionActivoFijo = 0;
                var listaRecepcionActivoFijoUbicacionTransfer = new List<RecepcionActivoFijoDetalle>();
                var listaFormDatosEspecificos = Request.Form.Where(c => c.Key.StartsWith("hhNombreActivoFijo_"));
                foreach (var item in listaFormDatosEspecificos)
                {
                    int posFormDatoEspecifico = int.Parse(item.Key.ToString().Split('_')[1]);
                    int idSubclaseActivoFijo = int.Parse(Request.Form[$"hhIdSubclaseActivoFijo_{posFormDatoEspecifico}"]);
                    int idModelo = int.Parse(Request.Form[$"hhModelo_{posFormDatoEspecifico}"]);
                    var activoFijo = new ActivoFijo
                    {
                        IdActivoFijo = int.Parse(Request.Form[$"hhIdActivoFijo_{posFormDatoEspecifico}"]),
                        IdSubClaseActivoFijo = idSubclaseActivoFijo,
                        SubClaseActivoFijo = JsonConvert.DeserializeObject<SubClaseActivoFijo>((await apiServicio.SeleccionarAsync<Response>(idSubclaseActivoFijo.ToString(), new Uri(WebApp.BaseAddressRM), "api/SubClaseActivoFijo")).Resultado.ToString()),
                        Cantidad = int.Parse(Request.Form[$"hhCantidad_{posFormDatoEspecifico}"]),
                        IdModelo = idModelo,
                        Modelo = JsonConvert.DeserializeObject<Modelo>((await apiServicio.SeleccionarAsync<Response>(idModelo.ToString(), new Uri(WebApp.BaseAddressRM), "api/Modelo")).Resultado.ToString()),
                        Nombre = Request.Form[$"hhNombreActivoFijo_{posFormDatoEspecifico}"],
                        ValorCompra = decimal.Parse(Request.Form[$"hhValorCompra_{posFormDatoEspecifico}"]),
                        Depreciacion = bool.Parse(Request.Form[$"hhDepreciacion_{posFormDatoEspecifico}"]),
                        ValidacionTecnica = bool.Parse(Request.Form[$"hhValidacionTecnica_{posFormDatoEspecifico}"])
                    };

                    var arrBodega = new string[0];
                    var arrNumeroClaveCatastral = new string[0];
                    var arrNumeroChasis = new string[0];
                    var arrNumeroMotor = new string[0];
                    var arrPlaca = new string[0];
                    var arrSerie = new string[0];
                    var arrEmpleado = new string[0];
                    var arrCodigoSecuencial = new string[0];
                    var arrIdCodigoActivoFijo = new string[0];
                    var arrUbicacion = new string[0];
                    var arrComponentes = new string[0];
                    var arrIdsRecepcionActivoFijoDetalle = new string[0];

                    if (activoFijo.SubClaseActivoFijo.ClaseActivoFijo.CategoriaActivoFijo.Nombre == Categorias.Edificio)
                        arrNumeroClaveCatastral = Request.Form[$"hhNumeroClaveCatastral_{posFormDatoEspecifico}"].ToString().Split(',');
                    else if (activoFijo.SubClaseActivoFijo.ClaseActivoFijo.CategoriaActivoFijo.Nombre == Categorias.Vehiculo)
                    {
                        arrNumeroChasis = Request.Form[$"hhNumeroChasis_{posFormDatoEspecifico}"].ToString().Split(',');
                        arrNumeroMotor = Request.Form[$"hhNumeroMotor_{posFormDatoEspecifico}"].ToString().Split(',');
                        arrPlaca = Request.Form[$"hhPlaca_{posFormDatoEspecifico}"].ToString().Split(',');
                    }
                    arrSerie = Request.Form[$"hhSerie_{posFormDatoEspecifico}"].ToString().Split(',');
                    arrEmpleado = Request.Form[$"hhEmpleado_{posFormDatoEspecifico}"].ToString().Split(',');
                    arrCodigoSecuencial = Request.Form[$"hhCodigoSecuencial_{posFormDatoEspecifico}"].ToString().Split(',');
                    arrIdCodigoActivoFijo = Request.Form[$"hhIdCodigoActivoFijo_{posFormDatoEspecifico}"].ToString().Split(',');
                    arrUbicacion = Request.Form[$"hhUbicacion_{posFormDatoEspecifico}"].ToString().Split(',');
                    arrIdsRecepcionActivoFijoDetalle = Request.Form[$"hhIdRecepcionActivoFijoDetalle_{posFormDatoEspecifico}"].ToString().Split(',');
                    arrBodega = Request.Form[$"hhBodega_{posFormDatoEspecifico}"].ToString().Split(',');
                    arrComponentes = Request.Form[$"hhComponentes_{posFormDatoEspecifico}"].ToString().Split('_');

                    for (int i = 0; i < arrBodega.Length; i++)
                    {
                        var rafd = new RecepcionActivoFijoDetalle
                        {
                            IdActivoFijo = activoFijo.IdActivoFijo,
                            ActivoFijo = activoFijo,
                            IdRecepcionActivoFijo = recepcionActivoFijoDetalle.IdRecepcionActivoFijo,
                            RecepcionActivoFijo = recepcionActivoFijoDetalle.RecepcionActivoFijo,
                            CodigoActivoFijo = new CodigoActivoFijo(),
                            ComponentesActivoFijoOrigen = new List<ComponenteActivoFijo>(),
                            UbicacionActivoFijoActual = new UbicacionActivoFijo
                            {
                                FechaUbicacion = recepcionActivoFijoDetalle.RecepcionActivoFijo.FechaRecepcion
                            }
                        };

                        int? idRecepcionActivoFijoDetalle = utilesServicio.TryParseInt(bd.webapprm.web.Helpers.StringExtensions.UnDash(arrIdsRecepcionActivoFijoDetalle[i]));
                        rafd.IdRecepcionActivoFijoDetalle = idRecepcionActivoFijoDetalle ?? 0;

                        int? idCodigoActivoFijo = utilesServicio.TryParseInt(bd.webapprm.web.Helpers.StringExtensions.UnDash(arrIdCodigoActivoFijo[i]));
                        rafd.CodigoActivoFijo.IdCodigoActivoFijo = idCodigoActivoFijo ?? 0;
                        rafd.IdCodigoActivoFijo = rafd.CodigoActivoFijo.IdCodigoActivoFijo;

                        if (activoFijo.SubClaseActivoFijo.ClaseActivoFijo.CategoriaActivoFijo.Nombre == Categorias.Edificio)
                        {
                            rafd.RecepcionActivoFijoDetalleEdificio = new RecepcionActivoFijoDetalleEdificio();
                            rafd.RecepcionActivoFijoDetalleEdificio.NumeroClaveCatastral = utilesServicio.TryParseString(bd.webapprm.web.Helpers.StringExtensions.UnDash(arrNumeroClaveCatastral[i]));
                            rafd.RecepcionActivoFijoDetalleEdificio.IdRecepcionActivoFijoDetalle = rafd.IdRecepcionActivoFijoDetalle;
                        }
                        else if (activoFijo.SubClaseActivoFijo.ClaseActivoFijo.CategoriaActivoFijo.Nombre == Categorias.Vehiculo)
                        {
                            rafd.RecepcionActivoFijoDetalleVehiculo = new RecepcionActivoFijoDetalleVehiculo();
                            rafd.RecepcionActivoFijoDetalleVehiculo.NumeroChasis = utilesServicio.TryParseString(bd.webapprm.web.Helpers.StringExtensions.UnDash(arrNumeroChasis[i]));
                            rafd.RecepcionActivoFijoDetalleVehiculo.NumeroMotor = utilesServicio.TryParseString(bd.webapprm.web.Helpers.StringExtensions.UnDash(arrNumeroMotor[i]));
                            rafd.RecepcionActivoFijoDetalleVehiculo.Placa = utilesServicio.TryParseString(bd.webapprm.web.Helpers.StringExtensions.UnDash(arrPlaca[i]));
                            rafd.RecepcionActivoFijoDetalleVehiculo.IdRecepcionActivoFijoDetalle = rafd.IdRecepcionActivoFijoDetalle;
                        }
                        rafd.Serie = utilesServicio.TryParseString(bd.webapprm.web.Helpers.StringExtensions.UnDash(arrSerie[i]));
                        rafd.CodigoActivoFijo.Codigosecuencial = arrCodigoSecuencial[i];

                        int? idUbicacionActivoFijo = utilesServicio.TryParseInt(bd.webapprm.web.Helpers.StringExtensions.UnDash(arrUbicacion[i]));
                        rafd.UbicacionActivoFijoActual.IdUbicacionActivoFijo = idUbicacionActivoFijo ?? 0;

                        int? idBodega = utilesServicio.TryParseInt(bd.webapprm.web.Helpers.StringExtensions.UnDash(arrBodega[i]));
                        rafd.UbicacionActivoFijoActual.IdBodega = idBodega;
                        if (idBodega != null)
                            rafd.UbicacionActivoFijoActual.Bodega = JsonConvert.DeserializeObject<Bodega>((await apiServicio.SeleccionarAsync<Response>(idBodega.ToString(), new Uri(WebApp.BaseAddressRM), "api/Bodega")).Resultado.ToString());

                        int? idEmpleado = utilesServicio.TryParseInt(bd.webapprm.web.Helpers.StringExtensions.UnDash(arrEmpleado[i]));
                        rafd.UbicacionActivoFijoActual.IdEmpleado = idEmpleado;
                        if (idEmpleado != null)
                            rafd.UbicacionActivoFijoActual.Empleado = JsonConvert.DeserializeObject<Empleado>((await apiServicio.SeleccionarAsync<Response>(idEmpleado.ToString(), new Uri(WebApp.BaseAddressTH), "api/Empleados")).Resultado.ToString());

                        rafd.Estado = listaEstado.SingleOrDefault(c => c.Nombre == (!activoFijo.ValidacionTecnica ? Estados.Recepcionado : Estados.ValidacionTecnica));
                        rafd.IdEstado = rafd.Estado.IdEstado;

                        var componentes = arrComponentes[i].Split(',');
                        foreach (var comp in componentes)
                        {
                            string idRecepcionActivoFijoDetalleComponente = bd.webapprm.web.Helpers.StringExtensions.UnDash(comp).Trim();
                            if (!String.IsNullOrEmpty(idRecepcionActivoFijoDetalleComponente) && idRecepcionActivoFijoDetalleComponente != "0")
                                rafd.ComponentesActivoFijoOrigen.Add(new ComponenteActivoFijo { IdRecepcionActivoFijoDetalleOrigen = rafd.IdRecepcionActivoFijoDetalle, IdRecepcionActivoFijoDetalleComponente = int.Parse(idRecepcionActivoFijoDetalleComponente), FechaAdicion = recepcionActivoFijoDetalle.RecepcionActivoFijo.FechaRecepcion });
                        }
                        listaRecepcionActivoFijoUbicacionTransfer.Add(rafd);
                    }
                }

                if (recepcionActivoFijoDetalle.IdRecepcionActivoFijo == 0)
                {
                    response = await apiServicio.InsertarAsync(listaRecepcionActivoFijoUbicacionTransfer, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/InsertarRecepcionActivoFijo");
                    if (response.IsSuccess)
                    {
                        idRecepcionActivoFijo = JsonConvert.DeserializeObject<List<RecepcionActivoFijoDetalle>>(response.Resultado.ToString()).FirstOrDefault().IdRecepcionActivoFijo;
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha creado una recepción de activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Recepción de Activo Fijo:", idRecepcionActivoFijo) });
                    }
                }
                else
                {
                    idRecepcionActivoFijo = recepcionActivoFijoDetalle.IdRecepcionActivoFijo;
                    response = await apiServicio.EditarAsync(recepcionActivoFijoDetalle.IdRecepcionActivoFijo.ToString(), listaRecepcionActivoFijoUbicacionTransfer, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos");
                    if (response.IsSuccess)
                        await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha editado una recepción de activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Recepción de Activo Fijo:", idRecepcionActivoFijo) });
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
                                var activoFijoDocumentoTransfer = new DocumentoActivoFijoTransfer { Nombre = item.FileName, Fichero = data, IdRecepcionActivoFijo = idRecepcionActivoFijo };
                                responseFile = await apiServicio.InsertarAsync(activoFijoDocumentoTransfer, new Uri(WebApp.BaseAddressRM), "api/DocumentoActivoFijo/UploadFiles");
                                if (responseFile != null && responseFile.IsSuccess)
                                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha subido un archivo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Documento de Activo Fijo:", activoFijoDocumentoTransfer.Nombre) });
                            }
                        }
                    }
                }

                if (response.IsSuccess)
                    return this.Redireccionar(responseFile.IsSuccess ? $"{Mensaje.Informacion}|{Mensaje.Satisfactorio}" : $"{Mensaje.Aviso}|{Mensaje.ErrorUploadFiles}", nameof(ActivosFijosRecepcionados));

                ViewData["ListadoRecepcionActivoFijoDetalle"] = listaRecepcionActivoFijoUbicacionTransfer.Select(c => new RecepcionActivoFijoDetalleSeleccionado { RecepcionActivoFijoDetalle = c, Seleccionado = false }).ToList();

                ViewData["MotivoAlta"] = new SelectList(await apiServicio.Listar<MotivoAlta>(new Uri(WebApp.BaseAddressRM), "api/MotivoAlta/ListarMotivoAlta"), "IdMotivoAlta", "Descripcion");
                ViewData["FondoFinanciamiento"] = new SelectList(await apiServicio.Listar<FondoFinanciamiento>(new Uri(WebApp.BaseAddressRM), "api/FondoFinanciamiento/ListarFondoFinanciamiento"), "IdFondoFinanciamiento", "Nombre");
                ViewData["Proveedor"] = new SelectList(await apiServicio.ObtenerElementoAsync<List<Proveedor>>(new ProveedorTransfer { LineaServicio = LineasServicio.ActivosFijos, Activo = true }, new Uri(WebApp.BaseAddressRM), "api/Proveedor/ListarProveedoresPorLineaServicioEstado"), "IdProveedor", "RazonSocial");

                var claimTransfer = claimsTransfer.ObtenerClaimsTransferHttpContext();
                ViewData["Sucursal"] = new SelectList(new List<Sucursal> { new Sucursal { IdSucursal = claimTransfer.IdSucursal, Nombre = claimTransfer.NombreSucursal } }, "IdSucursal", "Nombre");
                
                ViewData["CompaniaSeguro"] = new SelectList(await apiServicio.Listar<CompaniaSeguro>(new Uri(WebApp.BaseAddressRM), "api/CompaniaSeguro/ListarCompaniaSeguro"), "IdCompaniaSeguro", "Nombre");

                ViewData["Error"] = response.Message;
                return View(nameof(Recepcion), recepcionActivoFijoDetalle);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando recepción de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ActivosFijosRecepcionados));
            }
        }

        public async Task<IActionResult> DetallesRecepcion(int? id)
        {
            ViewData["IsVistaDetalles"] = true;
            return await Recepcion(id);
        }

        public async Task<IActionResult> DeleteRecepcion(string id, bool activoFijoRecepcionado)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/EliminarRecepcionActivoFijo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Sistema", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webapprm" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}", activoFijoRecepcionado ? nameof(ActivosFijosValidacionTecnica) : nameof(ActivosFijosRecepcionados));
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}", activoFijoRecepcionado ? nameof(ActivosFijosValidacionTecnica) : nameof(ActivosFijosRecepcionados));
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar recepción de activos fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}", activoFijoRecepcionado ? nameof(ActivosFijosValidacionTecnica) : nameof(ActivosFijosRecepcionados));
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
            var lista = new List<RecepcionActivoFijo>();
            ViewData["IsListadoRecepciones"] = true;
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarRecepcionActivo");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos recepcionados", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoRecepcionActivoFijo", lista);
        }

        public async Task<IActionResult> ActivosFijosValidacionTecnica()
        {
            var lista = new List<RecepcionActivoFijo>();
            ViewData["IsListadoValidacionesTecnicas"] = true;
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarRecepcionActivoValidacionTecnica");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando activos fijos con validación técnica", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoRecepcionActivoFijo", lista);
        }

        [HttpPost]
        public async Task<IActionResult> GestionarAprobacionActivoFijo(List<int> arrIdsActivoFijo, bool aprobacion)
        {
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Recepcionado }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Desaprobado }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                var response = await apiServicio.InsertarAsync(new AprobacionActivoFijoTransfer { IdsActivoFijo = arrIdsActivoFijo, NuevoEstadoActivoFijo = aprobacion ? Estados.Recepcionado : Estados.Desaprobado, ValidacionTecnica = !aprobacion }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/AprobacionActivoFijo");
                return StatusCode(response.IsSuccess ? 200 : 500);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando aprobación Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ActivosFijosValidacionTecnica));
            }
        }

        public async Task<IActionResult> RevisionActivoFijo(int? id)
        {
            ViewData["IsSeleccion"] = true;
            ViewData["IsRevisionActivoFijo"] = true;
            return await Recepcion(id);
        }

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
                    var response = await apiServicio.InsertarAsync(new RecepcionActivoFijoDetalle
                    {
                        IdRecepcionActivoFijoDetalle = rafd.IdRecepcionActivoFijoDetalle,
                        Serie = rafd.Serie,
                        RecepcionActivoFijoDetalleEdificio = new RecepcionActivoFijoDetalleEdificio
                        {
                            IdRecepcionActivoFijoDetalle = rafd.IdRecepcionActivoFijoDetalle,
                            NumeroClaveCatastral = rafd.NumeroClaveCatastral
                        },
                        RecepcionActivoFijoDetalleVehiculo = new RecepcionActivoFijoDetalleVehiculo
                        {
                            IdRecepcionActivoFijoDetalle = rafd.IdRecepcionActivoFijoDetalle,
                            NumeroChasis = rafd.NumeroChasis,
                            NumeroMotor = rafd.NumeroMotor,
                            Placa = rafd.Placa
                        }
                    }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ValidacionRecepcionActivoFijoDetalleDatosEspecificos");

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
        public async Task<IActionResult> DetallesActivoFijoResult(List<IdRecepcionActivoFijoDetalleSeleccionado> listadoRecepcionActivoFijoDetalleSeleccionado, ListadoDetallesActivosFijosViewModel arrConfiguraciones, bool mostrarNoSeleccionados)
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
            ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionListadoComponentes: true);
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
            ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionSeleccion: true, IsConfiguracionSeleccionComponentes: true);
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

        [HttpPost]
        public async Task<IActionResult> DatosActivoFijoResult(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle, List<RecepcionActivoFijoDetalle> listadoRecepcionActivoFijoDetalle, bool isEditar, bool isVistaDetalles)
        {
            try
            {
                ViewData["TipoActivoFijo"] = new SelectList(await apiServicio.Listar<TipoActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/TipoActivoFijo/ListarTipoActivoFijos"), "IdTipoActivoFijo", "Nombre");
                ViewData["ClaseActivoFijo"] = await ObtenerSelectListClaseActivoFijo(recepcionActivoFijoDetalle.ActivoFijo == null ? (ViewData["TipoActivoFijo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["TipoActivoFijo"] as SelectList).FirstOrDefault().Value) : -1 : recepcionActivoFijoDetalle?.ActivoFijo?.SubClaseActivoFijo?.ClaseActivoFijo?.IdTipoActivoFijo ?? -1);
                ViewData["SubClaseActivoFijo"] = await ObtenerSelectListSubClaseActivoFijo(recepcionActivoFijoDetalle.ActivoFijo == null ? (ViewData["ClaseActivoFijo"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["ClaseActivoFijo"] as SelectList).FirstOrDefault().Value) : -1 : recepcionActivoFijoDetalle?.ActivoFijo?.SubClaseActivoFijo?.IdClaseActivoFijo ?? -1);
                
                ViewData["Marca"] = new SelectList(await apiServicio.Listar<Marca>(new Uri(WebApp.BaseAddressRM), "api/Marca/ListarMarca"), "IdMarca", "Nombre");
                ViewData["Modelo"] = await ObtenerSelectListModelo(recepcionActivoFijoDetalle.ActivoFijo == null ? (ViewData["Marca"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Marca"] as SelectList).FirstOrDefault().Value) : -1 : recepcionActivoFijoDetalle?.ActivoFijo?.Modelo?.IdMarca ?? -1);
                
                foreach (var item in listadoRecepcionActivoFijoDetalle)
                {
                    if (item.UbicacionActivoFijoActual.IdBodega != null)
                        item.UbicacionActivoFijoActual.Bodega = JsonConvert.DeserializeObject<Bodega>((await apiServicio.SeleccionarAsync<Response>(item.UbicacionActivoFijoActual.IdBodega.ToString(), new Uri(WebApp.BaseAddressRM), "api/Bodega")).Resultado.ToString());

                    if (item.UbicacionActivoFijoActual.IdEmpleado != null)
                        item.UbicacionActivoFijoActual.Empleado = JsonConvert.DeserializeObject<Empleado>((await apiServicio.SeleccionarAsync<Response>(item.UbicacionActivoFijoActual.IdEmpleado.ToString(), new Uri(WebApp.BaseAddressTH), "api/Empleados")).Resultado.ToString());
                }

                ViewData["ListadoRecepcionActivoFijoDetalle"] = listadoRecepcionActivoFijoDetalle;
                ViewData["IsEditar"] = isEditar;
                ViewData["IsVistaDetalles"] = isVistaDetalles;

                if (recepcionActivoFijoDetalle.ActivoFijo == null)
                {
                    var primeraCategoria = await apiServicio.ObtenerElementoAsync<CategoriaActivoFijo>(null, new Uri(WebApp.BaseAddressRM), "api/CategoriaActivoFijo/ObtenerPrimeraCategoria");
                    ViewData["Categoria"] = primeraCategoria?.Nombre ?? Categorias.Edificio;
                }
                else
                {
                    var claseActivoFijo = JsonConvert.DeserializeObject<ClaseActivoFijo>((await apiServicio.SeleccionarAsync<Response>(recepcionActivoFijoDetalle?.ActivoFijo?.SubClaseActivoFijo?.IdClaseActivoFijo.ToString(), new Uri(WebApp.BaseAddressTH), "api/ClaseActivoFijo")).Resultado.ToString());
                    ViewData["Categoria"] = claseActivoFijo?.CategoriaActivoFijo?.Nombre ?? Categorias.Edificio;
                }
            }
            catch (Exception)
            { }
            return PartialView("_DatosActivoFijo", recepcionActivoFijoDetalle);
        }

        [HttpPost]
        public async Task<IActionResult> PolizaSeguroResult(int? idSubclaseActivoFijo)
        {
            try
            {
                var subclaseActivoFijo = JsonConvert.DeserializeObject<SubClaseActivoFijo>((await apiServicio.SeleccionarAsync<Response>(idSubclaseActivoFijo.ToString(), new Uri(WebApp.BaseAddressRM), "api/SubClaseActivoFijo")).Resultado.ToString());
                ViewData["Ramo"] = new SelectList(new List<Ramo>() { subclaseActivoFijo?.Subramo?.Ramo }, "IdRamo", "Nombre");
                ViewData["Subramo"] = new SelectList(new List<Subramo>() { subclaseActivoFijo?.Subramo }, "IdSubramo", "Nombre");
                return PartialView("_PolizaSeguroDisabled", new RecepcionActivoFijoDetalle());
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
        #endregion

        #region Póliza de Seguro
        public async Task<IActionResult> ActivosFijosRecepcionadosSinPoliza()
        {
            var lista = new List<RecepcionActivoFijo>();
            ViewData["IsListadoPolizaSeguroSinAsignar"] = true;
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarRecepcionActivoFijoSinPoliza");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando recepciones de activos fijos sin póliza de seguro", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoRecepcionActivoFijo", lista);
        }

        public async Task<IActionResult> ActivosFijosRecepcionadosConPoliza()
        {
            var lista = new List<RecepcionActivoFijo>();
            ViewData["IsListadoPolizaSeguroAsignadas"] = true;
            try
            {
                lista = await apiServicio.Listar<RecepcionActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarRecepcionActivoFijoConPoliza");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando recepciones de activos fijos con póliza de seguro", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoRecepcionActivoFijo", lista);
        }

        public async Task<IActionResult> AsignarPolizaSeguro(int? id)
        {
            ViewData["IsPolizaSeguro"] = true;
            ViewData["IsVistaDetalles"] = true;
            return await Recepcion(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarPolizaSeguro(RecepcionActivoFijoDetalle recepcionActivoFijoDetalle)
        {
            try
            {
                var response = await apiServicio.InsertarAsync(recepcionActivoFijoDetalle, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/AsignarPolizaSeguro");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha asignado el número de póliza a una recepción de activos fijos", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Póliza de seguro Activo Fijo:", recepcionActivoFijoDetalle.RecepcionActivoFijo.PolizaSeguroActivoFijo.NumeroPoliza) });
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ActivosFijosRecepcionadosSinPoliza));
                }
                ViewData["Error"] = response.Message;
                ViewData["IsPolizaSeguro"] = true;
                ViewData["IsVistaDetalles"] = true;
                return await Recepcion(recepcionActivoFijoDetalle.IdRecepcionActivoFijo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Asignando Póliza de seguro", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Error}", nameof(ActivosFijosRecepcionadosSinPoliza));
            }
        }
        #endregion

        #region Altas
        public async Task<IActionResult> ListadoActivosFijosAlta()
        {
            var lista = new List<RecepcionActivoFijoDetalleSeleccionado>();
            ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionListadoGenerales: true, IsConfiguracionOpciones: true);
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(new IdRecepcionActivoFijoDetalleSeleccionadoEstado { Estados = new List<string> { Estados.Alta }, ListaIdRecepcionActivoFijoDetalleSeleccionado = new List<IdRecepcionActivoFijoDetalleSeleccionado>() }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijoSeleccionadoPorEstado");
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
                ViewData["MotivoAlta"] = new SelectList((await apiServicio.Listar<MotivoAlta>(new Uri(WebApp.BaseAddressRM), "api/MotivoAlta/ListarMotivoAlta")).Where(c => c.Descripcion.ToLower().Trim() != "adición"), "IdMotivoAlta", "Descripcion");
                ViewData["TipoUtilizacionAlta"] = new SelectList(await apiServicio.Listar<TipoUtilizacionAlta>(new Uri(WebApp.BaseAddressRM), "api/TipoUtilizacionAlta/ListarTipoUtilizacionAlta"), "IdTipoUtilizacionAlta", "Nombre");
                ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionListadoAltasGestionar: true, IsConfiguracionAltasGestionarEditar: id != null, IsVisualizarNumeroRecepcion: true);
                
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
                    return View(nameof(GestionarAlta), altaActivoFijo);
                }
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = new List<RecepcionActivoFijoDetalleSeleccionado>();
                return View(nameof(GestionarAlta));
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
                    int idRecepcionActivoFijoDetalle = int.Parse(Request.Form[$"hIdRecepcionActivoFijoDetalle_{posFormDatoEspecifico}"]);
                    int idUbicacion = int.Parse(Request.Form[$"hUbicacion_{posFormDatoEspecifico}"]);
                    string[] arrComponentes = Request.Form[$"hComponentes_{posFormDatoEspecifico}"].ToString().Trim().Split(',');

                    var rafd = new RecepcionActivoFijoDetalle { IdRecepcionActivoFijoDetalle = idRecepcionActivoFijoDetalle, UbicacionActivoFijoActual = new UbicacionActivoFijo { IdEmpleado = idEmpleado } };
                    foreach (var comp in arrComponentes)
                    {
                        if (!String.IsNullOrEmpty(comp) && comp != "0")
                            rafd.ComponentesActivoFijoOrigen.Add(new ComponenteActivoFijo { IdRecepcionActivoFijoDetalleOrigen = rafd.IdRecepcionActivoFijoDetalle, IdRecepcionActivoFijoDetalleComponente = int.Parse(comp.Trim()), FechaAdicion = altaActivoFijo.FechaAlta });
                    }
                    altaActivoFijo.AltaActivoFijoDetalle.Add(new AltaActivoFijoDetalle {
                        IdRecepcionActivoFijoDetalle = rafd.IdRecepcionActivoFijoDetalle,
                        IdAltaActivoFijo = altaActivoFijo.IdAltaActivoFijo,
                        IdTipoUtilizacionAlta = idTipoUtilizacionAlta,
                        RecepcionActivoFijoDetalle = rafd,
                        IdUbicacionActivoFijo = idUbicacion
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

                ViewData["MotivoAlta"] = new SelectList((await apiServicio.Listar<MotivoAlta>(new Uri(WebApp.BaseAddressRM), "api/MotivoAlta/ListarMotivoAlta")).Where(c => c.Descripcion.ToLower().Trim() != "adición"), "IdMotivoAlta", "Descripcion");
                ViewData["TipoUtilizacionAlta"] = new SelectList(await apiServicio.Listar<TipoUtilizacionAlta>(new Uri(WebApp.BaseAddressRM), "api/TipoUtilizacionAlta/ListarTipoUtilizacionAlta"), "IdTipoUtilizacionAlta", "Nombre");
                ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionListadoAltasGestionar: true, IsConfiguracionAltasGestionarEditar: altaActivoFijo.IdAltaActivoFijo > 0, IsVisualizarNumeroRecepcion: true);
                
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
                return View(nameof(GestionarAlta), altaActivoFijo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando alta de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListadoActivosFijosAlta));
            }
        }

        public async Task<IActionResult> DetallesAlta(int? id)
        {
            ViewData["IsVistaDetalles"] = true;
            return await GestionarAlta(id);
        }

        [HttpPost]
        public async Task<IActionResult> NumeroRecepcionResult()
        {
            try
            {
                ViewData["IdRecepcionActivoFijo"] = new SelectList(await apiServicio.Listar<int>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarIdsRecepcionesActivosFijos"));
            }
            catch (Exception)
            { }
            return PartialView("_RecepcionSelect", new RecepcionActivoFijo());
        }

        [HttpPost]
        public async Task<IActionResult> ListadoActivosFijosSeleccionAltaResult(List<IdRecepcionActivoFijoDetalleSeleccionado> listadoRecepcionActivoFijoDetalleSeleccionado, List<IdRecepcionActivoFijoDetalleSeleccionado> objAdicional, int idRecepcionActivoFijo)
        {
            var lista = new List<RecepcionActivoFijoDetalleSeleccionado>();
            ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionSeleccion: true, IsConfiguracionSeleccionAltas: true, IsVisualizarNumeroRecepcion: true);
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(new IdRecepcionActivoFijoDetalleSeleccionadoIdsInicialesAltaBaja
                {
                    ListaIdRecepcionActivoFijoDetalleSeleccionado = listadoRecepcionActivoFijoDetalleSeleccionado,
                    ListaIdRecepcionActivoFijoDetalleSeleccionadoInicialesAltaBaja = objAdicional,
                    IdRecepcionActivoFijo = idRecepcionActivoFijo
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

        public async Task<IActionResult> ReversarAlta(int id)
        {
            try
            {
                var response = await apiServicio.InsertarAsync(id, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ReversarAltaActivosFijos");
                return this.Redireccionar(response.IsSuccess ? $"{Mensaje.Informacion}|{Mensaje.Satisfactorio}" : $"{Mensaje.Error}|{response.Message}", nameof(ListadoAltaActivosFijos));
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}", nameof(ListadoAltaActivosFijos));
            }
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

        public async Task<IActionResult> GestionarCambioCustodio(int? id)
        {
            try
            {
                ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionSeleccion: ViewData["IsVistaDetalles"] == null, IsConfiguracionSeleccionBajas: true);
                if (id != null)
                {
                    var response = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ObtenerTransferenciaActivoFijo");
                    if (!response.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoCambioCustodio));

                    var transferenciaActivoFijo = JsonConvert.DeserializeObject<TransferenciaActivoFijo>(response.Resultado.ToString());
                    ViewData["Empleado"] = await ObtenerSelectListEmpleado(transferenciaActivoFijo.TransferenciaActivoFijoDetalle.FirstOrDefault().UbicacionActivoFijoOrigen.Empleado.Dependencia.IdSucursal);
                    ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = transferenciaActivoFijo.TransferenciaActivoFijoDetalle.Select(c => new RecepcionActivoFijoDetalleSeleccionado { RecepcionActivoFijoDetalle = c.RecepcionActivoFijoDetalle, Seleccionado = true }).ToList();
                    var cambioCustodioViewModel = new CambioCustodioViewModel
                    {
                        IdTransferencia = transferenciaActivoFijo.IdTransferenciaActivoFijo,
                        IdEmpleadoEntrega = (int)transferenciaActivoFijo.TransferenciaActivoFijoDetalle.FirstOrDefault().UbicacionActivoFijoOrigen.IdEmpleado,
                        IdEmpleadoRecibe = (int)transferenciaActivoFijo.TransferenciaActivoFijoDetalle.FirstOrDefault().UbicacionActivoFijoDestino.IdEmpleado,
                        EmpleadoEntrega = transferenciaActivoFijo.TransferenciaActivoFijoDetalle.FirstOrDefault().UbicacionActivoFijoOrigen.Empleado,
                        EmpleadoRecibe = transferenciaActivoFijo.TransferenciaActivoFijoDetalle.FirstOrDefault().UbicacionActivoFijoDestino.Empleado,
                        Observaciones = transferenciaActivoFijo.Observaciones
                    };
                    return View(nameof(GestionarCambioCustodio), cambioCustodioViewModel);
                }
                var claimTransfer = claimsTransfer.ObtenerClaimsTransferHttpContext();
                ViewData["Empleado"] = await ObtenerSelectListEmpleado(claimTransfer.IdSucursal);
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(new CambioCustodioViewModel { IdEmpleadoEntrega = (ViewData["Empleado"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["Empleado"] as SelectList).FirstOrDefault().Value) : -1, ListadoIdRecepcionActivoFijoDetalle = new List<int>() }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijoSeleccionadoPorEmpleado");
                return View(nameof(GestionarCambioCustodio));
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
                ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionSeleccion: true, IsConfiguracionSeleccionBajas: true);
                var claimTransfer = claimsTransfer.ObtenerClaimsTransferHttpContext();
                ViewData["Empleado"] = await ObtenerSelectListEmpleado(claimTransfer.IdSucursal);
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = listaRecepcionActivoFijoDetalleSeleccionado;
                return View(cambioCustodioModel);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando cambio de custodio de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListadoCambioCustodio));
            }
        }

        public async Task<IActionResult> DetallesCambioCustodio(int? id)
        {
            ViewData["IsVistaDetalles"] = true;
            return await GestionarCambioCustodio(id);
        }

        public async Task<IActionResult> ExportarPDfCambioCustodio(int? id)
        {
            try
            {
                var fileContents = await apiServicio.ObtenerElementoAsync<byte[]>(id, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/PDFTransferenciaCambioCustodio");
                if (fileContents.Length > 0)
                {
                    var filename = "Acta de entrega al custodio.pdf";
                    return File(fileContents, MimeTypes.GetMimeType(filename), filename);
                }
            }
            catch (Exception)
            { }
            return StatusCode(500);
        }

        [HttpPost]
        public async Task<IActionResult> ListadoActivosFijosCustodioResult(int idEmpleado)
        {
            var lista = new List<RecepcionActivoFijoDetalleSeleccionado>();
            try
            {
                ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionSeleccion: true, IsConfiguracionSeleccionBajas: true);
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
                lista = await apiServicio.Listar<TransferenciaActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarTransferenciasCambiosUbicacionSolicitudActivosFijos");
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
                lista = await apiServicio.Listar<TransferenciaActivoFijo>(new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarTransferenciasCambiosUbicacionCreadasActivosFijos");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando transferencias creadas de activos fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View("ListadoTransferenciasSucursales", lista);
        }

        public async Task<IActionResult> GestionarTransferenciaSucursal(int? id)
        {
            try
            {
                var claimTransfer = claimsTransfer.ObtenerClaimsTransferHttpContext();
                var listaSucursales = (await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal")).Where(c=> c.IdSucursal != claimTransfer.IdSucursal);
                if (id != null)
                {
                    var response = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ObtenerTransferenciaActivoFijo");
                    if (!response.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoTransferenciasCreadas));

                    var transferenciaActivoFijo = JsonConvert.DeserializeObject<TransferenciaActivoFijo>(response.Resultado.ToString());
                    var transferenciaActivoFijoDetalle = transferenciaActivoFijo.TransferenciaActivoFijoDetalle.FirstOrDefault();
                    var cambioUbicacionSucursalViewModel = new CambioUbicacionSucursalViewModel
                    {
                        IdTransferenciaActivoFijo = (int)id,
                        IdSucursalOrigen = transferenciaActivoFijoDetalle?.UbicacionActivoFijoOrigen?.Empleado?.Dependencia?.IdSucursal ?? transferenciaActivoFijoDetalle.UbicacionActivoFijoOrigen.Bodega.IdSucursal,
                        SucursalOrigen = transferenciaActivoFijoDetalle?.UbicacionActivoFijoOrigen?.Empleado?.Dependencia?.Sucursal ?? transferenciaActivoFijoDetalle.UbicacionActivoFijoOrigen.Bodega.Sucursal,
                        IdEmpleadoEntrega = (int)transferenciaActivoFijo.TransferenciaActivoFijoDetalle.FirstOrDefault().UbicacionActivoFijoOrigen.IdEmpleado,
                        EmpleadoEntrega = transferenciaActivoFijo.TransferenciaActivoFijoDetalle.FirstOrDefault().UbicacionActivoFijoOrigen.Empleado,
                        IdEmpleadoResponsableEnvio = (int)transferenciaActivoFijo.IdEmpleadoResponsableEnvio,
                        EmpleadoResponsableEnvio = transferenciaActivoFijo.EmpleadoResponsableEnvio,
                        IdSucursalDestino = transferenciaActivoFijoDetalle?.UbicacionActivoFijoDestino?.Empleado?.Dependencia?.IdSucursal ?? transferenciaActivoFijoDetalle.UbicacionActivoFijoDestino.Bodega.IdSucursal,
                        SucursalDestino = transferenciaActivoFijoDetalle?.UbicacionActivoFijoDestino?.Empleado?.Dependencia?.Sucursal ?? transferenciaActivoFijoDetalle.UbicacionActivoFijoDestino.Bodega.Sucursal,
                        IdEmpleadoRecibe = (int)transferenciaActivoFijo.TransferenciaActivoFijoDetalle.FirstOrDefault().UbicacionActivoFijoDestino.IdEmpleado,
                        EmpleadoRecibe = transferenciaActivoFijo.TransferenciaActivoFijoDetalle.FirstOrDefault().UbicacionActivoFijoDestino.Empleado,
                        IdEmpleadoResponsableRecibo = (int)transferenciaActivoFijo.IdEmpleadoResponsableRecibo,
                        EmpleadoResponsableRecibo = transferenciaActivoFijo.EmpleadoResponsableRecibo,
                        FechaTransferencia = transferenciaActivoFijo.FechaTransferencia,
                        Observaciones = transferenciaActivoFijo.Observaciones
                    };
                    ViewData["SucursalOrigen"] = new SelectList(new List<Sucursal> { new Sucursal { IdSucursal = claimTransfer.IdSucursal, Nombre = claimTransfer.NombreSucursal } }, "IdSucursal", "Nombre", cambioUbicacionSucursalViewModel.IdSucursalDestino);
                    ViewData["SucursalDestino"] = new SelectList(listaSucursales, "IdSucursal", "Nombre", cambioUbicacionSucursalViewModel.IdSucursalDestino);

                    var listadoEmpleadoSucursalOrigen = (await apiServicio.ObtenerElementoAsync<List<DatosBasicosEmpleadoViewModel>>(new EmpleadosPorSucursalViewModel { IdSucursal = cambioUbicacionSucursalViewModel.IdSucursalOrigen, EmpleadosActivos = true }, new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleadosPorSucursal")).Select(c => new ListaEmpleadoViewModel { IdEmpleado = c.IdEmpleado, NombreApellido = $"{c.Nombres} {c.Apellidos}" });
                    ViewData["EmpleadoEntrega"] = new SelectList(listadoEmpleadoSucursalOrigen, "IdEmpleado", "NombreApellido", cambioUbicacionSucursalViewModel.IdEmpleadoEntrega);
                    ViewData["EmpleadoResponsableEnvio"] = new SelectList(listadoEmpleadoSucursalOrigen, "IdEmpleado", "NombreApellido", cambioUbicacionSucursalViewModel.IdEmpleadoResponsableEnvio);

                    var listadoEmpleadoSucursalDestino = (await apiServicio.ObtenerElementoAsync<List<DatosBasicosEmpleadoViewModel>>(new EmpleadosPorSucursalViewModel { IdSucursal = cambioUbicacionSucursalViewModel.IdSucursalDestino, EmpleadosActivos = true }, new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleadosPorSucursal")).Select(c => new ListaEmpleadoViewModel { IdEmpleado = c.IdEmpleado, NombreApellido = $"{c.Nombres} {c.Apellidos}" });
                    ViewData["EmpleadoRecibe"] = new SelectList(listadoEmpleadoSucursalDestino, "IdEmpleado", "NombreApellido", cambioUbicacionSucursalViewModel.IdEmpleadoRecibe);
                    ViewData["EmpleadoResponsableRecibo"] = new SelectList(listadoEmpleadoSucursalDestino, "IdEmpleado", "NombreApellido", cambioUbicacionSucursalViewModel.IdEmpleadoResponsableRecibo);

                    ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = transferenciaActivoFijo.TransferenciaActivoFijoDetalle.Select(c => new RecepcionActivoFijoDetalleSeleccionado { RecepcionActivoFijoDetalle = c.RecepcionActivoFijoDetalle, Seleccionado = true }).ToList();
                    ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionSeleccionBajas: true);
                    return View(nameof(GestionarTransferenciaSucursal), cambioUbicacionSucursalViewModel);
                }
                ViewData["SucursalOrigen"] = new SelectList(new List<Sucursal> { new Sucursal { IdSucursal = claimTransfer.IdSucursal, Nombre = claimTransfer.NombreSucursal } }, "IdSucursal", "Nombre");
                ViewData["SucursalDestino"] = new SelectList(listaSucursales, "IdSucursal", "Nombre");
                int idSucursalDestino = (ViewData["SucursalDestino"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["SucursalDestino"] as SelectList).FirstOrDefault().Value) : -1;
                
                var listaEmpleadoSucursalOrigen = await apiServicio.ObtenerElementoAsync<List<DatosBasicosEmpleadoViewModel>>(new EmpleadosPorSucursalViewModel { IdSucursal = claimTransfer.IdSucursal, EmpleadosActivos = true }, new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleadosPorSucursal");
                ViewData["EmpleadoEntrega"] = new SelectList(listaEmpleadoSucursalOrigen.Select(c => new ListaEmpleadoViewModel { IdEmpleado = c.IdEmpleado, NombreApellido = $"{c.Nombres} {c.Apellidos}" }), "IdEmpleado", "NombreApellido");
                ViewData["EmpleadoResponsableEnvio"] = ViewData["EmpleadoEntrega"];

                var listaEmpleadoSucursalDestino = idSucursalDestino != -1 ? await apiServicio.ObtenerElementoAsync<List<DatosBasicosEmpleadoViewModel>>(new EmpleadosPorSucursalViewModel { IdSucursal = idSucursalDestino, EmpleadosActivos = true }, new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleadosPorSucursal") : new List<DatosBasicosEmpleadoViewModel>();
                ViewData["EmpleadoRecibe"] = new SelectList(listaEmpleadoSucursalDestino.Select(c => new ListaEmpleadoViewModel { IdEmpleado = c.IdEmpleado, NombreApellido = $"{c.Nombres} {c.Apellidos}" }), "IdEmpleado", "NombreApellido");
                ViewData["EmpleadoResponsableRecibo"] = ViewData["EmpleadoRecibe"];

                int idEmpleadoEntrega = (ViewData["EmpleadoEntrega"] as SelectList).FirstOrDefault() != null ? int.Parse((ViewData["EmpleadoEntrega"] as SelectList).FirstOrDefault().Value) : -1;
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = idEmpleadoEntrega != -1 ? await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(new CambioCustodioViewModel { IdEmpleadoEntrega = idEmpleadoEntrega, ListadoIdRecepcionActivoFijoDetalle = new List<int>() }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijoSeleccionadoPorEmpleado") : new List<RecepcionActivoFijoDetalleSeleccionado>();
                ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionSeleccion: true, IsConfiguracionSeleccionBajas: true);
                return View(nameof(GestionarTransferenciaSucursal));
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
                
                ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionSeleccion: true, IsConfiguracionSeleccionBajas: true);
                var claimTransfer = claimsTransfer.ObtenerClaimsTransferHttpContext();
                ViewData["SucursalOrigen"] = new SelectList(new List<Sucursal>(){ new Sucursal{ IdSucursal = claimTransfer.IdSucursal, Nombre = claimTransfer.NombreSucursal } }, "IdSucursal", "Nombre", cambioUbicacionSucursalViewModel.IdSucursalOrigen);
                ViewData["SucursalDestino"] = new SelectList((await apiServicio.Listar<Sucursal>(new Uri(WebApp.BaseAddressTH), "api/Sucursal/ListarSucursal")).Where(c => c.IdSucursal != claimTransfer.IdSucursal), "IdSucursal", "Nombre", cambioUbicacionSucursalViewModel.IdSucursalDestino);
                
                var listadoEmpleadoSucursalOrigen = (await apiServicio.ObtenerElementoAsync<List<DatosBasicosEmpleadoViewModel>>(new EmpleadosPorSucursalViewModel { IdSucursal = claimTransfer.IdSucursal, EmpleadosActivos = true }, new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleadosPorSucursal")).Select(c => new ListaEmpleadoViewModel { IdEmpleado = c.IdEmpleado, NombreApellido = $"{c.Nombres} {c.Apellidos}" });
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
                return View(nameof(GestionarTransferenciaSucursal), cambioUbicacionSucursalViewModel);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando cambio de ubicación de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListadoTransferenciasCreadas));
            }
        }

        public async Task<IActionResult> DetallesTransferenciaSucursal(int? id)
        {
            ViewData["IsVistaDetalles"] = true;
            return await GestionarTransferenciaSucursal(id);
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
                return this.Redireccionar(response.IsSuccess ? $"{Mensaje.Informacion}|{Mensaje.Satisfactorio}" : $"{Mensaje.Error}|{Mensaje.Excepcion}", nameof(ListadoTransferenciasCreadas));
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Aceptando transferencia creada de activos fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}", nameof(ListadoTransferenciasCreadas));
            }
        }

        public async Task<IActionResult> ExportarPDfCambioUbicacionSucursales(int? id)
        {
            try
            {
                var fileContents = await apiServicio.ObtenerElementoAsync<byte[]>(id, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/PDFTransferenciaCambioUbicacionSucursales");
                if (fileContents.Length > 0)
                {
                    var filename = "Acta de cambio de ubicación entre sucursales.pdf";
                    return File(fileContents, MimeTypes.GetMimeType(filename), filename);
                }
            }
            catch (Exception)
            { }
            return StatusCode(500);
        }
        #endregion
        #endregion

        #region Bajas
        public async Task<IActionResult> ListadoActivosFijosBaja()
        {
            var lista = new List<RecepcionActivoFijoDetalleSeleccionado>();
            ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionListadoGenerales: true, IsConfiguracionDatosBaja: true);
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(new IdRecepcionActivoFijoDetalleSeleccionadoEstado { Estados = new List<string> { Estados.Baja }, ListaIdRecepcionActivoFijoDetalleSeleccionado = new List<IdRecepcionActivoFijoDetalleSeleccionado>() }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijoSeleccionadoPorEstado");
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
                ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionListadoBajasGestionar: true, IsConfiguracionBajasGestionarEditar: id != null);
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
                    return View(nameof(GestionarBaja), bajaActivoFijo);
                }
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = new List<RecepcionActivoFijoDetalleSeleccionado>();
                return View(nameof(GestionarBaja));
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
                ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionListadoBajasGestionar: true, IsConfiguracionBajasGestionarEditar: bajaActivoFijo.IdBajaActivoFijo > 0);
                var listaRecepcionActivoFijoDetalleSeleccionado = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(bajaActivoFijo.BajaActivoFijoDetalle.Select(c => new IdRecepcionActivoFijoDetalleSeleccionado { idRecepcionActivoFijoDetalle = c.IdRecepcionActivoFijoDetalle, seleccionado = true }), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijo");
                foreach (var item in listaRecepcionActivoFijoDetalleSeleccionado)
                {
                    var rafdBajaActivoFijoActual = bajaActivoFijo.BajaActivoFijoDetalle.FirstOrDefault(x => x.IdRecepcionActivoFijoDetalle == item.RecepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle).RecepcionActivoFijoDetalle;
                    item.RecepcionActivoFijoDetalle.UbicacionActivoFijoActual.IdEmpleado = rafdBajaActivoFijoActual.UbicacionActivoFijoActual.IdEmpleado;
                    item.RecepcionActivoFijoDetalle.UbicacionActivoFijoActual.Empleado = JsonConvert.DeserializeObject<Empleado>((await apiServicio.SeleccionarAsync<Response>(rafdBajaActivoFijoActual.UbicacionActivoFijoActual.IdEmpleado.ToString(), new Uri(WebApp.BaseAddressTH), "api/Empleados")).Resultado.ToString());
                }
                ViewData["Error"] = response.Message;
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = listaRecepcionActivoFijoDetalleSeleccionado;
                return View(nameof(GestionarBaja), bajaActivoFijo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando baja de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListadoActivosFijosBaja));
            }
        }

        public async Task<IActionResult> DetallesBaja(int? id)
        {
            ViewData["IsVistaDetalles"] = true;
            return await GestionarBaja(id);
        }

        [HttpPost]
        public async Task<IActionResult> ListadoActivosFijosSeleccionBajaResult(List<IdRecepcionActivoFijoDetalleSeleccionado> listadoRecepcionActivoFijoDetalleSeleccionado, List<IdRecepcionActivoFijoDetalleSeleccionado> objAdicional)
        {
            var lista = new List<RecepcionActivoFijoDetalleSeleccionado>();
            ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionSeleccion: true, IsConfiguracionSeleccionBajas: true);
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
                    return View(nameof(GestionarProcesoJudicial), respuesta.Resultado);
                }
                return View(nameof(GestionarProcesoJudicial));
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
                    return View(nameof(GestionarProcesoJudicial), procesoJudicialActivoFijo);
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
        
        public async Task<IActionResult> DetallesProcesoJudicial(string id, string id2)
        {
            ViewData["IsVistaDetalles"] = true;
            return await GestionarProcesoJudicial(id, id2);
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

        public async Task<IActionResult> GestionarInventarioManual(int? id)
        {
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.EnEjecucion }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Concluido }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                var listaEstado = await apiServicio.Listar<Estado>(new Uri(WebApp.BaseAddressTH), "api/Estados/ListarEstados");
                
                var listadoDetallesActivosFijosViewModel = new ListadoDetallesActivosFijosViewModel(IsConfiguracionSeleccion: true, IsConfiguracionSeleccionBajas: true);

                if (ViewData["IsVistaDetalles"] != null)
                    listadoDetallesActivosFijosViewModel.IsConfiguracionSeleccionDisabled = true;

                ViewData["Configuraciones"] = listadoDetallesActivosFijosViewModel;
                if (id != null)
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ObtenerInventarioActivosFijos");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoInventarioActivosFijos));

                    var inventarioActivoFijo = JsonConvert.DeserializeObject<InventarioActivoFijo>(respuesta.Resultado.ToString());
                    ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(new IdRecepcionActivoFijoDetalleSeleccionadoEstado { Estados = new List<string> { Estados.Alta }, ListaIdRecepcionActivoFijoDetalleSeleccionado = inventarioActivoFijo.InventarioActivoFijoDetalle.Select(c=> new IdRecepcionActivoFijoDetalleSeleccionado { idRecepcionActivoFijoDetalle = c.IdRecepcionActivoFijoDetalle, seleccionado = c.Constatado }).ToList() }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijoSeleccionadoPorEstado");
                    ViewData["Estado"] = new SelectList(listaEstado.Where(c => c.Nombre == Estados.EnEjecucion || c.Nombre == Estados.Concluido).OrderByDescending(c=> c.Nombre), "IdEstado", "Nombre", inventarioActivoFijo.IdEstado);
                    return View(nameof(GestionarInventarioManual), inventarioActivoFijo);
                }
                ViewData["Estado"] = new SelectList(listaEstado.Where(c => c.Nombre == Estados.EnEjecucion || c.Nombre == Estados.Concluido).OrderByDescending(c => c.Nombre), "IdEstado", "Nombre");
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(new IdRecepcionActivoFijoDetalleSeleccionadoEstado { Estados = new List<string> { Estados.Alta }, ListaIdRecepcionActivoFijoDetalleSeleccionado = new List<IdRecepcionActivoFijoDetalleSeleccionado>() }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijoSeleccionadoPorEstado");
                return View(nameof(GestionarInventarioManual));
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
                
                ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionSeleccion: true, IsConfiguracionSeleccionBajas: true);
                ViewData["Estado"] = new SelectList((await apiServicio.Listar<Estado>(new Uri(WebApp.BaseAddressTH), "api/Estados/ListarEstados")).Where(c => c.Nombre == Estados.EnEjecucion || c.Nombre == Estados.Concluido).OrderByDescending(c => c.Nombre), "IdEstado", "Nombre");
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(inventarioActivoFijo.InventarioActivoFijoDetalle.Select(c => new IdRecepcionActivoFijoDetalleSeleccionado { idRecepcionActivoFijoDetalle = c.IdRecepcionActivoFijoDetalle, seleccionado = c.Constatado }), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijo");
                return View(nameof(GestionarInventarioManual), inventarioActivoFijo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando inventario de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListadoInventarioActivosFijos));
            }
        }

        public async Task<IActionResult> DetallesInventarioManual(int? id)
        {
            ViewData["IsVistaDetalles"] = true;
            return await GestionarInventarioManual(id);
        }

        public async Task<IActionResult> GestionarInventarioAutomatico(int? id)
        {
            try
            {
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.EnEjecucion }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                await apiServicio.InsertarAsync(new Estado { Nombre = Estados.Concluido }, new Uri(WebApp.BaseAddressTH), "api/Estados/InsertarEstado");
                var listaEstado = await apiServicio.Listar<Estado>(new Uri(WebApp.BaseAddressTH), "api/Estados/ListarEstados");
                
                var listadoDetallesActivosFijosViewModel = new ListadoDetallesActivosFijosViewModel(IsConfiguracionSeleccion: true, IsConfiguracionSeleccionBajas: true, IsConfiguracionGestionarInventarioAutomatico: true);

                if (ViewData["IsVistaDetalles"] != null)
                    listadoDetallesActivosFijosViewModel.IsConfiguracionSeleccionDisabled = true;

                ViewData["Configuraciones"] = listadoDetallesActivosFijosViewModel;
                if (id != null)
                {
                    var respuesta = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ObtenerInventarioActivosFijos");
                    if (!respuesta.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoInventarioActivosFijos));

                    var inventarioActivoFijo = JsonConvert.DeserializeObject<InventarioActivoFijo>(respuesta.Resultado.ToString());
                    ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(inventarioActivoFijo.InventarioActivoFijoDetalle.Select(c => new IdRecepcionActivoFijoDetalleSeleccionado { idRecepcionActivoFijoDetalle = c.IdRecepcionActivoFijoDetalle, seleccionado = c.Constatado }), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijo");
                    ViewData["Estado"] = new SelectList(listaEstado.Where(c => c.Nombre == Estados.EnEjecucion || c.Nombre == Estados.Concluido).OrderByDescending(c => c.Nombre), "IdEstado", "Nombre", inventarioActivoFijo.IdEstado);
                    return View(nameof(GestionarInventarioAutomatico), inventarioActivoFijo);
                }
                ViewData["Estado"] = new SelectList(listaEstado.Where(c => c.Nombre == Estados.EnEjecucion || c.Nombre == Estados.Concluido).OrderByDescending(c => c.Nombre), "IdEstado", "Nombre");
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = new List<RecepcionActivoFijoDetalleSeleccionado>();
                return View(nameof(GestionarInventarioAutomatico));
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

                ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionSeleccion: true, IsConfiguracionSeleccionBajas: true, IsConfiguracionGestionarInventarioAutomatico: true);
                ViewData["Estado"] = new SelectList((await apiServicio.Listar<Estado>(new Uri(WebApp.BaseAddressTH), "api/Estados/ListarEstados")).Where(c => c.Nombre == Estados.EnEjecucion || c.Nombre == Estados.Concluido).OrderByDescending(c => c.Nombre), "IdEstado", "Nombre");
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(inventarioActivoFijo.InventarioActivoFijoDetalle.Select(c => new IdRecepcionActivoFijoDetalleSeleccionado { idRecepcionActivoFijoDetalle = c.IdRecepcionActivoFijoDetalle, seleccionado = c.Constatado }), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijo");
                return View(nameof(GestionarInventarioAutomatico), inventarioActivoFijo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando inventario automático de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListadoInventarioActivosFijos));
            }
        }

        public async Task<IActionResult> DetallesInventarioAutomatico(int? id)
        {
            ViewData["IsVistaDetalles"] = true;
            return await GestionarInventarioAutomatico(id);
        }

        [HttpPost]
        public async Task<IActionResult> DatosInventarioActivoFijoResult(string codigoSecuencial, List<int> listadoRafdSeleccionados)
        {
            try
            {
                ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionSeleccion: true);
                var lista = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalle>>(new InventarioTransfer { Codigosecuencial = codigoSecuencial, ListadoRafdSeleccionados = listadoRafdSeleccionados }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ObtenerDetalleActivoFijoParaInventario");
                if (lista.Count > 0)
                    return PartialView("_DatosInventarioActivoFijo", lista);
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
                ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionSeleccionMovilizaciones: true);
                if (id != null)
                {
                    var response = await apiServicio.SeleccionarAsync<Response>(id.ToString(), new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ObtenerMovilizacionActivosFijos");
                    if (!response.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Error}|{Mensaje.RegistroNoExiste}", nameof(ListadoActivosFijosBaja));

                    var movilizacionActivoFijo = JsonConvert.DeserializeObject<MovilizacionActivoFijo>(response.Resultado.ToString());
                    ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = movilizacionActivoFijo.MovilizacionActivoFijoDetalle.Select(c => new RecepcionActivoFijoDetalleSeleccionado
                    {
                        RecepcionActivoFijoDetalle = c.RecepcionActivoFijoDetalle,
                        Observaciones = c.Observaciones,
                        Componentes = c.Componentes,
                        Seleccionado = true
                    }).ToList();
                    return View(nameof(GestionarMovilizacion), movilizacionActivoFijo);
                }
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = new List<RecepcionActivoFijoDetalleSeleccionado>();
                return View(nameof(GestionarMovilizacion));
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoMovilizacionActivosFijos));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GestionarMovilizacion(MovilizacionActivoFijo movilizacionActivoFijo)
        {
            try
            {
                var response = new Response();
                movilizacionActivoFijo.MovilizacionActivoFijoDetalle = new List<MovilizacionActivoFijoDetalle>();
                var listaFormDatosEspecificos = Request.Form.Where(c => c.Key.StartsWith("hIdRecepcionActivoFijoDetalle_"));
                foreach (var item in listaFormDatosEspecificos)
                {
                    int posFormDatoEspecifico = int.Parse(item.Key.ToString().Split('_')[1]);
                    string observaciones = Request.Form[$"hTextAreaEditable_{posFormDatoEspecifico}"].ToString();
                    int idRecepcionActivoFijoDetalle = int.Parse(Request.Form[$"hIdRecepcionActivoFijoDetalle_{posFormDatoEspecifico}"]);

                    var rafd = new RecepcionActivoFijoDetalle { IdRecepcionActivoFijoDetalle = idRecepcionActivoFijoDetalle };
                    movilizacionActivoFijo.MovilizacionActivoFijoDetalle.Add(new MovilizacionActivoFijoDetalle
                    {
                        IdRecepcionActivoFijoDetalle = rafd.IdRecepcionActivoFijoDetalle,
                        IdMovilizacionActivoFijo = movilizacionActivoFijo.IdMovilizacionActivoFijo,
                        RecepcionActivoFijoDetalle = rafd,
                        Observaciones = observaciones
                    });
                }

                if (ModelState.IsValid)
                {
                    if (movilizacionActivoFijo.IdMovilizacionActivoFijo == 0)
                    {
                        response = await apiServicio.InsertarAsync(movilizacionActivoFijo, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/InsertarMovilizacionActivoFijo");
                        if (response.IsSuccess)
                            await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha realizado una movilización de activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Movilización de Activo Fijo:", movilizacionActivoFijo.IdMovilizacionActivoFijo) });
                    }
                    else
                    {
                        response = await apiServicio.EditarAsync(movilizacionActivoFijo.IdMovilizacionActivoFijo.ToString(), movilizacionActivoFijo, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/EditarMovilizacionActivoFijo");
                        if (response.IsSuccess)
                            await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), ExceptionTrace = null, Message = "Se ha editado una movilización de activo fijo", UserName = "Usuario 1", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), EntityID = string.Format("{0} {1}", "Movilización de Activo Fijo:", movilizacionActivoFijo.IdMovilizacionActivoFijo) });
                    }

                    if (response.IsSuccess)
                        return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ListadoMovilizacionActivosFijos));
                }
                else
                    response.Message = Mensaje.ModeloInvalido;

                ViewData["Error"] = response.Message;
                ViewData["Empleado"] = new SelectList(await apiServicio.Listar<ListaEmpleadoViewModel>(new Uri(WebApp.BaseAddressTH), "api/Empleados/ListarEmpleados"), "IdEmpleado", "NombreApellido");
                ViewData["MotivoTraslado"] = new SelectList(await apiServicio.Listar<MotivoTraslado>(new Uri(WebApp.BaseAddressRM), "api/MotivoTraslado/ListarMotivoTraslado"), "IdMotivoTraslado", "Descripcion");
                ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionSeleccionMovilizaciones: true);
                var listaRecepcionActivoFijoSeleccionado = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(new MovilizacionActivoFijoTransfer { ListadoRecepcionActivoFijoDetalleSeleccionado = movilizacionActivoFijo.MovilizacionActivoFijoDetalle.Select(c => new IdRecepcionActivoFijoDetalleSeleccionado { idRecepcionActivoFijoDetalle = c.IdRecepcionActivoFijoDetalle, seleccionado = true }).ToList(), SeleccionarTodasAltas = false }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijoSeleccionadoPorMovilizacion");
                listaRecepcionActivoFijoSeleccionado.ForEach(c => c.Observaciones = movilizacionActivoFijo.MovilizacionActivoFijoDetalle.FirstOrDefault(x => x.IdRecepcionActivoFijoDetalle == c.RecepcionActivoFijoDetalle.IdRecepcionActivoFijoDetalle).Observaciones);
                ViewData["ListadoRecepcionActivoFijoDetalleSeleccionado"] = listaRecepcionActivoFijoSeleccionado;
                return View(nameof(GestionarMovilizacion), movilizacionActivoFijo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Creando movilización de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Create), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP WebAppRM" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCrear}", nameof(ListadoMovilizacionActivosFijos));
            }
        }

        public async Task<IActionResult> DetallesMovilizacion(int? id)
        {
            ViewData["IsVistaDetalles"] = true;
            return await GestionarMovilizacion(id);
        }

        [HttpPost]
        public async Task<IActionResult> ListadoActivosFijosSeleccionMovilizacionResult(List<IdRecepcionActivoFijoDetalleSeleccionado> listadoRecepcionActivoFijoDetalleSeleccionado, int? objAdicional)
        {
            var lista = new List<RecepcionActivoFijoDetalleSeleccionado>();
            ViewData["Configuraciones"] = new ListadoDetallesActivosFijosViewModel(IsConfiguracionSeleccion: true, IsConfiguracionListadoMovilizacionesGestionar: true);
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(new MovilizacionActivoFijoTransfer { ListadoRecepcionActivoFijoDetalleSeleccionado = listadoRecepcionActivoFijoDetalleSeleccionado, SeleccionarTodasAltas = true, IdMovilizacionActivoFijo = objAdicional }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijoSeleccionadoPorMovilizacion");
            }
            catch (Exception)
            { }
            return PartialView("_ListadoDetallesActivosFijos", lista);
        }

        public async Task<IActionResult> EliminarMovilizacion(string id)
        {
            try
            {
                var response = await apiServicio.EliminarAsync(id, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/EliminarMovilizacionActivoFijo");
                if (response.IsSuccess)
                {
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Sistema", id), Message = "Registro eliminado", LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), UserName = "Usuario APP webapprm" });
                    return this.Redireccionar($"{Mensaje.Informacion}|{response.Message}", nameof(ListadoMovilizacionActivosFijos));
                }
                return this.Redireccionar($"{Mensaje.Error}|{response.Message}", nameof(ListadoMovilizacionActivosFijos));
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Eliminar movilización", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Delete), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.Excepcion}", nameof(ListadoMovilizacionActivosFijos));
            }
        }

        [HttpPost]
        public async Task<IActionResult> DetallesMovilizacionActivoFijoResult(List<IdRecepcionActivoFijoDetalleSeleccionado> listadoRecepcionActivoFijoDetalleSeleccionado, ListadoDetallesActivosFijosViewModel arrConfiguraciones, int? objAdicional)
        {
            var lista = new List<RecepcionActivoFijoDetalleSeleccionado>();
            ViewData["Configuraciones"] = arrConfiguraciones;
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<RecepcionActivoFijoDetalleSeleccionado>>(new MovilizacionActivoFijoTransfer { ListadoRecepcionActivoFijoDetalleSeleccionado = listadoRecepcionActivoFijoDetalleSeleccionado, SeleccionarTodasAltas = false, IdMovilizacionActivoFijo = objAdicional }, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/DetallesActivoFijoSeleccionadoPorMovilizacion");
            }
            catch (Exception)
            { }
            return PartialView("_ListadoDetallesActivosFijos", lista);
        }

        public async Task<IActionResult> ExportarExcelMovilizacion(int? id)
        {
            try
            {
                var fileContents = await apiServicio.ObtenerElementoAsync<byte[]>(id, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ExcelMovilizacion");
                if (fileContents.Length > 0)
                {
                    var fileName = "Autorización de salida de bienes del BDE.xlsx";
                    return File(fileContents, MimeTypes.GetMimeType(fileName), fileName);
                }
            }
            catch (Exception)
            { }
            return StatusCode(500);
        }
        #endregion

        #region Revalorizaciones
        public async Task<IActionResult> ListadoRevalorizaciones(string id)
        {
            var lista = new List<RevalorizacionActivoFijo>();
            ViewData["IdRecepcionActivoFijoDetalle"] = id;
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<RevalorizacionActivoFijo>>(id, new Uri(WebApp.BaseAddressRM), "api/RevalorizacionActivoFijo/ListarRevalorizacionActivoFijoPorIdDetalleActivoFijo");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando revalorizaciones de Activos Fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }

        public async Task<IActionResult> GestionarRevalorizacion(string id)
        {
            try
            {
                ViewData["IdRecepcionActivoFijoDetalle"] = id;
                decimal ultimaRevalorizacionActivoFijo = await apiServicio.ObtenerElementoAsync<decimal>(id, new Uri(WebApp.BaseAddressRM), "api/RevalorizacionActivoFijo/UltimoValorCompra");
                return View(new RevalorizacionActivoFijo {
                    FechaRevalorizacion = DateTime.Now,
                    ValorCompraAnterior = ultimaRevalorizacionActivoFijo
                });
            }
            catch (Exception)
            {
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorCargarDatos}", nameof(ListadoRevalorizaciones), routeValues: new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GestionarRevalorizacion(RevalorizacionActivoFijo revalorizacionActivoFijo)
        {
            try
            {
                ViewData["IdRecepcionActivoFijoDetalle"] = revalorizacionActivoFijo.IdRecepcionActivoFijoDetalle;
                var response = new Response();
                response = await apiServicio.InsertarAsync(revalorizacionActivoFijo, new Uri(WebApp.BaseAddressRM), "api/RevalorizacionActivoFijo/InsertarRevalorizacionActivoFijo");
                if (response.IsSuccess)
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), EntityID = string.Format("{0} : {1}", "Revalorización", revalorizacionActivoFijo.IdRevalorizacionActivoFijo.ToString()), LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ADV), Message = "Se ha creado una revalorización de Activo Fijo", UserName = "Usuario 1" });

                if (response.IsSuccess)
                    return this.Redireccionar($"{Mensaje.Informacion}|{Mensaje.Satisfactorio}", nameof(ListadoRevalorizaciones), routeValues: new { id = revalorizacionActivoFijo.IdRecepcionActivoFijoDetalle });

                ViewData["Error"] = response.Message;
                return View(revalorizacionActivoFijo);
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Editando una revalorización de Activo Fijo", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.Edit), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                return this.Redireccionar($"{Mensaje.Error}|{Mensaje.ErrorEditar}", nameof(ListadoRevalorizaciones), routeValues: new { id = revalorizacionActivoFijo.IdRecepcionActivoFijoDetalle });
            }
        }
        #endregion

        #region Tabla de amortización
        public async Task<IActionResult> TablaAmortizacionActivoFijo(int? id)
        {
            var lista = new List<DepreciacionActivoFijo>();
            ViewData["IdRecepcionActivoFijoDetalle"] = id;
            try
            {
                lista = await apiServicio.ObtenerElementoAsync<List<DepreciacionActivoFijo>>(id, new Uri(WebApp.BaseAddressRM), "api/ActivosFijos/ListarDepreciacionActivoFijo");
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer { ApplicationName = Convert.ToString(Aplicacion.WebAppRM), Message = "Listando tabla de amortización de Activos Fijos", ExceptionTrace = ex.Message, LogCategoryParametre = Convert.ToString(LogCategoryParameter.NetActivity), LogLevelShortName = Convert.ToString(LogLevelParameter.ERR), UserName = "Usuario APP webapprm" });
                TempData["Mensaje"] = $"{Mensaje.Error}|{Mensaje.ErrorListado}";
            }
            return View(lista);
        }
        #endregion

        #region Descargar Archivos
        public async Task<IActionResult> DescargarArchivo(int id)
        {
            try
            {
                var response = await apiServicio.ObtenerElementoAsync<Response>(id, new Uri(WebApp.BaseAddressRM), "api/DocumentoActivoFijo/GetFile");
                if (response.IsSuccess)
                {
                    var documentoActivoFijoTransfer = JsonConvert.DeserializeObject<DocumentoActivoFijoTransfer>(response.Resultado.ToString());
                    return File(documentoActivoFijoTransfer.Fichero, MimeTypes.GetMimeType(documentoActivoFijoTransfer.Nombre), documentoActivoFijoTransfer.Nombre);
                }
            }
            catch (Exception)
            { }
            return StatusCode(500);
        }
        #endregion

        #region Reportes
        public async Task<IActionResult> AltaReporte()
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
                return new SelectList(new List<Subramo>());
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