$(document).ready(function () {
    Init_Select2();
    Init_DatetimePicker("Fecha", true, true);
    Init_DatetimePicker("Factura_FechaFactura", true, true);
    Init_Touchspin();
    eventoProveedor();
    partialViewProveedor();
    Init_FileInput("file");
    inicializarDetallesActivoSeleccion();
    initDataTableFiltrado("tableDetallesOrdenCompra", []);
    eventoMotivoRecepcionArticulos();

    if (!isEditar)
        partialViewProveedorEmpleadoDevolucion();

    eventoSucursal();
    eventoGuardar();
    eventoSpinner();
    calcularCostos();
    obtenerIdSucursalObjAdicional();
    eventoCloseModalArticulos();
});

function eventoCloseModalArticulos()
{
    $("#myModal").on("hidden.bs.modal", function () {
        $("#modalBodyDatosEspecificos").html("");
    });
}

function obtenerIdSucursalObjAdicional()
{
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

function partialViewProveedorEmpleadoDevolucion() {
    var motivoRecepcionArticulos = $("#IdMotivoRecepcionArticulos option:selected").text();
    if (motivoRecepcionArticulos.toLowerCase() == "compra") {
        mostrarLoadingPanel("checkout-form", "Cargando datos de proveedor...");
        $.ajax({
            url: proveedorCompraResult,
            method: "POST",
            data: { idOrdenCompra: $("#IdOrdenCompra").val() },
            success: function (data) {
                mostrarOcultarFieldsetEmpleadoDevolucion(false);
                $("#divDetallesProveedor").html(data);
                mostrarOcultarFieldsetProveedor(true);
                partialViewProveedor();
                eventoProveedor();
            },
            complete: function (data) {
                Init_Select2();
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
            },
            complete: function (data) {
                Init_Select2();
                $("#checkout-form").waitMe("hide");
            }
        });
    }
}

function mostrarOcultarFieldsetProveedor(mostrar) {
    if (mostrar) {
        $("#legendDetallesProveedor").removeClass("hide");
        $("#divDetallesProveedor").removeClass("hide");
    }
    else {
        $("#legendDetallesProveedor").addClass("hide");
        $("#divDetallesProveedor").addClass("hide");
        $("#divDetallesProveedor").html("");
        $("#divDatosProveedor").html("");
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
        $("#divDetallesEmpleadoDevolucion").html("");
    }
}

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
    var motivoRecepcionArticulos = $("#IdMotivoRecepcionArticulos option:selected").text();
    if (motivoRecepcionArticulos.toLowerCase() == "compra")
    {
        mostrarLoadingPanel("checkout-form", "Cargando datos de proveedor...");
        $.ajax({
            url: proveedorSelectResult,
            method: "POST",
            data: { idProveedor: $("#IdProveedor").val(), namePartialView: "_ProveedorOrdenCompra" },
            success: function (data) {
                $("#divDatosProveedor").html(data);
            },
            complete: function (data) {
                $("#checkout-form").waitMe("hide");
            }
        });
    }
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
        addRowDetallesActivosFijos("tableDetallesOrdenCompra", "tableDetallesArticulos", idRecepcionActivoFijoDetalle, [thClassName.nombreArticulo, thClassName.unidadMedida, thClassName.cantidad, thClassName.valorUnitario, thClassName.valorTotal, hIdRecepcionActivoFijoDetalle + btnEliminarArticulo], true);
        $('#spinner_' + idRecepcionActivoFijoDetalle).spinner('changed', function (e, newVal, oldVal) {
            calcularCostos();
        });
        Init_TouchspinPorId("valorUnitario_" + idRecepcionActivoFijoDetalle);
        $("#valorUnitario_" + idRecepcionActivoFijoDetalle).val("0.00");
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

        var motivoRecepcionArticulos = $("#IdMotivoRecepcionArticulos option:selected").text();
        var valorEmpleadoProveedor = $(motivoRecepcionArticulos.toLowerCase() == "compra" ? "#IdProveedor" : "#IdEmpleadoDevolucion").val();
        var isEmpleado = motivoRecepcionArticulos.toLowerCase() == "compra" ? false : true;
        $("#modalBodyDatosEspecificos").html("");

        if (valorEmpleadoProveedor == null || valorEmpleadoProveedor == "" || !valorEmpleadoProveedor) {
            mostrarNotificacion("Error", "Tiene que seleccionar un " + (isEmpleado ? "Empleado" : "Proveedor") + ".");
            validar = false;
        }
        if (api.rows().nodes().length == 0) {
            mostrarNotificacion("Error", "Tiene que seleccionar al menos un Artículo.");
            validar = false;
        }
        else {
            var touchspins = $(".touchspin_tasa");
            var alertCeros = false;
            $.each($(".touchspin_tasa"), function (index, value) {
                var valorTouchspin = $(value).val().toString();
                if (valorTouchspin == "")
                    $(value).val("0.00");

                if (valorTouchspin == "0.00")
                    alertCeros = true;
            });

            if (alertCeros)
            {
                mostrarNotificacion("Error", "Los valores unitarios de los artículos tienen que ser mayor que cero.");
                validar = false;
            }
        }
        if (isAdminZonalProveeduria) {
            var idBodega = $("#IdBodega").val();
            if (idBodega == null || idBodega == "") {
                mostrarNotificacion("Error", "Tiene que asignar una Bodega a la dependencia " + $("#IdDependencia").val() + ".");
                validar = false;
            }
        }

        if (!form.valid())
            validar = false;

        if (validar)
            $("#btn-guardar").prop("type", "submit");
    });
}