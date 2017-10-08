$(document).ready(function () {
    Init_Select2();
    eventoPais();
    eventoProvincia();
    eventoCiudad();
    eventoSucursal();    
});

function eventoPais() {
    $("#ActivoFijo_LibroActivoFijo_Sucursal_Ciudad_Provincia_IdPais").on("change", function (e) {
        partialViewProvincia(e.val);
    });
}

function eventoProvincia() {
    $("#ActivoFijo_LibroActivoFijo_Sucursal_Ciudad_IdProvincia").on("change", function (e) {
        partialViewCiudad(e.val);
    });
}

function eventoCiudad() {
    $("#ActivoFijo_LibroActivoFijo_Sucursal_IdCiudad").on("change", function (e) {
        partialViewSucursal(e.val);
    });
}

function eventoSucursal() {
    $("#ActivoFijo_LibroActivoFijo_IdSucursal").on("change", function (e) {
        partialViewLibroActivoFijo(e.val);
    });
}

function partialViewProvincia(idPais) {
    mostrarLoadingPanel("checkout-form", "Cargando provincias...");
    $.ajax({
        url: "/ActivoFijo/Provincia_SelectResult",
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
            partialViewCiudad($("#ActivoFijo_LibroActivoFijo_Sucursal_Ciudad_IdProvincia").val());
        }
    });
}

function partialViewCiudad(idProvincia) {
    mostrarLoadingPanel("checkout-form", "Cargando ciudades...");
    $.ajax({
        url: "/ActivoFijo/Ciudad_SelectResult",
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
            partialViewSucursal($("#ActivoFijo_LibroActivoFijo_Sucursal_IdCiudad").val());
        }
    });
}

function partialViewSucursal(idCiudad) {
    mostrarLoadingPanel("checkout-form", "Cargando sucursales...");
    $.ajax({
        url: "/ActivoFijo/Sucursal_SelectResult",
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
            partialViewLibroActivoFijo($("#ActivoFijo_LibroActivoFijo_IdSucursal").val());
        }
    });
}

function partialViewLibroActivoFijo(idSucursal) {
    mostrarLoadingPanel("checkout-form", "Cargando libros de activo fijo...");
    $.ajax({
        url: "/ActivoFijo/LibroActivoFijo_SelectResult",
        method: "POST",
        data: { idSucursal: idSucursal != null ? idSucursal : -1 },
        success: function (data) {
            $("#div_libroActivoFijo").html(data);
            Init_Select2();
        },
        complete: function (data) {
            $("#checkout-form").waitMe("hide");
            eventoProvincia();
            eventoCiudad();
            eventoSucursal();
        }
    });
}