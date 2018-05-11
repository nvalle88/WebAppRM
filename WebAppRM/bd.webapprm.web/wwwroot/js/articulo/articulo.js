$(document).ready(function () {
    Init_Select2();
    eventoTipoArticulo();
    eventoClaseArticulo();
    eventoMarca();
});

function eventoTipoArticulo() {
    $("#SubClaseArticulo_ClaseArticulo_IdTipoArticulo").on("change", function (e) {
        partialViewTipoArticulo(e.val);
    });
}

function eventoClaseArticulo()
{
    $("#SubClaseArticulo_IdClaseArticulo").on("change", function (e) {
        partialViewClaseArticulo(e.val);
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
            eventoClaseArticulo();
            partialViewClaseArticulo($("#SubClaseArticulo_IdClaseArticulo").val());
        }
    });
}

function partialViewClaseArticulo(idClaseArticulo) {
    mostrarLoadingPanel("checkout-form", "Cargando subclases de artículo...");
    $.ajax({
        url: subClaseArticuloSelectResult,
        method: "POST",
        data: { idClaseArticulo: idClaseArticulo != null ? idClaseArticulo : -1 },
        success: function (data) {
            $("#div_subClaseArticulo").html(data);
            Init_Select2();
        },
        complete: function (data) {
            $("#checkout-form").waitMe("hide");
        }
    });
}

function eventoMarca() {
    $("#Modelo_IdMarca").on("change", function (e) {
        mostrarLoadingPanel("checkout-form", "Cargando modelos...");
        $.ajax({
            url: modeloSelectResult,
            method: "POST",
            data: { idMarca: $("#Modelo_IdMarca").val() },
            success: function (data) {
                $("#div_modelo").html(data);
                Init_Select2();
            },
            complete: function (data) {
                $("#checkout-form").waitMe("hide");
            }
        });
    });
}