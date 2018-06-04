$(document).ready(function () {
    Init_Select2();
    Init_DatetimePicker("FechaSalida");
    Init_DatetimePicker("FechaRetorno");
    inicializarDetallesActivoSeleccion();
    inicializarObjetoAdicional();
    initDataTableFiltrado("tableDetallesActivoFijoSeleccionados", [13, 15, 16, 17, 18, 19, 20, 21, 22]);
});

function callBackInicializarTableListadoSeleccion() {
    initDataTableFiltrado("tableDetallesActivoFijoBajas", []);
}