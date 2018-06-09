$(document).ready(function () {
    Init_Select2();
    Init_DatetimePicker("FacturaActivoFijo_FechaFactura", true);
    Init_DatetimePicker("FechaAlta", true);
    eventoMotivoAlta();
    inicializarDetallesActivoSeleccion();
    inicializarObjetoAdicional();
    initDataTableFiltrado("tableDetallesActivoFijoSeleccionados", [13, 15, 16, 17, 18, 19]);
    Init_FileInput("file");
    Init_FileInput("fileFactura");
    eventoGuardarDatosEmpleado();
    eventoGuardar();
    partialViewFacturaActivoFijoIsCompra();
});

function eventoMotivoAlta() {
    $("#IdMotivoAlta").on("change", function (e) {
        partialViewFacturaActivoFijoIsCompra();
    });
}

function partialViewFacturaActivoFijoIsCompra()
{
    var motivoAlta = $("#IdMotivoAlta option:selected").text();
    if (motivoAlta == "Compra") {
        mostrarLoadingPanel("checkout-form", "Cargando detalles de factura...");
        $.ajax({
            url: urlDetalleFacturaAltaActivos,
            method: "POST",
            data: { idFacturaActivoFijo: $("#IdFacturaActivoFijo").val() },
            success: function (data) {
                $("#divOpcionCompra").html(data);
                Init_DatetimePicker("FacturaActivoFijo_FechaFactura", true);
                Init_FileInput("fileFactura");
            },
            complete: function (data) {
                $("#checkout-form").waitMe("hide");
            }
        });
    }
    else
        $("#divOpcionCompra").html("");
}

function callBackFunctionEliminarDatoEspecifico(idRecepcionActivoFijoDetalle)
{
    deleteRowDetallesActivosFijos("tableDetallesActivoFijoSeleccionados", idRecepcionActivoFijoDetalle);
    eliminarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
    eliminarComponente(idRecepcionActivoFijoDetalle);
}

function callBackInicializarTableListadoSeleccion()
{
    initDataTableFiltrado("tableDetallesActivoFijoAltas", [14, 16, 17, 18, 19, 20]);
}

function callBackFunctionSeleccionAlta(idRecepcionActivoFijoDetalle, seleccionado) {
    var componentes = $("#hComunesComponentes_" + idRecepcionActivoFijoDetalle).val();
    if (seleccionado) {
        adicionarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle, true);
        addComponenteToArray(idRecepcionActivoFijoDetalle, idRecepcionActivoFijoDetalle, componentes.split(","));
        var idSucursal = $("#tableDetallesActivoFijoAltas" + idRecepcionActivoFijoDetalle + "Sucursal").data("idsucursal");
        var idLibroActivoFijo = $("#tableDetallesActivoFijoAltas" + idRecepcionActivoFijoDetalle + "Sucursal").data("idlibroactivofijo");
        var idEmpleado = $("#tableDetallesActivoFijoAltas" + idRecepcionActivoFijoDetalle + "Empleado").data("idempleado");
        
        var hComponentes = '<input type="hidden" id="hComponentes_' + idRecepcionActivoFijoDetalle + '" name="hComponentes_' + idRecepcionActivoFijoDetalle + '" value="' + componentes + '" />';
        var hIdRecepcionActivoFijoDetalle = '<input type="hidden" class="hiddenIdRecepcionActivoFijoDetalle" id="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" name="hIdRecepcionActivoFijoDetalle_' + idRecepcionActivoFijoDetalle + '" value="' + idRecepcionActivoFijoDetalle + '" />';
        var hEmpleado = '<input type="hidden" class="hiddenHEmpleado" id="hEmpleado_' + idRecepcionActivoFijoDetalle + '" name="hEmpleado_' + idRecepcionActivoFijoDetalle + '" value="' + idEmpleado + '" />';
        var hSucursal = '<input type="hidden" id="hSucursal_' + idRecepcionActivoFijoDetalle + '" name="hSucursal_' + idRecepcionActivoFijoDetalle + '" value="' + idSucursal + '" />';
        var hLibroActivoFijo = '<input type="hidden" id="hLibroActivoFijo_' + idRecepcionActivoFijoDetalle + '" name="hLibroActivoFijo_' + idRecepcionActivoFijoDetalle + '" value="' + idLibroActivoFijo + '" />';
        var btnEmpleado = '<a href="javascript:void(0);" onclick="cargarFormularioDatosEmpleado(' + idRecepcionActivoFijoDetalle + ')" class="btnDatosEmpleado" data-idfila="' + idRecepcionActivoFijoDetalle + '" data-toggle="modal" data-target="#myModalEmpleado">Custodio</a>';
        var btnComponentes = '<span> | </span><a href="javascript:void(0);" onclick="cargarFormularioComponentesDatosEspecificos(' + idRecepcionActivoFijoDetalle + ')" class="btnComponentesDatosEspecificos" data-idfila="' + idRecepcionActivoFijoDetalle + '" data-idorigen="' + idRecepcionActivoFijoDetalle + '" data-toggle="modal" data-target="#myModalComponente">Componentes</a>';
        var btnEliminarAlta = "<div id='divEliminarDatosEspecificos_" + idRecepcionActivoFijoDetalle + "' class='btnEliminarDatosEspecificos' style='display:inline;'><span> | </span><a href='javascript: void(0);' id='btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "' onclick=abrirVentanaConfirmacion('btnEliminarDatosEspecifico_" + idRecepcionActivoFijoDetalle + "') data-funcioncallback=callBackFunctionEliminarDatoEspecifico('" + idRecepcionActivoFijoDetalle + "') data-titulo='Eliminar' data-descripcion='&#191; Desea eliminar el Activo Fijo seleccionado... ?'>Eliminar</a></div>";
        addRowDetallesActivosFijos("tableDetallesActivoFijoSeleccionados", "tableDetallesActivoFijoAltas", idRecepcionActivoFijoDetalle, ['Codigosecuencial', 'TipoActivoFijo', 'ClaseActivoFijo', 'SubclaseActivoFijo', 'NombreActivoFijo', 'Marca', 'Modelo', 'Serie', 'NumeroChasis', 'NumeroMotor', 'Placa', 'NumeroClaveCatastral', 'Sucursal', 'Bodega', 'Empleado', 'Proveedor', 'MotivoRecepcion', 'FechaRecepcion', 'OrdenCompra', 'FondoFinanciamiento', hComponentes + hIdRecepcionActivoFijoDetalle + hEmpleado + hSucursal + hLibroActivoFijo + btnEmpleado + btnComponentes + btnEliminarAlta], true);
    }
    else
        callBackFunctionEliminarDatoEspecifico(idRecepcionActivoFijoDetalle);
}

