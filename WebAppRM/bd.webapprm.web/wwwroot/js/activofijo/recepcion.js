var objName = {
    nameNumeroClaveCatastral: "NumeroClaveCatastral",
    nameNumeroChasis: "NumeroChasis",
    nameNumeroMotor: "NumeroMotor",
    namePlaca: "Placa",
    nameSerie: "Serie"
};
var objCategorias = {
    Edificio: "EDIFICIOS",
    MueblesEnseres: "MUEBLES Y ENSERES",
    EquiposOficina: "EQUIPOS DE OFICINA",
    Vehiculo: "VEHÍCULOS",
    EquiposComputoSoftware: "EQUIPOS DE CÓMPUTO Y SOFTWARE"
};
var arrIdsfilas = Array();
$(document).ready(function () {
    Init_Select2();
    Init_DatetimePicker("RecepcionActivoFijo_FechaRecepcion", true);
    Init_FileInput("file");
    $("#IdSucursal").prop("disabled", "disabled");

    if (isRevisionActivoFijo)
        adicionarArrRecepcionActivoFijoDetalle();

    initDataTableFiltrado("tableDetallesRecepcion", [], function () {
        var table = $("#tableDetallesRecepcion").dataTable();
        var api = table.api();
        var rows = api.rows({ page: 'current' }).nodes();
        var last = null;
        var groupadmin = [];
        crearGrupoSubtotal(api, rows, last, groupadmin, 1, "Clase de activo fijo", 6, isSeleccion ? 12 : 11, 7);
    });
    $('#tableDetallesRecepcion').DataTable().page.len(-1).draw();
    if (isVistaDetalles)
    {
        $("#RecepcionActivoFijo_IdMotivoAlta").prop("disabled", "disabled");
        $("#RecepcionActivoFijo_IdProveedor").prop("disabled", "disabled");
        $("#RecepcionActivoFijo_IdFondoFinanciamiento").prop("disabled", "disabled");
        $("#RecepcionActivoFijo_FechaRecepcion").prop("disabled", "disabled");
        $("#RecepcionActivoFijo_OrdenCompra").prop("disabled", "disabled");
        $("#RecepcionActivoFijo_PolizaSeguroActivoFijo_Subramo_IdRamo").prop("disabled", "disabled");
        $("#RecepcionActivoFijo_PolizaSeguroActivoFijo_IdSubramo").prop("disabled", "disabled");
        $("#RecepcionActivoFijo_PolizaSeguroActivoFijo_IdCompaniaSeguro").prop("disabled", "disabled");

        if (!isPolizaSeguro)
            $("#RecepcionActivoFijo_PolizaSeguroActivoFijo_NumeroPoliza").prop("disabled", "disabled");
    }
    if (isRevisionActivoFijo)
        inicializarIdsArrRecepcionActivoFijoDetalleTodos();
});

function ocultarDatosTablaEspecificos()
{
    $(".ColVis").hide();
    $('#tbDatosEspecificos').DataTable().page.len(-1).draw();
    $(".dataTables_length").hide();
}

function gestionarWizard(isVistaDetalles)
{
    var wizard = $('.wizard').wizard({ disablePreviousStep: true});
    wizard.on('finished.fu.wizard', function (e, data) {
        var validar = validarDatosEspecificosPertenecenBodegaEmpleadoCodigosecuencial();
        if (!validar)
            mostrarNotificacion("Error", "Tiene que asignar cada dato específico a una Bodega o Empleado y a un Código secuencial.");
        else
            guardarDatosActivoFijoRow();
    });
    wizard.on('change.fu.wizard', function (e, data)
    {
        if (data.direction === 'previous')
        {
            if (isVistaDetalles)
                $(".btn-next").removeClass("hide");
            return;
        }
        if (isVistaDetalles) {
            $(".btn-next").addClass("hide");
            return true;
        }
        return validarWizard();
    });
    wizard.on('stepclick', function (e, data) {
        if (isVistaDetalles) {
            $(".btn-next").removeClass("hide");
            return true;
        }
    });
}

function generarCodigosecuencial()
{
    $("#CodigoActivoFijo_SUBCAF").val($("#ActivoFijo_IdSubClaseActivoFijo").val());
    $("#CodigoActivoFijo_CAF").val($("#ActivoFijo_SubClaseActivoFijo_IdClaseActivoFijo").val());
    $("#CodigoActivoFijo_SUC").val($("#IdSucursal").val());
    var idSucursal = $("#IdSucursal").val() + ".";
    var idClase = agregarCeros($("#ActivoFijo_SubClaseActivoFijo_IdClaseActivoFijo").val(), 3) + ".";
    var idSubClase = agregarCeros($("#ActivoFijo_IdSubClaseActivoFijo").val(), 3) + ".";
    return idSucursal + idClase + idSubClase;
}

function actualizarCodigosSecuenciales()
{
    var validar = true;
    for (var i = 0; i < arrIdsfilas.length; i++) {
        var idFila = arrIdsfilas[i];
        var codigoSecuencialOld = $("#hCodigoSecuencial_" + idFila).val();
        if (codigoSecuencialOld != "" && codigoSecuencialOld != "-" && codigoSecuencialOld != null) {
            var nuevoCodigoSecuencial = generarCodigosecuencial();
            nuevoCodigoSecuencial += codigoSecuencialOld.split(".")[3];
            if (validarCodificacionTablaActivosFijos(nuevoCodigoSecuencial)) {
                mostrarLoadingPanel("checkout-form", "Actualizando códigos secuenciales...");
                $.ajax({
                    url: urlValidarCodigoUnico,
                    method: "POST",
                    data: { idCodigoActivoFijo: $("#hIdCodigoActivoFijo_" + idFila).val(), codigoSecuencial: nuevoCodigoSecuencial },
                    success: function (data) {
                        if (data.toString().toLowerCase() == "false")
                            putDatoCodificacion(idFila, nuevoCodigoSecuencial);
                        else {
                            putDatoCodificacion(idFila, "");
                            validar = false;
                        }
                    },
                    error: function (errorMessage) {
                        putDatoCodificacion(idFila, "");
                        validar = false;
                    },
                    complete: function (data) {
                        $("#checkout-form").waitMe("hide");
                    }
                });
            }
            else {
                putDatoCodificacion(idFila, "");
                validar = false;
            }
        }
    }
    if (!validar)
        mostrarNotificacion("Aviso", "Se eliminaron algunos Códigos secuenciales pues se encuentran asignados a un Activo Fijo adicionado ó ya existen en el sistema.");
}

function validarWizard()
{
    var form = $("#checkout-form");
    if (!form.valid()) {
        mostrarNotificacion("Error", "Existen errores en el formulario.");
        return false;
    }
    return validarActivoFijoExiste($("#ActivoFijo_IdSubClaseActivoFijo").val(), $("#ActivoFijo_IdModelo").val(), $("#ActivoFijo_Nombre").val());
}

function validarRecepcion()
{
    var api = $("#tableDetallesRecepcion").dataTable().api();
    var form = $("#formDatosActivo");
    var validar = true;

    if (api.rows().nodes().length == 0) {
        mostrarNotificacion("Error", "Tiene que adicionar al menos un Activo Fijo.");
        validar = false;
    }
    if (!form.valid()) {
        mostrarNotificacion("Error", "Existen errores en el formulario.");
        validar = false;
    }
    if (validar)
        $("#formDatosActivo").submit();
}

function eventoTipoActivoFijo() {
    $("#ActivoFijo_SubClaseActivoFijo_ClaseActivoFijo_IdTipoActivoFijo").on("change", function (e) {
        partialViewTipoActivoFijo(e.val);
    });
}

function eventoClaseActivoFijo() {
    $("#ActivoFijo_SubClaseActivoFijo_IdClaseActivoFijo").on("change", function (e) {
        partialViewClaseActivoFijo(e.val);
    });
}

function eventoSubClaseActivoFijo()
{
    $("#ActivoFijo_IdSubClaseActivoFijo").on("change", function (e) {
        actualizarCodigosSecuenciales();
    });
}

function eventoMarca()
{
    $("#ActivoFijo_Modelo_IdMarca").on("change", function (e) {
        mostrarLoadingPanel("checkout-form", "Cargando modelos...");
        $.ajax({
            url: urlModeloSelectResult,
            method: "POST",
            data: { idMarca: e.val },
            success: function (data) {
                $("#div_modelo").html(data);
                Init_Select2();
            },
            complete: function (data) {
                $("#checkout-form").waitMe("hide");
            }
        });
    });
}

