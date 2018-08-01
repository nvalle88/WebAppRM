$(document).ready(function () {
    Init_Select2();
    Init_DatetimePicker("FechaSalida", false, true);
    Init_DatetimePicker("FechaRetorno", false, true);
    inicializarDetallesActivoSeleccion();
    initDataTableFiltrado("tableDetallesActivoFijoSeleccionados", [13, 15, 16, 17, 18, 19, 20, 21]);
    $('#tableDetallesActivoFijoSeleccionados').DataTable().page.len(-1).draw();
    eventoGuardar();
    Init_XEditable(function (idRecepcionActivoFijoDetalle, newValue) {
        callbackXEditableSuccess(idRecepcionActivoFijoDetalle, newValue.toUpperCase());
    });

    if (isVistaDetalles) {
        $("#FechaSalida").prop("disabled", "disabled");
        $("#FechaRetorno").prop("disabled", "disabled");
        $("#IdMotivoTraslado").prop("disabled", "disabled");
        $("#IdEmpleadoResponsable").prop("disabled", "disabled");
        $("#IdEmpleadoSolicita").prop("disabled", "disabled");
        $("#IdEmpleadoAutorizado").prop("disabled", "disabled");
    }
});

function callbackXEditableSuccess(idRecepcionActivoFijoDetalle, newValue)
{
    $("#hTextAreaEditable_" + idRecepcionActivoFijoDetalle).val(newValue.toUpperCase());
}

function callBackInicializarTableListadoSeleccion() {
    initDataTableFiltrado("tableDetallesActivoFijoAltas", [14, 16, 17, 18, 19, 20, 21, 22]);
}

function callBackFunctionSeleccionAlta(idRecepcionActivoFijoDetalle, seleccionado) {
    if (seleccionado) {
        adicionarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle, true);

        var arrColumnasVisibleTableACopiar = obtenerArrColumnasVisible("tableDetallesActivoFijoSeleccionados");
        mostrarOcultarColumnasPorArray("tableDetallesActivoFijoSeleccionados", true);

        var arrColumnasVisibleTableACopiando = obtenerArrColumnasVisible("tableDetallesActivoFijoAltas");
        mostrarOcultarColumnasPorArray("tableDetallesActivoFijoAltas", true);

        var hIdRecepcionActivoFijoDetalle = '<input type="hidden" class="hiddenIdRecepcionActivoFijoDetalle" id="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" name="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" value="' + idRecepcionActivoFijoDetalle + '" />';
        var btnEliminarMovilizacion = "<div id='divEliminarDatosEspecificos_" + idRecepcionActivoFijoDetalle + "' class='btnEliminarDatosEspecificos' style='display:inline;'><a href='javascript: void(0);' id='btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "' onclick=abrirVentanaConfirmacion('btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "') data-funcioncallback=callBackFunctionEliminarDatoEspecifico('" + idRecepcionActivoFijoDetalle + "') data-titulo='Eliminar' data-descripcion='&#191; Desea eliminar el Activo Fijo seleccionado... ?'>Eliminar</a></div>";
        var arrValores = obtenerArrValores("tableDetallesActivoFijoAltas", idRecepcionActivoFijoDetalle, ['Codigosecuencial', 'TipoActivoFijo', 'ClaseActivoFijo', 'SubclaseActivoFijo', 'NombreActivoFijo', 'Marca', 'Modelo', 'Serie', 'NumeroChasis', 'NumeroMotor', 'Placa', 'NumeroClaveCatastral', 'Sucursal', 'Bodega', 'Empleado', 'Proveedor', 'MotivoAlta', 'FechaRecepcion', 'OrdenCompra', 'FondoFinanciamiento', 'FechaAlta', 'NumeroFactura', 'Componentes'], false);

        var valueObservaciones = $("#tableDetallesActivoFijoAltas" + idRecepcionActivoFijoDetalle + "Observaciones").html().toString().trim();
        var observaciones = valueObservaciones == "" || valueObservaciones == "-" || valueObservaciones == null ? "" : valueObservaciones;
        var btnObserbaciones = '<a href="form-x-editable.html#" id="textAreaEditable_' + idRecepcionActivoFijoDetalle + '" class="btntextAreaEditable" data-idrecepcionactivofijodetalle="' + idRecepcionActivoFijoDetalle + '" data-type="textarea" data-pk="' + idRecepcionActivoFijoDetalle + '" data-original-title="Observaciones">' + observaciones + '</a>' + '<input type="hidden" id="hTextAreaEditable_' + idRecepcionActivoFijoDetalle + '" name="hTextAreaEditable_' + idRecepcionActivoFijoDetalle + '" value="' + observaciones + '" />';
        arrValores.push(btnObserbaciones);

        arrValores.push(hIdRecepcionActivoFijoDetalle + btnEliminarMovilizacion);
        addRowDetallesActivosFijosPorArray("tableDetallesActivoFijoSeleccionados", idRecepcionActivoFijoDetalle, ['Codigosecuencial', 'TipoActivoFijo', 'ClaseActivoFijo', 'SubclaseActivoFijo', 'NombreActivoFijo', 'Marca', 'Modelo', 'Serie', 'NumeroChasis', 'NumeroMotor', 'Placa', 'NumeroClaveCatastral', 'Sucursal', 'Bodega', 'Empleado', 'Proveedor', 'MotivoAlta', 'FechaRecepcion', 'OrdenCompra', 'FondoFinanciamiento', 'FechaAlta', 'NumeroFactura', 'Componentes'], arrValores, true);

        mostrarOcultarColumnas("tableDetallesActivoFijoSeleccionados", arrColumnasVisibleTableACopiar);
        mostrarOcultarColumnas("tableDetallesActivoFijoAltas", arrColumnasVisibleTableACopiando);

        Init_XEditable(function (idRecepcionActivoFijoDetalle, newValue) {
            callbackXEditableSuccess(idRecepcionActivoFijoDetalle, newValue.toUpperCase());
        });
    }
    else
        callBackFunctionEliminarDatoEspecifico(idRecepcionActivoFijoDetalle);
}

function callBackFunctionEliminarDatoEspecifico(idRecepcionActivoFijoDetalle) {
    deleteRowDetallesActivosFijos("tableDetallesActivoFijoSeleccionados", idRecepcionActivoFijoDetalle);
    eliminarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
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