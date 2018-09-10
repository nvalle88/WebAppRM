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
    var arrAgrupacion = [{ propiedad: thClassName.fechaRecepcion, valor: "Fecha" }];
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