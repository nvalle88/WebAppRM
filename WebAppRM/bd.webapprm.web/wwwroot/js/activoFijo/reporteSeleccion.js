jQuery(document).ready(function () {
    adicionarArrRecepcionActivoFijoDetalle();
    inicializarIdsArrRecepcionActivoFijoDetalleTodos();

    var objMostrarOcultarTodos = { mostrarTodos: false, ocultarTodos: false };
    if (isReimprimirEtiquetas) {
        objMostrarOcultarTodos.mostrarTodos = true;
        objMostrarOcultarTodos.ocultarTodos = true;
    }
    initDataTableFiltrado("dt_basic", [], null, objMostrarOcultarTodos);
    if (isReimprimirEtiquetas) {
        eventoGuardar();
    }
});

function adicionarArrRecepcionActivoFijoDetalle() {
    var arrIds = idsRecepcionActivoFijoDetalle.split(",");
    $.each(arrIds, function (index, value) {
        if (value != "" && value != null) {
            var arrValores = value.split("_");
            adicionarRecepcionActivoFijoDetalleSeleccionado(arrValores[0], arrValores[1].toLowerCase() === "true");
        }
    });
}

function callBackFunctionSeleccionBaja(idRecepcionActivoFijoDetalle, seleccionado) {
    var rafd = obtenerRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
    var table = $('#dt_basic').DataTable();
    var row = table.row("#dt_basic" + idRecepcionActivoFijoDetalle);

    if (seleccionado) {
        rafd.seleccionado = true;
        try {
            if (isReimprimirEtiquetas) {
                var codigoSecuencial = table.cell(row.index(), obtenerPosColumna("dt_basic", thClassName.codigoSecuencial)).data();
                AsignarCodigoBarrasPorId("barcode1", codigoSecuencial);
                var srcImagen = $("#barcode1").prop("src");
                var inputHiddenSrcImg = '<input type="hidden" id="codigoSecuencial_' + idRecepcionActivoFijoDetalle + '" name="codigoSecuencial_' + idRecepcionActivoFijoDetalle + '" value="' + srcImagen + '" />';
                var htmlDivContenedorInputEtiquetas = $("#divContenedorInputEtiquetas").html();
                $("#divContenedorInputEtiquetas").html(htmlDivContenedorInputEtiquetas + inputHiddenSrcImg);
            }
        } catch (e) { }
    }
    else {
        rafd.seleccionado = false;
        try {
            if (isReimprimirEtiquetas) {
                $("#codigoSecuencial_" + idRecepcionActivoFijoDetalle).remove();
            }
        } catch (e) { }
    }
}

function eventoGuardar() {
    $("#btn-guardar").on("click", function (e) {
        var form = $("#checkout-form");
        var validar = true;

        if (isReimprimirEtiquetas) {
            var arrSeleccionados = obtenerListadoRecepcionActivoFijoDetalleSeleccionado();
            if (arrSeleccionados.length == 0) {
                mostrarNotificacion("Error", "Tiene que seleccionar al menos un Activo fijo.");
                validar = false;
            }
        }
        
        if (!form.valid())
            validar = false;

        if (validar) {
            $("#checkout-form").submit();
        }
    });
}