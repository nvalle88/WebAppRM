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

function Init_Select2()
{
    $('select').select2({
        theme: 'classic',
        allowClear: false,
        placeholder: 'Seleccione...',
        language: 'es'
    });
}

function Init_Date(idElemento)
{
    $('#' + idElemento).datetimepicker({
        'format': 'D-M-Y'
    });
}

function Init_DatetimePicker(idElemento)
{
    $('#' + idElemento).datetimepicker({
        'format': 'D-M-Y hh:mm A'
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
    switch (titulo)
    {
        case "Información": color = "#3276B1"; icon = "exclamation-circle"; break;
        case "Error": color = "#C46A69"; icon = "times-circle"; break;
        case "Aviso": color = "#c79121"; icon = "exclamation-triangle"; break;
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

function initDataTableFiltrado(idTabla, arrColumnHidden, fnDrawCallBack)
{
    var responsiveHelper_datatable_fixed_column = undefined;

    var breakpointDefinition = {
        tablet: 1024,
        phone: 480
    };

    var otable = $('#' + idTabla).DataTable({
        "columnDefs": [
            { "visible": false, "targets": arrColumnHidden }
        ],
        "lengthMenu": [[10, 25, 50, 100, -1], [10, 25, 50, 100, "Todos"]],
        "sDom": "<'dt-toolbar'<'col-xs-12 col-sm-6'f><'col-sm-6 col-xs-6 hidden-xs'l'C>r>" +
        "t" +
        "<'dt-toolbar-footer'<'col-sm-6 col-xs-12 hidden-xs'i><'col-sm-6 col-xs-12'p>>",
        "autoWidth": true,
        "preDrawCallback": function () {
            if (!responsiveHelper_datatable_fixed_column)
                responsiveHelper_datatable_fixed_column = new ResponsiveDatatablesHelper($('#' + idTabla), breakpointDefinition);
        },
        "rowCallback": function (nRow) {
            responsiveHelper_datatable_fixed_column.createExpandIcon(nRow);
        },
        "drawCallback": function (oSettings) {
            responsiveHelper_datatable_fixed_column.respond();
            try { fnDrawCallBack(); } catch (e) { }
        }
    });
    
    $("#" + idTabla + " thead th input[type=text]").on('keyup change', function () {
        otable.column($(this).parent().index() + ':visible').search(this.value).draw();
    });

    for (var i = 0; i < arrColumnHidden.length; i++) {
        otable.column(arrColumnHidden[i]).visible(false);
    }
}

function crearGrupo(api, rows, last, groupadmin, idColumna, textoLugar, paddingLeft, colspan) {
    api.column(idColumna, { page: 'current' }).data().each(function (group, i) {
        if (last !== group) {
            $(rows).eq(i).before(
                '<tr class="group" id="' + i + '"><td colspan="' + colspan + '" style="font-weight:bold;">' + "<i class='fa fa-angle-down' style='font-weight:bold;padding-left:" + paddingLeft + "px;'></i> " + textoLugar + ": " + group + '</td ></tr > '
            );
            groupadmin.push(i);
            last = group;
        }
    });
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