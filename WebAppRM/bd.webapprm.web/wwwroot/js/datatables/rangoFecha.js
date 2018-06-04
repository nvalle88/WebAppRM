var emplearRangoFecha = true;
var idTabla = "dt_basic";
var idDivContenedor = "divTablaListado";
jQuery(document).ready(function () {
    Init_DateRangePicker(new Date((new Date).getFullYear(), 0, 1), moment(), "defaultrange");

    $("#chkRangoFecha").on("change", function (a) {
        emplearRangoFecha = $("#chkRangoFecha").prop("checked");
        if (emplearRangoFecha) {
            InitListadoTabla(urlListadoTabla, idDivContenedor, {
                fechaInicial: $("#defaultrange").data("daterangepicker").startDate.format("MM/DD/YYYY"),
                fechaFinal: $("#defaultrange").data("daterangepicker").endDate.format("MM/DD/YYYY")
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
            initDataTableFiltrado(idTabla, []);
        },
        error: function (errorMessage) {
            mostrarNotificacion("Error", "Ocurrió un error mientras se cargaban los datos.");
        },
        complete: function (data) {
            $("#content").waitMe("hide");
        }
    });
}