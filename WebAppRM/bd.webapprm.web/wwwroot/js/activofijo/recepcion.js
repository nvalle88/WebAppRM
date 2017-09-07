$(document).ready(function () {
    Init_Select2();
    eventoTipoActivoFijo();
    partialViewTipoActivoFijo($("#IdTipoActivoFijo").val());
});

function eventoTipoActivoFijo()
{
    $("#IdTipoActivoFijo").on("change", function (e) {
        partialViewTipoActivoFijo(e.val);
    });
}

function eventoClaseActivoFijo()
{
    $("#IdClaseActivoFijo").on("change", function (e) {
        partialViewClaseActivoFijo(e.val);
    });
}

function partialViewTipoActivoFijo(idTipoActivoFijo)
{
    mostrarLoadingPanel("checkout-form", "Cargando clases de activo fijo...");
    $.ajax({
        url: urlClaseActivoFijo,
        method: "POST",
        data: { idTipoActivoFijo: idTipoActivoFijo != null ? idTipoActivoFijo : -1 },
        success: function (data) {
            $("#div_claseActivoFijo").html(data);
            Init_Select2();
            partialViewClaseActivoFijo($("#IdClaseActivoFijo").val());
            eventoClaseActivoFijo();
        },
        complete: function (data)
        {
            ocultarLoadingPanel("checkout-form");
        }
    });
}

function partialViewClaseActivoFijo(idClaseActivoFijo)
{
    mostrarLoadingPanel("checkout-form", "Cargando sub clases de activo fijo...");
    $.ajax({
        url: urlSubClaseActivoFijo,
        method: "POST",
        data: { idClaseActivoFijo: idClaseActivoFijo != null ? idClaseActivoFijo : -1 },
        success: function (data) {
            $("#div_subClaseActivoFijo").html(data);
            Init_Select2();
        },
        complete: function (data) {
            ocultarLoadingPanel("checkout-form");
        }
    });
}