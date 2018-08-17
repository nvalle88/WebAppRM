$(document).ready(function () {
    inicializarDetallesActivoSeleccion();
    initDataTableFiltrado("tableDetallesRequerimientos", [], null, { mostrarTodos: true }, true);
    eventoGuardar();
});

function callBackInicializarTableListadoSeleccion() {
    initDataTableFiltrado("tableDetallesArticulos", [], null, { mostrarTodos: true, ocultarTodos: true }, true);
}

function callBackFunctionEliminarDatoEspecifico(idRecepcionActivoFijoDetalle) {
    deleteRowDetallesActivosFijos("tableDetallesRequerimientos", idRecepcionActivoFijoDetalle);
    eliminarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
}

function CallbackFunctionCheckBoxArticulo(idRecepcionActivoFijoDetalle, seleccionado) {
    if (seleccionado) {
        adicionarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle, true);
        var hIdRecepcionActivoFijoDetalle = '<input type="hidden" class="hiddenIdRecepcionActivoFijoDetalle" id="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" name="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" value="' + idRecepcionActivoFijoDetalle + '" />';
        var btnEliminarArticulo = "<div id='divEliminarDatosEspecificos_" + idRecepcionActivoFijoDetalle + "' class='btnEliminarDatosEspecificos' style='display:inline;'><a href='javascript: void(0);' id='btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "' onclick=abrirVentanaConfirmacion('btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "') data-funcioncallback=callBackFunctionEliminarDatoEspecifico('" + idRecepcionActivoFijoDetalle + "') data-titulo='Eliminar' data-descripcion='&#191; Desea eliminar el Artículo seleccionado... ?'>Eliminar</a></div>";
        addRowDetallesActivosFijos("tableDetallesRequerimientos", "tableDetallesArticulos", idRecepcionActivoFijoDetalle, [thClassName.nombreArticulo, thClassName.cantidad, thClassName.cantidadBodega, hIdRecepcionActivoFijoDetalle + btnEliminarArticulo], true);
        $('#spinner_' + idRecepcionActivoFijoDetalle).spinner();
    }
    else
        callBackFunctionEliminarDatoEspecifico(idRecepcionActivoFijoDetalle);
}

function eventoGuardar() {
    $("#btn-guardar").on("click", function (e) {
        var api = $("#tableDetallesRequerimientos").dataTable().api();
        var form = $("#checkout-form");
        var validar = true;

        if (api.rows().nodes().length == 0) {
            mostrarNotificacion("Error", "Tiene que seleccionar al menos un Artículo.");
            validar = false;
        }

        var idBodega = $("#IdBodega").val().toString();
        if (idBodega == null || idBodega == "") {
            mostrarNotificacion("Error", "Tiene que asignar una Bodega a la dependencia " + $("#IdDependencia").val() + ".");
            validar = false;
        }

        if (!form.valid())
            validar = false;

        if (validar)
            $("#btn-guardar").prop("type", "submit");
    });
}