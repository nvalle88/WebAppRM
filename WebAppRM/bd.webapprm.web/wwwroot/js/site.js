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
        'format': 'D-M-Y hh:mm'
    });
}

function Init_FileInput(idElemento)
{
    $("#" + idElemento).fileinput({
        showUpload: false,
        language: 'es'
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

function Asignar_Codigo_Barras(valor) {
    try {
        JsBarcode(".imgBarCode", valor, {
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
                    callBackFunctionEliminar(id);
                } catch (e) { }
            }
        }
    });
}

function initDataTableFiltrado(idTabla, arrColumnHidden)
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
        }
    });
    
    $("#" + idTabla + " thead th input[type=text]").on('keyup change', function () {
        otable.column($(this).parent().index() + ':visible').search(this.value).draw();
    });

    for (var i = 0; i < arrColumnHidden.length; i++) {
        otable.column(arrColumnHidden[i]).visible(false);
    }
}

function abrirVentanaDetallesActivoFijo(id)
{
    mostrarLoadingPanel("modalContentTableActivosFijos", "Cargando detalles de activo fijo...");
    var btn_detalles = $("#btnDetallesActivoFijo_" + id);
    var arrIdsRecepcionActivoFijoDetalle = btn_detalles.data("ids").toString().split(",");
    $.ajax({
        url: btn_detalles.data("url"),
        method: "POST",
        data: { idsRecepcionActivoFijoDetalle: arrIdsRecepcionActivoFijoDetalle },
        success: function (data) {
            $("#modalBodyTableActivosFijos").html(data);
        },
        complete: function (data) {
            $("#modalContentTableActivosFijos").waitMe("hide");
        }
    });
}