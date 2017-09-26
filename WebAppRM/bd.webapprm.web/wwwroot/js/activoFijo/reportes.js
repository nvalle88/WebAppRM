function mostrarOcultar(idRecepcionActivoFijoDetalle) {
    var trHojaVida = $("#trHojaVida_" + idRecepcionActivoFijoDetalle);
    var i = $("#i_" + idRecepcionActivoFijoDetalle);

    if (trHojaVida.css("display") == "none") {
        trHojaVida.show();
        i.removeClass("fa fa-plus").addClass("fa fa-minus");
        i.attr("title", "Ocultar Hoja de Vida");
    }
    else
    {
        trHojaVida.hide();
        i.removeClass("fa fa-minus").addClass("fa fa-plus");
        i.attr("title", "Mostrar Hoja de Vida");
    }
}