function eventoRamo() {
    $("#RecepcionActivoFijo_PolizaSeguroActivoFijo_Subramo_IdRamo").on("change", function (e) {
        mostrarLoadingPanel("checkout-form", "Cargando subramos...");
        $.ajax({
            url: urlSubramoSelectResult,
            method: "POST",
            data: { idRamo: e.val },
            success: function (data) {
                $("#div_subramo").html(data);
                Init_Select2();
            },
            complete: function (data) {
                $("#checkout-form").waitMe("hide");
            }
        });
    });
}

function partialViewTipoActivoFijo(idTipoActivoFijo) {
    mostrarLoadingPanel("checkout-form", "Cargando clases de activo fijo...");
    $.ajax({
        url: claseActivoFijoSelectResult,
        method: "POST",
        data: { idTipoActivoFijo: obtenerIdAjax(idTipoActivoFijo) },
        success: function (data) {
            $("#div_claseActivoFijo").html(data);
            Init_Select2();
        },
        error: function (data) {
            $("#checkout-form").waitMe("hide");
        },
        complete: function (data)
        {
            partialViewClaseActivoFijo($("#ActivoFijo_SubClaseActivoFijo_IdClaseActivoFijo").val());
            eventoClaseActivoFijo();
        }
    });
}

function partialViewClaseActivoFijo(idClaseActivoFijo) {
    mostrarLoadingPanel("checkout-form", "Cargando subclases de activo fijo...");
    $.ajax({
        url: subClaseActivoFijoSelectResult,
        method: "POST",
        data: { idClaseActivoFijo: obtenerIdAjax(idClaseActivoFijo) },
        success: function (data) {
            $("#div_subClaseActivoFijo").html(data);
            Init_Select2();
        },
        complete: function (data) {
            $("#checkout-form").waitMe("hide");
            obtenerCategoria(function () {
                eventoCambiarCategoria();
            });
            actualizarCodigosSecuenciales();
        }
    });
}

function eventoSpinnerCantidad() {
    $(".spinnerCantidad").spinner('delay', 200).spinner('changed', function (e, newVal, oldVal) {
        if (newVal > arrIdsfilas.length) {
            crearFilas(newVal - arrIdsfilas.length);
            $.each($(".btnEliminarDatosEspecificos"), function (index, value) {
                $(value).removeClass("hide");
            });
        }
        else
            eliminarFilas(arrIdsfilas.length - newVal);
    });
}

function obtenerCategoria(callbackFunctionCategoria)
{
    mostrarLoadingPanel("tbDatosEspecificos", "Cargando datos por categoría...");
    $.ajax({
        url: urlCategoria,
        method: "POST",
        data: { idClaseActivoFijo: $("#ActivoFijo_SubClaseActivoFijo_IdClaseActivoFijo").val() },
        success: function (data) {
            categoria = data.toUpperCase();
        },
        error: function ()
        {
            categoria = "";
        },
        complete: function (data) {
            if (callbackFunctionCategoria)
                callbackFunctionCategoria();
            $("#tbDatosEspecificos").waitMe("hide");
        }
    });
}

function initArrIdFilas()
{
    for (var i = 0; i < maxIdFila; i++) {
        arrIdsfilas.push(i);
    }
}

function crearFilas(cantidad)
{
    for (var i = 0; i < cantidad; i++)
    {
        var nuevoIdFila = arrIdsfilas.length;
        var hNumeroClaveCatastral = "<input type='hidden' id='h" + objName.nameNumeroClaveCatastral + "_" + nuevoIdFila + "'" + "name='h" + objName.nameNumeroClaveCatastral + "_" + nuevoIdFila + "'" + "/>";
        var hNumeroChasis = "<input type='hidden' id='h" + objName.nameNumeroChasis + "_" + nuevoIdFila + "'" + "name='h" + objName.nameNumeroChasis + "_" + nuevoIdFila + "'" + "/>";
        var hNumeroMotor = "<input type='hidden' id='h" + objName.nameNumeroMotor + "_" + nuevoIdFila + "'" + "name='h" + objName.nameNumeroMotor + "_" + nuevoIdFila + "'" + "/>";
        var hPlaca = "<input type='hidden' id='h" + objName.namePlaca + "_" + nuevoIdFila + "'" + "name='h" + objName.namePlaca + "_" + nuevoIdFila + "'" + "/>";
        var hSerie = "<input type='hidden' id='h" + objName.nameSerie + "_" + nuevoIdFila + "'" + "name='h" + objName.nameSerie + "_" + nuevoIdFila + "'" + "/>";
        var hBodega = "<input type='hidden' id='hBodega_" + nuevoIdFila + "'" + "name='hBodega_" + nuevoIdFila + "'" + "/>";
        var hEmpleado = "<input type='hidden' id='hEmpleado_" + nuevoIdFila + "'" + "name='hEmpleado_" + nuevoIdFila + "'" + "/>";
        var hRecepcionActivoFijoDetalle = "<input type='hidden' id='hIdRecepcionActivoFijoDetalle_" + nuevoIdFila + "' name='hIdRecepcionActivoFijoDetalle_" + nuevoIdFila + "' />";
        var hUbicacion = "<input type='hidden' id='hUbicacion_" + nuevoIdFila + "' name='hUbicacion_" + nuevoIdFila + "' />";
        var hComponentes = "<input type='hidden' id='hComponentes_" + nuevoIdFila + "' name='hComponentes_" + nuevoIdFila + "' />";
        var hCodigoSecuencial = "<input type='hidden' id='hCodigoSecuencial_" + nuevoIdFila + "' name='hCodigoSecuencial_" + nuevoIdFila + "' />";
        var hIdCodigoActivoFijo = "<input type='hidden' id='hIdCodigoActivoFijo_" + nuevoIdFila + "' name='hIdCodigoActivoFijo_" + nuevoIdFila + "' />";
        var btnEditarDatosEspecificos = "<a href='javascript: void(0);' onclick='cargarFormularioDatosEspecificos(" + nuevoIdFila + ")' class='btnEditarDatosEspecificos' data-idfila='" + nuevoIdFila + "'>" + "Editar</a>";
        var btnEditarCodificacion = "<span> | </span><a href='javascript: void(0);' onclick='cargarFormularioCodificacion(" + nuevoIdFila + ")' class='btnEditarCodificacion' data-idfila='" + nuevoIdFila + "'>" + "Codificaci&oacute;n</a>";
        var btnComponentesDatosEspecificos = "<span> | </span><a href='javascript: void(0);' onclick='cargarFormularioComponentesDatosEspecificos(" + nuevoIdFila + ")' class='btnComponentesDatosEspecificos' data-idfila='" + nuevoIdFila + "' data-idorigen='' data-idscomponentes=''>" + "Componentes</a>";
        var btnEliminarDatosEspecificos = "<div id='divEliminarDatosEspecificos_" + nuevoIdFila + "' class='btnEliminarDatosEspecificos" + (arrIdsfilas.length == 0 ? " hide" : "") + "' style='display:inline;'><span> | </span><a href='javascript: void(0);' id='btnEliminarDatosEspecifico_" + nuevoIdFila + "' onclick=abrirVentanaConfirmacion('btnEliminarDatosEspecifico_" + nuevoIdFila + "') data-funcioncallback='callBackFunctionEliminarDatoEspecifico(" + nuevoIdFila + ")' data-titulo='Eliminar' data-descripcion='&#191; Desea eliminar el Dato Espec&iacute; fico... ?'>Eliminar</a>";

        mostrarOcultarColumnas("tbDatosEspecificos", [true, true, true, true, true]);
        addRowTableDatosEspecificos(['-', '-', '-', '-', '-', '-', '-', '-',
            hNumeroClaveCatastral + hNumeroChasis + hNumeroMotor + hPlaca + hSerie + hBodega + hEmpleado + hRecepcionActivoFijoDetalle + hUbicacion + hComponentes + hCodigoSecuencial + hIdCodigoActivoFijo + btnEditarDatosEspecificos + btnEditarCodificacion + btnComponentesDatosEspecificos + btnEliminarDatosEspecificos],
            nuevoIdFila, [objName.nameNumeroClaveCatastral, objName.nameNumeroChasis, objName.nameNumeroMotor, objName.namePlaca, objName.nameSerie, "Bodega", "Empleado", "Codificacion"]);
        eventoCambiarCategoria();

        arrIdsfilas.push(nuevoIdFila);
        addComponenteToArray(nuevoIdFila, 0, []);
    }
}

