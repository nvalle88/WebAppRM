var categoria = 3;
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
    eventoSpinnerNumeroConsecutivo();
    eventoSpinnerCantidad();
    initArrIdFilas();
    eventoCategoria();
    obtenerCategoria();
    eventoRadioDatosEspecificos();
    eventoGuardarDatosEspecificos();
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
                var validar = validarDatosEspecificosPertenecenBodegaoEmpleado();
                if (!validar) {
                    mostrarNotificacion("Error", "Tiene que asignar cada dato específico al menos a una Bodega o Empleado.");
                    $("#spanError").html("El modelo es inválido.");
                }
            }
            else if (data.step == 3 && isEditar)
            {
                $("#checkout-form").submit();
                return false;
            }
            else
            {
                if (data.step == 1)
                {
                    $("#ActivoFijo_CodigoActivoFijo_SUBCAF").val($("#ActivoFijo_IdSubClaseActivoFijo").val());
                    $("#ActivoFijo_CodigoActivoFijo_CAF").val($("#ActivoFijo_SubClaseActivoFijo_IdClaseActivoFijo").val());
                    $("#ActivoFijo_CodigoActivoFijo_SUC").val($("#LibroActivoFijo_IdSucursal").val());
                    var idSucursal = $("#LibroActivoFijo_IdSucursal").val() + ". ";
                    var idClase = agregarCeros($("#ActivoFijo_SubClaseActivoFijo_IdClaseActivoFijo").val(), 3) + ". ";
                    var idSubClase = agregarCeros($("#ActivoFijo_IdSubClaseActivoFijo").val(), 3) + ". ";
                    $("#spanCodigoSecuencial").html(idSucursal + idClase + idSubClase);
                    generarNumeroConsecutivo();
                }
            }
        }
        return validar;
    });
}

function generarNumeroConsecutivo()
{
    var numeroConsecutivo = $("#ActivoFijo_CodigoActivoFijo_Consecutivo").val();
    $("#spanNumeroConsecutivo").html(numeroConsecutivo);
}

