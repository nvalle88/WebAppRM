$(document).ready(function () {
    Init_Select2();
    eventoTipoActivoFijo();
});

function eventoTipoActivoFijo() {
    $("#ClaseActivoFijo_IdTipoActivoFijo").on("change", function (e) {
        partialViewTipoActivoFijo(e.val);
    });
}

function partialViewTipoActivoFijo(idTipoActivoFijo) {
    mostrarLoadingPanel("checkout-form", "Cargando clases de activo fijo...");
    $.ajax({
        url: claseActivoFijoSelectResult,
        method: "POST",
        data: { idTipoActivoFijo: idTipoActivoFijo != null ? idTipoActivoFijo : -1 },
        success: function (data) {
            $("#div_claseActivoFijo").html(data);
            Init_Select2();
        },
        complete: function (data) {
            $("#checkout-form").waitMe("hide");
        }
    });
}