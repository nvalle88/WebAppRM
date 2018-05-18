$(document).ready(function () {
    Init_Select2();
    Init_DatetimePicker("FacturaActivoFijo_FechaFactura");
    eventoMotivoAlta();
    initDataTableFiltrado("tableDetallesActivoFijoSeleccionados", []);
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

function cargarListadoActivosFijosParaSeleccion()
{
    mostrarLoadingPanel("modalContentDatosEspecificos", "Cargando listado de activos fijos, por favor espere...");
    $.ajax({
        url: urlListadoActivosFijosResult,
        method: "POST",
        data: { /*componentesActivo: obtenerRecepcionActivoFijoDetalleComponente()*/ },
        success: function (data) {
            $("#modalBodyDatosEspecificos").html(data);
            initDataTableFiltrado("tableDetallesActivoFijoAltas", []);
        },
        error: function (errorMessage) {
            mostrarNotificacion("Error", "Ocurrió un error al cargar el formulario.");
        },
        complete: function (data) {
            $("#modalContentDatosEspecificos").waitMe("hide");
        }
    });
}