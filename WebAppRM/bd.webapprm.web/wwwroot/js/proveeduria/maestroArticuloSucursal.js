$(document).ready(function () {
    Init_Select2();

    if (!isEditar)
        obtenerGrupoArticulo();
});

function obtenerGrupoArticulo()
{
    $.ajax({
        url: codigoArticuloSelectResult,
        method: "POST",
        data: { idArticulo: $("#IdArticulo").val() },
        success: function (data) {
            $("#spanCodigoSecuencial").html(data);
            $("#GrupoArticulo").val(data);
        },
        error: function (errorMessage)
        {
            $("#GrupoArticulo").val("");
            mostrarNotificacion("Error", "Ocurrió un error al cargar el código del artículo");
        }
    });
}

function putNumeroConsecutivo(inputCodigoArticulo)
{
    var numeroConsecutivo = $(inputCodigoArticulo).val();
    $("#spanNumeroConsecutivo").html(numeroConsecutivo && numeroConsecutivo != "" && numeroConsecutivo != null ? ("." + numeroConsecutivo) : "");
}