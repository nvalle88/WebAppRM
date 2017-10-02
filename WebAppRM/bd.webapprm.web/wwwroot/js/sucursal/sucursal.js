$(document).ready(function () {
    Init_Select2();
    eventoPais();
    eventoProvincia();
});

function eventoPais() {
    $("#Ciudad_Provincia_IdPais").on("change", function (e) {
        partialViewProvincia(e.val);
    });
}

function eventoProvincia() {
    $("#Ciudad_IdProvincia").on("change", function (e) {
        partialViewCiudad(e.val);
    });
}

function partialViewProvincia(idPais) {
    mostrarLoadingPanel("checkout-form", "Cargando provincias...");
    $.ajax({
        url: "/Sucursal/Provincia_SelectResult",
        method: "POST",
        data: { idPais: idPais != null ? idPais : -1 },
        success: function (data) {
            $("#div_provincia").html(data);
            Init_Select2();
        },
        error: function (data) {
            $("#checkout-form").waitMe("hide");
        },
        complete: function (data) {
            partialViewCiudad($("#Ciudad_IdProvincia").val());
        }
    });
}

function partialViewCiudad(idProvincia) {
    mostrarLoadingPanel("checkout-form", "Cargando ciudades...");
    $.ajax({
        url: "/Sucursal/Ciudad_SelectResult",
        method: "POST",
        data: { idProvincia: idProvincia != null ? idProvincia : -1 },
        success: function (data) {
            $("#div_ciudad").html(data);
            Init_Select2();
        },
        complete: function (data) {
            $("#checkout-form").waitMe("hide");
            eventoProvincia();
        }
    });
}