$(document).ready(function () {
    Init_Select2();
    eventoProveedor();
    partialViewProveedor();
});

function eventoProveedor() {
    $("#IdProveedor").on("change", function (e) {
        partialViewProveedor();
    });
}

function partialViewProveedor() {
    mostrarLoadingPanel("content", "Cargando art&iacute;culos...");
    var idProveedor = $("#IdProveedor").val();
    $("#idProveedor").val(idProveedor);
    $.ajax({
        url: urlListadoArticulosPorProveedor,
        method: "POST",
        data: { idProveedor: idProveedor },
        success: function (data) {
            $("#divListadoArticulos").html(data);
        },
        error: function (errorMessage) {
            mostrarNotificacion("Error", "Ocurrió un error al cargar los artículos, inténtelo nuevamente.");
        },
        complete: function (data) {
            mostrarBtnAgrupar = false;
            arrAgrupacionColumnas = [];
            var arrAgrupacion = [{ propiedad: thClassName.proveedor, valor: "Proveedor" }];
            gestionarArrAgrupacionColumnas("tableDetallesArticulos", arrAgrupacion[0].propiedad, true);
            initDataTableFiltrado("tableDetallesArticulos", [], function () {
                crearGrupo("tableDetallesArticulos", arrAgrupacion);
            });
            $("#content").waitMe("hide");
        }
    });
}