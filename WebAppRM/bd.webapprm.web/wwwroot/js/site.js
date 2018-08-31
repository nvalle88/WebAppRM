var objCategorias = {
    Edificio: "EDIFICIOS",
    MueblesEnseres: "MUEBLES Y ENSERES",
    EquiposOficina: "EQUIPOS DE OFICINA",
    Vehiculo: "VEHÍCULOS",
    EquiposComputoSoftware: "EQUIPOS DE CÓMPUTO Y SOFTWARE"
};

$(document).ready(function () {
    eventoInputMayuscula();
});

function eventoInputMayuscula() {
    $(":input").on("keyup", function (e) {
        e.currentTarget.value = e.currentTarget.value.toUpperCase();
    });
}

function Init_Touchspin()
{
    $('.touchspin_tasa').TouchSpin({
        min: 0.00,
        max: 99999999999,
        decimals: 2,
        prefix: '$',
        step: 0.01,
        buttondown_class: 'btn btn-primary',
        buttonup_class: 'btn btn-primary'
    });
}

function Init_TouchspinPorId(idElemento) {
    $('#' + idElemento).TouchSpin({
        min: 0.00,
        max: 99999999999,
        decimals: 2,
        prefix: '$',
        step: 0.01,
        buttondown_class: 'btn btn-primary',
        buttonup_class: 'btn btn-primary'
    });
}

function Init_Select2()
{
    $('select').select2({
        theme: 'classic',
        allowClear: false,
        placeholder: 'Seleccione...',
        language: 'es'
    });
}

function Init_DatetimePicker(idElemento, forceMaxDate, isModeDate)
{
    var formato = 'DD/MM/YYYY';
    var objData = {
        format: isModeDate ? formato : (formato + ' hh:mm a')
    };

    if (forceMaxDate)
        objData.maxDate = moment();

    var datetimePicker = $('#' + idElemento).datetimepicker(objData);
    datetimePicker.on("dp.show", function (e) {
        if (forceMaxDate)
            $(this).data("DateTimePicker").maxDate(moment());
    });
}

