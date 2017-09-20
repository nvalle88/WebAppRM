$(document).ready(function () {
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
    $('.datepicker').datetimepicker({
        'format': 'D-M-Y hh:mm'
    });

    var wizard = $('.wizard').wizard();
    wizard.on('finished', function (e, data) {
        alert("Finalizar");
        //$("#fuelux-wizard").submit();
        //console.log("submitted!");
        //$.smallBox({
        //    title: "Congratulations! Your form was submitted",
        //    content: "<i class='fa fa-clock-o'></i> <i>1 seconds ago...</i>",
        //    color: "#5F895F",
        //    iconSmall: "fa fa-check bounce animated",
        //    timeout: 4000
        //});
    });
});

function eventoValidacionTecnicaChange()
{
    $("#RecepcionActivoFijo_ValidacionTecnica").on("change", function (e) {
        mostrarOcultarDatosEspecificosCodificacion(e.currentTarget.checked);
    });
}

function mostrarOcultarDatosEspecificosCodificacion(mostrarOcultar)
{
    if (mostrarOcultar)
    {
        $("#btn_next").hide();
        $("#li_codificacion").hide();
    }
    else
    {
        $("#btn_next").show();
        $("#li_codificacion").show();
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
        data: { idPais: idPais != null ? idPais : -1 },
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
        data: { idProvincia: idProvincia != null ? idProvincia : -1 },
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
        data: { idCiudad: idCiudad != null ? idCiudad : -1 },
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
        data: { idSucursal: idSucursal != null ? idSucursal : -1 },
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
        data: { idTipoActivoFijo: idTipoActivoFijo != null ? idTipoActivoFijo : -1 },
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
        data: { idClaseActivoFijo: idClaseActivoFijo != null ? idClaseActivoFijo : -1 },
        success: function (data) {
            $("#div_subClaseActivoFijo").html(data);
            Init_Select2();
        },
        complete: function (data) {
            $("#checkout-form").waitMe("hide");
        }
    });
}