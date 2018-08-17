$(document).ready(function () {
    Init_Select2();
    Init_DatetimePicker("Fecha", true, true);
    inicializarDetallesActivoSeleccion();
    initDataTableFiltrado("tableDetallesAjustesInventario", [], null, { mostrarTodos: true }, true);
    eventoSucursal();
    eventoBodega();
    eventoGuardar();
    obtenerIdSucursalObjAdicional();
    eventoCloseModalArticulos();
});

function eventoCloseModalArticulos() {
    $("#myModal").on("hidden.bs.modal", function () {
        $("#modalBodyDatosEspecificos").html("");
    });
}

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
            partialViewEmpleadoAutoriza();
        }
    });
}

function partialViewEmpleadoAutoriza()
{
    mostrarLoadingPanel("checkout-form", "Cargando datos de empleados...");
    $.ajax({
        url: urlEmpleadoAutorizaSelectResult,
        method: "POST",
        data: { idSucursal: $("#Bodega_IdSucursal").val() },
        success: function (data) {
            $("#divDetallesEmpleadoDevolucion").html(data);
        },
        complete: function (data) {
            Init_Select2();
            partialViewArticulosInventario();
        }
    });
}

function callBackInicializarTableListadoSeleccion() {
    initDataTableFiltrado("tableDetallesArticulos", [], null, { mostrarTodos: true, ocultarTodos: true }, true);
}

function callBackFunctionEliminarDatoEspecifico(idRecepcionActivoFijoDetalle) {
    deleteRowDetallesActivosFijos("tableDetallesAjustesInventario", idRecepcionActivoFijoDetalle);
    eliminarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
}

function CallbackFunctionCheckBoxArticulo(idRecepcionActivoFijoDetalle, seleccionado) {
    if (seleccionado) {
        adicionarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle, true);
        var hIdRecepcionActivoFijoDetalle = '<input type="hidden" class="hiddenIdRecepcionActivoFijoDetalle" id="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" name="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" value="' + idRecepcionActivoFijoDetalle + '" />';
        var btnEliminarArticulo = "<div id='divEliminarDatosEspecificos_" + idRecepcionActivoFijoDetalle + "' class='btnEliminarDatosEspecificos' style='display:inline;'><a href='javascript: void(0);' id='btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "' onclick=abrirVentanaConfirmacion('btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "') data-funcioncallback=callBackFunctionEliminarDatoEspecifico('" + idRecepcionActivoFijoDetalle + "') data-titulo='Eliminar' data-descripcion='&#191; Desea eliminar el Artículo seleccionado... ?'>Eliminar</a></div>";
        addRowDetallesActivosFijos("tableDetallesAjustesInventario", "tableDetallesArticulos", idRecepcionActivoFijoDetalle, [thClassName.nombreArticulo, thClassName.cantidad, hIdRecepcionActivoFijoDetalle + btnEliminarArticulo], true);
        $('#spinner_' + idRecepcionActivoFijoDetalle).spinner();
    }
    else
        callBackFunctionEliminarDatoEspecifico(idRecepcionActivoFijoDetalle);
}

function eventoBodega()
{
    $("#IdBodega").on("change", function (e) {
        partialViewArticulosInventario();
    });
}

function partialViewArticulosInventario()
{
    mostrarLoadingPanel("checkout-form", "Cargando datos de inventario...");
    arrRecepcionActivoFijoDetalleSeleccionado = [];
    $.ajax({
        url: urlArticulosInventarioSelectResult,
        method: "POST",
        data: { idBodega: $("#IdBodega").val() },
        success: function (data) {
            $("#divArticulosInventario").html(data);
            inicializarDetallesActivoSeleccion();
            initDataTableFiltrado("tableDetallesAjustesInventario", []);

            $.each(arrRecepcionActivoFijoDetalleSeleccionado, function (index, value) {
                $('#spinner_' + value.idRecepcionActivoFijoDetalle).spinner();
            });
        },
        complete: function (data) {
            $("#checkout-form").waitMe("hide");
        }
    });
}

function eventoGuardar() {
    $("#btn-guardar").on("click", function (e) {
        var form = $("#checkout-form");
        var validar = true;

        var idBodega = $("#IdBodega").val();
        if (idBodega == null || idBodega == "") {
            mostrarNotificacion("Error", "Tiene que establecer una Bodega.");
            validar = false;
        }
        if (!form.valid())
            validar = false;

        if (validar)
            $("#checkout-form").submit();
    });
}