$(document).ready(function () {
    Init_Select2();
    Init_DatetimePicker("FechaBaja", true);
    inicializarDetallesActivoSeleccion();
    inicializarObjetoAdicional();
    initDataTableFiltrado("tableDetallesActivoFijoSeleccionados", [thClassName.bodega, thClassName.proveedor, thClassName.motivoAlta, thClassName.fechaRecepcion, thClassName.ordenCompra, thClassName.fondoFinanciamiento, thClassName.fechaAlta, thClassName.numeroFactura], null, { mostrarTodos: true });
    eventoGuardar();
});

function callBackFunctionEliminarDatoEspecifico(idRecepcionActivoFijoDetalle) {
    deleteRowDetallesActivosFijos("tableDetallesActivoFijoSeleccionados", idRecepcionActivoFijoDetalle);
    eliminarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
}

function callBackInicializarTableListadoSeleccion() {
    initDataTableFiltrado("tableDetallesActivoFijoBajas", [thClassName.bodega, thClassName.proveedor, thClassName.motivoAlta, thClassName.fechaRecepcion, thClassName.ordenCompra, thClassName.fondoFinanciamiento, thClassName.fechaAlta, thClassName.numeroFactura], null, { mostrarTodos: true, ocultarTodos: true });
}

function callBackFunctionSeleccionBaja(idRecepcionActivoFijoDetalle, seleccionado) {
    if (seleccionado) {
        adicionarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle, true);
        var idEmpleado = $("#tableDetallesActivoFijoBajas" + idRecepcionActivoFijoDetalle + "Empleado").data("idempleado");
        var hIdRecepcionActivoFijoDetalle = '<input type="hidden" class="hiddenIdRecepcionActivoFijoDetalle" id="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" name="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" value="' + idRecepcionActivoFijoDetalle + '" />';
        var hEmpleado = '<input type="hidden" class="hiddenHEmpleado" id="hEmpleado_' + idRecepcionActivoFijoDetalle + '" name="hEmpleado_' + idRecepcionActivoFijoDetalle + '" value="' + idEmpleado + '" />';
        var btnEliminarBaja = "<div id='divEliminarDatosEspecificos_" + idRecepcionActivoFijoDetalle + "' class='btnEliminarDatosEspecificos' style='display:inline;'><a href='javascript: void(0);' id='btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "' onclick=abrirVentanaConfirmacion('btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "') data-funcioncallback=callBackFunctionEliminarDatoEspecifico('" + idRecepcionActivoFijoDetalle + "') data-titulo='Eliminar' data-descripcion='&#191; Desea eliminar el Activo Fijo seleccionado... ?'>Eliminar</a></div>";
        addRowDetallesActivosFijos("tableDetallesActivoFijoSeleccionados", "tableDetallesActivoFijoBajas", idRecepcionActivoFijoDetalle, [thClassName.codigoSecuencial, thClassName.tipoActivoFijo, thClassName.claseActivoFijo, thClassName.subClaseActivoFijo, thClassName.nombreActivoFijo, thClassName.marca, thClassName.modelo, thClassName.serie, thClassName.numeroChasis, thClassName.numeroMotor, thClassName.placa, thClassName.numeroClaveCatastral, thClassName.sucursal, thClassName.dependencia, thClassName.bodega, thClassName.empleado, thClassName.proveedor, thClassName.motivoAlta, thClassName.fechaRecepcion, thClassName.ordenCompra, thClassName.fondoFinanciamiento, thClassName.fechaAlta, thClassName.numeroFactura, thClassName.componentes, hIdRecepcionActivoFijoDetalle + hEmpleado + btnEliminarBaja], true);
    }
    else
        callBackFunctionEliminarDatoEspecifico(idRecepcionActivoFijoDetalle);
}

function eventoGuardar() {
    $("#btn-guardar").on("click", function (e) {
        var api = $("#tableDetallesActivoFijoSeleccionados").dataTable().api();
        var form = $("#checkout-form");
        var validar = true;

        if (api.rows().nodes().length == 0) {
            mostrarNotificacion("Error", "Tiene que seleccionar al menos un Activo Fijo.");
            validar = false;
        }
        if (!form.valid())
            validar = false;

        if (validar)
            $("#btn-guardar").prop("type", "submit");
    });
}