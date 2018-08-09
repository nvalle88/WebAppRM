funcionEvaluacionSeleccionarTodos = function ()
{
    var contArrRecepcionActivoFijoDetalleSeleccionado = 0;
    for (var i = 0; i < arrRecepcionActivoFijoDetalleSeleccionado.length; i++) {
        var posTodos = obtenerPosArrRecepcionActivoFijoDetalleTodos(arrRecepcionActivoFijoDetalleSeleccionado[i].idRecepcionActivoFijoDetalle);
        if (posTodos != -1)
            contArrRecepcionActivoFijoDetalleSeleccionado++;
    }
    return contArrRecepcionActivoFijoDetalleSeleccionado === arrRecepcionActivoFijoDetalleTodos.length;
}

$(document).ready(function () {
    Init_Select2();
    Init_DatetimePicker("FacturaActivoFijo_FechaFactura", true);
    Init_DatetimePicker("FechaAlta", true);
    Init_DatetimePicker("FechaPago");
    eventoMotivoAlta();
    inicializarDetallesActivoSeleccion();
    inicializarObjetoAdicional();
    initDataTableFiltrado("tableDetallesActivoFijoSeleccionados", [thClassName.bodega, thClassName.dependencia, thClassName.proveedor, thClassName.motivoAlta, thClassName.fechaRecepcion, thClassName.ordenCompra, thClassName.fondoFinanciamiento], function () {
        var table = $("#tableDetallesActivoFijoSeleccionados").dataTable();
        var api = table.api();
        var rows = api.rows({ page: 'current' }).nodes();
        var last = null;
        var groupadmin = [];
        crearGrupo(api, rows, last, groupadmin, 0, "No. de recepción", 0, 23);
    });
    Init_FileInput("file");
    Init_FileInput("fileFactura");
    partialViewFacturaActivoFijoIsCompra();
    $('#tableDetallesActivoFijoSeleccionados').DataTable().page.len(-1).draw();
});

function eventoMotivoAlta() {
    $("#IdMotivoAlta").on("change", function (e) {
        partialViewFacturaActivoFijoIsCompra();
    });
}

function partialViewFacturaActivoFijoIsCompra()
{
    var motivoAlta = $("#IdMotivoAlta option:selected").text();
    if (motivoAlta && motivoAlta != "" && motivoAlta != null) {
        if (motivoAlta.toLowerCase().indexOf("compra") != -1) {
            mostrarLoadingPanel("checkout-form", "Cargando detalles de factura...");
            $.ajax({
                url: urlDetalleFacturaAltaActivos,
                method: "POST",
                data: { idFacturaActivoFijo: $("#IdFacturaActivoFijo").val() },
                success: function (data) {
                    $("#divOpcionCompra").html(data);
                    Init_DatetimePicker("FacturaActivoFijo_FechaFactura", true);
                    Init_FileInput("fileFactura");
                },
                complete: function (data) {
                    $("#checkout-form").waitMe("hide");
                }
            });
        }
        else
            $("#divOpcionCompra").html("");
    }
    else
        $("#divOpcionCompra").html("");
}

function callBackFunctionEliminarDatoEspecifico(idRecepcionActivoFijoDetalle)
{
    deleteRowDetallesActivosFijos("tableDetallesActivoFijoSeleccionados", idRecepcionActivoFijoDetalle);
    eliminarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
    eliminarComponente(idRecepcionActivoFijoDetalle);
}