function addRowTableDatosEspecificos(arrColumnas, idFila, arrNames)
{
    var table = $('#tbDatosEspecificos').DataTable();
    var rowNode = table.row.add(arrColumnas).draw().node();
    $(rowNode).prop("id", "trDatosEspecificos_" + idFila);
    $.each($(rowNode).children(), function (index, value) {
        if (index < arrNames.length)
            $(value).prop("id", "td" + arrNames[index] + "_" + idFila);
    });
}

function deleteRowTableDatosEspecificos(idFila) {
    var row = $("#trDatosEspecificos_" + idFila);
    $('#tbDatosEspecificos').dataTable().fnDeleteRow(row);
}

function eliminarFilas(cantidad)
{
    var pos = arrIdsfilas.length - 1;
    while (cantidad > 0) {
        var idFila = arrIdsfilas[pos];
        deleteRowTableDatosEspecificos(idFila);
        arrIdsfilas.splice(pos, 1);
        eliminarComponente(idFila);
        pos--;
        cantidad--;
    }
}

function obtenerPosIdFila(idFila)
{
    for (var i = 0; i < arrIdsfilas.length; i++) {
        if (arrIdsfilas[i] == idFila)
            return i;
    }
}

function eventoCambiarCategoria()
{
    if (categoria == objCategorias.Edificio)
        mostrarOcultarColumnas("tbDatosEspecificos", [true, false, false, false, true]);
    else if (categoria == objCategorias.Vehiculo)
        mostrarOcultarColumnas("tbDatosEspecificos", [false, true, true, true, true]);
    else if (categoria == objCategorias.EquiposComputoSoftware)
        mostrarOcultarColumnas("tbDatosEspecificos", [false, false, false, false, true]);
    else
        mostrarOcultarColumnas("tbDatosEspecificos", [false, false, false, false, true]);
}

function cargarFormularioDatosEspecificos(idFila)
{
    mostrarLoadingPanel("checkout-form", "Cargando datos, por favor espere...");
    var objData = new Object();
    objData.RecepcionActivoFijoDetalleEdificio = new Object();
    objData.RecepcionActivoFijoDetalleVehiculo = new Object();

    if (categoria == objCategorias.Edificio)
        objData.RecepcionActivoFijoDetalleEdificio.NumeroClaveCatastral = $("#h" + objName.nameNumeroClaveCatastral + "_" + idFila).val();
    else if (categoria == objCategorias.Vehiculo)
    {
        objData.RecepcionActivoFijoDetalleVehiculo.NumeroChasis = $("#h" + objName.nameNumeroChasis + "_" + idFila).val();
        objData.RecepcionActivoFijoDetalleVehiculo.NumeroMotor = $("#h" + objName.nameNumeroMotor + "_" + idFila).val();
        objData.RecepcionActivoFijoDetalleVehiculo.Placa = $("#h" + objName.namePlaca + "_" + idFila).val();
    }
    objData.Serie = $("#h" + objName.nameSerie + "_" + idFila).val();
    objData.IdBodega = $("#hBodega_" + idFila).val();
    objData.IdEmpleado = $("#hEmpleado_" + idFila).val();

    $.ajax({
        url: urlModalDatosEspecificosResult,
        method: "POST",
        data: { rafd: objData, categoria: categoria, idSucursal: $("#IdSucursal").val(), idBodega: objData.IdBodega, idEmpleado: objData.IdEmpleado },
        success: function (data) {
            Init_BootBox("Editar", data, "large", null, {
                isGuardar: true, hideAlGuardar: false, callbackGuardar: function () {
                    guardarDatosEspecificos();
                }
            });
            $("#hIdFilaModalDatosEspecificos").val(idFila);
            $("#hIdRecepcionActivoFijoDetalle").val($("#hIdRecepcionActivoFijoDetalle_" + idFila).val());
            $("#hIdUbicacionActivoFijo").val($("#hUbicacion_" + idFila).val());
            $("#mFuncionCallbak").val("guardarDatosEspecificos");
        },
        error: function (errorMessage) {
            mostrarNotificacion("Error", "Ocurrió un error al cargar el formulario, inténtelo nuevamente.");
        },
        complete: function (data) {
            Init_Select2();
            eventoRadioDatosEspecificos();
            $("#checkout-form").waitMe("hide");
        }
    });
}

function eventoRadioDatosEspecificos()
{
    $(".radioDatosEspecificos").on("change", function (e) {
        var ubicacion = $(e.currentTarget).data("ubicacion");
        switch (ubicacion) {
            case "Bodega": {
                $("#divDatosEspecificosEmpleado").addClass("hide");
                $("#divDatosEspecificosBodega").removeClass("hide");
                break;
            }
            default: {
                $("#divDatosEspecificosBodega").addClass("hide");
                $("#divDatosEspecificosEmpleado").removeClass("hide");
                break;
            }
        }
    });
}

function guardarDatosEspecificos()
{
    mostrarLoadingPanel("divSmartFormModalDatosEspecificos", "Validando datos, por favor espere...");
    $.each($(".validationDatosEspecificos"), function (index, value) {
        $(value).html("");
    });

    var objData = new Object();
    if (categoria == objCategorias.Edificio)
        objData.NumeroClaveCatastral = $("#RecepcionActivoFijoDetalleEdificio_" + objName.nameNumeroClaveCatastral).val();
    else if (categoria == objCategorias.Vehiculo) {
        objData.NumeroChasis = $("#RecepcionActivoFijoDetalleVehiculo_" + objName.nameNumeroChasis).val();
        objData.NumeroMotor = $("#RecepcionActivoFijoDetalleVehiculo_" + objName.nameNumeroMotor).val();
        objData.Placa = $("#RecepcionActivoFijoDetalleVehiculo_" + objName.namePlaca).val();
    }
    objData.Serie = $("#" + objName.nameSerie).val();
    objData.IdFila = $("#hIdFilaModalDatosEspecificos").val();
    objData.IdRecepcionActivoFijoDetalle = $("#hIdRecepcionActivoFijoDetalle").val();
    objData.IdBodega = $("#IdBodega").val();
    objData.IdEmpleado = $("#IdEmpleado").val();
    objData.IsBodega = $("#radioBodegaDatosEspecificos").prop("checked");

    if (validarDatosEspecificosExisten(objData) && validarDatosEspecificosTablaActivosFijosAdicionados(objData)) {
        $.ajax({
            url: urlValidacionDatosEspecificosResult,
            method: "POST",
            data: { rafd: objData },
            success: function (data) {
                if (data.length > 0) {
                    $.each(data, function (index, value) {
                        $("#val" + value.propiedad).html(value.valor);
                    });
                }
                else {
                    var idFila = $("#hIdFilaModalDatosEspecificos").val();
                    var table = $('#tbDatosEspecificos').DataTable();
                    if (categoria == objCategorias.Edificio)
                        putDatoNumeroClaveCatastralFila(idFila, objData.NumeroClaveCatastral);
                    else if (categoria == objCategorias.Vehiculo) {
                        putDatoNumeroChasisFila(idFila, objData.NumeroChasis);
                        putDatoNumeroMotorFila(idFila, objData.NumeroMotor);
                        putDatoPlacaFila(idFila, objData.Placa);
                    }
                    putDatoSerieFila(idFila, objData.Serie);

                    if (objData.IsBodega) {
                        var isTodosBodegasDatosEspecificos = $("#chkTodosBodegasDatosEspecificos").prop("checked");
                        if (isTodosBodegasDatosEspecificos) {
                            for (var i = 0; i < arrIdsfilas.length; i++) {
                                putDatoBodegaEmpleadoFila(arrIdsfilas[i], "Bodega", $("#IdBodega option:selected").text(), $("#IdBodega").val());
                                putDatoBodegaEmpleadoFila(arrIdsfilas[i], "Empleado", "-", "");
                            }
                        }
                        else {
                            putDatoBodegaEmpleadoFila(idFila, "Bodega", $("#IdBodega option:selected").text(), $("#IdBodega").val());
                            putDatoBodegaEmpleadoFila(idFila, "Empleado", "-", "");
                        }
                    }
                    else {
                        var isTodosEmpleadosDatosEspecificos = $("#chkTodosEmpleadosDatosEspecificos").prop("checked");
                        if (isTodosEmpleadosDatosEspecificos) {
                            for (var i = 0; i < arrIdsfilas.length; i++) {
                                putDatoBodegaEmpleadoFila(arrIdsfilas[i], "Empleado", $("#IdEmpleado option:selected").text(), $("#IdEmpleado").val());
                                putDatoBodegaEmpleadoFila(arrIdsfilas[i], "Bodega", "-", "");
                            }
                        }
                        else {
                            putDatoBodegaEmpleadoFila(idFila, "Empleado", $("#IdEmpleado option:selected").text(), $("#IdEmpleado").val());
                            putDatoBodegaEmpleadoFila(idFila, "Bodega", "-", "");
                        }
                    }
                    closeBootBox();
                }
            },
            error: function (errorMessage) {
                mostrarNotificacion("Error", "Ocurrió un error al validar el formulario.");
            },
            complete: function (data) {
                $("#divSmartFormModalDatosEspecificos").waitMe("hide");
            }
        });
    }
    else
        $("#divSmartFormModalDatosEspecificos").waitMe("hide");
}

