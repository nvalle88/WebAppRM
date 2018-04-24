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
    $.smallBox({
        title: titulo,
        content: texto,
        color: color,
        icon: "fa fa-" + icon + " shake animated",
        timeout: 6000
    });
}

function Asignar_Codigo_Barras(idElemento, valor) {
    JsBarcode("#" + idElemento, valor, {
        format: "CODE128",
        displayValue: true,
        fontSize: 20
    });
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