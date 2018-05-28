$(document).ready(function () {
    Init_Select2();
    Init_DatetimePicker("FechaTransferencia");
    initDataTableFiltrado("tableDetallesActivoFijoBajas", []);
    inicializarDetallesActivoSeleccion();
    eventoCustodioEntrega();
    eventoGuardar();
    adicionarArrRecepcionActivoFijoDetalle();
    eventoSucursalOrigen();
    eventoSucursalDestino();

    if (idEditar)
        deshabilitarCamposFormulario();
});

function deshabilitarCamposFormulario()
{
    $("#IdSucursalOrigen").prop("disabled", "disabled");
    $("#IdSucursalDestino").prop("disabled", "disabled");
    $("#IdEmpleadoEntrega").prop("disabled", "disabled");
    $("#IdEmpleadoRecibe").prop("disabled", "disabled");
}

function adicionarArrRecepcionActivoFijoDetalle() {
    var arrIds = idsRecepcionActivoFijoDetalle.split(",");
    $.each(arrIds, function (index, value) {
        if (value != "" && value != null)
            adicionarRecepcionActivoFijoDetalleSeleccionado(value, true);
    });
}

function eventoSucursalOrigen()
{
    $("#IdSucursalOrigen").on("change", function (e) {
        partialViewEmpleadosPorSucursal("divEmpleadosSucursalOrigen", "IdSucursalOrigen", "_EmpleadosTransferenciaSucursalOrigen");
    });
}

function eventoSucursalDestino() {
    $("#IdSucursalDestino").on("change", function (e) {
        partialViewEmpleadosPorSucursal("divEmpleadosSucursalDestino", "IdSucursalDestino", "_EmpleadosTransferenciaSucursalDestino");
    });
}

function partialViewEmpleadosPorSucursal(idDivPartialViewEmpleado, nameIdSucursal, namePartialView)
{
    mostrarLoadingPanel("checkout-form", "Cargando empleados...");
    $.ajax({
        url: urlEmpleadoTransferenciaResult,
        method: "POST",
        data: { idSucursal: $("#" + nameIdSucursal).val(), namePartialView: namePartialView },
        success: function (data) {
            $("#" + idDivPartialViewEmpleado).html(data);
            Init_Select2();
        },
        complete: function (data) {
            if (idDivPartialViewEmpleado == "divEmpleadosSucursalOrigen")
                partialViewActivosFijosPorCustodio();
            else
            {
                if ($("#IdSucursalOrigen").val() == $("#IdSucursalDestino").val())
                    copiarSucursales();
                else
                    $("#checkout-form").waitMe("hide");
            }
            eventoCustodioEntrega();
        }
    });
}

function eventoCustodioEntrega() {
    $("#IdEmpleadoEntrega").on("change", function (e) {
        partialViewActivosFijosPorCustodio();
    });
}

function partialViewActivosFijosPorCustodio() {
    mostrarLoadingPanel("checkout-form", "Cargando listado de activos fijos...");
    $.ajax({
        url: urlListadoActivosFijosCustodioResult,
        method: "POST",
        data: { idEmpleado: $("#IdEmpleadoEntrega").val() },
        success: function (data) {
            $("#divActivosFijosATransferir").html(data);
            initDataTableFiltrado("tableDetallesActivoFijoBajas", []);
            arrRecepcionActivoFijoDetalleSeleccionado = [];
        },
        complete: function (data) {
            if ($("#IdSucursalOrigen").val() == $("#IdSucursalDestino").val())
                copiarSucursales();
            else
                $("#checkout-form").waitMe("hide");
        }
    });
}

function callBackFunctionSeleccionBaja(idRecepcionActivoFijoDetalle, seleccionado) {
    if (seleccionado)
        adicionarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle, seleccionado);
    else
        eliminarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
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

        if (validar) {
            var arrIdsRecepcionActivoFijoDetalle = [];
            for (var i = 0; i < arrRecepcionActivoFijoDetalleSeleccionado.length; i++)
                arrIdsRecepcionActivoFijoDetalle.push(arrRecepcionActivoFijoDetalleSeleccionado[i].idRecepcionActivoFijoDetalle);

            var idsRecepcionActivoFijoDetalle = arrIdsRecepcionActivoFijoDetalle.join(',').toString();
            $("#idsRecepcionActivoFijoDetalle").val(idsRecepcionActivoFijoDetalle);
            $("#btn-guardar").prop("type", "submit");
        }
    });
}

function copiarSucursales() {
    $("#IdSucursalDestino").html("");
    $('#IdSucursalDestino').select2('data', null);
    var idSucursalOrigenSeleccionado = $("#IdSucursalOrigen").val();
    var listaSucursalesOrigen = document.getElementById("IdSucursalOrigen").children;
    for (var i = 0; i < listaSucursalesOrigen.length; i++) {
        var idSucursalActual = listaSucursalesOrigen[i].value;
        if (idSucursalActual != idSucursalOrigenSeleccionado)
        {
            var newOption = new Option(listaSucursalesOrigen[i].text, idSucursalActual, false, false);
            $('#IdSucursalDestino').append(newOption).trigger('change');
        }
    }
    partialViewEmpleadosPorSucursal("divEmpleadosSucursalDestino", "IdSucursalDestino", "_EmpleadosTransferenciaSucursalDestino");
}