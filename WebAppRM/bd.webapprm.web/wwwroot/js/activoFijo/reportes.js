function mostrarOcultar(idRecepcionActivoFijoDetalle) {
    var trHojaVida = $("#trHojaVida_" + idRecepcionActivoFijoDetalle);
    if (trHojaVida.css("display") == "none")
        trHojaVida.show();
    else
        trHojaVida.hide();
}