mostrarBtnAgrupar = false;
fnAfterComplete = function () {
    if (emplearRangoFecha)
        putFechaInicialFinalHidden();
    else {
        $("#fechaInicial").val("");
        $("#fechaFinal").val("");
    }
};

fnDrawCallBack = function () {
    arrAgrupacionColumnas = [];
    var arrAgrupacion = [];

    switch (tipoReporte) {
        case "MovimientosRecepcion": arrAgrupacion = [{ propiedad: thClassName.fechaRecepcion, valor: "Fecha" }]; break;
        case "MovimientosSalida": arrAgrupacion = [{ propiedad: thClassName.fechaAprobadoDenegado, valor: "Fecha de aprobación" }]; break;
    }
    
    gestionarArrAgrupacionColumnas("dt_basic", arrAgrupacion[0].propiedad, true);
    crearGrupo("dt_basic", arrAgrupacion);
};

$(document).ready(function () {
    putFechaInicialFinalHidden();
    InitListadoTabla(urlListadoTabla, idDivContenedor, {
        fechaInicial: obtenerFechaInicial(),
        fechaFinal: obtenerFechaFinal()
    });
});

function putFechaInicialFinalHidden() {
    $("#fechaInicial").val(obtenerFechaInicial());
    $("#fechaFinal").val(obtenerFechaFinal());
}

function eventoSubmitExcel() {
    var api = $("#dt_basic").dataTable().api();
    var form = $("#checkout-form");
    var validar = true;

    if (api.rows().nodes().length == 0) {
        mostrarNotificacion("Error", "No existen registros para generar el reporte.");
        validar = false;
    }
    if (!form.valid())
        validar = false;

    if (validar)
        $("#checkout-form").submit();
}