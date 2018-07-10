$(document).ready(function () {
    Init_Select2();
    eventoMotivoSalidaArticulos();
});

function eventoMotivoSalidaArticulos() {
    $("#IdMotivoSalidaArticulos").on("change", function (e) {
        partialViewProveedorEmpleadoDevolucion();
    });
}

function partialViewBajaDevolucionDespacho() {
    var motivoSalidaArticulos = $("#IdMotivoSalidaArticulos option:selected").text();
    if (motivoSalidaArticulos == "Baja de inventario") {
        mostrarLoadingPanel("checkout-form", "Cargando datos de empleados...");
        //$.ajax({
        //    url: proveedorSelectResult,
        //    method: "POST",
        //    data: { idOrdenCompra: $("#IdOrdenCompra").val() },
        //    success: function (data) {
        //        mostrarOcultarFieldsetEmpleadoDevolucion(false);
        //        $("#divDetallesProveedor").html(data);
        //        mostrarOcultarFieldsetProveedor(true);
        //    },
        //    complete: function (data) {
        //        $("#checkout-form").waitMe("hide");
        //    }
        //});
    }
    else if (motivoSalidaArticulos == "Despacho") {
        mostrarLoadingPanel("checkout-form", "Cargando datos de empleados...");
        //$.ajax({
        //    url: urlEmpleadoDevolucionSelectResult,
        //    method: "POST",
        //    data: { idSucursal: $("#Bodega_IdSucursal").val() },
        //    success: function (data) {
        //        mostrarOcultarFieldsetProveedor(false);
        //        $("#divDetallesEmpleadoDevolucion").html(data);
        //        mostrarOcultarFieldsetEmpleadoDevolucion(true);
        //        Init_Select2();
        //    },
        //    complete: function (data) {
        //        $("#checkout-form").waitMe("hide");
        //    }
        //});
    }
    else {//Devolución a proveedor
            
    }
}