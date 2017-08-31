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