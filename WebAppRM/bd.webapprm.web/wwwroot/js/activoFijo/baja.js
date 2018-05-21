$(document).ready(function () {
    Init_Select2();
    Init_DatetimePicker("FechaBaja");
    initDataTableFiltrado("tableDetallesActivoFijoSeleccionados", []);
    inicializarDetallesActivoSeleccion();
    inicializarObjetoAdicional();
    eventoGuardar();
});

function callBackFunctionEliminarDatoEspecifico(idRecepcionActivoFijoDetalle) {
    deleteRowDetallesActivosFijos("tableDetallesActivoFijoSeleccionados", idRecepcionActivoFijoDetalle);
    eliminarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
}

function callBackInicializarTableListadoSeleccion() {
    initDataTableFiltrado("tableDetallesActivoFijoBajas", []);
}

function callBackFunctionSeleccionBaja(idRecepcionActivoFijoDetalle, seleccionado) {
    if (seleccionado) {
        adicionarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle, true);
        var idEmpleado = $("#tableDetallesActivoFijoBajas" + idRecepcionActivoFijoDetalle + "Empleado").data("idempleado");
        var hIdRecepcionActivoFijoDetalle = '<input type="hidden" class="hiddenIdRecepcionActivoFijoDetalle" id="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" name="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" value="' + idRecepcionActivoFijoDetalle + '" />';
        var hEmpleado = '<input type="hidden" class="hiddenHEmpleado" id="hEmpleado_' + idRecepcionActivoFijoDetalle + '" name="hEmpleado_' + idRecepcionActivoFijoDetalle + '" value="' + idEmpleado + '" />';
        var btnEliminarBaja = "<div id='divEliminarDatosEspecificos_" + idRecepcionActivoFijoDetalle + "' class='btnEliminarDatosEspecificos' style='display:inline;'><a href='javascript: void(0);' id='btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "' onclick=abrirVentanaConfirmacion('btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "') data-funcioncallback=callBackFunctionEliminarDatoEspecifico('" + idRecepcionActivoFijoDetalle + "') data-titulo='Eliminar' data-descripcion='&#191; Desea eliminar el Activo Fijo seleccionado... ?'>Eliminar</a></div>";
        addRowDetallesActivosFijos("tableDetallesActivoFijoSeleccionados", "tableDetallesActivoFijoBajas", idRecepcionActivoFijoDetalle, ['TipoActivoFijo', 'ClaseActivoFijo', 'SubclaseActivoFijo', 'NombreActivoFijo', 'Codigosecuencial', 'Serie', 'NumeroChasis', 'NumeroMotor', 'Placa', 'NumeroClaveCatastral', 'Sucursal', 'Bodega', 'Empleado', 'Proveedor', 'MotivoRecepcion', 'FechaRecepcion', 'OrdenCompra', 'FondoFinanciamiento', 'FechaAlta', 'MotivoAlta', 'NumeroFactura', hIdRecepcionActivoFijoDetalle + hEmpleado + btnEliminarBaja], true);
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