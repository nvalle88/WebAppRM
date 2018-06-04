var objName = {
    nameNumeroClaveCatastral: "NumeroClaveCatastral",
    nameNumeroChasis: "NumeroChasis",
    nameNumeroMotor: "NumeroMotor",
    namePlaca: "Placa",
    nameSerie: "Serie"
};
var objCategorias = {
    Edificio: "Edificios",
    MueblesEnseres: "Muebles y enseres",
    EquiposOficina: "Equipos de oficina",
    Vehiculo: "Vehículos",
    EquiposComputoSoftware: "Equipos de cómputo y software",
};
var arrIdsfilas = Array();
$(document).ready(function () {
    Init_Select2();
    Init_Touchspin();
    eventoTipoActivoFijo();
    eventoClaseActivoFijo();
    eventoMarca();
    eventoRamo();
    eventoSucursal();
    gestionarWizard();
    Init_DatetimePicker("RecepcionActivoFijo_FechaRecepcion");
    Init_FileInput("file");
    eventoSpinnerCantidad();
    initArrIdFilas();
    eventoRadioDatosEspecificos();
    eventoGuardarDatosEspecificos();
    initDataTableFiltrado("tbDatosEspecificos", []);
    if (!isEditar) {
        obtenerCategoria(function () {
            crearFilas(1);
            eventoCambiarCategoria();
        });
    }
    else
        eventoCambiarCategoria();
    $(".ColVis").hide();
});

function gestionarWizard()
{
    var wizard = $('.wizard').wizard();
    wizard.on('finished.fu.wizard', function (e, data) {
        var validar = validarWizard();
        if (validar)
            $("#checkout-form").submit();
        else
            return false;
    });
    wizard.on('change.fu.wizard', function (e, data)
    {
        if (data.direction === 'previous')
            return;

        var validar = validarWizard();
        if (validar)
        {
            if (data.step == 2)
            {
                var validar = validarDatosEspecificosPertenecenBodegaEmpleadoCodigosecuencial();
                if (!validar) {
                    mostrarNotificacion("Error", "Tiene que asignar cada dato específico a una Bodega o Empleado y a un Código secuencial.");
                    $("#spanError").html("El modelo es inválido.");
                }
            }
            else if (data.step == 3 && isEditar)
            {
                $("#checkout-form").submit();
                return false;
            }
        }
        return validar;
    });
}

function generarCodigosecuencial()
{
    $("#CodigoActivoFijo_SUBCAF").val($("#ActivoFijo_IdSubClaseActivoFijo").val());
    $("#CodigoActivoFijo_CAF").val($("#ActivoFijo_SubClaseActivoFijo_IdClaseActivoFijo").val());
    $("#CodigoActivoFijo_SUC").val($("#LibroActivoFijo_IdSucursal").val());
    var idSucursal = $("#LibroActivoFijo_IdSucursal").val() + ".";
    var idClase = agregarCeros($("#ActivoFijo_SubClaseActivoFijo_IdClaseActivoFijo").val(), 3) + ".";
    var idSubClase = agregarCeros($("#ActivoFijo_IdSubClaseActivoFijo").val(), 3) + ".";
    return idSucursal + idClase + idSubClase;
}

function validarWizard()
{
    var form = $("#checkout-form");
    if (!form.valid()) {
        mostrarNotificacion("Error", "Existen errores en el formulario.");
        $("#spanError").html("El modelo es inválido.");
        return false;
    }
    else
        $("#spanError").html("");
    return true;
}

function eventoTipoActivoFijo() {
    $("#ActivoFijo_SubClaseActivoFijo_ClaseActivoFijo_IdTipoActivoFijo").on("change", function (e) {
        partialViewTipoActivoFijo(e.val);
    });
}

function eventoClaseActivoFijo() {
    $("#ActivoFijo_SubClaseActivoFijo_IdClaseActivoFijo").on("change", function (e) {
        partialViewClaseActivoFijo(e.val);
    });
}

