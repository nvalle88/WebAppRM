$(document).ready(function () {
    Init_Select2();
    Init_DatetimePicker("Fecha", true);
    Init_DatetimePicker("Factura_FechaFactura", true);
    Init_Touchspin();
    eventoProveedor();
    partialViewProveedor();
    Init_FileInput("file");
    inicializarDetallesActivoSeleccion();
    initDataTableFiltrado("tableDetallesOrdenCompra", []);
    eventoGuardar();
    eventoSpinner();
    calcularCostos();
});

function eventoSpinner()
{
    $('.spinner').spinner('changed', function (e, newVal, oldVal) {
        calcularCostos();
    });
}

function callBackInicializarTableListadoSeleccion() {
    initDataTableFiltrado("tableDetallesArticulos", []);
}

function eventoProveedor()
{
    $("#IdProveedor").on("change", function (e) {
        partialViewProveedor();
    });
}

function partialViewProveedor()
{
    mostrarLoadingPanel("checkout-form", "Cargando datos de proveedor...");
    $.ajax({
        url: proveedorSelectResult,
        method: "POST",
        data: { idProveedor: $("#IdProveedor").val() },
        success: function (data) {
            $("#divDatosProveedor").html(data);
        },
        complete: function (data) {
            $("#checkout-form").waitMe("hide");
        }
    });
}

function callBackFunctionEliminarDatoEspecifico(idRecepcionActivoFijoDetalle) {
    deleteRowDetallesActivosFijos("tableDetallesOrdenCompra", idRecepcionActivoFijoDetalle);
    eliminarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
    calcularCostos();
}

function CallbackFunctionCheckBoxArticulo(idRecepcionActivoFijoDetalle, seleccionado) {
    if (seleccionado) {
        adicionarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle, true);
        var hIdRecepcionActivoFijoDetalle = '<input type="hidden" class="hiddenIdRecepcionActivoFijoDetalle" id="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" name="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" value="' + idRecepcionActivoFijoDetalle + '" />';
        var btnEliminarArticulo = "<div id='divEliminarDatosEspecificos_" + idRecepcionActivoFijoDetalle + "' class='btnEliminarDatosEspecificos' style='display:inline;'><a href='javascript: void(0);' id='btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "' onclick=abrirVentanaConfirmacion('btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "') data-funcioncallback=callBackFunctionEliminarDatoEspecifico('" + idRecepcionActivoFijoDetalle + "') data-titulo='Eliminar' data-descripcion='&#191; Desea eliminar el Artículo seleccionado... ?'>Eliminar</a></div>";
        addRowDetallesActivosFijos("tableDetallesOrdenCompra", "tableDetallesArticulos", idRecepcionActivoFijoDetalle, ['NombreArticulo', 'UnidadMedida', 'Cantidad', 'ValorUnitario', 'ValorTotal', hIdRecepcionActivoFijoDetalle + btnEliminarArticulo], true);
        $('#spinner_' + idRecepcionActivoFijoDetalle).spinner('changed', function (e, newVal, oldVal) {
            calcularCostos();
        });
        Init_TouchspinPorId("valorUnitario_" + idRecepcionActivoFijoDetalle);
    }
    else
        callBackFunctionEliminarDatoEspecifico(idRecepcionActivoFijoDetalle);
}

function calcularCostos()
{
    var total = 0;
    for (var i = 0; i < arrRecepcionActivoFijoDetalleSeleccionado.length; i++) {
        var idArticulo = arrRecepcionActivoFijoDetalleSeleccionado[i].idRecepcionActivoFijoDetalle;
        var cantidadArticuloActual = parseInt($("#cantidad_" + idArticulo).val());
        var valorUnitarioArticuloActual = parseFloat($("#valorUnitario_" + idArticulo).val());

        if (cantidadArticuloActual > 0 && valorUnitarioArticuloActual > 0) {
            var valorTotal = cantidadArticuloActual * valorUnitarioArticuloActual;
            $("#spanValorTotal_" + idArticulo).html("$" + valorTotal.toFixed(2));
            total += valorTotal;
        }
    }
    $("#spanTotal").html(total.toFixed(2));
}

function eventoGuardar() {
    $("#btn-guardar").on("click", function (e) {
        var api = $("#tableDetallesOrdenCompra").dataTable().api();
        var form = $("#checkout-form");
        var validar = true;

        if (api.rows().nodes().length == 0) {
            mostrarNotificacion("Error", "Tiene que seleccionar al menos un Artículo.");
            validar = false;
        }
        if (!form.valid())
            validar = false;

        if (validar)
            $("#btn-guardar").prop("type", "submit");
    });
}