$(document).ready(function () {
    Init_Select2();
    inicializarDetallesActivoSeleccion();
    initDataTableFiltrado("tableDetallesRequerimientos", []);
    eventoMotivoSalidaArticulos();
    partialViewBajaDevolucionDespacho();
    eventoGuardar();
});

function eventoMotivoSalidaArticulos() {
    $("#IdMotivoSalidaArticulos").on("change", function (e) {
        partialViewBajaDevolucionDespacho();
    });
}

function callBackFunctionEliminarDatoEspecifico(idRecepcionActivoFijoDetalle) {
    deleteRowDetallesActivosFijos("tableDetallesRequerimientos", idRecepcionActivoFijoDetalle);
    eliminarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
}

function partialViewBajaDevolucionDespacho() {
    var motivoSalidaArticulos = $("#IdMotivoSalidaArticulos option:selected").text();
    $("#MotSalida").val(motivoSalidaArticulos);

    if (motivoSalidaArticulos.toLowerCase() == "baja de inventarios" || motivoSalidaArticulos.toLowerCase() == "despacho") {
        mostrarLoadingPanel("checkout-form", "Cargando empleados...");
        $.ajax({
            url: urlEmpleadoDevolucionSelectResult,
            method: "POST",
            data: { idSucursal: $("#IdSucursal").val() },
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
    else {
        mostrarLoadingPanel("checkout-form", "Cargando proveedores...");
        $.ajax({
            url: proveedorSelectResult,
            method: "POST",
            success: function (data) {
                mostrarOcultarFieldsetEmpleadoDevolucion(false);
                $("#divDetallesProveedor").html(data);
                Init_Select2();
                mostrarOcultarFieldsetProveedor(true);
            },
            complete: function (data) {
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

function eventoGuardar() {
    $("#btn-guardar").on("click", function (e) {
        var api = $("#tableDetallesRequerimientos").dataTable().api();
        var form = $("#checkout-form");
        var validar = true;

        var motivoSalidaArticulos = $("#IdMotivoSalidaArticulos option:selected").text();
        var valorEmpleadoProveedor = null;
        var idEmpleado = true;

        if (motivoSalidaArticulos.toLowerCase() == "baja de inventarios" || motivoSalidaArticulos.toLowerCase() == "despacho") {
            valorEmpleadoProveedor = $("#IdEmpleadoDevolucion").val();
        }
        else {
            valorEmpleadoProveedor = $("#IdProveedorDevolucion").val();
            idEmpleado = false;
        }

        if (valorEmpleadoProveedor == null || valorEmpleadoProveedor == "" || !valorEmpleadoProveedor) {
            mostrarNotificacion("Error", "Tiene que seleccionar un " + (idEmpleado ? "Empleado" : "Proveedor") + ".");
            validar = false;
        }
        if (api.rows().nodes().length == 0) {
            mostrarNotificacion("Error", "Tiene que seleccionar al menos un Artículo.");
            validar = false;
        }

        if (!form.valid())
            validar = false;

        if (validar)
        {
            $.ajax({
                url: bodegaExisteResult,
                method: "POST",
                data: { idDependencia: $("#FuncionarioSolicitante_IdDependencia").val() },
                success: function (data) {
                    $("#checkout-form").submit();
                },
                error: function (errorMessage)
                {
                    mostrarNotificacion("Error", "Ocurrió un error al tratar de identificar la Bodega para la dependencia " + dependenciaRequerimiento + ".");
                }
            });
        }
    });
}