function eventoMarca()
{
    $("#ActivoFijo_Modelo_IdMarca").on("change", function (e) {
        mostrarLoadingPanel("checkout-form", "Cargando modelos...");
        $.ajax({
            url: urlModeloSelectResult,
            method: "POST",
            data: { idMarca: e.val },
            success: function (data) {
                $("#div_modelo").html(data);
                Init_Select2();
            },
            complete: function (data) {
                $("#checkout-form").waitMe("hide");
            }
        });
    });
}

function eventoRamo() {
    $("#ActivoFijo_PolizaSeguroActivoFijo_Subramo_IdRamo").on("change", function (e) {
        mostrarLoadingPanel("checkout-form", "Cargando subramos...");
        $.ajax({
            url: urlSubramoSelectResult,
            method: "POST",
            data: { idRamo: e.val },
            success: function (data) {
                $("#div_subramo").html(data);
                Init_Select2();
            },
            complete: function (data) {
                $("#checkout-form").waitMe("hide");
            }
        });
    });
}

function eventoSucursal() {
    $("#LibroActivoFijo_IdSucursal").on("change", function (e) {
        partialViewLibroActivoFijo(e.val);
    });
}

function partialViewLibroActivoFijo(idSucursal) {
    mostrarLoadingPanel("checkout-form", "Cargando libros de activo fijo...");
    $.ajax({
        url: libroActivoFijoSelectResult,
        method: "POST",
        data: { idSucursal: obtenerIdAjax(idSucursal) },
        success: function (data) {
            $("#div_libroActivoFijo").html(data);
            Init_Select2();
        },
        complete: function (data) {
            clearDatosEspecificosBodegaEmpleado();
            $("#checkout-form").waitMe("hide");
        }
    });
}

function partialViewTipoActivoFijo(idTipoActivoFijo) {
    mostrarLoadingPanel("checkout-form", "Cargando clases de activo fijo...");
    $.ajax({
        url: claseActivoFijoSelectResult,
        method: "POST",
        data: { idTipoActivoFijo: obtenerIdAjax(idTipoActivoFijo) },
        success: function (data) {
            $("#div_claseActivoFijo").html(data);
            Init_Select2();
        },
        error: function (data) {
            $("#checkout-form").waitMe("hide");
        },
        complete: function (data)
        {
            partialViewClaseActivoFijo($("#ActivoFijo_SubClaseActivoFijo_IdClaseActivoFijo").val());
            eventoClaseActivoFijo();
        }
    });
}

function partialViewClaseActivoFijo(idClaseActivoFijo) {
    mostrarLoadingPanel("checkout-form", "Cargando subclases de activo fijo...");
    $.ajax({
        url: subClaseActivoFijoSelectResult,
        method: "POST",
        data: { idClaseActivoFijo: obtenerIdAjax(idClaseActivoFijo) },
        success: function (data) {
            $("#div_subClaseActivoFijo").html(data);
            Init_Select2();
        },
        complete: function (data) {
            $("#checkout-form").waitMe("hide");
            obtenerCategoria(function () {
                eventoCambiarCategoria();
            });
        }
    });
}

function eventoSpinnerCantidad() {
    $(".spinnerCantidad").spinner('delay', 200).spinner('changed', function (e, newVal, oldVal) {
        if (newVal > arrIdsfilas.length)
        {
            crearFilas(newVal - arrIdsfilas.length);
            $.each($(".btnEliminarDatosEspecificos"), function (index, value) {
                $(value).removeClass("hide");
            });
        }
        else
            eliminarFilas(arrIdsfilas.length - newVal);
    });
}

function obtenerCategoria(callbackFunctionCategoria)
{
    mostrarLoadingPanel("tbDatosEspecificos", "Cargando datos por categoría...");
    $.ajax({
        url: urlCategoria,
        method: "POST",
        data: { idClaseActivoFijo: $("#ActivoFijo_SubClaseActivoFijo_IdClaseActivoFijo").val() },
        success: function (data) {
            categoria = data;
        },
        error: function ()
        {
            categoria = "";
        },
        complete: function (data) {
            if (callbackFunctionCategoria)
                callbackFunctionCategoria();
            $("#tbDatosEspecificos").waitMe("hide");
        }
    });
}

