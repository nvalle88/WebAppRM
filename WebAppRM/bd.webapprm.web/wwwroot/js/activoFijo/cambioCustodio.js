$(document).ready(function () {
    Init_Select2();
    eventoCustodioEntrega();
    eventoGuardar();
    inicializarDetallesActivoSeleccion();
    initDataTableFiltrado("tableDetallesActivoFijoBajas", [14, 16, 17, 18, 19, 20, 21, 22, 23]);
    copiarEmpleados();
    adicionarArrRecepcionActivoFijoDetalle();
});

function adicionarArrRecepcionActivoFijoDetalle()
{
    var arrIds = idsRecepcionActivoFijoDetalle.split(",");
    $.each(arrIds, function (index, value) {
        if (value != "" && value != null)
            adicionarRecepcionActivoFijoDetalleSeleccionado(value, true);
    });
}

function eventoCustodioEntrega()
{
    $("#IdEmpleadoEntrega").on("change", function (e) {
        partialViewActivosFijosPorCustodio();
    });
}

function partialViewActivosFijosPorCustodio()
{
    mostrarLoadingPanel("checkout-form", "Cargando listado de activos fijos...");
    $.ajax({
        url: urlListadoActivosFijosCustodioResult,
        method: "POST",
        data: { idEmpleado: $("#IdEmpleadoEntrega").val() },
        success: function (data) {
            $("#divActivosFijosATransferir").html(data);
            initDataTableFiltrado("tableDetallesActivoFijoBajas", [14, 16, 17, 18, 19, 20, 21, 22, 23]);
            arrRecepcionActivoFijoDetalleSeleccionado = [];
        },
        complete: function (data) {
            copiarEmpleados();
            $("#checkout-form").waitMe("hide");
        }
    });
}

function copiarEmpleados()
{
    $("#IdEmpleadoRecibe").html("");
    $('#IdEmpleadoRecibe').select2('data', null);
    var idEmpleadoEntregaSeleccionado = $("#IdEmpleadoEntrega").val();
    var listaEmpleadosEntregan = document.getElementById("IdEmpleadoEntrega").children;
    for (var i = 0; i < listaEmpleadosEntregan.length; i++)
    {
        var idEmpleadoActual = listaEmpleadosEntregan[i].value;
        if (idEmpleadoActual != idEmpleadoEntregaSeleccionado)
        {
            var newOption = new Option(listaEmpleadosEntregan[i].text, idEmpleadoActual, false, false);
            $('#IdEmpleadoRecibe').append(newOption).trigger('change');
        }
    }
}

function eventoGuardar() {
    $("#btn-guardar").on("click", function (e) {
        var form = $("#checkout-form");
        var validar = true;

        if (arrRecepcionActivoFijoDetalleSeleccionado.length == 0) {
            mostrarNotificacion("Error", "Tiene que seleccionar al menos un Activo Fijo.");
            validar = false;
        }
        if (!form.valid())
            validar = false;

        if (validar)
        {
            var arrIdsRecepcionActivoFijoDetalle = [];
            for (var i = 0; i < arrRecepcionActivoFijoDetalleSeleccionado.length; i++)
                arrIdsRecepcionActivoFijoDetalle.push(arrRecepcionActivoFijoDetalleSeleccionado[i].idRecepcionActivoFijoDetalle);

            var idsRecepcionActivoFijoDetalle = arrIdsRecepcionActivoFijoDetalle.join(',').toString();
            $("#idsRecepcionActivoFijoDetalle").val(idsRecepcionActivoFijoDetalle);
            $("#btn-guardar").prop("type", "submit");
        }
    });
}

function callBackFunctionSeleccionBaja(idRecepcionActivoFijoDetalle, seleccionado) {
    if (seleccionado)
        adicionarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle, seleccionado);
    else
        eliminarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
}