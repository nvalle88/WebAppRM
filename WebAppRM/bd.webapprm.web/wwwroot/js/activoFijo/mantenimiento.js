$(document).ready(function () {
    Init_Select2();
    Init_Touchspin();

    $('#FechaMantenimiento').datetimepicker({
        'format': 'D-M-Y'
    });
    $('#FechaDesde').datetimepicker({
        'format': 'D-M-Y'
    });
    $('#FechaHasta').datetimepicker({
        'format': 'D-M-Y'
    });
});