function initArrIdFilas()
{
    for (var i = 0; i < maxIdFila; i++) {
        arrIdsfilas.push(i);
    }
}

function crearFilas(cantidad)
{
    for (var i = 0; i < cantidad; i++)
    {
        var nuevoIdFila = arrIdsfilas.length;
        var hNumeroClaveCatastral = "<input type='hidden' id='h" + objName.nameNumeroClaveCatastral + "_" + nuevoIdFila + "'" + "name='h" + objName.nameNumeroClaveCatastral + "_" + nuevoIdFila + "'" + "/>";
        var hNumeroChasis = "<input type='hidden' id='h" + objName.nameNumeroChasis + "_" + nuevoIdFila + "'" + "name='h" + objName.nameNumeroChasis + "_" + nuevoIdFila + "'" + "/>";
        var hNumeroMotor = "<input type='hidden' id='h" + objName.nameNumeroMotor + "_" + nuevoIdFila + "'" + "name='h" + objName.nameNumeroMotor + "_" + nuevoIdFila + "'" + "/>";
        var hPlaca = "<input type='hidden' id='h" + objName.namePlaca + "_" + nuevoIdFila + "'" + "name='h" + objName.namePlaca + "_" + nuevoIdFila + "'" + "/>";
        var hSerie = "<input type='hidden' id='h" + objName.nameSerie + "_" + nuevoIdFila + "'" + "name='h" + objName.nameSerie + "_" + nuevoIdFila + "'" + "/>";
        var hBodega = "<input type='hidden' id='hBodega_" + nuevoIdFila + "'" + "name='hBodega_" + nuevoIdFila + "'" + "/>";
        var hEmpleado = "<input type='hidden' id='hEmpleado_" + nuevoIdFila + "'" + "name='hEmpleado_" + nuevoIdFila + "'" + "/>";
        var hRecepcionActivoFijoDetalle = "<input type='hidden' id='hIdRecepcionActivoFijoDetalle_" + nuevoIdFila + "' name='hIdRecepcionActivoFijoDetalle_" + nuevoIdFila + "' />";
        var hUbicacion = "<input type='hidden' id='hUbicacion_" + nuevoIdFila + "' name='hUbicacion_" + nuevoIdFila + "' />";
        var hComponentes = "<input type='hidden' id='hComponentes_" + nuevoIdFila + "' name='hComponentes_" + nuevoIdFila + "' />";
        var hCodigoSecuencial = "<input type='hidden' id='hCodigoSecuencial_" + nuevoIdFila + "' name='hCodigoSecuencial_" + nuevoIdFila + "' />";
        var hIdCodigoActivoFijo = "<input type='hidden' id='hIdCodigoActivoFijo_" + nuevoIdFila + "' name='hIdCodigoActivoFijo_" + nuevoIdFila + "' />";
        var btnEditarDatosEspecificos = "<a href='javascript: void(0);' onclick='cargarFormularioDatosEspecificos(" + nuevoIdFila + ")' class='btnEditarDatosEspecificos' data-idfila='" + nuevoIdFila + "' data-toggle='modal' data-target='#myModal'>" + "Editar</a>";
        var btnEditarCodificacion = "<span> | </span><a href='javascript: void(0);' onclick='cargarFormularioCodificacion(" + nuevoIdFila + ")' class='btnEditarCodificacion' data-idfila='" + nuevoIdFila + "' data-toggle='modal' data-target='#myModal'>" + "Codificaci&oacute;n</a>";
        var btnComponentesDatosEspecificos = "<span> | </span><a href='javascript: void(0);' onclick='cargarFormularioComponentesDatosEspecificos(" + nuevoIdFila + ")' class='btnComponentesDatosEspecificos' data-idfila='" + nuevoIdFila + "' data-idorigen='' data-idscomponentes='' data-toggle='modal' data-target='#myModalComponente'>" + "Componentes</a>";
        var btnEliminarDatosEspecificos = "<div id='divEliminarDatosEspecificos_" + nuevoIdFila + "' class='btnEliminarDatosEspecificos" + (arrIdsfilas.length == 0 ? " hide" : "") + "' style='display:inline;'><span> | </span><a href='javascript: void(0);' id='btnEliminarDatosEspecifico_" + nuevoIdFila + "' onclick=abrirVentanaConfirmacion('btnEliminarDatosEspecifico_" + nuevoIdFila + "') data-funcioncallback='callBackFunctionEliminarDatoEspecifico(" + nuevoIdFila + ")' data-titulo='Eliminar' data-descripcion='&#191; Desea eliminar el Dato Espec&iacute; fico... ?'>Eliminar</a>";

        mostrarOcultarColumnas([true, true, true, true, true]);
        addRowTableDatosEspecificos(['-', '-', '-', '-', '-', '-', '-', '-',
            hNumeroClaveCatastral + hNumeroChasis + hNumeroMotor + hPlaca + hSerie + hBodega + hEmpleado + hRecepcionActivoFijoDetalle + hUbicacion + hComponentes + hCodigoSecuencial + btnEditarDatosEspecificos + btnEditarCodificacion + btnComponentesDatosEspecificos + btnEliminarDatosEspecificos],
            nuevoIdFila, [objName.nameNumeroClaveCatastral, objName.nameNumeroChasis, objName.nameNumeroMotor, objName.namePlaca, objName.nameSerie, "Bodega", "Empleado", "Codificacion"]);
        eventoCambiarCategoria();

        arrIdsfilas.push(nuevoIdFila);
        addComponenteToArray(nuevoIdFila, 0, []);
    }
}