function cargarFormularioDatosEmpleado(idRecepcionActivoFijoDetalle)
{
    var idSucursal = $("#hSucursal_" + idRecepcionActivoFijoDetalle).val();
    var idEmpleado = $("#hEmpleado_" + idRecepcionActivoFijoDetalle).val();
    mostrarLoadingPanel("modalContentEmpleado", "Cargando listados de custodios por sucursal...");
    $.ajax({
        url: urlModalEmpleadosResult,
        method: "POST",
        data: { idSucursal: idSucursal, idEmpleado: idEmpleado, idRecepcionActivoFijoDetalle: idRecepcionActivoFijoDetalle },
        success: function (data) {
            $("#inputEmpleado").html(data);
            Init_Select2();
        },
        complete: function (data) {
            $("#modalContentEmpleado").waitMe("hide");
            $("#chkTodosEmpleadosDatosEspecificos").prop("checked", "");
        }
    });
}

function eventoGuardarDatosEmpleado()
{
    $("#btnGuardarDatosEmpleado").on("click", function (e) {
        mostrarLoadingPanel("modalContentEmpleado", "Validando datos, por favor espere...");
        var idEmpleado = $("#IdEmpleado").val();
        if (idEmpleado != "" && idEmpleado != null) {
            var idRecepcionActivoFijoDetalle = $("#hIdRecepcionActivoFijoDetalleEmpleado").val();
            var idSucursal = $("#hSucursal_" + idRecepcionActivoFijoDetalle).val();
            var isTodosEmpleado = $("#chkTodosEmpleadosDatosEspecificos").prop("checked");
            if (isTodosEmpleado)
            {
                var arrHEmpleado = $(".hiddenIdRecepcionActivoFijoDetalle");
                $.each(arrHEmpleado, function (index, value) {
                    var idrafd = $(value).val();
                    var idSuc = $("#hSucursal_" + idrafd).val();
                    if (idSucursal == idSuc)
                        putDatoEmpleadoTable(idrafd, idEmpleado);
                });
            }
            else
                putDatoEmpleadoTable(idRecepcionActivoFijoDetalle, idEmpleado);
            $("#btnCancelarDatosEmpleado").click();
        }
        else
            $("#valIdEmpleado").html("Tiene que seleccionar un Custodio.");
        $("#modalContentEmpleado").waitMe("hide");
    });
}

function putDatoEmpleadoTable(idRecepcionActivoFijoDetalle, idEmpleado) {
    $("#hEmpleado_" + idRecepcionActivoFijoDetalle).val(idEmpleado);
    $("#tableDetallesActivoFijoSeleccionados" + idRecepcionActivoFijoDetalle + "Empleado").html($("#IdEmpleado option:selected").text());
}

function validarDatosEspecificosPertenecenEmpleado() {
    var arrHEmpleado = $(".hiddenHEmpleado");
    var validar = true;
    $.each(arrHEmpleado, function (index, value) {
        var idEmpleado = $(value).val();
        if (idEmpleado.toString() == "" || idEmpleado == null)
            validar = false;
    });
    return validar;
}

function eventoGuardar()
{
    $("#btn-guardar").on("click", function (e) {
        var api = $("#tableDetallesActivoFijoSeleccionados").dataTable().api();
        var form = $("#checkout-form");
        var validar = true;

        if (api.rows().nodes().length == 0)
        {
            mostrarNotificacion("Error", "Tiene que seleccionar al menos un Activo Fijo.");
            validar = false;
        }
        if (!validarDatosEspecificosPertenecenEmpleado())
        {
            mostrarNotificacion("Error", "Tiene que seleccionar el Custodio para todos los Activos Fijos seleccionados.");
            validar = false;
        }
        if (!form.valid())
            validar = false;

        if (validar)
            $("#btn-guardar").prop("type", "submit");
    });
}