$(document).ready(function () {
    Init_Select2();
    eventoTipoArticulo();
});

function eventoTipoArticulo() {
    $("#ClaseArticulo_IdTipoArticulo").on("change", function (e) {
        partialViewTipoArticulo(e.val);
    });
}

function partialViewTipoArticulo(idTipoArticulo) {
    mostrarLoadingPanel("checkout-form", "Cargando clases de artículo...");
    $.ajax({
        url: claseArticuloSelectResult,
        method: "POST",
        data: { idTipoArticulo: idTipoArticulo != null ? idTipoArticulo : -1 },
        success: function (data) {
            $("#div_claseArticulo").html(data);
            Init_Select2();
        },
        complete: function (data) {
            $("#checkout-form").waitMe("hide");
        }
    });
}