function addRowTableDatosEspecificos(arrColumnas, idFila, arrNames)
{
    var table = $('#tbDatosEspecificos').DataTable();
    var rowNode = table.row.add(arrColumnas).draw().node();
    $(rowNode).prop("id", "trDatosEspecificos_" + idFila);
    $.each($(rowNode).children(), function (index, value) {
        if (index < arrNames.length)
            $(value).prop("id", "td" + arrNames[index] + "_" + idFila);
    });
}

function deleteRowTableDatosEspecificos(idFila) {
    var row = $("#trDatosEspecificos_" + idFila);
    $('#tbDatosEspecificos').dataTable().fnDeleteRow(row);
}

function eliminarFilas(cantidad)
{
    var pos = arrIdsfilas.length - 1;
    while (cantidad > 0) {
        var idFila = arrIdsfilas[pos];
        deleteRowTableDatosEspecificos(idFila);
        arrIdsfilas.splice(pos, 1);
        eliminarComponente(idFila);
        pos--;
        cantidad--;
    }
}

function obtenerPosIdFila(idFila)
{
    for (var i = 0; i < arrIdsfilas.length; i++) {
        if (arrIdsfilas[i] == idFila)
            return i;
    }
}

function eventoCambiarCategoria()
{
    if (categoria == objCategorias.Edificio)
        mostrarOcultarColumnas([true, false, false, false, true]);
    else if (categoria == objCategorias.Vehiculo)
        mostrarOcultarColumnas([false, true, true, true, true]);
    else if (categoria == objCategorias.EquiposComputoSoftware)
        mostrarOcultarColumnas([false, false, false, false, true]);
    else
        mostrarOcultarColumnas([false, false, false, false, true]);
}

function mostrarOcultarColumnas(arrObj)
{
    var otable = $('#tbDatosEspecificos').DataTable();
    for (var i = 0; i < arrObj.length; i++) {
        otable.column(i).visible(arrObj[i]);
    }
}

