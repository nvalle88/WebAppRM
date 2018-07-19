jQuery(document).ready(function (e) {
    Init_DatetimePicker("FechaCorteInventario", true);
    Init_DatetimePicker("FechaInforme", true);
    Init_Select2();
    adicionarArrRecepcionActivoFijoDetalle();
    initDataTableFiltrado("tableDetallesActivoFijoBajas", [14, 16, 17, 18, 19, 20, 21, 22, 23], function () {
        var table = $("#tableDetallesActivoFijoBajas").dataTable();
        var api = table.api();
        var rows = api.rows({ page: 'current' }).nodes();
        var last = null;
        var groupadmin = [];

        crearGrupo(api, rows, last, groupadmin, 18, "Fondo de financiamiento", 0, 23);
        crearGrupo(api, rows, last, groupadmin, 3, "Clase de activo fijo", 6, 23);
        crearGrupo(api, rows, last, groupadmin, 4, "Subclase de activo fijo", 12, 23);
    });
    eventoGuardar();
    eventoLectorCodigoBarras();
    inicializarIdsArrRecepcionActivoFijoDetalleTodos();
});

function eventoLectorCodigoBarras()
{
    Init_DeteccionLectorCodigoBarras("CodigoActivoFijo_Codigosecuencial", function (barcode, qty) {
        eventoScannerCodigosecuencial(barcode, qty);
    });
}

function eventoScannerCodigosecuencial(barcode, qty)
{
    mostrarLoadingPanel("checkout-form", "Cargando datos de activo fijo...");
    limpiarCampos();
    $("#CodigoActivoFijo_Codigosecuencial").val(barcode);
    $.ajax({
        url: urlDatosInventarioActivoFijo,
        method: "POST",
        data: { codigoSecuencial: barcode },
        success: function (data) {
            $("#divDatosInventarioActivoFijo").html(data);
            if (existeActivoFijoEnTabla())
            {
                mostrarNotificacion("Error", "El activo fijo con el código secuencial " + barcode + " ya se encuentra en el listado.");
                limpiarCampos();
            }
            else {
                HabilitarCheckConstatadoBtnRegistrar(true);
                eventoRegistrar();
            }
        },
        error: function (errorObj) {
            mostrarNotificacion("Error", errorObj.responseText);
            limpiarCampos();
        },
        complete: function (data) {
            eventoLectorCodigoBarras();
            $("#checkout-form").waitMe("hide");
        }
    });
}

function existeActivoFijoEnTabla()
{
    var idRecepcionActivoFijoDetalle = $("#IdRecepcionActivoFijoDetalle").val();
    var rafd = obtenerRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
    return rafd != null;
}

