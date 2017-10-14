$(document).ready(function () {
    Init_Select2();
    eventoPais();
    eventoProvincia();
    eventoCiudad();

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
            url: "/MaestroArticuloSucursal/Provincia_SelectResult",
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
                eventoProvincia();
                partialViewCiudad($("#Sucursal_Ciudad_IdProvincia").val());
            }
        });
    }

    function partialViewCiudad(idProvincia) {
        mostrarLoadingPanel("checkout-form", "Cargando ciudades...");
        $.ajax({
            url: "/MaestroArticuloSucursal/Ciudad_SelectResult",
            method: "POST",
            data: { idProvincia: idProvincia != null ? idProvincia : -1 },
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
            url: "/MaestroArticuloSucursal/Sucursal_SelectResult",
            method: "POST",
            data: { idCiudad: idCiudad != null ? idCiudad : -1 },
            success: function (data) {
                $("#div_sucursal").html(data);
                Init_Select2();
            },
            error: function (data) {
                $("#checkout-form").waitMe("hide");
            },
            complete: function (data) {
                $("#checkout-form").waitMe("hide");
            }
        });
    }
});