function cargarFormularioDatosEspecificos(idFila)
{
    mostrarLoadingPanel("modalContentDatosEspecificos", "Cargando datos, por favor espere...");
    $("#hIdFilaModalDatosEspecificos").val(idFila);
    $("#hIdRecepcionActivoFijoDetalle").val($("#hIdRecepcionActivoFijoDetalle_" + idFila).val());
    $("#hIdUbicacionActivoFijo").val($("#hUbicacion_" + idFila).val());
    $("#myModalLabel").html("Editar");
    $("#mFuncionCallbak").val("guardarDatosEspecificos");

    var objData = new Object();
    if (categoria == objCategorias.Edificio)
        objData.NumeroClaveCatastral = $("#h" + objName.nameNumeroClaveCatastral + "_" + idFila).val();
    else if (categoria == objCategorias.Vehiculo)
    {
        objData.NumeroChasis = $("#h" + objName.nameNumeroChasis + "_" + idFila).val();
        objData.NumeroMotor = $("#h" + objName.nameNumeroMotor + "_" + idFila).val();
        objData.Placa = $("#h" + objName.namePlaca + "_" + idFila).val();
    }
    else if (categoria == objCategorias.EquiposComputoSoftware)
        objData.Serie = $("#h" + objName.nameSerie + "_" + idFila).val();

    objData.IdBodega = $("#hBodega_" + idFila).val();
    objData.IdEmpleado = $("#hEmpleado_" + idFila).val();

    $.ajax({
        url: urlModalDatosEspecificosResult,
        method: "POST",
        data: { rafd: objData, categoria: categoria, idSucursal: $("#LibroActivoFijo_IdSucursal").val(), idBodega: objData.IdBodega, idEmpleado: objData.IdEmpleado },
        success: function (data) {
            $("#modalBodyDatosEspecificos").html(data);
            Init_Select2();
            eventoRadioDatosEspecificos();
        },
        error: function (errorMessage) {
            mostrarNotificacion("Error", "Ocurrió un error al cargar el formulario.");
        },
        complete: function (data) {
            $("#modalContentDatosEspecificos").waitMe("hide");
        }
    });
}

function eventoRadioDatosEspecificos()
{
    $(".radioDatosEspecificos").on("change", function (e) {
        var ubicacion = $(e.currentTarget).data("ubicacion");
        switch (ubicacion) {
            case "Bodega": {
                $("#divDatosEspecificosEmpleado").addClass("hide");
                $("#divDatosEspecificosBodega").removeClass("hide");
                break;
            }
            default: {
                $("#divDatosEspecificosBodega").addClass("hide");
                $("#divDatosEspecificosEmpleado").removeClass("hide");
                break;
            }
        }
    });
}

function eventoGuardarDatosEspecificos()
{
    $("#btnGuardarDatosEspecificos").on("click", function (e) {
        var funcionCallback = $("#mFuncionCallbak").val();
        eval(funcionCallback + "()");
    });
}