function dash(valor)
{
    valor = valor.toString().trim();
    if (valor == "")
        valor = "-";
    return valor;
}

function undash(valor)
{
    valor = valor.toString().trim();
    if (valor == "-")
        valor = "";
    return valor;
}

function clearDatosEspecificosBodegaEmpleado()
{
    for (var i = 0; i < arrIdsfilas.length; i++) {
        putDatoBodegaEmpleadoFila(arrIdsfilas[i], "Bodega", "-", "");
        putDatoBodegaEmpleadoFila(arrIdsfilas[i], "Empleado", "-", "");
    }
}

function putDatoBodegaEmpleadoFila(idFila, selector, datoSpan, datoHidden)
{
    $('#tbDatosEspecificos').DataTable().cell($('#td' + selector + "_" + idFila)).data(datoSpan).draw();
    $("#h" + selector + "_" + idFila).val(datoHidden);
}

function putDatoNumeroClaveCatastralFila(idFila, dato)
{
    $('#tbDatosEspecificos').DataTable().cell($('#td' + objName.nameNumeroClaveCatastral + "_" + idFila)).data(dash(dato)).draw();
    $("#h" + objName.nameNumeroClaveCatastral + "_" + idFila).val(dato);
}

function putDatoNumeroChasisFila(idFila, dato)
{
    $('#tbDatosEspecificos').DataTable().cell($('#td' + objName.nameNumeroChasis + "_" + idFila)).data(dash(dato)).draw();
    $("#h" + objName.nameNumeroChasis + "_" + idFila).val(dato);
}

function putDatoNumeroMotorFila(idFila, dato) {
    $('#tbDatosEspecificos').DataTable().cell($('#td' + objName.nameNumeroMotor + "_" + idFila)).data(dash(dato)).draw();
    $("#h" + objName.nameNumeroMotor + "_" + idFila).val(dato);
}

function putDatoPlacaFila(idFila, dato) {
    $('#tbDatosEspecificos').DataTable().cell($('#td' + objName.namePlaca + "_" + idFila)).data(dash(dato)).draw();
    $("#h" + objName.namePlaca + "_" + idFila).val(dato);
}

function putDatoSerieFila(idFila, dato) {
    $('#tbDatosEspecificos').DataTable().cell($('#td' + objName.nameSerie + "_" + idFila)).data(dash(dato)).draw();
    $("#h" + objName.nameSerie + "_" + idFila).val(dato);
}

function putDatoCodificacion(idFila, dato)
{
    $('#tbDatosEspecificos').DataTable().cell($('#tdCodificacion_' + idFila)).data(dash(dato)).draw();
    $("#hCodigoSecuencial_" + idFila).val(dato);
}

function validarDatosEspecificosPertenecenBodegaEmpleadoCodigosecuencial()
{
    for (var i = 0; i < arrIdsfilas.length; i++) {
        var idFila = arrIdsfilas[i];
        var idBodega = $("#hBodega_" + idFila).val();
        var idEmpleado = $("#hEmpleado_" + idFila).val();
        var codigoSecuencial = $("#hCodigoSecuencial_" + idFila).val();

        if ((idBodega == "" || idBodega == null) && (idEmpleado == "" || idEmpleado == null) || (codigoSecuencial == "" || codigoSecuencial == null))
            return false;
    }
    return true;
}

function validarDatosEspecificosTablaActivosFijosAdicionados(objData) {
    var idFilaGestion = parseInt($("#IdFilaGestion").val());
    var validar = true;
    if (categoria == objCategorias.Edificio) {
        var arrNumerosClaveCatastral = $(".hidden" + objName.nameNumeroClaveCatastral).toArray();
        for (var j = 0; j < arrNumerosClaveCatastral.length; j++) {
            var numClaveCatastral = $(arrNumerosClaveCatastral[j]);
            var idFila = numClaveCatastral.prop("id").split("_")[1];
            if (idFilaGestion != idFila && objData.NumeroClaveCatastral != null && objData.NumeroClaveCatastral != "") {
                if (numClaveCatastral.val() == objData.NumeroClaveCatastral) {
                    $("#val" + objName.nameNumeroClaveCatastral).html("El Número de clave catastral: está asignado a un Activo Fijo adicionado.");
                    validar = false;
                }
            }
        }
    }
    else if (categoria == objCategorias.Vehiculo) {
        var arrNumerosChasis = $(".hidden" + objName.nameNumeroChasis).toArray();
        for (var j = 0; j < arrNumerosChasis.length; j++) {
            var numChasis = $(arrNumerosChasis[j]);
            var idFila = numChasis.prop("id").split("_")[1];
            if (idFilaGestion != idFila && objData.NumeroChasis != null && objData.NumeroChasis != "") {
                if (numChasis.val() == objData.NumeroChasis) {
                    $("#val" + objName.nameNumeroChasis).html("El Número de chasis: está asignado a un Activo Fijo adicionado.");
                    validar = false;
                }
            }
        }

        var arrNumerosMotor = $(".hidden" + objName.nameNumeroMotor).toArray();
        for (var j = 0; j < arrNumerosMotor.length; j++) {
            var numMotor = $(arrNumerosMotor[j]);
            var idFila = numMotor.prop("id").split("_")[1];
            if (idFilaGestion != idFila && objData.NumeroMotor != null && objData.NumeroMotor != "") {
                if (numMotor.val() == objData.NumeroMotor) {
                    $("#val" + objName.nameNumeroMotor).html("El Número de motor: está asignado a un Activo Fijo adicionado.");
                    validar = false;
                }
            }
        }

        var arrNumerosPlaca = $(".hidden" + objName.namePlaca).toArray();
        for (var j = 0; j < arrNumerosPlaca.length; j++) {
            var numPlaca = $(arrNumerosPlaca[j]);
            var idFila = numPlaca.prop("id").split("_")[1];
            if (idFilaGestion != idFila && objData.Placa != null && objData.Placa != "") {
                if (numPlaca.val() == objData.Placa) {
                    $("#val" + objName.namePlaca).html("La Placa: está asignada a un Activo Fijo adicionado.");
                    validar = false;
                }
            }
        }
    }
    var arrNumerosSerie = $(".hidden" + objName.nameSerie).toArray();
    for (var j = 0; j < arrNumerosSerie.length; j++) {
        var numSerie = $(arrNumerosSerie[j]);
        var idFila = numSerie.prop("id").split("_")[1];
        if (idFilaGestion != idFila && objData.Serie != null && objData.Serie != "") {
            if (numSerie.val() == objData.Serie) {
                $("#val" + objName.nameSerie).html("La Serie: está asignada a un Activo Fijo adicionado.");
                validar = false;
            }
        }
    }
    return validar;
}

function validarDatosEspecificosExisten(objData)
{
    var validar = true;
    for (var i = 0; i < arrIdsfilas.length; i++) {
        var idFila = arrIdsfilas[i];
        if (objData.IdFila != idFila)
        {
            if (categoria == objCategorias.Edificio)
            {
                if (objData.NumeroClaveCatastral == $("#td" + objName.nameNumeroClaveCatastral + "_" + idFila).html()) {
                    $("#val" + objName.nameNumeroClaveCatastral).html("El Número de clave catastral: ya existe.");
                    return false;
                }
            }
            else if (categoria == objCategorias.Vehiculo)
            {
                var validar = true;
                if (objData.NumeroChasis == $("#td" + objName.nameNumeroChasis + "_" + idFila).html()) {
                    $("#val" + objName.nameNumeroChasis).html("El Número de chasis: ya existe.");
                    validar = false;
                }
                if (objData.NumeroMotor == $("#td" + objName.nameNumeroMotor + "_" + idFila).html()) {
                    $("#val" + objName.nameNumeroMotor).html("El Número de motor: ya existe.");
                    validar = false;
                }
                if (objData.Placa == $("#td" + objName.namePlaca + "_" + idFila).html()) {
                    $("#val" + objName.namePlaca).html("La Placa: ya existe.");
                    validar = false;
                }
            }
            if (objData.Serie == $("#td" + objName.nameSerie + "_" + idFila).html()) {
                $("#val" + objName.nameSerie).html("La Serie: ya existe.");
                validar = false;
            }
        }
    }
    return validar;
}