function callBackFunctionSeleccionAlta(idRecepcionActivoFijoDetalle, seleccionado) {
    var componentes = $("#hComunesComponentes_" + idRecepcionActivoFijoDetalle).val();
    if (seleccionado) {
        adicionarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle, true);
        addComponenteToArray(idRecepcionActivoFijoDetalle, idRecepcionActivoFijoDetalle, componentes.split(","));
        var idSucursal = $("#tableDetallesActivoFijoAltas" + idRecepcionActivoFijoDetalle + "Sucursal").data("idsucursal");
        var idEmpleado = $("#tableDetallesActivoFijoAltas" + idRecepcionActivoFijoDetalle + "Empleado").data("idempleado");
        
        var hComponentes = '<input type="hidden" id="hComponentes_' + idRecepcionActivoFijoDetalle + '" name="hComponentes_' + idRecepcionActivoFijoDetalle + '" value="' + componentes + '" />';
        var hIdRecepcionActivoFijoDetalle = '<input type="hidden" class="hiddenIdRecepcionActivoFijoDetalle" id="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" name="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" value="' + idRecepcionActivoFijoDetalle + '" />';
        var hEmpleado = '<input type="hidden" class="hiddenHEmpleado" id="hEmpleado_' + idRecepcionActivoFijoDetalle + '" name="hEmpleado_' + idRecepcionActivoFijoDetalle + '" value="' + idEmpleado + '" />';
        var hSucursal = '<input type="hidden" id="hSucursal_' + idRecepcionActivoFijoDetalle + '" name="hSucursal_' + idRecepcionActivoFijoDetalle + '" value="' + idSucursal + '" />';
        var hUbicacion = '<input type="hidden" id="hUbicacion_' + idRecepcionActivoFijoDetalle + '" name="hUbicacion_' + idRecepcionActivoFijoDetalle + '" value="' + 0 + '" />';
        var btnEmpleado = '<a href="javascript:void(0);" onclick="cargarFormularioDatosEmpleado(' + idRecepcionActivoFijoDetalle + ')" class="btnDatosEmpleado" data-idfila="' + idRecepcionActivoFijoDetalle + '">Custodio</a>';
        var btnComponentes = '<span> | </span><a href="javascript:void(0);" onclick="cargarFormularioComponentesDatosEspecificos(' + idRecepcionActivoFijoDetalle + ')" class="btnComponentesDatosEspecificos" data-idfila="' + idRecepcionActivoFijoDetalle + '" data-idorigen="' + idRecepcionActivoFijoDetalle + '">Componentes</a>';
        var btnEliminarAlta = "<div id='divEliminarDatosEspecificos_" + idRecepcionActivoFijoDetalle + "' class='btnEliminarDatosEspecificos' style='display:inline;'><span> | </span><a href='javascript: void(0);' id='btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "' onclick=abrirVentanaConfirmacion('btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "') data-funcioncallback=callBackFunctionEliminarDatoEspecifico('" + idRecepcionActivoFijoDetalle + "') data-titulo='Eliminar' data-descripcion='&#191; Desea eliminar el Activo Fijo seleccionado... ?'>Eliminar</a></div>";
        addRowDetallesActivosFijos("tableDetallesActivoFijoSeleccionados", "tableDetallesActivoFijoAltas", idRecepcionActivoFijoDetalle, [thClassName.numeroRecepcion, thClassName.codigoSecuencial, thClassName.tipoActivoFijo, thClassName.claseActivoFijo, thClassName.subClaseActivoFijo, thClassName.nombreActivoFijo, thClassName.marca, thClassName.modelo, thClassName.serie, thClassName.numeroChasis, thClassName.numeroMotor, thClassName.placa, thClassName.numeroClaveCatastral, thClassName.sucursal, thClassName.dependencia, thClassName.bodega, thClassName.empleado, thClassName.proveedor, thClassName.motivoAlta, thClassName.fechaRecepcion, thClassName.ordenCompra, thClassName.fondoFinanciamiento, hComponentes + hIdRecepcionActivoFijoDetalle + hEmpleado + hSucursal + hUbicacion + btnEmpleado + btnComponentes + btnEliminarAlta], true);
    }
    else
        callBackFunctionEliminarDatoEspecifico(idRecepcionActivoFijoDetalle);
}

function cargarFormularioDatosEmpleado(idRecepcionActivoFijoDetalle)
{
    var idSucursal = $("#hSucursal_" + idRecepcionActivoFijoDetalle).val();
    var idEmpleado = $("#hEmpleado_" + idRecepcionActivoFijoDetalle).val();
    mostrarLoadingPanel("checkout-form", "Cargando listados de custodios por sucursal...");
    $.ajax({
        url: urlModalEmpleadosResult,
        method: "POST",
        data: { idSucursal: idSucursal, idEmpleado: idEmpleado, idRecepcionActivoFijoDetalle: idRecepcionActivoFijoDetalle },
        success: function (data) {
            Init_BootBox("Selecci&oacute;n de Custodio", data, null, null, {
                isGuardar: true, hideAlGuardar: false, callbackGuardar: function () {
                    guardarDatosEmpleado();
                }
            });
        },
        complete: function (data) {
            Init_Select2();
            $("#chkTodosEmpleadosDatosEspecificos").prop("checked", "");
            $("#checkout-form").waitMe("hide");
        }
    });
}

function guardarDatosEmpleado()
{
    mostrarLoadingPanel("divEmpleadoModal", "Validando datos, por favor espere...");
    var idEmpleado = $("#IdEmpleado").val();
    if (idEmpleado != "" && idEmpleado != null) {
        var idRecepcionActivoFijoDetalle = $("#hIdRecepcionActivoFijoDetalleEmpleado").val();
        var idSucursal = $("#hSucursal_" + idRecepcionActivoFijoDetalle).val();
        var isTodosEmpleado = $("#chkTodosEmpleadosDatosEspecificos").prop("checked");
        if (isTodosEmpleado) {
            var arrHEmpleado = $(".hiddenIdRecepcionActivoFijoDetalle");
            $.each(arrHEmpleado, function (index, value) {
                var idrafd = $(value).val();
                var idSuc = $("#hSucursal_" + idrafd).val();
                if (idSucursal == idSuc)
                    putDatoEmpleadoTable(idrafd, idEmpleado);
            });
        }
        else
            putDatoEmpleadoTable(idRecepcionActivoFijoDetalle, idEmpleado);
        closeBootBox();
    }
    else
        $("#valIdEmpleado").html("Tiene que seleccionar un Custodio.");
    $("#divEmpleadoModal").waitMe("hide");
}