function guardarDatosEspecificos()
{
    mostrarLoadingPanel("modalContentDatosEspecificos", "Validando datos, por favor espere...");
    $.each($(".validationDatosEspecificos"), function (index, value) {
        $(value).html("");
    });

    var objData = new Object();
    if (categoria == objCategorias.Edificio)
        objData.NumeroClaveCatastral = $("#" + objName.nameNumeroClaveCatastral).val();
    else if (categoria == objCategorias.Vehiculo) {
        objData.NumeroChasis = $("#" + objName.nameNumeroChasis).val();
        objData.NumeroMotor = $("#" + objName.nameNumeroMotor).val();
        objData.Placa = $("#" + objName.namePlaca).val();
    }
    else if (categoria == objCategorias.EquiposComputoSoftware)
        objData.Serie = $("#" + objName.nameSerie).val();

    objData.IdFila = $("#hIdFilaModalDatosEspecificos").val();
    objData.IdRecepcionActivoFijoDetalle = $("#hIdRecepcionActivoFijoDetalle").val();
    objData.IdBodega = $("#IdBodega").val();
    objData.IdEmpleado = $("#IdEmpleado").val();
    objData.IsBodega = $("#radioBodegaDatosEspecificos").prop("checked");

    if (validarDatosEspecificosExisten(objData)) {
        $.ajax({
            url: urlValidacionDatosEspecificosResult,
            method: "POST",
            data: { rafd: objData },
            success: function (data) {
                if (data.length > 0) {
                    $.each(data, function (index, value) {
                        $("#val" + value.propiedad).html(value.valor);
                    });
                }
                else {
                    var idFila = $("#hIdFilaModalDatosEspecificos").val();
                    var table = $('#tbDatosEspecificos').DataTable();
                    if (categoria == objCategorias.Edificio) {
                        table.cell($('#td' + objName.nameNumeroClaveCatastral + "_" + idFila)).data(formatearDatosEspecifico(objData.NumeroClaveCatastral)).draw();
                        $("#h" + objName.nameNumeroClaveCatastral + "_" + idFila).val(objData.NumeroClaveCatastral);
                    }
                    else if (categoria == objCategorias.Vehiculo) {
                        table.cell($('#td' + objName.nameNumeroChasis + "_" + idFila)).data(formatearDatosEspecifico(objData.NumeroChasis)).draw();
                        $("#h" + objName.nameNumeroChasis + "_" + idFila).val(objData.NumeroChasis);

                        table.cell($('#td' + objName.nameNumeroMotor + "_" + idFila)).data(formatearDatosEspecifico(objData.NumeroMotor)).draw();
                        $("#h" + objName.nameNumeroMotor + "_" + idFila).val(objData.NumeroMotor);

                        table.cell($('#td' + objName.namePlaca + "_" + idFila)).data(formatearDatosEspecifico(objData.Placa)).draw();
                        $("#h" + objName.namePlaca + "_" + idFila).val(objData.Placa);
                    }
                    else if (categoria == objCategorias.EquiposComputoSoftware) {
                        table.cell($('#td' + objName.nameSerie + "_" + idFila)).data(formatearDatosEspecifico(objData.Serie)).draw();
                        $("#h" + objName.nameSerie + "_" + idFila).val(objData.Serie);
                    }
                    if (objData.IsBodega) {
                        var isTodosBodegasDatosEspecificos = $("#chkTodosBodegasDatosEspecificos").prop("checked");
                        if (isTodosBodegasDatosEspecificos) {
                            for (var i = 0; i < arrIdsfilas.length; i++) {
                                putDatoBodegaEmpleadoFila(arrIdsfilas[i], "Bodega", $("#IdBodega option:selected").text(), $("#IdBodega").val());
                                putDatoBodegaEmpleadoFila(arrIdsfilas[i], "Empleado", "-", "");
                            }
                        }
                        else {
                            putDatoBodegaEmpleadoFila(idFila, "Bodega", $("#IdBodega option:selected").text(), $("#IdBodega").val());
                            putDatoBodegaEmpleadoFila(idFila, "Empleado", "-", "");
                        }
                    }
                    else {
                        var isTodosEmpleadosDatosEspecificos = $("#chkTodosEmpleadosDatosEspecificos").prop("checked");
                        if (isTodosEmpleadosDatosEspecificos) {
                            for (var i = 0; i < arrIdsfilas.length; i++) {
                                putDatoBodegaEmpleadoFila(arrIdsfilas[i], "Empleado", $("#IdEmpleado option:selected").text(), $("#IdEmpleado").val());
                                putDatoBodegaEmpleadoFila(arrIdsfilas[i], "Bodega", "-", "");
                            }
                        }
                        else {
                            putDatoBodegaEmpleadoFila(idFila, "Empleado", $("#IdEmpleado option:selected").text(), $("#IdEmpleado").val());
                            putDatoBodegaEmpleadoFila(idFila, "Bodega", "-", "");
                        }
                    }
                    $("#btnCancelarDatosEspecificos").click();
                }
            },
            error: function (errorMessage) {
                mostrarNotificacion("Error", "Ocurrió un error al validar el formulario.");
            },
            complete: function (data) {
                $("#modalContentDatosEspecificos").waitMe("hide");
            }
        });
    }
    else
        $("#modalContentDatosEspecificos").waitMe("hide");
}

