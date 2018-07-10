$(document).ready(function () {
    Init_Select2();
    Init_DatetimePicker("FechaRecepcion", true);
    eventoMotivoRecepcionArticulos();
    partialViewProveedorEmpleadoDevolucion();
    eventoSucursal();
});

function eventoSucursal() {
    $("#Bodega_IdSucursal").on("change", function (e) {
        partialViewBodega();
    });
}

function partialViewBodega()
{
    mostrarLoadingPanel("checkout-form", "Cargando bodegas...");
    $.ajax({
        url: bodegaSelectResult,
        method: "POST",
        data: { idSucursal: $("#Bodega_IdSucursal").val() },
        success: function (data) {
            $("#divBodega").html(data);
            Init_Select2();
        },
        complete: function (data) {
            partialViewProveedorEmpleadoDevolucion();
        }
    });
}

function eventoMotivoRecepcionArticulos() {
    $("#IdMotivoRecepcionArticulos").on("change", function (e) {
        partialViewProveedorEmpleadoDevolucion();
    });
}

function partialViewProveedorEmpleadoDevolucion()
{
    var motivoRecepcionArticulos = $("#IdMotivoRecepcionArticulos option:selected").text();
    if (motivoRecepcionArticulos == "Compra") {
        mostrarLoadingPanel("checkout-form", "Cargando datos de proveedor...");
        $.ajax({
            url: proveedorSelectResult,
            method: "POST",
            data: { idOrdenCompra: $("#IdOrdenCompra").val() },
            success: function (data) {
                mostrarOcultarFieldsetEmpleadoDevolucion(false);
                $("#divDetallesProveedor").html(data);
                mostrarOcultarFieldsetProveedor(true);
            },
            complete: function (data) {
                $("#checkout-form").waitMe("hide");
            }
        });
    }
    else {
        mostrarLoadingPanel("checkout-form", "Cargando datos de empleados...");
        $.ajax({
            url: urlEmpleadoDevolucionSelectResult,
            method: "POST",
            data: { idSucursal: $("#Bodega_IdSucursal").val() },
            success: function (data) {
                mostrarOcultarFieldsetProveedor(false);
                $("#divDetallesEmpleadoDevolucion").html(data);
                mostrarOcultarFieldsetEmpleadoDevolucion(true);
                Init_Select2();
            },
            complete: function (data) {
                $("#checkout-form").waitMe("hide");
            }
        });
    }
}

function mostrarOcultarFieldsetProveedor(mostrar)
{
    if (mostrar) {
        $("#legendDetallesProveedor").removeClass("hide");
        $("#divDetallesProveedor").removeClass("hide");
    }
    else {
        $("#legendDetallesProveedor").addClass("hide");
        $("#divDetallesProveedor").addClass("hide");
    }
}

function mostrarOcultarFieldsetEmpleadoDevolucion(mostrar) {
    if (mostrar) {
        $("#legendDetallesEmpleadoDevolucion").removeClass("hide");
        $("#divDetallesEmpleadoDevolucion").removeClass("hide");
    }
    else {
        $("#legendDetallesEmpleadoDevolucion").addClass("hide");
        $("#divDetallesEmpleadoDevolucion").addClass("hide");
    }
}