function HabilitarCheckConstatadoBtnRegistrar(habilitar)
{
    if (habilitar) {
        $("#chkConstatado").prop("disabled", "");
        $("#btn-registrar").prop("disabled", "");
    }
    else {
        $("#chkConstatado").prop("disabled", "disabled");
        $("#btn-registrar").prop("disabled", "disabled");
    }
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

function callBackFunctionSeleccionBaja(idRecepcionActivoFijoDetalle, seleccionado)
{
    var rafd = obtenerRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
    if (seleccionado)
        rafd.seleccionado = true;
    else
        rafd.seleccionado = false;
}

function callBackFunctionSeleccionConstatado(idRecepcionActivoFijoDetalle, seleccionado) {
    adicionarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle, seleccionado);
    var hIdRecepcionActivoFijoDetalle = '<input type="hidden" class="hiddenIdRecepcionActivoFijoDetalle" id="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" name="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" value="' + idRecepcionActivoFijoDetalle + '" />';
    var btnEliminarActivoFijo = "<div id='divEliminarDatosEspecificos_" + idRecepcionActivoFijoDetalle + "' class='btnEliminarDatosEspecificos' style='display:inline;'><a href='javascript: void(0);' id='btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "' onclick=abrirVentanaConfirmacion('btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "') data-funcioncallback=callBackFunctionEliminarDatoEspecifico('" + idRecepcionActivoFijoDetalle + "') data-titulo='Eliminar' data-descripcion='&#191; Desea eliminar el Activo Fijo seleccionado... ?'>Eliminar</a></div>";
    
    addRowDetallesActivosFijosPorArray("tableDetallesActivoFijoBajas", idRecepcionActivoFijoDetalle, ['', 'Codigosecuencial', 'TipoActivoFijo', 'ClaseActivoFijo', 'SubclaseActivoFijo', 'NombreActivoFijo', 'Marca', 'Modelo', 'Serie', 'NumeroChasis', 'NumeroMotor', 'Placa', 'NumeroClaveCatastral', 'Sucursal', 'Bodega', 'Empleado', 'Proveedor', 'MotivoAlta', 'FechaRecepcion', 'OrdenCompra', 'FondoFinanciamiento', 'FechaAlta', 'MotivoAlta', 'NumeroFactura', ''],
        [
        addRowCheckBox(idRecepcionActivoFijoDetalle, seleccionado, "callBackFunctionSeleccionBaja", false, "", "", false),
        $("#CodigoActivoFijo_Codigosecuencial").val(),
        $("#ActivoFijo_SubClaseActivoFijo_ClaseActivoFijo_TipoActivoFijo_Nombre").val(),
        $("#ActivoFijo_SubClaseActivoFijo_ClaseActivoFijo_Nombre").val(),
        $("#ActivoFijo_SubClaseActivoFijo_Nombre").val(),
        $("#ActivoFijo_Nombre").val(),
        $("#ActivoFijo_Modelo_Marca_Nombre").val(),
        $("#ActivoFijo_Modelo_Nombre").val(),
        agregarDashValorEmpty($("#Serie").val()),
        agregarDashValorEmpty($("#NumeroChasis").val()),
        agregarDashValorEmpty($("#NumeroMotor").val()),
        agregarDashValorEmpty($("#Placa").val()),
        agregarDashValorEmpty($("#NumeroClaveCatastral").val()),
        $("#SucursalActual_Nombre").val(),
        agregarDashValorEmpty($("#UbicacionActivoFijoActual_Bodega_Nombre").val()),
        $("#UbicacionActivoFijoActual_IdEmpleado").val(),
        $("#RecepcionActivoFijo_IdProveedor").val(),
        $("#RecepcionActivoFijo_MotivoAlta_Descripcion").val(),
        $("#RecepcionActivoFijo_FechaRecepcion").val(),
        $("#RecepcionActivoFijo_OrdenCompra").val(),
        $("#RecepcionActivoFijo_FondoFinanciamiento_Nombre").val(),
        $("#AltaActivoFijoActual_FechaAlta").val(),
        $("#AltaActivoFijoActual_MotivoAlta_Descripcion").val(),
        agregarDashValorEmpty($("#AltaActivoFijoActual_FacturaActivoFijo_NumeroFactura").val()),
        hIdRecepcionActivoFijoDetalle + btnEliminarActivoFijo
    ], true);
}

function callBackFunctionEliminarDatoEspecifico(idRecepcionActivoFijoDetalle) {
    deleteRowDetallesActivosFijos("tableDetallesActivoFijoBajas", idRecepcionActivoFijoDetalle);
    eliminarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
    $("#CodigoActivoFijo_Codigosecuencial").focus();
}

function eventoRegistrar()
{
    $("#btn-registrar").on("click", function (e) {
        var idRecepcionActivoFijoDetalle = $("#IdRecepcionActivoFijoDetalle").val();
        arrRecepcionActivoFijoDetalleTodos.push(idRecepcionActivoFijoDetalle);
        var seleccionado = $("#chkConstatado").prop("checked");
        callBackFunctionSeleccionConstatado(idRecepcionActivoFijoDetalle, seleccionado);
        limpiarCampos();
        tryMarcarCheckBoxTodos();
    });
}

function limpiarCampos()
{
    $("#CodigoActivoFijo_Codigosecuencial").val("");
    $("#ActivoFijo_Nombre").val("");
    $("#SucursalActual_Nombre").val("");
    $("#UbicacionActivoFijoActual_IdEmpleado").val("");
    $("#chkConstatado").prop("checked", "");
    HabilitarCheckConstatadoBtnRegistrar(false);
    $("#CodigoActivoFijo_Codigosecuencial").focus();
}

function eventoGuardar() {
    $("#btn-guardar").on("click", function (e) {
        var form = $("#checkout-form");
        var validar = true;

        if (arrRecepcionActivoFijoDetalleSeleccionado.length == 0) {
            mostrarNotificacion("Error", "Tiene que existir al menos un Activo Fijo en el Inventario.");
            validar = false;
        }
        if (!form.valid())
            validar = false;

        if (validar) {
            var arrIdsRecepcionActivoFijoDetalle = [];
            for (var i = 0; i < arrRecepcionActivoFijoDetalleSeleccionado.length; i++)
                arrIdsRecepcionActivoFijoDetalle.push(arrRecepcionActivoFijoDetalleSeleccionado[i].idRecepcionActivoFijoDetalle + "_" + arrRecepcionActivoFijoDetalleSeleccionado[i].seleccionado);

            var idsRecepcionActivoFijoDetalle = arrIdsRecepcionActivoFijoDetalle.join(',').toString();
            $("#idsRecepcionActivoFijoDetalleSeleccionado").val(idsRecepcionActivoFijoDetalle);
            $("#btn-guardar").prop("type", "submit");
        }
    });
}