function formatearDatosEspecifico(valor)
{
    valor = valor.toString().trim();
    if (valor == "")
        valor = "-";
    return valor;
}

function clearDatosEspecificosBodegaEmpleado()
{
    for (var i = 0; i < arrIdsfilas.length; i++) {
        putDatoBodegaEmpleadoFila(arrIdsfilas[i], "Bodega", "-", "");
        putDatoBodegaEmpleadoFila(arrIdsfilas[i], "Empleado", "-", "");
    }
}

function putDatoBodegaEmpleadoFila(idFila, selector, datoSpan, datoHiddeh)
{
    $('#tbDatosEspecificos').DataTable().cell($('#td' + selector + "_" + idFila)).data(datoSpan).draw();
    $("#h" + selector + "_" + idFila).val(datoHiddeh);
}

function validarDatosEspecificosPertenecenBodegaEmpleadoCodigosecuencial()
{
    for (var i = 0; i < arrIdsfilas.length; i++) {
        var idFila = arrIdsfilas[i];
        var idBodega = $("#hBodega_" + idFila).val();
        var idEmpleado = $("#hEmpleado_" + idFila).val();
        var codigoSecuencial = $("#hCodigoSecuencial_" + idFila).val();

        if ((idBodega == "" || idBodega == null) && (idEmpleado == "" || idEmpleado == null) || (codigoSecuencial == "" || codigoSecuencial == null))
            return false;
    }
    return true;
}

function validarDatosEspecificosExisten(objData)
{
    for (var i = 0; i < arrIdsfilas.length; i++) {
        var idFila = arrIdsfilas[i];
        if (objData.IdFila != idFila)
        {
            if (categoria == objCategorias.Edificio)
            {
                if (objData.NumeroClaveCatastral == $("#td" + objName.nameNumeroClaveCatastral + "_" + idFila).html()) {
                    $("#val" + objName.nameNumeroClaveCatastral).html("El Número de clave catastral: ya existe.");
                    return false;
                }
            }
            else if (categoria == objCategorias.Vehiculo)
            {
                var validar = true;
                if (objData.NumeroChasis == $("#td" + objName.nameNumeroChasis + "_" + idFila).html()) {
                    $("#val" + objName.nameNumeroChasis).html("El Número de chasis: ya existe.");
                    validar = false;
                }
                if (objData.NumeroMotor == $("#td" + objName.nameNumeroMotor + "_" + idFila).html()) {
                    $("#val" + objName.nameNumeroMotor).html("El Número de motor: ya existe.");
                    validar = false;
                }
                if (objData.Placa == $("#td" + objName.namePlaca + "_" + idFila).html()) {
                    $("#val" + objName.namePlaca).html("La Placa: ya existe.");
                    validar = false;
                }
                if (!validar)
                    return false;
            }
            else if (categoria == objCategorias.EquiposComputoSoftware)
            {
                if (objData.Serie == $("#td" + objName.nameSerie + "_" + idFila).html()) {
                    $("#val" + objName. nameSerie).html("La Serie: ya existe.");
                    return false;
                }
            }
        }
    }
    return true;
}

function callBackFunctionEliminarDatoEspecifico(idFila)
{
    var posFila = obtenerPosIdFila(idFila);
    deleteRowTableDatosEspecificos(posFila);
    arrIdsfilas.splice(posFila, 1);
    eliminarComponente(idFila);
    $("#RecepcionActivoFijo_Cantidad").val(arrIdsfilas.length);

    if (arrIdsfilas.length == 1)
        $("#divEliminarDatosEspecificos_" + arrIdsfilas[0]).addClass("hide");
}

