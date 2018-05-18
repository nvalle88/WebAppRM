$(document).ready(function () {
    Init_Select2();
    Init_DatetimePicker("FacturaActivoFijo_FechaFactura");
    eventoMotivoAlta();
});

function eventoMotivoAlta() {
    $("#IdMotivoAlta").on("change", function (e) {
        var motivoAlta = $("#IdMotivoAlta option:selected").text();
        if (motivoAlta == "Compra")
        {
            mostrarLoadingPanel("checkout-form", "Cargando detalles de factura...");
            $.ajax({
                url: urlDetalleFacturaAltaActivos,
                method: "POST",
                success: function (data) {
                    $("#divOpcionCompra").html(data);
                    Init_DatetimePicker("FacturaActivoFijo_FechaFactura");
                },
                complete: function (data) {
                    $("#checkout-form").waitMe("hide");
                }
            });
        }
        else
            $("#divOpcionCompra").html("");
    });
}