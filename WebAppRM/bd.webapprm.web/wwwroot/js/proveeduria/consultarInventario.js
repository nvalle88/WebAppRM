$(document).ready(function () {
    Init_Select2();
    Init_DatetimePicker("Fecha", true, true);
    eventoSucursal();
    obtenerIdSucursalObjAdicional();
});

function obtenerIdSucursalObjAdicional() {
    if (isAdminNacionalProveeduria) {
        objAdicional = $("#Bodega_IdSucursal").val();
    }
}

function eventoSucursal() {
    $("#Bodega_IdSucursal").on("change", function (e) {
        obtenerIdSucursalObjAdicional();
        partialViewBodega();
    });
}

function eventoBodega() {
    $("#IdBodega").on("change", function (e) {
        partialViewArticulosInventario();
    });
}

function partialViewBodega() {
    mostrarLoadingPanel("checkout-form", "Cargando bodegas...");
    $.ajax({
        url: bodegaSelectResult,
        method: "POST",
        data: { idSucursal: $("#Bodega_IdSucursal").val() },
        success: function (data) {
            $("#divBodega").html(data);
            Init_Select2();
            eventoBodega();
        },
        complete: function (data) {
            partialViewArticulosInventario();
        }
    });
}

function partialViewArticulosInventario() {
    mostrarLoadingPanel("checkout-form", "Cargando datos de inventario...");
    $.ajax({
        url: articulosBodegaFechaSelectResult,
        method: "POST",
        data: { idBodega: $("#IdBodega").val(), fecha: $("#Fecha").val() },
        success: function (data) {
            $("#divDetallesInventarioArticulos").html(data);
        },
        complete: function (data) {
            $("#checkout-form").waitMe("hide");
        }
    });
}