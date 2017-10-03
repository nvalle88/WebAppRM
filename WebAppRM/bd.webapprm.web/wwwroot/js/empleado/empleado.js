$(document).ready(function () {
    Init_Select2();
    eventoPais();
    eventoProvincia();
    eventoPaisSufragio();

    $('#Persona_FechaNacimiento').datetimepicker({
        'format': 'D-M-Y'
    });

    $('#FechaIngreso').datetimepicker({
        'format': 'D-M-Y hh:mm'
    });

    $('#FechaIngresoSectorPublico').datetimepicker({
        'format': 'D-M-Y hh:mm'
    });
});

function eventoPais() {
    $("#CiudadNacimiento_Provincia_IdPais").on("change", function (e) {
        partialViewProvincia(e.val);
    });
}

function eventoPaisSufragio() {
    $("#ProvinciaSufragio_IdPais").on("change", function (e) {
        partialViewProvinciaSufragio(e.val);
    });
}

function eventoProvincia() {
    $("#CiudadNacimiento_IdProvincia").on("change", function (e) {
        partialViewCiudad(e.val);
    });
}

function partialViewProvincia(idPais) {
    mostrarLoadingPanel("checkout-form", "Cargando provincias...");
    $.ajax({
        url: "/Empleado/Provincia_SelectResult",
        method: "POST",
        data: { idPais: idPais != null ? idPais : -1, partialView: "_ProvinciaSelect" },
        success: function (data) {
            $("#div_provincia").html(data);
            Init_Select2();
        },
        error: function (data) {
            $("#checkout-form").waitMe("hide");
        },
        complete: function (data) {
            partialViewCiudad($("#CiudadNacimiento_IdProvincia").val());
        }
    });
}

function partialViewCiudad(idProvincia) {
    mostrarLoadingPanel("checkout-form", "Cargando ciudades...");
    $.ajax({
        url: "/Empleado/Ciudad_SelectResult",
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

function partialViewProvinciaSufragio(idPais) {
    mostrarLoadingPanel("checkout-form", "Cargando provincias...");
    $.ajax({
        url: "/Empleado/Provincia_SelectResult",
        method: "POST",
        data: { idPais: idPais != null ? idPais : -1, partialView: "_ProvinciaSufragioSelect" },
        success: function (data) {
            $("#div_provinciaSufragio").html(data);
            Init_Select2();
        },
        complete: function (data) {
            $("#checkout-form").waitMe("hide");
        }
    });
}