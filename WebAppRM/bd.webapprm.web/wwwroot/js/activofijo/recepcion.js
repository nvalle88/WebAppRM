﻿$(document).ready(function () {
    Init_Select2();
    Init_Touchspin();
    eventoTipoActivoFijo();
    eventoClaseActivoFijo();
    eventoMarca();
    eventoPais();
    eventoProvincia();
    eventoCiudad();
    eventoSucursal();
    eventoValidacionTecnicaChange();
    eventoBtnAtras();
    var wizard = $('.wizard').wizard();
    $('#RecepcionActivoFijo_FechaRecepcion').datetimepicker({
        'format': 'D-M-Y hh:mm'
    });
    mostrarOcultarDatosEspecificosCodificacion($("#RecepcionActivoFijo_ValidacionTecnica").prop('checked'));
    gestionarTab();
});

function gestionarTab()
{
    var valor_tab = parseInt($("#tab").val());
    if (valor_tab === 1) {
        $("#btn_guardar").attr("value", "Siguiente");
        $("#btn_atras").addClass("hide");
    }
    else {
        $("#btn_guardar").attr("value", "Guardar");
        $("#btn_atras").removeClass("hide");
    }
}

function eventoBtnAtras()
{
    $("#btn_atras").on("click", function (e) {
        $("#tab").val(1);

        $("#li_codificacion").removeClass("active");
        $("#li_datosGenerales").addClass("active");

        $("#step2").removeClass("active");
        $("#step1").addClass("active");

        $("#btn_guardar").attr("value", "Siguiente");
        $("#btn_atras").addClass("hide");
    });
}

function eventoValidacionTecnicaChange()
{
    $("#RecepcionActivoFijo_ValidacionTecnica").on("change", function (e) {
        mostrarOcultarDatosEspecificosCodificacion(e.currentTarget.checked);
    });
}

function mostrarOcultarDatosEspecificosCodificacion(mostrarOcultar)
{
    if (mostrarOcultar) {
        $("#li_codificacion").hide();
        $("#btn_guardar").attr("value", "Guardar");
    }
    else
    {
        $("#li_codificacion").show();
        $("#btn_guardar").attr("value", "Siguiente");
    }
}

function eventoTipoActivoFijo() {
    $("#RecepcionActivoFijo_SubClaseActivoFijo_ClaseActivoFijo_IdTipoActivoFijo").on("change", function (e) {
        partialViewTipoActivoFijo(e.val);
    });
}

function eventoClaseActivoFijo() {
    $("#RecepcionActivoFijo_SubClaseActivoFijo_IdClaseActivoFijo").on("change", function (e) {
        partialViewClaseActivoFijo(e.val);
    });
}

function eventoMarca()
{
    $("#ActivoFijo_Modelo_IdMarca").on("change", function (e) {
        mostrarLoadingPanel("checkout-form", "Cargando modelos...");
        $.ajax({
            url: "/ActivoFijo/Modelo_SelectResult",
            method: "POST",
            data: { idMarca: e.val },
            success: function (data) {
                $("#div_modelo").html(data);
                Init_Select2();
            },
            complete: function (data) {
                $("#checkout-form").waitMe("hide");
            }
        });
    });
}

function eventoPais() {
    $("#ActivoFijo_LibroActivoFijo_Sucursal_Ciudad_Provincia_IdPais").on("change", function (e) {
        partialViewProvincia(e.val);
    });
}

function eventoProvincia()
{
    $("#ActivoFijo_LibroActivoFijo_Sucursal_Ciudad_IdProvincia").on("change", function (e) {
        partialViewCiudad(e.val);
    });
}

function eventoCiudad()
{
    $("#ActivoFijo_LibroActivoFijo_Sucursal_IdCiudad").on("change", function (e) {
        partialViewSucursal(e.val);
    });
}

function eventoSucursal() {
    $("#ActivoFijo_LibroActivoFijo_IdSucursal").on("change", function (e) {
        partialViewLibroActivoFijo(e.val);
    });
}