function validarActivoFijoExiste(idSubClaseActivoFijo, idModelo, nombreActivoFijo)
{
    var idFilaGestion = parseInt($("#IdFilaGestion").val());
    var arrNombresActivoFijo = $(".hiddenNombreActivoFijo").toArray();
    for (var j = 0; j < arrNombresActivoFijo.length; j++) {
        var nombreAF = $(arrNombresActivoFijo[j]);
        var idFila = nombreAF.prop("id").split("_")[1];
        if (idFilaGestion != idFila) {
            var idSubclaseAF = $("#hhIdSubclaseActivoFijo_" + idFila).val();
            var idModeloAF = $("#hhModelo_" + idFila).val();
            if (nombreAF.val() == nombreActivoFijo && idSubclaseAF == idSubClaseActivoFijo && idModeloAF == idModelo) {
                mostrarNotificacion("Error", "Ya existe un Activo Fijo con la misma Subclase de activo fijo, Modelo y Nombre.");
                return false;
            }
        }
    }
    return true;
}

function callBackFunctionEliminarDatoEspecifico(idFila)
{
    var posFila = obtenerPosIdFila(idFila);
    deleteRowTableDatosEspecificos(idFila);
    arrIdsfilas.splice(posFila, 1);
    eliminarComponente(idFila);
    $("#ActivoFijo_Cantidad").val(arrIdsfilas.length);

    if (arrIdsfilas.length == 1)
        $("#divEliminarDatosEspecificos_" + arrIdsfilas[0]).addClass("hide");
}

function cargarFormularioCodificacion(idFila)
{
    mostrarLoadingPanel("checkout-form", "Cargando datos, por favor espere...");
    var hCodificacion = $("#hCodigoSecuencial_" + idFila).val();
    var numeroConsecutivo = 1;
    if (hCodificacion != "" && hCodificacion != null) {
        try {
            var arrCodificacion = hCodificacion.split(".");
            numeroConsecutivo = arrCodificacion[arrCodificacion.length - 1];
        } catch (e) { }
    }

    $.ajax({
        url: urlCodificacion,
        method: "POST",
        data: { Codigosecuencial: $("#hCodigoSecuencial_" + idFila).val() },
        success: function (data) {
            Init_BootBox("Codificación", data, "large", null, {
                isGuardar: true, hideAlGuardar: false, callbackGuardar: function () {
                    guardarCodificacion();
                }
            });

            $("#hIdFilaModalDatosEspecificos").val(idFila);
            $("#hIdRecepcionActivoFijoDetalle").val($("#hIdRecepcionActivoFijoDetalle_" + idFila).val());
            $("#mFuncionCallbak").val("guardarCodificacion");
            
            $("#CodigoActivoFijo_Consecutivo").val(numeroConsecutivo);
            $(".spinnerNumeroConsecutivo").spinner();
            $("#spanCodigoSecuencial").html(generarCodigosecuencial());
            asignarNumeroConsecutivoCodigoBarras();
            eventoSpinnerNumeroConsecutivo();
        },
        error: function (errorMessage) {
            mostrarNotificacion("Error", "Ocurrió un error al cargar el formulario.");
        },
        complete: function (data) {
            $("#checkout-form").waitMe("hide");
        }
    });
}

function guardarCodificacion()
{
    asignarNumeroConsecutivoCodigoBarras();
    mostrarLoadingPanel("checkout-form", "Validando datos, por favor espere...");
    $.each($(".validationCodificacion"), function (index, value) {
        $(value).html("");
    });
    var idFila = $("#hIdFilaModalDatosEspecificos").val();

    var objData = {
        IdFila: idFila,
        IdRecepcionActivoFijoDetalle: $("#hIdRecepcionActivoFijoDetalle").val(),
        Codigosecuencial: $("#spanCodigoSecuencial").html() + $("#spanNumeroConsecutivo").html(),
        IdCodigoActivoFijo: $("#hIdCodigoActivoFijo_" + idFila).val()
    };
    if (validarCodificacionNoExiste(objData))
    {
        $.ajax({
            url: urlValidarCodigoUnico,
            method: "POST",
            data: { idCodigoActivoFijo: objData.IdCodigoActivoFijo, codigoSecuencial: objData.Codigosecuencial },
            success: function (data)
            {
                if (data.toString().toLowerCase() == "false")
                {
                    putDatoCodificacion(objData.IdFila, objData.Codigosecuencial);
                    closeBootBox();
                }
                else
                    $("#valConsecutivo").html("El Código secuencial: ya existe.");
            },
            error: function (errorMessage) {
                mostrarNotificacion("Error", "Ocurrió un error al validar el formulario.");
            },
            complete: function (data) {
                $("#checkout-form").waitMe("hide");
            }
        });
    }
    else
        $("#checkout-form").waitMe("hide");
}

function validarCodificacionNoExiste(objData)
{
    for (var i = 0; i < arrIdsfilas.length; i++) {
        var idFila = arrIdsfilas[i];
        if (objData.IdFila != idFila)
        {
            if (objData.Codigosecuencial == $("#hCodigoSecuencial_" + idFila).val()) {
                $("#valConsecutivo").html("El Código secuencial: ya existe.");
                return false;
            }
        }
    }
    return validarCodificacionTablaActivosFijos(objData.Codigosecuencial); 
}

function validarCodificacionTablaActivosFijos(codigoSecuencial)
{
    var idFilaGestion = parseInt($("#IdFilaGestion").val());
    var arrCodigosSecuenciales = $(".hiddenCodigoSecuencial").toArray();
    for (var j = 0; j < arrCodigosSecuenciales.length; j++) {
        var codSecuencial = $(arrCodigosSecuenciales[j]);
        var idFila = codSecuencial.prop("id").split("_")[1];
        if (idFilaGestion != idFila) {
            if (codSecuencial.val() == codigoSecuencial) {
                $("#valConsecutivo").html("El Código secuencial: está asignado a un Activo Fijo adicionado.");
                return false;
            }
        }
    }
    return true;
}

function obtenerObjVacioRafd()
{
    return {
        ActivoFijo: {
            SubClaseActivoFijo: {
                ClaseActivoFijo: {}
            },
            Modelo: {
                Marca: {}
            }
        },
        RecepcionActivoFijoDetalleEdificio: {},
        RecepcionActivoFijoDetalleVehiculo: {},
        UbicacionActivoFijoActual: {},
        CodigoActivoFijo: {},
        ComponentesActivoFijoOrigen: []
    };
}

