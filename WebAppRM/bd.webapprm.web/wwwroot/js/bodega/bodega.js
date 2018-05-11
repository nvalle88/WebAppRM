$(document).ready(function () {
    Init_Select2();
    eventoPais();
    eventoProvincia();
    eventoCiudad();
});

function eventoPais() {
    $("#Sucursal_Ciudad_Provincia_IdPais").on("change", function (e) {
        partialViewProvincia(e.val);
    });
}

function eventoProvincia() {
    $("#Sucursal_Ciudad_IdProvincia").on("change", function (e) {
        partialViewCiudad(e.val);
    });
}

function eventoCiudad() {
    $("#Sucursal_IdCiudad").on("change", function (e) {
        partialViewSucursal(e.val);
    });
}

function partialViewProvincia(idPais) {
    mostrarLoadingPanel("checkout-form", "Cargando provincias...");
    $.ajax({
        url: provinciaSelectResult,
        method: "POST",
        data: { idPais: obtenerIdAjax(idPais) },
        success: function (data) {
            $("#div_provincia").html(data);
            Init_Select2();
        },
        error: function (data) {
            $("#checkout-form").waitMe("hide");
        },
        complete: function (data) {
            eventoProvincia();
            partialViewCiudad($("#Sucursal_Ciudad_IdProvincia").val());
        }
    });
}

function partialViewCiudad(idProvincia) {
    mostrarLoadingPanel("checkout-form", "Cargando ciudades...");
    $.ajax({
        url: ciudadSelectResult,
        method: "POST",
        data: { idProvincia: obtenerIdAjax(idProvincia) },
        success: function (data) {
            $("#div_ciudad").html(data);
            Init_Select2();
        },
        error: function (data) {
            $("#checkout-form").waitMe("hide");
        },
        complete: function (data) {
            eventoCiudad();
            partialViewSucursal($("#Sucursal_IdCiudad").val());
        }
    });
}

function partialViewSucursal(idCiudad) {
    mostrarLoadingPanel("checkout-form", "Cargando sucursales...");
    $.ajax({
        url: sucursalSelectResult,
        method: "POST",
        data: { idCiudad: obtenerIdAjax(idCiudad) },
        success: function (data) {
            $("#div_sucursal").html(data);
            Init_Select2();
        },
        complete: function (data) {
            $("#checkout-form").waitMe("hide");
        }
    });
}