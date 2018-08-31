jQuery(document).ready(function (e) {
    Init_DatetimePicker("FechaCorteInventario", true);
    Init_DatetimePicker("FechaInforme", true);
    Init_Select2();
    adicionarArrRecepcionActivoFijoDetalle();
    initDataTableFiltrado("tableDetallesActivoFijoBajas", [thClassName.bodega, thClassName.proveedor, thClassName.motivoAlta, thClassName.fechaRecepcion, thClassName.ordenCompra, thClassName.fondoFinanciamiento, thClassName.fechaAlta, thClassName.numeroFactura], function () {
        crearGrupo("tableDetallesActivoFijoBajas", [
            { propiedad: thClassName.fondoFinanciamiento, valor: "Fondo de financiamiento" },
            { propiedad: thClassName.claseActivoFijo, valor: "Clase de activo fijo" },
            { propiedad: thClassName.subClaseActivoFijo, valor: "Subclase de activo fijo" }
        ]);
    }, null, { mostrarTodos: true });
    eventoGuardar();
    eventoLectorCodigoBarras();
    inicializarIdsArrRecepcionActivoFijoDetalleTodos();

    //Simula la detección de un código (El parámetro debe ser un código de un activo fijo en alta)
    //$("#CodigoActivoFijo_Codigosecuencial").scannerDetection('1.002.003.999');
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
    $("#CodigoActivoFijo_Codigosecuencial").val(barcode);

    var arrIdsRecepcionActivoFijoDetalle = [];
    for (var i = 0; i < arrRecepcionActivoFijoDetalleSeleccionado.length; i++)
        arrIdsRecepcionActivoFijoDetalle.push(arrRecepcionActivoFijoDetalleSeleccionado[i].idRecepcionActivoFijoDetalle);

    $.ajax({
        url: urlDatosInventarioActivoFijo,
        method: "POST",
        data: { codigoSecuencial: barcode, listadoRafdSeleccionados: arrIdsRecepcionActivoFijoDetalle },
        success: function (data) {
            Init_BootBox("Activos fijos a constatar", data, "large", null, null, function () {
                limpiarCampos();
            });
            ajustarBootboxPorCiento(80);
        },
        error: function (errorObj) {
            mostrarNotificacion("Error", errorObj.responseText);
            limpiarCampos();
        },
        complete: function (data) {
            eventoLectorCodigoBarras();
            initDataTableFiltrado("tableDetallesDatosInventario", [], null, { mostrarTodos: true, ocultarTodos: true }, true);
            $("#checkout-form").waitMe("hide");
        }
    });
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

function callBackFunctionSeleccionConstatado(idRecepcionActivoFijoDetalle, idFila, seleccionado) {
    adicionarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle, seleccionado);
    var hIdRecepcionActivoFijoDetalle = '<input type="hidden" class="hiddenIdRecepcionActivoFijoDetalle" id="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" name="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" value="' + idRecepcionActivoFijoDetalle + '" />';
    var btnEliminarActivoFijo = "<div id='divEliminarDatosEspecificos_" + idRecepcionActivoFijoDetalle + "' class='btnEliminarDatosEspecificos' style='display:inline;'><a href='javascript: void(0);' id='btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "' onclick=abrirVentanaConfirmacion('btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "') data-funcioncallback=callBackFunctionEliminarDatoEspecifico('" + idRecepcionActivoFijoDetalle + "') data-titulo='Eliminar' data-descripcion='&#191; Desea eliminar el Activo Fijo seleccionado... ?'>Eliminar</a></div>";

    addRowDetallesActivosFijosPorArray("tableDetallesActivoFijoBajas", idRecepcionActivoFijoDetalle, ['', thClassName.codigoSecuencial, thClassName.tipoActivoFijo, thClassName.claseActivoFijo, thClassName.subClaseActivoFijo, thClassName.nombreActivoFijo, thClassName.marca, thClassName.modelo, thClassName.serie, thClassName.numeroChasis, thClassName.numeroMotor, thClassName.placa, thClassName.numeroClaveCatastral, thClassName.sucursal, thClassName.dependencia, thClassName.bodega, thClassName.empleado, thClassName.proveedor, thClassName.motivoAlta, thClassName.fechaRecepcion, thClassName.ordenCompra, thClassName.fondoFinanciamiento, thClassName.fechaAlta, thClassName.motivoAlta, thClassName.numeroFactura, ''],
        [
        addRowCheckBox(idRecepcionActivoFijoDetalle, seleccionado, "callBackFunctionSeleccionBaja", false, "", "", false),
        $("#hhCodigoActivoFijo_" + idFila).val(),
        $("#hhIdTipoActivoFijo_" + idFila).val(),
        $("#hhIdClaseActivoFijo_" + idFila).val(),
        $("#hhIdSubclaseActivoFijo_" + idFila).val(),
        $("#hhNombreActivoFijo_" + idFila).val(),
        $("#hhMarca_" + idFila).val(),
        $("#hhModelo_" + idFila).val(),
        agregarDashValorEmpty($("#hhSerie_" + idFila).val()),
        agregarDashValorEmpty($("#hhNumeroChasis_" + idFila).val()),
        agregarDashValorEmpty($("#hhNumeroMotor_" + idFila).val()),
        agregarDashValorEmpty($("#hhPlaca_" + idFila).val()),
        agregarDashValorEmpty($("#hhNumeroClaveCatastral_" + idFila).val()),
        $("#hhSucursal_" + idFila).val(),
        agregarDashValorEmpty($("#hhDependencia_" + idFila).val()),
        agregarDashValorEmpty($("#hhBodega_" + idFila).val()),
        $("#hhEmpleado_" + idFila).val(),
        $("#hhProveedor_" + idFila).val(),
        $("#hhMotivoAlta_" + idFila).val(),
        $("#hhFechaRecepcion_" + idFila).val(),
        $("#hhOrdenCompra_" + idFila).val(),
        $("#hhFondoFinanciamiento_" + idFila).val(),
        agregarDashValorEmpty($("#hhFechaAlta_" + idFila).val()),
        agregarDashValorEmpty($("#hhNumeroFactura_" + idFila).val()),
        hIdRecepcionActivoFijoDetalle + btnEliminarActivoFijo
    ], true);
}

function callBackFunctionEliminarDatoEspecifico(idRecepcionActivoFijoDetalle) {
    deleteRowDetallesActivosFijos("tableDetallesActivoFijoBajas", idRecepcionActivoFijoDetalle);
    eliminarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
    $("#CodigoActivoFijo_Codigosecuencial").focus();
}

function eventoRegistrar(btnRegistrar)
{
    var idFila = $(btnRegistrar).data("idfila");
    var idRecepcionActivoFijoDetalle = $("#hhIdRecepcionActivoFijoDetalle_" + idFila).val();
    arrRecepcionActivoFijoDetalleTodos.push(idRecepcionActivoFijoDetalle);
    var seleccionado = $("#chkConstatado_" + idFila).prop("checked");
    callBackFunctionSeleccionConstatado(idRecepcionActivoFijoDetalle, idFila, seleccionado);
    deleteRowDetallesActivosFijos("tableDetallesDatosInventario", idRecepcionActivoFijoDetalle);
    tryMarcarCheckBoxTodos();
}

function limpiarCampos()
{
    $("#CodigoActivoFijo_Codigosecuencial").val("");
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