function cargarModalDatosActivoFijo(idFila, isVistaDetalles)
{
    mostrarLoadingPanel("formDatosActivo", "Cargando datos de activo fijo...");
    arrIdsfilas = [];
    maxIdFila = 0;

    var rafd = obtenerObjVacioRafd();
    var listadoRafd = [];
    if (idFila != -1) {
        rafd.ActivoFijo.IdActivoFijo = $("#hhIdActivoFijo_" + idFila).val();
        rafd.ActivoFijo.SubClaseActivoFijo.ClaseActivoFijo.IdTipoActivoFijo = $("#hhIdTipoActivoFijo_" + idFila).val();
        rafd.ActivoFijo.SubClaseActivoFijo.IdClaseActivoFijo = $("#hhIdClaseActivoFijo_" + idFila).val();
        rafd.ActivoFijo.IdSubClaseActivoFijo = $("#hhIdSubclaseActivoFijo_" + idFila).val();
        rafd.ActivoFijo.Cantidad = $("#hhCantidad_" + idFila).val();
        rafd.ActivoFijo.Modelo.IdMarca = $("#hhMarca_" + idFila).val();
        rafd.ActivoFijo.IdModelo = $("#hhModelo_" + idFila).val();
        rafd.ActivoFijo.Nombre = $("#hhNombreActivoFijo_" + idFila).val();
        rafd.ActivoFijo.ValorCompra = $("#hhValorCompra_" + idFila).val();
        rafd.ActivoFijo.Depreciacion = $("#hhDepreciacion_" + idFila).val();
        rafd.ActivoFijo.ValidacionTecnica = $("#hhValidacionTecnica_" + idFila).val();

        try { var arrNumeroClaveCatastral = $("#hh" + objName.nameNumeroClaveCatastral + "_" + idFila).val().split(","); } catch (e) { var arrNumeroClaveCatastral = []; }
        try { var arrNumeroChasis = $("#hh" + objName.nameNumeroChasis + "_" + idFila).val().split(","); } catch (e) { var arrNumeroChasis = []; }
        try { var arrNumeroMotor = $("#hh" + objName.nameNumeroMotor + "_" + idFila).val().split(","); } catch (e) { var arrNumeroMotor = []; }
        try { var arrPlaca = $("#hh" + objName.namePlaca + "_" + idFila).val().split(","); } catch (e) { var arrPlaca = []; }
        try { var arrSerie = $("#hh" + objName.nameSerie + "_" + idFila).val().split(","); } catch (e) { var arrSerie = []; }
        try { var arrEmpleado = $("#hhEmpleado_" + idFila).val().split(","); } catch (e) { var arrEmpleado = []; }
        try { var arrCodigoSecuencial = $("#hhCodigoSecuencial_" + idFila).val().split(","); } catch (e) { var arrCodigoSecuencial = []; }
        try { var arrCodigoActivoFijo = $("#hhIdCodigoActivoFijo_" + idFila).val().split(","); } catch (e) { var arrCodigoActivoFijo = []; }
        try { var arrUbicacion = $("#hhUbicacion_" + idFila).val().split(","); } catch (e) { var arrUbicacion = []; }
        try { var arrComponentes = $("#hhComponentes_" + idFila).val().split("_"); } catch (e) { var arrComponentes = []; }
        try { var arrIdsRecepcionActivoFijoDetalle = $("#hhIdRecepcionActivoFijoDetalle_" + idFila).val().split(","); } catch (e) { var arrIdsRecepcionActivoFijoDetalle = []; }

        var arrBodega = $("#hhBodega_" + idFila).val().split(",");
        for (var i = 0; i < arrBodega.length; i++) {
            var rafdDatoEspecifico = obtenerObjVacioRafd();

            try { rafdDatoEspecifico.RecepcionActivoFijoDetalleEdificio.NumeroClaveCatastral = undash(arrNumeroClaveCatastral[i]); } catch (e) { }
            try { rafdDatoEspecifico.RecepcionActivoFijoDetalleVehiculo.NumeroChasis = undash(arrNumeroChasis[i]); } catch (e) { }
            try { rafdDatoEspecifico.RecepcionActivoFijoDetalleVehiculo.NumeroMotor = undash(arrNumeroMotor[i]); } catch (e) { }
            try { rafdDatoEspecifico.RecepcionActivoFijoDetalleVehiculo.Placa = undash(arrPlaca[i]); } catch (e) { }
            try { rafdDatoEspecifico.Serie = undash(arrSerie[i]); } catch (e) { }
            try { rafdDatoEspecifico.CodigoActivoFijo.Codigosecuencial = arrCodigoSecuencial[i]; } catch (e) { }
            try { rafdDatoEspecifico.CodigoActivoFijo.IdCodigoActivoFijo = arrCodigoActivoFijo[i]; } catch (e) { }
            try { rafdDatoEspecifico.UbicacionActivoFijoActual.IdUbicacionActivoFijo = arrUbicacion[i]; } catch (e) { }
            try { rafdDatoEspecifico.IdRecepcionActivoFijoDetalle = arrIdsRecepcionActivoFijoDetalle[i]; } catch (e) { }
            try {
                var idBodega = undash(arrBodega[i]);
                if (idBodega != null && idBodega != "")
                    rafdDatoEspecifico.UbicacionActivoFijoActual.IdBodega = idBodega;
            } catch (e) { }

            try {
                var idEmpleado = undash(arrEmpleado[i]);
                if (idEmpleado != null && idEmpleado != "")
                    rafdDatoEspecifico.UbicacionActivoFijoActual.IdEmpleado = idEmpleado;
            } catch (e) { }

            if (arrComponentes.length > 0) {
                try {
                    var arrComp = undash(arrComponentes[i]).split(",");
                    for (var j = 0; j < arrComp.length; j++) {
                        rafdDatoEspecifico.ComponentesActivoFijoOrigen.push({
                            IdRecepcionActivoFijoDetalleComponente: arrComp[j]
                        });
                    }
                } catch (e) { }
            }
            listadoRafd.push(rafdDatoEspecifico);
        }
    }
    $.ajax({
        url: urlDatosActivoFijoResult,
        method: "POST",
        data: { recepcionActivoFijoDetalle: rafd, listadoRecepcionActivoFijoDetalle: listadoRafd, isEditar: isEditar, isVistaDetalles: isVistaDetalles },
        success: function (data) {
            var texto = isVistaDetalles ? "Detalles de" : "Gestionar";
            Init_BootBox(texto + " activo fijo", data, "large", null);
            $.validator.unobtrusive.parse(document);
        },
        error: function (errorMessage) {
            mostrarNotificacion("Error", "Ocurrió un error al cargar el formulario de activos fijos, inténtelo nuevamente.");
        },
        complete: function (data) {
            Init_Select2();
            Init_Touchspin();
            eventoTipoActivoFijo();
            eventoClaseActivoFijo();
            eventoSubClaseActivoFijo();
            eventoMarca();
            eventoRamo();
            gestionarWizard(isVistaDetalles);
            eventoSpinnerCantidad();
            initArrIdFilas();
            eventoRadioDatosEspecificos();
            initComponentes();
            initDataTableFiltrado("tbDatosEspecificos", []);
            obtenerCategoria(function () {
                if (idFila == -1)
                    crearFilas(1);
                eventoCambiarCategoria();
            });
            ocultarDatosTablaEspecificos();
            $("#IdFilaGestion").val(idFila);

            if (isVistaDetalles) {
                $("#ActivoFijo_SubClaseActivoFijo_ClaseActivoFijo_IdTipoActivoFijo").prop("disabled", "disabled");
                $("#ActivoFijo_SubClaseActivoFijo_IdClaseActivoFijo").prop("disabled", "disabled");
                $("#ActivoFijo_IdSubClaseActivoFijo").prop("disabled", "disabled");
                $("#ActivoFijo_Cantidad").prop("disabled", "disabled");
                $("#ActivoFijo_Modelo_IdMarca").prop("disabled", "disabled");
                $("#ActivoFijo_IdModelo").prop("disabled", "disabled");
                $("#ActivoFijo_Nombre").prop("disabled", "disabled");
                $("#ActivoFijo_ValorCompra").prop("disabled", "disabled");
                $("#ActivoFijo_Depreciacion").prop("disabled", "disabled");
                $("#ActivoFijo_ValidacionTecnica").prop("disabled", "disabled");
            }
            $("#formDatosActivo").waitMe("hide");
        }
    });
}

