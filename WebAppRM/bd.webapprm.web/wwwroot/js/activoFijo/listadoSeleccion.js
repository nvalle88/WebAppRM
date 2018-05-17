var arrRecepcionActivoFijoDetalleSeleccionado = Array();
jQuery(document).ready(function () {
    initDataTableFiltrado("tableActivosFijos", []);
    inicializarDetallesActivo();
});

function inicializarDetallesActivo()
{
    $.each($(".btnDetallesActivosFijos"), function (index, value) {
        var ids = $(value);
        var arrIds = ids.data("ids").toString().split(",");
        $.each(arrIds, function (index, value) {
            arrRecepcionActivoFijoDetalleSeleccionado.push({
                idRecepcionActivoFijoDetalle: value,
                seleccionado: false
            });
        });
    });
}

function abrirVentanaDetallesActivoFijo(id) {
    mostrarLoadingPanel("modalContentTableActivosFijos", "Cargando detalles de activo fijo...");
    $("#modalBodyTableActivosFijos").html("");
    var btn_detalles = $("#btnDetallesActivoFijo_" + id);
    var arrIds = btn_detalles.data("ids").toString().split(",");

    var arrAux = Array();
    for (var i = 0; i < arrIds.length; i++) {
        var rafdSeleccionado = obtenerRecepcionActivoFijoDetalleSeleccionado(arrIds[i]);
        arrAux.push(rafdSeleccionado);
    }

    $.ajax({
        url: btn_detalles.data("url"),
        method: "POST",
        data: { listadoRecepcionActivoFijoDetalleSeleccionado: arrAux, arrConfiguraciones: arrConfiguraciones },
        success: function (data) {
            $("#modalBodyTableActivosFijos").html(data);
            eventoCheckboxDetallesActivoFijo();
        },
        complete: function (data) {
            if (existeConfiguracion("IsConfiguracionRecepcion"))
                initDataTableFiltrado("tableDetallesActivoFijo", []);

            else if (existeConfiguracion("IsConfiguracionMantenimiento"))
                initDataTableFiltrado("tableDetallesActivoFijo", []);

            $("#modalContentTableActivosFijos").waitMe("hide");
        }
    });
}

function obtenerRecepcionActivoFijoDetalleSeleccionado(valor) {
    for (var i = 0; i < arrRecepcionActivoFijoDetalleSeleccionado.length; i++) {
        if (arrRecepcionActivoFijoDetalleSeleccionado[i].idRecepcionActivoFijoDetalle == valor)
            return arrRecepcionActivoFijoDetalleSeleccionado[i];
    }
    return null;
}

function existeConfiguracion(propiedad)
{
    var configuracion = obtenerConfiguracion(propiedad);
    return configuracion != null ? configuracion.valor : false;
}

function obtenerConfiguracion(propiedad)
{
    for (var i = 0; i < arrConfiguraciones.length; i++) {
        if (arrConfiguraciones[i].propiedad == propiedad)
            return arrConfiguraciones[i];
    }
    return null;
}

function eventoCheckboxDetallesActivoFijo()
{
    $(".chkDetallesActivoFijo").on("change", function (e) {
        var chk = $(e.currentTarget);
        try {
            var idRecepcionActivoFijoDetalle = chk.data("idrecepcionactivofijodetalle");
            var rafdSeleccionado = obtenerRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
            rafdSeleccionado.seleccionado = chk.prop("checked");
        }
        catch (e) { }
        try {
            var nombreFuncionCallback = chk.data("funcioncallback");
            eval(nombreFuncionCallback + "(" + idRecepcionActivoFijoDetalle + "," + chk.prop("checked") + ")");
        } catch (e) { }
    });
}

function addRowDetallesActivosFijos(idTableACopiar, idTableCopiando, idRecepcionActivoFijoDetalle, arrCeldasCopiando, isOpcionesUltimaColumna) {
    var table = $('#' + idTableACopiar).DataTable();
    var arrValores = [];
    for (var i = 0; i < arrCeldasCopiando.length; i++) {
        if (i == (arrCeldasCopiando.length - 1)) {
            if (isOpcionesUltimaColumna) {
                arrValores.push(arrCeldasCopiando[i]);
            }
            else
                arrValores.push($("#" + idTableCopiando + idRecepcionActivoFijoDetalle + arrCeldasCopiando[i]).html());
        }
        else
            arrValores.push($("#" + idTableCopiando + idRecepcionActivoFijoDetalle + arrCeldasCopiando[i]).html());
    }
    var rowNode = table.row.add(arrValores).draw().node();
    $(rowNode).prop("id", idTableACopiar + idRecepcionActivoFijoDetalle);
}

function deleteRowDetallesActivosFijos(idTableEliminar, idRecepcionActivoFijoDetalle) {
    var row = $("#" + idTableEliminar + idRecepcionActivoFijoDetalle);
    $('#' + idTableEliminar).dataTable().fnDeleteRow(row);
}