function putDatoEmpleadoTable(idRecepcionActivoFijoDetalle, idEmpleado) {
    $("#hEmpleado_" + idRecepcionActivoFijoDetalle).val(idEmpleado);
    $("#tableDetallesActivoFijoSeleccionados" + idRecepcionActivoFijoDetalle + "Empleado").html($("#IdEmpleado option:selected").text());
}

function validarDatosEspecificosPertenecenEmpleado() {
    var arrHEmpleado = $(".hiddenHEmpleado");
    var validar = true;
    $.each(arrHEmpleado, function (index, value) {
        var idEmpleado = $(value).val();
        if (idEmpleado.toString() == "" || idEmpleado == null)
            validar = false;
    });
    return validar;
}

function eventoGuardar()
{
    var api = $("#tableDetallesActivoFijoSeleccionados").dataTable().api();
    var form = $("#checkout-form");
    var validar = true;

    if (api.rows().nodes().length == 0) {
        mostrarNotificacion("Error", "Tiene que seleccionar al menos un Activo Fijo.");
        validar = false;
    }
    if (!validarDatosEspecificosPertenecenEmpleado()) {
        mostrarNotificacion("Error", "Tiene que seleccionar el Custodio para todos los Activos Fijos seleccionados.");
        validar = false;
    }
    if (!form.valid())
        validar = false;

    if (validar)
        $("#btn-guardar").prop("type", "submit");
}

function cargarFormularioSeleccionNumeroRecepcion()
{
    mostrarLoadingPanel("checkout-form", "Cargando recepciones de activos fijos...");
    $.ajax({
        url: urlNumeroRecepcionResult,
        method: "POST",
        success: function (data) {
            var divRecepciones = '<div class="smart-form" id="divContenedorRecepciones">';
            var divListadoRecepciones = '<div class="row" id="divContenedorListadoRecepciones"></div>';
            Init_BootBox("Recepciones de Activos Fijos", divRecepciones + data + divListadoRecepciones + "</div>", "large", null);
            eventoSeleccionNumeroRecepcion();
        },
        error: function (errorMessage) {
            mostrarNotificacion("Error", "Ocurrió un error al cargar las recepciones, inténtelo nuevamente.");
        },
        complete: function (data) {
            Init_Select2();
            ajustarBootboxPorCiento(80);
            partialViewListadoActivosFijosParaSeleccion();
            $("#checkout-form").waitMe("hide");
        }
    });
}

function eventoSeleccionNumeroRecepcion()
{
    $("#numeroRecepcion").on("change", function () {
        partialViewListadoActivosFijosParaSeleccion();
    });
}

function partialViewListadoActivosFijosParaSeleccion()
{
    mostrarLoadingPanel("divContenedorRecepciones", "Cargando recepciones de activos fijos...");
    $.ajax({
        url: urlListadoActivosFijosSeleccionResult,
        method: "POST",
        data: { listadoRecepcionActivoFijoDetalleSeleccionado: arrRecepcionActivoFijoDetalleSeleccionado, objAdicional: objAdicional, idRecepcionActivoFijo: $("#numeroRecepcion").val() },
        success: function (data) {
            $("#divContenedorListadoRecepciones").html(data);
        },
        error: function (errorMessage) {
            mostrarNotificacion("Error", "Ocurrió un error al cargar las recepciones, inténtelo nuevamente.");
        },
        complete: function (data) {
            initDataTableFiltrado("tableDetallesActivoFijoAltas", [thClassName.numeroRecepcion, thClassName.bodega, thClassName.proveedor, thClassName.motivoAlta, thClassName.fechaRecepcion, thClassName.ordenCompra, thClassName.fondoFinanciamiento], function () {
                var table = $("#tableDetallesActivoFijoAltas").dataTable();
                var api = table.api();
                var rows = api.rows({ page: 'current' }).nodes();
                var last = null;
                var groupadmin = [];
                crearGrupo(api, rows, last, groupadmin, 6, "Descripción de activo fijo", 0, 24);
            });
            $('#tableDetallesActivoFijoAltas').DataTable().page.len(-1).draw();
            tryMarcarCheckBoxTodos();
            $("#divContenedorRecepciones").waitMe("hide");
        }
    });
}