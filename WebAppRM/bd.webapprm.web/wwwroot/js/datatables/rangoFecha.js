var emplearRangoFecha = true;
var idTabla = "dt_basic";
var idDivContenedor = "divTablaListado";
var fnDrawCallBack = null;
var objMostrarOcultarTodos = null;
var ocultarVis = null;
var fnAfterComplete = null;

jQuery(document).ready(function () {
    Init_DateRangePicker(new Date((new Date).getFullYear(), 0, 1), moment(), "defaultrange");

    $("#chkRangoFecha").on("change", function (a) {
        emplearRangoFecha = $("#chkRangoFecha").prop("checked");
        if (emplearRangoFecha) {
            InitListadoTabla(urlListadoTabla, idDivContenedor, {
                fechaInicial: obtenerFechaInicial(),
                fechaFinal: obtenerFechaFinal()
            });
        }
        else
            InitListadoTabla(urlListadoTabla, idDivContenedor, null);
    });
});

function onValueChangedDateRangerPicker(a, e) {
    emplearRangoFecha && InitListadoTabla(urlListadoTabla, idDivContenedor, {
        fechaInicial: a,
        fechaFinal: e
    });
}

function InitListadoTabla(url, divContenedor, data) {
    mostrarLoadingPanel("content", "Cargando datos, por favor espere...")
    $.ajax({
        url: url,
        data: data,
        method: "POST",
        success: function (data) {
            $("#" + divContenedor).html(data);
            initDataTableFiltrado(idTabla, [], function () {
                try {
                    if (fnDrawCallBack && fnDrawCallBack != null)
                        fnDrawCallBack();
                } catch (e) { }
            }, objMostrarOcultarTodos, ocultarVis);
        },
        error: function (errorMessage) {
            mostrarNotificacion("Error", "Ocurrió un error mientras se cargaban los datos.");
        },
        complete: function (data) {
            try {
                if (fnAfterComplete && fnAfterComplete != null)
                    fnAfterComplete();
            } catch (e) { }
            $("#content").waitMe("hide");
        }
    });
}

function obtenerFechaInicial() {
    return $("#defaultrange").data("daterangepicker").startDate.format("MM/DD/YYYY");
}

function obtenerFechaFinal() {
    return $("#defaultrange").data("daterangepicker").endDate.format("MM/DD/YYYY");
}