jQuery(document).ready(function (e) {
    Init_DatetimePicker("FechaCorteInventario", true);
    Init_DatetimePicker("FechaInforme", true);
    Init_Select2();
    adicionarArrRecepcionActivoFijoDetalle();
    initDataTableFiltrado("tableDetallesActivoFijoBajas", [thClassName.bodega, thClassName.proveedor, thClassName.motivoAlta, thClassName.fechaRecepcion, thClassName.ordenCompra, thClassName.fondoFinanciamiento, thClassName.fechaAlta, thClassName.numeroFactura], function () {
        crearGrupo("tableDetallesActivoFijoBajas", [
            { propiedad: thClassName.fondoFinanciamiento, valor: "Fondo de financiamiento" },
            { propiedad: thClassName.claseActivoFijo, valor: "Clase de activo fijo" },
            { propiedad: thClassName.subClaseActivoFijo, valor: "Subclase de activo fijo" }
        ]);
    }, { mostrarTodos: true });
    eventoGuardar();
    inicializarIdsArrRecepcionActivoFijoDetalleTodos();
});

function adicionarArrRecepcionActivoFijoDetalle() {
    var arrIds = idsRecepcionActivoFijoDetalle.split(",");
    $.each(arrIds, function (index, value) {
        if (value != "" && value != null) {
            var arrValores = value.split("_");
            adicionarRecepcionActivoFijoDetalleSeleccionado(arrValores[0], arrValores[1].toLowerCase() === "true");
        }
    });
}

function callBackFunctionSeleccionBaja(idRecepcionActivoFijoDetalle, seleccionado) {
    var rafd = obtenerRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
    if (seleccionado)
        rafd.seleccionado = true;
    else
        rafd.seleccionado = false;
}

function eventoGuardar() {
    $("#btn-guardar").on("click", function (e) {
        var form = $("#checkout-form");
        var validar = true;

        if (arrRecepcionActivoFijoDetalleSeleccionado.length == 0) {
            mostrarNotificacion("Error", "Tiene que existir al menos un Activo Fijo en el Inventario.");
            validar = false;
        }
        if (!form.valid())
            validar = false;

        if (validar) {
            var arrIdsRecepcionActivoFijoDetalle = [];
            for (var i = 0; i < arrRecepcionActivoFijoDetalleSeleccionado.length; i++)
                arrIdsRecepcionActivoFijoDetalle.push(arrRecepcionActivoFijoDetalleSeleccionado[i].idRecepcionActivoFijoDetalle + "_" + arrRecepcionActivoFijoDetalleSeleccionado[i].seleccionado);

            var idsRecepcionActivoFijoDetalle = arrIdsRecepcionActivoFijoDetalle.join(',').toString();
            $("#idsRecepcionActivoFijoDetalleSeleccionado").val(idsRecepcionActivoFijoDetalle);
            $("#btn-guardar").prop("type", "submit");
        }
    });
}