function eventoSpinnerNumeroConsecutivo() {
    $(".spinnerNumeroConsecutivo").spinner('delay', 200).spinner('changed', function (e, newVal, oldVal) {
        generarNumeroConsecutivo();
    });
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

function configurarDropzone()
{
    var acceptedFiles = 'image/*,.pdf,.xlsx,.xls,.txt,.docx,.doc';
    $("#spanExtensionesPermitidas").html(acceptedFiles);

    Dropzone.autoDiscover = false;
    $("#mydropzone").dropzone({
        acceptedFiles: acceptedFiles,
        addRemoveLinks: true,
        uploadMultiple: true,
        parallelUploads: 100,
        paramName: "file",
        autoProcessQueue: false,
        createImageThumbnails: true,
        maxThumbnailFilesize: 10,
        maxFilesize: 10,
        dictDefaultMessage: '<span class="text-center"><span class="font-lg visible-xs-block visible-sm-block visible-lg-block"><span class="font-xs"><i class="fa fa-caret-right text-danger"></i> Guardar documentaci&oacute;n o fotograf&iacute;as </span><span>&nbsp&nbsp<h4 class="display-inline"> (Haga clic)</h4></span>',
        dictResponseError: '¡Error subiendo archivo!',
        dictRemoveFile: 'Eliminar archivo',
        dictFileTooBig: 'El archivo es muy grande ({{filesize}} MB). Tamaño máximo permitido: {{maxFilesize}} MB.',
        dictInvalidFileType: 'El archivo no está permitido.',
        dictResponseError: 'Error {{statusCode}} al intentar subir el archivo.',
        dictCancelUpload: 'Cancelar archivo',
        dictCancelUploadConfirmation: '¿Desea cancelar la subida del archivo?',
        dictMaxFilesExceeded: 'Ha excedido el número de archivos a subir ({{maxFiles}} ficheros).',
        init: function () {
            this.on("removedfile", function (file, data) {
                $.ajax({
                    url: urlEliminarActivoFijo,
                    method: "POST",
                    data: { fileName: file.name, dir: $("#dir").val() },
                    error: function (errorMessage)
                    {
                        mostrarNotificacion("Error", "No se pudo eliminar o no se encontró el archivo " + file.name + ".");
                    }
                });
            });

            this.on("success", function (file, data) {
                $("#dir").val(data.value);
                $("#nombreCarpeta").val(data.value);
            });

            this.on("error", function (file, data) {
                this.removeFile(file);
                mostrarNotificacion("Error", "El sistema no tiene permisos suficientes para guardar la información.");
            });
        }
    });
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

function obtenerCategoria()
{
    categoria = parseInt($("#IdCategoria").val());
    return categoria;
}

function initArrIdFilas()
{
    for (var i = 0; i < maxIdFila; i++) {
        arrIdsfilas.push(i);
    }
}

function crearFilas(cantidad)
{
    obtenerCategoria();
    for (var i = 0; i < cantidad; i++)
    {
        var tdRecepcionActivoFijoDetalle = "";
        var nuevoIdFila = arrIdsfilas.length;
        switch (categoria) {
            case 1: {
                tdRecepcionActivoFijoDetalle = crearColumnaVaciaInmueble();
                break;
            }
            case 2: {
                tdRecepcionActivoFijoDetalle = crearColumnaVaciaVehiculo(nuevoIdFila);
                break;
            }
            default: {
                tdRecepcionActivoFijoDetalle = crearColumnaVaciaOtro(nuevoIdFila);
                break;
            }
        }
        var tdBodega = "<td id='tdBodega_" + nuevoIdFila + "'>" + "<span id='spanBodega_" + nuevoIdFila + "'>-</span>" + "<input type='hidden' id='hBodega_" + nuevoIdFila + "'" + "name='hBodega_" + nuevoIdFila + "'" + "/></td>";
        var tdEmpleado = "<td id='tdEmpleado_" + nuevoIdFila + "'>" + "<span id='spanEmpleado_" + nuevoIdFila + "'>-</span>" + "<input type='hidden' id='hEmpleado_" + nuevoIdFila + "'" + "name='hEmpleado_" + nuevoIdFila + "'" + "/></td>";
        var btnEditarDatosEspecificos = "<td><input type='hidden' id='hIdRecepcionActivoFijoDetalle_" + nuevoIdFila + "' name='hIdRecepcionActivoFijoDetalle_" + nuevoIdFila + "' /><input type='hidden' id='hUbicacion_" + nuevoIdFila + "' name='hUbicacion_" + nuevoIdFila + "' /><a href='javascript: void(0);' onclick='cargarFormularioDatosEspecificos(" + nuevoIdFila + ")' class='btnEditarDatosEspecificos' data-idfila='" + nuevoIdFila + "' data-toggle='modal' data-target='#myModal'>" + "Editar</a>";
        var btnComponentesDatosEspecificos = "<span> | </span><a href='javascript: void(0);' onclick='cargarFormularioComponentesDatosEspecificos(" + nuevoIdFila + ")' class='btnComponentesDatosEspecificos' data-idfila='" + nuevoIdFila + "' data-toggle='modal' data-target='#myModalComponente'>" + "Componentes</a>";
        var btnEliminarDatosEspecificos = "<div id='divEliminarDatosEspecificos_" + nuevoIdFila + "' class='btnEliminarDatosEspecificos' style='display:inline;'><span>| </span><a href='javascript: void(0);' id='btnEliminarDatosEspecifico_" + nuevoIdFila + "' onclick=abrirVentanaConfirmacion('btnEliminarDatosEspecifico_" + nuevoIdFila + "') data-titulo='Eliminar' data-descripcion='&#191; Desea eliminar el Dato Espec&iacute; fico... ?'>Eliminar</a></div>";
        $("#tBodyDatosEspecificos").append("<tr id='trDatosEspecificos_" + nuevoIdFila + "'>" + tdRecepcionActivoFijoDetalle + tdBodega + tdEmpleado + btnEditarDatosEspecificos + btnComponentesDatosEspecificos + btnEliminarDatosEspecificos + "</td></tr>");
        arrIdsfilas.push(nuevoIdFila);
    }
}

function eliminarFilas(cantidad)
{
    var pos = arrIdsfilas.length - 1;
    while (cantidad > 0) {
        var idFila = arrIdsfilas[pos];
        $("#trDatosEspecificos_" + idFila).remove();
        arrIdsfilas.splice(pos, 1);
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

function eventoCategoria()
{
    $("#IdCategoria").on("change", function (e) {
        var nuevaCategoria = parseInt($("#IdCategoria").val());
        switch (categoria) {
            case 1: {
                $("#tdDatosEspecificosNumeroClaveCatastral").remove();
                break;
            }
            case 2: {
                $("#tdDatosEspecificosNumeroChasis").remove();
                $("#tdDatosEspecificosNumeroMotor").remove();
                $("#tdDatosEspecificosPlaca").remove();
                break;
            }
            default: {
                $("#tdDatosEspecificosSerie").remove();
                break;
            }
        }
        for (var i = 0; i < arrIdsfilas.length; i++)
        {
            try {
                switch (categoria) {
                    case 1: {
                        $("#tdNumeroClaveCatastral_" + i).remove();
                        break;
                    }
                    case 2: {
                        $("#tdNumeroChasis_" + i).remove();
                        $("#tdNumeroMotor_" + i).remove();
                        $("#tdPlaca_" + i).remove();
                        break;
                    }
                    default: {
                        $("#tdSerie_" + i).remove();
                        break;
                    }
                }
            } catch (e) { }
        }
        switch (nuevaCategoria) {
            case 1: {
                $("<th id='tdDatosEspecificosNumeroClaveCatastral'>" + headerNumeroClaveCatastral + "</th>").insertBefore("#tdDatosEspecificosBodega");
                break;
            }
            case 2: {
                $("<th id='tdDatosEspecificosPlaca'>" + headerPlaca + "</th>").insertBefore("#tdDatosEspecificosBodega");
                $("<th id='tdDatosEspecificosNumeroMotor'>" + headerNumeroMotor + "</th>").insertBefore("#tdDatosEspecificosPlaca");
                $("<th id='tdDatosEspecificosNumeroChasis'>" + headerNumeroChasis + "</th>").insertBefore("#tdDatosEspecificosNumeroMotor");
                break;
            }
            default: {
                $("<th id='tdDatosEspecificosSerie'>" + headerSerie + "</th>").insertBefore("#tdDatosEspecificosBodega");
                break;
            }
        }
        for (var i = 0; i < arrIdsfilas.length; i++) {
            try {
                switch (nuevaCategoria) {
                    case 1: {
                        $(crearColumnaVaciaInmueble(i)).insertBefore("#tdBodega_" + i);
                        break;
                    }
                    case 2: {
                        $(crearColumnaVaciaVehiculo(i)).insertBefore("#tdBodega_" + i);
                        break;
                    }
                    default: {
                        $(crearColumnaVaciaOtro(i)).insertBefore("#tdBodega_" + i);
                        break;
                    }
                }
            } catch (e) { }
        }
        categoria = nuevaCategoria;
    });
}

function crearColumnaVaciaInmueble(idColumna) {
    return "<td id='tdNumeroClaveCatastral_" + idColumna + "'>" + "<span id='spanNumeroClaveCatastral_" + idColumna + "'>-</span>" + "<input type='hidden' id='hNumeroClaveCatastral_" + idColumna + "'" + "name='hNumeroClaveCatastral_" + idColumna + "'" + "/></td>";
}

function crearColumnaVaciaVehiculo(idColumna)
{
    return "<td id='tdNumeroChasis_" + idColumna + "'>" + "<span id='spanNumeroChasis_" + idColumna + "'>-</span>" + "<input type='hidden' id='hNumeroChasis_" + idColumna + "'" + "name='hNumeroChasis_" + idColumna + "'" + "/></td>" +
        "<td id='tdNumeroMotor_" + idColumna + "'>" + "<span id='spanNumeroMotor_" + idColumna + "'>-</span>" + "<input type='hidden' id='hNumeroMotor_" + idColumna + "'" + "name='hNumeroMotor_" + idColumna + "'" + "/></td>" +
        "<td id='tdPlaca_" + idColumna + "'>" + "<span id='spanPlaca_" + idColumna + "'>-</span>" + "<input type='hidden' id='hPlaca_" + idColumna + "'" + "name='hPlaca_" + idColumna + "'" + "/></td>";
}

function crearColumnaVaciaOtro(idColumna)
{
    return "<td id='tdSerie_" + idColumna + "'>" + "<span id='spanSerie_" + idColumna + "'>-</span>" + "<input type='hidden' id='hSerie_" + idColumna + "'" + "name='hSerie_" + idColumna + "'" + "/></td>";
}

function cargarFormularioDatosEspecificos(idFila)
{
    mostrarLoadingPanel("modalContentDatosEspecificos", "Cargando datos, por favor espere...");
    $("#hIdFilaModalDatosEspecificos").val(idFila);
    $("#hIdRecepcionActivoFijoDetalle").val($("#hIdRecepcionActivoFijoDetalle_" + idFila).val());
    $("#hIdUbicacionActivoFijo").val($("#hUbicacion_" + idFila).val());
    obtenerCategoria();

    var objData = new Object();
    switch (categoria) {
        case 1: {
            objData.NumeroClaveCatastral = $("#hNumeroClaveCatastral_" + idFila).val();
            break;
        }
        case 2: {
            objData.NumeroChasis = $("#hNumeroChasis_" + idFila).val();
            objData.NumeroMotor = $("#hNumeroMotor_" + idFila).val();
            objData.Placa = $("#hPlaca_" + idFila).val();
            break;
        }
        default: {
            objData.Serie = $("#hSerie_" + idFila).val();
            break;
        }
    }
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
        mostrarLoadingPanel("modalContentDatosEspecificos", "Validando datos, por favor espere...");
        $.each($(".validationDatosEspecificos"), function (index, value) {
            $(value).html("");
        });

        var objData = new Object();
        switch (categoria) {
            case 1: {
                objData.NumeroClaveCatastral = $("#NumeroClaveCatastral").val();
                break;
            }
            case 2: {
                objData.NumeroChasis = $("#NumeroChasis").val();
                objData.NumeroMotor = $("#NumeroMotor").val();
                objData.Placa = $("#Placa").val();
                break;
            }
            default: {
                objData.Serie = $("#Serie").val();
                break;
            }
        }
        objData.IdFila = $("#hIdFilaModalDatosEspecificos").val();
        objData.IdRecepcionActivoFijoDetalle = $("#hIdRecepcionActivoFijoDetalle").val();
        objData.IdBodega = $("#IdBodega").val();
        objData.IdEmpleado = $("#IdEmpleado").val();
        objData.IsBodega = $("#radioBodegaDatosEspecificos").prop("checked");

        if (validarDatosEspecificosExisten(objData))
        {
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
                        switch (categoria) {
                            case 1: {
                                $("#spanNumeroClaveCatastral_" + idFila).html(objData.NumeroClaveCatastral);
                                $("#hNumeroClaveCatastral_" + idFila).val(objData.NumeroClaveCatastral);
                                break;
                            }
                            case 2: {
                                $("#spanNumeroChasis_" + idFila).html(objData.NumeroChasis);
                                $("#hNumeroChasis_" + idFila).val(objData.NumeroChasis);

                                $("#spanNumeroMotor_" + idFila).html(objData.NumeroMotor);
                                $("#hNumeroMotor_" + idFila).val(objData.NumeroMotor);

                                $("#spanPlaca_" + idFila).html(objData.Placa);
                                $("#hPlaca_" + idFila).val(objData.Placa);
                                break;
                            }
                            default: {
                                $("#spanSerie_" + idFila).html(objData.Serie);
                                $("#hSerie_" + idFila).val(objData.Serie);
                                break;
                            }
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
    });
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
    $("#span" + selector + "_" + idFila).html(datoSpan);
    $("#h" + selector + "_" + idFila).val(datoHiddeh);
}

function validarDatosEspecificosPertenecenBodegaoEmpleado()
{
    for (var i = 0; i < arrIdsfilas.length; i++) {
        var idFila = arrIdsfilas[i];
        var idBodega = $("#hBodega_" + idFila).val();
        var idEmpleado = $("#hEmpleado_" + idFila).val();

        if ((idBodega == "" || idBodega == null) && (idEmpleado == "" || idEmpleado == null))
            return false;
    }
    return true;
}

function validarDatosEspecificosExisten(objData)
{
    for (var i = 0; i < arrIdsfilas.length; i++) {
        var idFila = arrIdsfilas[i];
        if (objData.IdFila != idFila) {
            switch (categoria) {
                case 1: {
                    if (objData.NumeroClaveCatastral == $("#spanNumeroClaveCatastral_" + idFila).html()) {
                        $("#valNumeroClaveCatastral").html("El Número de clave catastral: ya existe.");
                        return false;
                    }
                    break;
                }
                case 2: {
                    var validar = true;
                    if (objData.NumeroChasis == $("#spanNumeroChasis_" + idFila).html()) {
                        $("#valNumeroChasis").html("El Número de chasis: ya existe.");
                        validar = false;
                    }
                    if (objData.NumeroMotor == $("#spanNumeroMotor_" + idFila).html()) {
                        $("#valNumeroMotor").html("El Número de motor: ya existe.");
                        validar = false;
                    }
                    if (objData.Placa == $("#spanPlaca_" + idFila).html()) {
                        $("#valPlaca").html("La Placa: ya existe.");
                        validar = false;
                    }

                    if (!validar)
                        return false;
                    break;
                }
                default: {
                    if (objData.Serie == $("#spanSerie_" + idFila).html()) {
                        $("#valSerie").html("La Serie: ya existe.");
                        return false;
                    }
                    break;
                }
            }
        }
    }
    return true;
}

function callBackFunctionEliminar(id)
{
    var idFila = id.split("_")[1];
    var posFila = obtenerPosIdFila(idFila);
    $("#trDatosEspecificos_" + arrIdsfilas[posFila]).remove();
    arrIdsfilas.splice(posFila, 1);
    $("#RecepcionActivoFijo_Cantidad").val(arrIdsfilas.length);

    if (arrIdsfilas.length == 1) {
        $("#divEliminarDatosEspecificos_" + arrIdsfilas[0]).addClass("hide");
    }
}