function Init_DateRangePicker(fechaInicial, fechaFinal, idElemento) {
    $('#' + idElemento).daterangepicker({
        opens: ('left'),
        format: 'MM/DD/YYYY',
        separator: ' a ',
        applyClass: 'btn btn-primary',
        cancelClass: 'btn btn-default',
        endDate: moment(),
        startDate: fechaInicial,
        endDate: fechaFinal,
        maxDate: moment(),
    },
        function (start, end) {
            $('#' + idElemento + ' input').val(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
            try { onValueChangedDateRangerPicker(start.format('MM/DD/YYYY'), end.format('MM/DD/YYYY')); } catch (e) { }
        }
    );
    $('#' + idElemento + ' input').val(moment(fechaInicial.toLocaleDateString("es-ES"), 'MM/DD/YYYY').format('MMMM D, YYYY') + ' - ' + moment().format('MMMM D, YYYY'));
}

function Init_FileInput(idElemento)
{
    $("#" + idElemento).fileinput({
        showUpload: false,
        language: 'es'
    });
}

function Init_DeteccionLectorCodigoBarras(idElemento, fnDrawCallBack)
{
    $("#" + idElemento).scannerDetection({
        timeBeforeScanTest: 200,
        startChar: [120],
        endChar: [13],
        avgTimeByChar: 40,
        onComplete: function (barcode, qty) {
            try { fnDrawCallBack(barcode, qty); } catch (e) { }
        },
        onError: function (string)
        {
            mostrarNotificacion("Error", string);
        }
    });
}

function Init_XEditable(callbackFunction)
{
    $('.btntextAreaEditable').editable({
        showbuttons: 'bottom',
        emptytext: '&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;',
        success: function (response, newValue) {
            var idRecepcionActivoFijoDetalle = $(this).data("idrecepcionactivofijodetalle");
            callbackFunction(idRecepcionActivoFijoDetalle, newValue);
        },
        display: function (value, sourceData) {
            if (!value) {
                $(this).empty();
                return;
            }
            $(this).html(value.toUpperCase());
        }
    });
}

var arrBootBox = [];
function closeBootBox()
{
    arrBootBox[arrBootBox.length - 1].modal("hide");
}

function ajustarBootboxPorCiento(porCiento)
{
    var innerDialog = arrBootBox[arrBootBox.length - 1].find(".modal-dialog");
    innerDialog.prop("style", "width:" + porCiento + "% !important;");
}

function ajustarHeightBootbox()
{
    $('.modal-body').attr('style', 'max-height:' + (($(window).height() - $(window).height() / 4)) + 'px;overflow-y:auto;');
}

function Init_BootBox(titulo, cuerpo, size, buttonsAdicionales, objConfiguracionGuardar, callbackCancelar) {
    var objBootBox = {
        title: titulo,
        message: cuerpo
    };

    if (size)
        objBootBox.size = size;

    objBootBox.buttons = {
        cancel: {
            label: "Cancelar",
            className: 'btn-default',
            callback: function () {
                if (callbackCancelar)
                    callbackCancelar();

                closeBootBox();
                return false;
            }
        }
    };

    if (buttonsAdicionales && buttonsAdicionales != null) {
        $.each(buttonsAdicionales, function (index, value) {
            objBootBox.buttons[value.key] = value.obj;
        });
    }

    if (objConfiguracionGuardar && objConfiguracionGuardar != null) {
        if (objConfiguracionGuardar.isGuardar) {
            objBootBox.buttons.confirm = {
                label: "Guardar",
                className: 'btn-primary',
                callback: function () {
                    if (objConfiguracionGuardar.callbackGuardar)
                        objConfiguracionGuardar.callbackGuardar();

                    if (objConfiguracionGuardar.hideAlGuardar)
                        closeBootBox();
                    return false;
                }
            };
        }
    }
    var openedDialog = bootbox.dialog(objBootBox);
    arrBootBox.push(openedDialog);

    ajustarHeightBootbox();
    eventoInputMayuscula();

    openedDialog.on("hidden.bs.modal", function () {
        arrBootBox.splice(arrBootBox.length - 1, 1);

        if (callbackCancelar)
            callbackCancelar();
    });

    //Ejemplo de llamada cargando de datos todos los parámetros
    //Los botones adicionales tienen que manejar su función de callback y si cierran el modal o no.
    //Init_BootBox("Hola mundo", "Soy Carlos Avila", "large",
    //    [{
    //        key: "create",
    //        obj: {
    //            label: "Crear",
    //            className: 'btn-success',
    //            callback: function () {
    //                alert("Crear");
    //                this.openedDialog.modal("hide");
    //                return false;
    //            }
    //        }
    //    }], {
    //        isGuardar: true, hideAlGuardar: true, callbackGuardar: function () {
    //            alert("Aceptar");
    //        }
    //    }, function () {
    //        alert("Cancelar");
    //    });
}

function mostrarLoadingPanel(idElemento, texto)
{
    $('#' + idElemento).waitMe({
        effect: 'roundBounce',
        text: texto,
        bg: 'rgba(255, 255, 255, 0.7)',
        color: '#ef4c0c',
        fontSize: '15px'
    });
}

function mostrarNotificacion(titulo, texto)
{
    var color = "";
    var icon = "";
    if (titulo.indexOf("Informaci") > -1) {
        color = "#3276B1";
        icon = "exclamation-circle";
    }
    else if (titulo == "Error") {
        color = "#C46A69";
        icon = "times-circle";
    }
    else {//Aviso
        color = "#c79121";
        icon = "exclamation-triangle";
    }
    $.sound_on = true;
    $.smallBox({
        title: titulo,
        content: texto,
        color: color,
        icon: "fa fa-" + icon + " shake animated",
        timeout: 6000
    });
}

function AsignarCodigoBarrasPorClase(valor) {
    try {
        JsBarcode(".imgBarCode", valor, {
            format: "CODE128",
            displayValue: true,
            fontSize: 20
        });
    } catch (e) { }
}

function AsignarCodigoBarrasPorId(idElemento, valor) {
    try {
        JsBarcode("#" + idElemento, valor, {
            format: "CODE128",
            displayValue: true,
            fontSize: 20
        });
    } catch (e) { }
}

function obtenerIdAjax(id)
{
    try {
        return parseInt(id);
    } catch (e) {
        return -1;
    }
}

function Gestionar_Msg() {
    var mensaje = $("#span_mensaje").html();
    if (mensaje != "" && mensaje != null) {
        var arr_msg = mensaje.split('|');
        mostrarNotificacion(arr_msg[0], arr_msg[1]);
    }

    var mensajeError = $("#span_error").html();
    if (mensajeError != "" && mensajeError != null)
        mostrarNotificacion("Error", mensajeError);
}

function initLoadingForm()
{
    $(this).on("submit", function (event) {
        if (!$(event.target).hasClass("noFormLoading"))
        {
            $("#btn-guardar").prop("disabled", "disabled");
            $("#btn-guardar").html("<i class='fa fa-spinner fa-spin'></i> " + $("#btn-guardar").html());
        }
    });
}

function agregarCeros(valor, cantidadCeros)
{
    if (valor.length < cantidadCeros)
    {
        var longitud = cantidadCeros - valor.length;
        for (var i = 0; i < longitud; i++)
            valor = "0" + valor;
    }
    return valor;
}

function abrirVentanaConfirmacion(id) {
    var btn_eliminar = $("#" + id);
    $.SmartMessageBox({
        title: btn_eliminar.data("titulo"),
        content: btn_eliminar.data("descripcion"),
        buttons: '[No][Si]'
    }, function (ButtonPressed) {
        if (ButtonPressed === "Si") {
            var dataUrl = btn_eliminar.data("url");
            if (dataUrl != null && dataUrl != "")
                window.location.href = btn_eliminar.data("url");
            else {
                try {
                    eval(btn_eliminar.data("funcioncallback"));
                } catch (e) { }
            }
        }
    });
}

function initDataTableFiltrado(idTabla, arrColumnHidden, fnDrawCallBack, objMostrarOcultarTodos, ocultarVis)
{
    var responsiveHelper_datatable_fixed_column = undefined;
    var breakpointDefinition = {
        tablet: 1024,
        phone: 480
    };

    var otable = $('#' + idTabla).DataTable({
        "lengthMenu": [[10, 25, 50, 100, -1], [10, 25, 50, 100, "Todos"]],
        "sDom": "<'dt-toolbar'<'col-xs-12 col-sm-6'f><'col-sm-6 col-xs-6 hidden-xs'l'C>r>" +
        "t" +
        "<'dt-toolbar-footer'<'col-sm-6 col-xs-12 hidden-xs'i><'col-sm-6 col-xs-12'p>>",
        "autoWidth": true,
        "order": [],
        "preDrawCallback": function () {
            if (!responsiveHelper_datatable_fixed_column)
                responsiveHelper_datatable_fixed_column = new ResponsiveDatatablesHelper($('#' + idTabla), breakpointDefinition);
        },
        "rowCallback": function (nRow) {
            responsiveHelper_datatable_fixed_column.createExpandIcon(nRow);
        },
        "drawCallback": function (oSettings) {
            responsiveHelper_datatable_fixed_column.respond();
            try {
                if (fnDrawCallBack && fnDrawCallBack != null)
                    fnDrawCallBack();
            } catch (e) { }
        }
    });
    
    $("#" + idTabla + " thead th input[type=text]").on('keyup change', function () {
        otable.column($(this).parent().index() + ':visible').search(this.value).draw();
    });

    for (var i = 0; i < arrColumnHidden.length; i++) {
        var columna = otable.column(".th" + arrColumnHidden[i]);
        if (columna && columna != null) {
            columna.visible(false);
        }
    }

    if (objMostrarOcultarTodos && objMostrarOcultarTodos != null)
        ocultarDatosTabla(idTabla, objMostrarOcultarTodos);

    if (ocultarVis && ocultarVis != null)
        ocultarColVis(idTabla);

    try {
        if (mostrarBtnAgrupar && mostrarBtnAgrupar == true) {
            var btn = '<button type="button" class="btn btn-default btnAgruparFilas" data-idtabla="' + idTabla + '" onclick="eventoWndAgrupar(this)" style="float:right !important;margin-right:3px !important;"><i class="fa fa-list"></i> Agrupar por columnas</button>';
            $(btn).insertAfter("#" + idTabla + "_length + div");
        }
    } catch (e) { }
}

function changeDrawDataTableFiltrado(idTabla, fnDrawCallBack)
{
    var table = $('#' + idTabla).DataTable();
    table.off('preDraw');
    table.off('row');
    table.off('draw');

    var responsiveHelper_datatable_fixed_column = undefined;
    var breakpointDefinition = {
        tablet: 1024,
        phone: 480
    };

    table.on('preDraw', function () {
        if (!responsiveHelper_datatable_fixed_column)
            responsiveHelper_datatable_fixed_column = new ResponsiveDatatablesHelper($('#' + idTabla), breakpointDefinition);
    });

    table.on('row', function () {
        responsiveHelper_datatable_fixed_column.createExpandIcon(nRow);
    });

    table.on('draw', function () {
        responsiveHelper_datatable_fixed_column.respond();
        fnDrawCallBack();
    });

    var oTable = $('#' + idTabla).dataTable();
    oTable.fnDraw();
}

function crearGrupo(idTabla, arrIdColumna)
{
    var table = $("#" + idTabla).dataTable();
    var api = table.api();
    var rows = api.rows().nodes();
    var last = null;
    var groupadmin = [];

    var paddingLeft = 0;
    for (var j = 0; j < arrIdColumna.length; j++) {
        api.column("#th" + idTabla + arrIdColumna[j].propiedad).data().each(function (group, i) {
            if (last !== group) {
                $(rows).eq(i).before(
                    '<tr class="group ' + (idTabla + arrIdColumna[j].propiedad) + '" id="grupo' + i + '"><td colspan="' + table.fnSettings().aoColumns.length + '" style="font-weight:bold;">' + "<i class='fa fa-angle-down' style='font-weight:bold;padding-left:" + paddingLeft + "px;'></i> " + arrIdColumna[j].valor + ": " + group + '</td></tr>'
                );
                groupadmin.push(i);
                last = group;
            }
        });
        paddingLeft += 10;
    }
}

function crearGrupoSubtotal(idTabla, columnasGrupoAgrupacion, nameColumnaAgruparSubtotal, isEditar)
{
    var table = $("#" + idTabla).dataTable();
    var api = table.api();
    var rows = api.rows().nodes();
    var last = null;
    var groupadmin = [];

    if (!isEditar)
        isEditar = false;

    if (isEditar == false)
        crearGrupo(idTabla, columnasGrupoAgrupacion);

    var cantidadFilas = api.rows().nodes().length;
    var total = 0;
    if (cantidadFilas > 0) {
        var posInicial = 0;
        var posAnterior = 0;
        do {
            posInicial = obtenerPosArrMismoTextoDatatable(posInicial, idTabla, api, nameColumnaAgruparSubtotal);
            var subtotal = 0;
            var nombreGrupo = "";
            for (var i = posAnterior; i <= posInicial; i++) {
                var idFila = arrIdsFilasSeleccionados[i];
                subtotal += Number($("#hhValorCompra_" + idFila).val());

                if (nombreGrupo == "")
                    nombreGrupo = $("#hhIdClaseActivoFijo_" + idFila).val();
            }
            total += subtotal;
            if (isEditar == false) {
                $(rows).eq(posInicial).after(
                    '<tr class="trSpanSubtotal"><td colspan="' + (table.fnSettings().aoColumns.length - 2) + '" style="background:white !important;">' + '</td>' +
                    '<td colspan="2" style="font-weight:bold;background:#ddd !important;">' + "Subtotal" + ": $" + '<span id="spanSubtotal_' + nombreGrupo + '">' + subtotal.toFixed(2) + '</span>' + '</td></tr>' +
                    '<tr class="trEmtpySpace"><td colspan="' + table.fnSettings().aoColumns.length + '" style="padding-top:20px !important;">' + '</td></tr>'
                );
            }
            else {
                $("#spanSubtotal_" + nombreGrupo).html(subtotal.toFixed(2));
            }
            posInicial++;
            posAnterior = posInicial;
        } while (posInicial < cantidadFilas);
    }
    $("#spanTotal").html(total.toFixed(2));
}

function inicializarArrIdsFilasSeleccionados(nombreTabla) {
    $.each($(".trDetallesRecepcion"), function (index, value) {
        var idFila = $(value).prop("id").replace(nombreTabla, "");
        arrIdsFilasSeleccionados.push(idFila);
    });
}

function obtenerPosArrMismoTextoDatatable(posInicial, idTabla, api, columnaTexto)
{
    var table = $('#' + idTabla).DataTable();
    var cantidadFilas = api.rows().nodes().length;
    var texto = "";
    for (var i = posInicial; i < cantidadFilas; i++) {
        var idFila = $(table.row(i).node()).prop("id").replace(idTabla, "");
        var datoCelda = table.cell("#" + idTabla + idFila + columnaTexto).data();

        if (texto == "") {
            texto = datoCelda;
        }
        else {
            if (datoCelda != texto) {
                return i - 1;
            }
        }
        if (i == (cantidadFilas - 1))
            return i;
    }
    return cantidadFilas;
}

function initTreeView()
{
    $('.tree > ul').attr('role', 'tree').find('ul').attr('role', 'group');
    $('.tree').find('li:has(ul)').addClass('parent_li').attr('role', 'treeitem').find(' > span').attr('title', 'Cerrar').on('click', function (e) {
        var children = $(this).parent('li.parent_li').find(' > ul > li');
        if (children.is(':visible')) {
            children.hide('fast');
            $(this).attr('title', 'Expandir').find(' > i').removeClass().addClass('fa fa-lg fa-filter');
        } else {
            children.show('fast');
            $(this).attr('title', 'Cerrar').find(' > i').removeClass().addClass('fa fa-lg fa-minus-circle');
        }
        e.stopPropagation();
    });
}

function animacionAnclas() {
    $('.ancla_enlace').click(function (e) {
        e.preventDefault();
        irAncla($(this));
    });
}

function irAncla(elemento) {
    try {
        $('html, body').stop().animate({ scrollTop: $(elemento.attr('href')).offset().top }, 1000);
    } catch (e) { }
}