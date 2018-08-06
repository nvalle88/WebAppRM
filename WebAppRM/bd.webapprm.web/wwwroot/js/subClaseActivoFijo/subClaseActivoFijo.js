$(document).ready(function () {
    Init_Select2();
    eventoTipoActivoFijo();
    eventoRamo();
});

function eventoTipoActivoFijo() {
    $("#ClaseActivoFijo_IdTipoActivoFijo").on("change", function (e) {
        partialViewTipoActivoFijo(e.val);
    });
}

function eventoRamo() {
    $("#Subramo_IdRamo").on("change", function (e) {
        mostrarLoadingPanel("checkout-form", "Cargando subramos...");
        $.ajax({
            url: urlSubramoSelectResult,
            method: "POST",
            data: { idRamo: e.val },
            success: function (data) {
                $("#div_subramo").html(data);
                Init_Select2();
            },
            complete: function (data) {
                $("#checkout-form").waitMe("hide");
            }
        });
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