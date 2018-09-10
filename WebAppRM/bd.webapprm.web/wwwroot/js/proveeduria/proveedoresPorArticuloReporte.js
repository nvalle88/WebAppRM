$(document).ready(function () {
    Init_Select2();
    eventoArticulo();
    partialViewArticulo();
});

function eventoArticulo() {
    $("#IdArticulo").on("change", function (e) {
        partialViewArticulo();
    });
}

function partialViewArticulo() {
    mostrarLoadingPanel("content", "Cargando proveedores...");
    var idArticulo = $("#IdArticulo").val();
    $("#idArticulo").val(idArticulo);
    $.ajax({
        url: urlListadoProveedoresPorArticulo,
        method: "POST",
        data: { idArticulo: idArticulo },
        success: function (data) {
            $("#divListadoProveedores").html(data);
        },
        error: function (errorMessage) {
            mostrarNotificacion("Error", "Ocurrió un error al cargar los proveedores, inténtelo nuevamente.");
        },
        complete: function (data) {
            mostrarBtnAgrupar = false;
            arrAgrupacionColumnas = [];
            var arrAgrupacion = [{ propiedad: thClassName.nombreArticulo, valor: "Artículo" }];
            gestionarArrAgrupacionColumnas("dt_basic", arrAgrupacion[0].propiedad, true);
            initDataTableFiltrado("dt_basic", [], function () {
                crearGrupo("dt_basic", arrAgrupacion);
            });
            $("#content").waitMe("hide");
        }
    });
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