function guardarDatosActivoFijoRow()
{
    var idFilaGestion = parseInt($("#IdFilaGestion").val());
    var idFila = -1;
    if (idFilaGestion == -1) {
        var api = $("#tableDetallesRecepcion").dataTable().api();
        idFila = api.rows().nodes().length + 1;
    }
    else
        idFila = idFilaGestion;

    var idActivoFijo = $("#IdActivoFijo").val();
    var idTipoActivoFijo = $("#ActivoFijo_SubClaseActivoFijo_ClaseActivoFijo_IdTipoActivoFijo").val();
    var idClaseActivoFijo = $("#ActivoFijo_SubClaseActivoFijo_IdClaseActivoFijo").val();
    var idSubClaseActivoFijo = $("#ActivoFijo_IdSubClaseActivoFijo").val();
    var idMarca = $("#ActivoFijo_Modelo_IdMarca").val();
    var idModelo = $("#ActivoFijo_IdModelo").val();
    var cantidad = $("#ActivoFijo_Cantidad").val();
    var nombreActivoFijo = $("#ActivoFijo_Nombre").val();
    var valorCompra = $("#ActivoFijo_ValorCompra").val();
    var depreciacion = $("#ActivoFijo_Depreciacion").prop("checked");
    var validacionTecnica = $("#ActivoFijo_ValidacionTecnica").prop("checked");

    var textIdTipoActivoFijo = $("#ActivoFijo_SubClaseActivoFijo_ClaseActivoFijo_IdTipoActivoFijo option:selected").text();
    var textIdClaseActivoFijo = $("#ActivoFijo_SubClaseActivoFijo_IdClaseActivoFijo option:selected").text();
    var textIdSubclaseActivoFijo = $("#ActivoFijo_IdSubClaseActivoFijo option:selected").text();
    var textMarca = $("#ActivoFijo_Modelo_IdMarca option:selected").text();
    var textModelo = $("#ActivoFijo_IdModelo option:selected").text();

    if (validarActivoFijoExiste(idFila, idSubClaseActivoFijo, idModelo, nombreActivoFijo)) {
        var arrIdsRecepcionActivoFijoDetalle = [];
        var arrIdsUbicacionActivoFijo = [];
        var arrIdsCodigoActivoFijo = [];
        var arrNumeroClaveCatastral = [];
        var arrNumeroChasis = [];
        var arrNumeroMotor = [];
        var arrPlacas = [];
        var arrSeries = [];
        var arrBodegas = [];
        var arrEmpleados = [];
        var arrCodigosSecuencial = [];
        var arrComponentes = [];

        for (var i = 0; i < arrIdsfilas.length; i++) {
            var row = arrIdsfilas[i];
            if (categoria == objCategorias.Edificio) {
                arrNumeroClaveCatastral.push($("#td" + objName.nameNumeroClaveCatastral + "_" + row).html());
            }
            else if (categoria == objCategorias.Vehiculo) {
                arrNumeroChasis.push($("#td" + objName.nameNumeroChasis + "_" + row).html());
                arrNumeroMotor.push($("#td" + objName.nameNumeroMotor + "_" + row).html());
                arrPlacas.push($("#td" + objName.namePlaca + "_" + row).html());
            }
            arrSeries.push($("#td" + objName.nameSerie + "_" + row).html());
            arrBodegas.push(dash($("#hBodega_" + row).val()));
            arrEmpleados.push(dash($("#hEmpleado_" + row).val()));
            arrCodigosSecuencial.push(dash($("#hCodigoSecuencial_" + row).val()));
            arrComponentes.push(dash($("#hComponentes_" + row).val()));
            arrIdsRecepcionActivoFijoDetalle.push(dash($("#hIdRecepcionActivoFijoDetalle_" + row).val()));
            arrIdsUbicacionActivoFijo.push(dash($("#hUbicacion_" + row).val()));
            arrIdsCodigoActivoFijo.push(dash($("#hIdCodigoActivoFijo_" + row).val()));
        }
        var numerosClaveCatastral = arrNumeroClaveCatastral.join(',');
        var numerosChasis = arrNumeroChasis.join(',');
        var numerosMotor = arrNumeroMotor.join(',');
        var numerosPlaca = arrPlacas.join(',');
        var numerosSerie = arrSeries.join(',');
        var bodegas = arrBodegas.join(',');
        var empleados = arrEmpleados.join(',');
        var codigosSecuenciales = arrCodigosSecuencial.join(',');
        var componentes = arrComponentes.join('_');
        var idsRecepcionesActivoFijoDetalle = arrIdsRecepcionActivoFijoDetalle.join(',');
        var idsUbicacionesActivoFijo = arrIdsUbicacionActivoFijo.join(',');
        var idsCodigosActivoFijo = arrIdsCodigoActivoFijo.join(',');

        if (idFilaGestion == -1) {
            var hRecepcionActivoFijoDetalle = '<input type="hidden" class="hiddenIdRecepcionActivoFijoDetalle" id="hhIdRecepcionActivoFijoDetalle_' + idFila + '" name="hhIdRecepcionActivoFijoDetalle_' + idFila + '" value="' + idsRecepcionesActivoFijoDetalle + '" />';
            var hIdActivoFijo = '<input type="hidden" class="hiddenIdActivoFijo" id="hhIdActivoFijo_' + idFila + '" name="hhIdActivoFijo_' + idFila + '" value="' + idActivoFijo + '" />';
            var hNumeroClaveCatastral = "<input type='hidden' class='hiddenNumeroClaveCatastral' id='hhNumeroClaveCatastral_" + idFila + "'" + "name='hhNumeroClaveCatastral_" + idFila + "' value='" + numerosClaveCatastral + "' />";
            var hNumeroChasis = "<input type='hidden' class='hiddenNumeroChasis' id='hhNumeroChasis_" + idFila + "'" + "name='hhNumeroChasis_" + idFila + "' value='" + numerosChasis + "' />";
            var hNumeroMotor = "<input type='hidden' class='hiddenNumeroMotor' id='hhNumeroMotor_" + idFila + "'" + "name='hhNumeroMotor_" + idFila + "' value='" + numerosMotor + "' />";
            var hPlaca = "<input type='hidden' class='hiddenPlaca' id='hhPlaca_" + idFila + "'" + "name='hhPlaca_" + idFila + "' value='" + numerosPlaca + "' />";
            var hSerie = "<input type='hidden' class='hiddenSerie' id='hhSerie_" + idFila + "'" + "name='hhSerie_" + idFila + "' value='" + numerosSerie + "' />";
            var hBodega = "<input type='hidden' class='hiddenBodega' id='hhBodega_" + idFila + "'" + "name='hhBodega_" + idFila + "' value='" + bodegas + "' />";
            var hEmpleado = "<input type='hidden' class='hiddenEmpleado' id='hhEmpleado_" + idFila + "'" + "name='hhEmpleado_" + idFila + "' value='" + empleados + "' />";
            var hUbicacion = "<input type='hidden' class='hiddenUbicacion' id='hhUbicacion_" + idFila + "' name='hhUbicacion_" + idFila + "' value='" + idsUbicacionesActivoFijo + "' />";
            var hComponentes = "<input type='hidden' class='hiddenComponentes' id='hhComponentes_" + idFila + "' name='hhComponentes_" + idFila + "' value='" + componentes + "' />";
            var hCodigoSecuencial = "<input type='hidden' class='hiddenCodigoSecuencial' id='hhCodigoSecuencial_" + idFila + "' name='hhCodigoSecuencial_" + idFila + "' value='" + codigosSecuenciales + "' />";
            var hIdCodigoActivoFijo = "<input type='hidden' class='hiddenCodigoActivoFijo' id='hhIdCodigoActivoFijo_" + idFila + "' name='hhIdCodigoActivoFijo_" + idFila + "' value='" + idsCodigosActivoFijo + "' />";
            var hTipoActivoFijo = "<input type='hidden' class='hiddenTipoActivoFijo' id='hhIdTipoActivoFijo_" + idFila + "' name='hhIdTipoActivoFijo_" + idFila + "' value='" + idTipoActivoFijo + "' />";
            var hClaseActivoFijo = "<input type='hidden' class='hiddenClaseActivoFijo' id='hhIdClaseActivoFijo_" + idFila + "' name='hhIdClaseActivoFijo_" + idFila + "' value='" + idClaseActivoFijo + "' />";
            var hSubclaseActivoFijo = "<input type='hidden' class='hiddenSubclaseActivoFijo' id='hhIdSubclaseActivoFijo_" + idFila + "' name='hhIdSubclaseActivoFijo_" + idFila + "' value='" + idSubClaseActivoFijo + "' />";
            var hMarca = "<input type='hidden' class='hiddenMarca' id='hhMarca_" + idFila + "' name='hhMarca_" + idFila + "' value='" + idMarca + "' />";
            var hModelo = "<input type='hidden' class='hiddenModelo' id='hhModelo_" + idFila + "' name='hhModelo_" + idFila + "' value='" + idModelo + "' />";
            var hCantidad = "<input type='hidden' class='hiddenCantidad' id='hhCantidad_" + idFila + "' name='hhCantidad_" + idFila + "' value='" + cantidad + "' />";
            var hNombreActivoFijo = "<input type='hidden' class='hiddenNombreActivoFijo' id='hhNombreActivoFijo_" + idFila + "' name='hhNombreActivoFijo_" + idFila + "' value='" + nombreActivoFijo + "' />";
            var hValorCompra = "<input type='hidden' class='hiddenValorCompra' id='hhValorCompra_" + idFila + "' name='hhValorCompra_" + idFila + "' value='" + valorCompra + "' />";
            var hDepreciacion = "<input type='hidden' class='hiddenDepreciacion' id='hhDepreciacion_" + idFila + "' name='hhDepreciacion_" + idFila + "' value='" + depreciacion + "' />";
            var hValidacionTecnica = "<input type='hidden' class='hiddenValidacionTecnica' id='hhValidacionTecnica_" + idFila + "' name='hhValidacionTecnica_" + idFila + "' value='" + validacionTecnica + "' />";
            var btnDetallesActivoFijo = '<a href="javascript: void(0);" onclick="cargarModalDatosActivoFijo(' + idFila + ',true)">Detalles</a><span> | </span>';
            var btnEditarActivoFijo = '<a href="javascript: void(0);" onclick="cargarModalDatosActivoFijo(' + idFila + ',false)">Editar</a><span> | </span>';
            var btnEliminarActivoFijo = "<div id='divEliminarDatosActivoFijo_" + idFila + "' style='display:inline;'><a href='javascript: void(0);' id='btnEliminarDatosActivoFijo_" + idFila + "' onclick=abrirVentanaConfirmacion('btnEliminarDatosActivoFijo_" + idFila + "') data-funcioncallback=callBackFunctionEliminarDatoActivoFijo('" + idFila + "') data-titulo='Eliminar' data-descripcion='&#191; Desea eliminar el Activo Fijo seleccionado... ?'>Eliminar</a></div>";

            addRowDetallesActivosFijosPorArray("tableDetallesRecepcion", idFila, [thClassName.tipoActivoFijo, thClassName.claseActivoFijo, thClassName.subClaseActivoFijo, thClassName.nombreActivoFijo, thClassName.marca, thClassName.modelo, thClassName.cantidad, thClassName.valorCompra, thClassName.depreciacion, thClassName.validacionTecnica, ''],
                [
                    textIdTipoActivoFijo,
                    textIdClaseActivoFijo,
                    textIdSubclaseActivoFijo,
                    nombreActivoFijo,
                    textMarca,
                    textModelo,
                    cantidad,
                    "$" + valorCompra,
                    addRowCheckBox(idFila, depreciacion, null, true),
                    addRowCheckBox(idFila, validacionTecnica, null, true),
                    hNumeroClaveCatastral + hNumeroChasis + hNumeroMotor + hPlaca + hSerie + hBodega + hEmpleado + hRecepcionActivoFijoDetalle + hIdActivoFijo + hUbicacion + hComponentes + hCodigoSecuencial + hIdCodigoActivoFijo + hTipoActivoFijo + hClaseActivoFijo + hSubclaseActivoFijo + hMarca + hModelo + hCantidad + hNombreActivoFijo + hValorCompra + hDepreciacion + hValidacionTecnica + btnDetallesActivoFijo + btnEditarActivoFijo + btnEliminarActivoFijo
                ], true);
        }
        else {
            $("#hhNumeroClaveCatastral_" + idFila).val(numerosClaveCatastral);
            $("#hhNumeroChasis_" + idFila).val(numerosChasis);
            $("#hhNumeroMotor_" + idFila).val(numerosMotor);
            $("#hhPlaca_" + idFila).val(numerosPlaca);
            $("#hhSerie_" + idFila).val(numerosSerie);
            $("#hhBodega_" + idFila).val(bodegas);
            $("#hhEmpleado_" + idFila).val(empleados);
            $("#hhIdRecepcionActivoFijoDetalle_" + idFila).val(idsRecepcionesActivoFijoDetalle);
            $("#hhUbicacion_" + idFila).val(idsUbicacionesActivoFijo);
            $("#hhComponentes_" + idFila).val(componentes);
            $("#hhCodigoSecuencial_" + idFila).val(codigosSecuenciales);
            $("#hhIdCodigoActivoFijo_" + idFila).val(idsCodigosActivoFijo);
            $("#hhIdTipoActivoFijo_" + idFila).val(idTipoActivoFijo);
            $("#hhIdClaseActivoFijo_" + idFila).val(idClaseActivoFijo);
            $("#hhIdSubclaseActivoFijo_" + idFila).val(idSubClaseActivoFijo);
            $("#hhMarca_" + idFila).val(idMarca);
            $("#hhModelo_" + idFila).val(idModelo);
            $("#hhCantidad_" + idFila).val(cantidad);
            $("#hhNombreActivoFijo_" + idFila).val(nombreActivoFijo);
            $("#hhValorCompra_" + idFila).val(valorCompra);
            $("#hhDepreciacion_" + idFila).val(depreciacion);
            $("#hhValidacionTecnica_" + idFila).val(validacionTecnica);
            $("#tableDetallesRecepcion" + idFila + "TipoActivoFijo").html(textIdTipoActivoFijo);
            $("#tableDetallesRecepcion" + idFila + "ClaseActivoFijo").html(textIdClaseActivoFijo);
            $("#tableDetallesRecepcion" + idFila + "SubclaseActivoFijo").html(textIdSubclaseActivoFijo);
            $("#tableDetallesRecepcion" + idFila + "NombreActivoFijo").html(nombreActivoFijo);
            $("#tableDetallesRecepcion" + idFila + "Marca").html(textMarca);
            $("#tableDetallesRecepcion" + idFila + "Modelo").html(textModelo);
            $("#tableDetallesRecepcion" + idFila + "Cantidad").html(cantidad);
            $("#tableDetallesRecepcion" + idFila + "ValorCompra").html(valorCompra);
            $("#tableDetallesRecepcion" + idFila + "Depreciacion").html(addRowCheckBox(idFila, depreciacion, null, true));
            $("#tableDetallesRecepcion" + idFila + "ValidacionTecnica").html(addRowCheckBox(idFila, validacionTecnica, null, true));
        }
        closeBootBox();
    }
}

