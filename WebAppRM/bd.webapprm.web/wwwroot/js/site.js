function Init_Touchspin()
{
    $('.touchspin_tasa').TouchSpin({
        min: 0,
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
        text: texto != null ? texto : 'Procesando datos, por favor espere...',
        bg: 'rgba(255, 255, 255, 0.7)',
        color: '#ef4c0c'
    });
}