function partialViewProvincia(idPais) {
    mostrarLoadingPanel("checkout-form", "Cargando provincias...");
    
    $.ajax({
        url: "/ActivoFijo/Provincia_SelectResult",
        method: "POST",
        data: { idPais: obtenerIdAjax(idPais) },
        success: function (data) {
            $("#div_provincia").html(data);
            Init_Select2();
        },
        error: function (data) {
            $("#checkout-form").waitMe("hide");
        },
        complete: function (data)
        {
            partialViewCiudad($("#ActivoFijo_LibroActivoFijo_Sucursal_Ciudad_IdProvincia").val());
        }
    });
}

function partialViewCiudad(idProvincia) {
    mostrarLoadingPanel("checkout-form", "Cargando ciudades...");
    $.ajax({
        url: "/ActivoFijo/Ciudad_SelectResult",
        method: "POST",
        data: { idProvincia: obtenerIdAjax(idProvincia) },
        success: function (data) {
            $("#div_ciudad").html(data);
            Init_Select2();
        },
        error: function (data) {
            $("#checkout-form").waitMe("hide");
        },
        complete: function (data) {
            partialViewSucursal($("#ActivoFijo_LibroActivoFijo_Sucursal_IdCiudad").val());
        }
    });
}

function partialViewSucursal(idCiudad) {
    mostrarLoadingPanel("checkout-form", "Cargando sucursales...");
    $.ajax({
        url: "/ActivoFijo/Sucursal_SelectResult",
        method: "POST",
        data: { idCiudad: obtenerIdAjax(idCiudad) },
        success: function (data) {
            $("#div_sucursal").html(data);
            Init_Select2();
        },
        error: function (data) {
            $("#checkout-form").waitMe("hide");
        },
        complete: function (data) {
            partialViewLibroActivoFijo($("#ActivoFijo_LibroActivoFijo_IdSucursal").val());
        }
    });
}

function partialViewLibroActivoFijo(idSucursal) {
    mostrarLoadingPanel("checkout-form", "Cargando libros de activo fijo...");
    $.ajax({
        url: "/ActivoFijo/LibroActivoFijo_SelectResult",
        method: "POST",
        data: { idSucursal: obtenerIdAjax(idSucursal) },
        success: function (data) {
            $("#div_libroActivoFijo").html(data);
            Init_Select2();
        },
        complete: function (data) {
            $("#checkout-form").waitMe("hide");
            eventoProvincia();
            eventoCiudad();
            eventoSucursal();
        }
    });
}

function partialViewTipoActivoFijo(idTipoActivoFijo) {
    mostrarLoadingPanel("checkout-form", "Cargando clases de activo fijo...");
    $.ajax({
        url: "/ActivoFijo/ClaseActivoFijo_SelectResult",
        method: "POST",
        data: { idTipoActivoFijo: obtenerIdAjax(idTipoActivoFijo) },
        success: function (data) {
            $("#div_claseActivoFijo").html(data);
            Init_Select2();
        },
        error: function (data) {
            $("#checkout-form").waitMe("hide");
        },
        complete: function (data)
        {
            partialViewClaseActivoFijo($("#RecepcionActivoFijo_SubClaseActivoFijo_IdClaseActivoFijo").val());
            eventoClaseActivoFijo();
        }
    });
}

function partialViewClaseActivoFijo(idClaseActivoFijo) {
    mostrarLoadingPanel("checkout-form", "Cargando sub clases de activo fijo...");
    $.ajax({
        url: "/ActivoFijo/SubClaseActivoFijo_SelectResult",
        method: "POST",
        data: { idClaseActivoFijo: obtenerIdAjax(idClaseActivoFijo) },
        success: function (data) {
            $("#div_subClaseActivoFijo").html(data);
            Init_Select2();
        },
        complete: function (data) {
            $("#checkout-form").waitMe("hide");
        }
    });
}