function callBackFunctionEliminarDatoActivoFijo(idFila)
{
    deleteRowDetallesActivosFijos("tableDetallesRecepcion", idFila);
}

function adicionarArrRecepcionActivoFijoDetalle() {
    var arrIds = idsRecepcionActivoFijoDetalle.split(",");
    $.each(arrIds, function (index, value) {
        if (value != "" && value != null) {
            var arrValores = value.split("_");
            adicionarRecepcionActivoFijoDetalleSeleccionado(arrValores[0], arrValores[1].toLowerCase() === "true");
        }
    });
}

function callBackFunctionSeleccionActivoFijo(idRecepcionActivoFijoDetalle, seleccionado) {
    var rafd = obtenerRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
    if (seleccionado)
        rafd.seleccionado = true;
    else
        rafd.seleccionado = false;
}

function callBackEmitirAprobacion(aprobacion)
{
    var arrIdsActivoFijo = [];
    for (var i = 0; i < arrRecepcionActivoFijoDetalleSeleccionado.length; i++) {
        if (arrRecepcionActivoFijoDetalleSeleccionado[i].seleccionado)
            arrIdsActivoFijo.push(arrRecepcionActivoFijoDetalleSeleccionado[i].idRecepcionActivoFijoDetalle);
    }

    if (arrIdsActivoFijo.length == 0)
        mostrarNotificacion("Error", "Tiene que seleccionar al menos un Activo Fijo.");
    else {
        mostrarLoadingPanel("formDatosActivo", "Procesando datos, por favor espere...");
        $.ajax({
            url: urlGestionarRevisionActivoFijo,
            method: "POST",
            data: { arrIdsActivoFijo: arrIdsActivoFijo, aprobacion: aprobacion },
            success: function (data) {
                mostrarNotificacion("Información", "La acción se ha realizado satisfactoriamente.");
                for (var i = 0; i < arrIdsActivoFijo.length; i++) {
                    var idFila = obtenerPosArrRecepcionActivoFijoDetalleTodos(arrIdsActivoFijo[i]);
                    callBackFunctionEliminarDatoActivoFijo(idFila);
                }
                $("#formDatosActivo").waitMe("hide");

                var api = $("#tableDetallesRecepcion").dataTable().api();
                if (api.rows().nodes().length == 0)
                {
                    mostrarLoadingPanel("formDatosActivo", "Redireccionando a listado de Validaciones técnicas...");
                    window.location.href = urlListadoValidacionesTecnicas;
                }
            },
            error: function (errorMessage)
            {
                mostrarNotificacion("Error", "Ocurrió un error al procesar los datos, inténtelo nuevamente.");
                $("#formDatosActivo").waitMe("hide");
            }
        });
    }
}

function validarPolizaSeguro()
{
    var numeroPoliza = $("#RecepcionActivoFijo_PolizaSeguroActivoFijo_NumeroPoliza").val().trim();
    if (numeroPoliza == "" || numeroPoliza == null)
    {
        $("#errorNumeroPoliza").html("Debe introducir el Número de póliza:");
        mostrarNotificacion("Error", "Existen errores en el formulario.");
    }
    else
        $("#formDatosActivo").submit();
}