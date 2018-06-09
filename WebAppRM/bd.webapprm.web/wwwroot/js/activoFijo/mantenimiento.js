$(document).ready(function () {
    Init_Select2();
    Init_Touchspin();
    Init_DatetimePicker("FechaMantenimiento", true, true);
    Init_DatetimePicker("FechaDesde", true, true);
    Init_DatetimePicker("FechaHasta", false, true);
});