function cargarFormularioCodificacion(idFila)
{
    mostrarLoadingPanel("modalContentDatosEspecificos", "Cargando datos, por favor espere...");
    $("#hIdFilaModalDatosEspecificos").val(idFila);
    $("#hIdRecepcionActivoFijoDetalle").val($("#hIdRecepcionActivoFijoDetalle_" + idFila).val());
    $("#myModalLabel").html("Codificaci&oacute;n");
    $("#mFuncionCallbak").val("guardarCodificacion");

    var hCodificacion = $("#hCodigoSecuencial_" + idFila).val();
    var numeroConsecutivo = 1;
    if (hCodificacion != "" && hCodificacion != null) {
        try {
            var arrCodificacion = hCodificacion.split(".");
            numeroConsecutivo = arrCodificacion[arrCodificacion.length - 1];
        } catch (e) { }
    }

    $.ajax({
        url: urlCodificacion,
        method: "POST",
        data: { Codigosecuencial: $("#hCodigoSecuencial_" + idFila).val() },
        success: function (data) {
            $("#modalBodyDatosEspecificos").html(data);
            $("#CodigoActivoFijo_Consecutivo").val(numeroConsecutivo);
            $(".spinnerNumeroConsecutivo").spinner();
            $("#spanCodigoSecuencial").html(generarCodigosecuencial());
            asignarNumeroConsecutivoCodigoBarras();
            eventoSpinnerNumeroConsecutivo();
        },
        error: function (errorMessage) {
            mostrarNotificacion("Error", "Ocurrió un error al cargar el formulario.");
        },
        complete: function (data) {
            $("#modalContentDatosEspecificos").waitMe("hide");
        }
    });
}

function guardarCodificacion()
{
    asignarNumeroConsecutivoCodigoBarras();
    mostrarLoadingPanel("modalContentDatosEspecificos", "Validando datos, por favor espere...");
    $.each($(".validationCodificacion"), function (index, value) {
        $(value).html("");
    });

    var objData = {
        IdFila: $("#hIdFilaModalDatosEspecificos").val(),
        IdRecepcionActivoFijoDetalle: $("#hIdRecepcionActivoFijoDetalle").val(),
        Codigosecuencial: $("#spanCodigoSecuencial").html() + $("#spanNumeroConsecutivo").html(),
        IdCodigoActivoFijo: $("#hIdCodigoActivoFijo_").val()
    };
    if (validarCodificacionNoExiste(objData))
    {
        $.ajax({
            url: urlValidarCodigoUnico,
            method: "POST",
            data: { idCodigoActivoFijo: objData.IdCodigoActivoFijo, codigoSecuencial: objData.Codigosecuencial },
            success: function (data)
            {
                if (data.toString().toLowerCase() == "false")
                {
                    $('#tbDatosEspecificos').DataTable().cell($('#tdCodificacion_' + objData.IdFila)).data(objData.Codigosecuencial).draw();
                    $("#hCodigoSecuencial_" + objData.IdFila).val(objData.Codigosecuencial);
                    $("#btnCancelarDatosEspecificos").click();
                }
                else
                    $("#valConsecutivo").html("El Código secuencial: ya existe.");
            },
            error: function (errorMessage) {
                mostrarNotificacion("Error", "Ocurrió un error al validar el formulario.");
            },
            complete: function (data) {
                $("#modalContentDatosEspecificos").waitMe("hide");
            }
        });
    }
    else
        $("#modalContentDatosEspecificos").waitMe("hide");
}

function validarCodificacionNoExiste(objData)
{
    for (var i = 0; i < arrIdsfilas.length; i++) {
        var idFila = arrIdsfilas[i];
        if (objData.IdFila != idFila)
        {
            if (objData.Codigosecuencial == $("#hCodigoSecuencial_" + idFila).val()) {
                $("#valConsecutivo").html("El Código secuencial: ya existe.");
                return false;
            }
        }
    }
    return true;
}