$(document).ready(function () {
    Init_Select2();
    eventoTipoArticulo();
    eventoClaseArticulo();
    eventoSubClaseArticulo();
    eventoPais();
    eventoProvincia();
    eventoCiudad();
    eventoSucursal();
    $('#Fecha').datetimepicker({
        'format': 'D-M-Y hh:mm'
    });

    function eventoTipoArticulo() {
        $("#Articulo_SubClaseArticulo_ClaseArticulo_IdTipoArticulo").on("change", function (e) {
            partialViewTipoArticulo(e.val);
        });
    }

    function eventoClaseArticulo() {
        $("#Articulo_SubClaseArticulo_IdClaseArticulo").on("change", function (e) {
            partialViewClaseArticulo(e.val);
        });
    }

    function eventoSubClaseArticulo() {
        $("#Articulo_IdSubClaseArticulo").on("change", function (e) {
            partialViewSubClaseArticulo(e.val);
        });
    }

    function eventoPais() {
        $("#MaestroArticuloSucursal_Sucursal_Ciudad_Provincia_IdPais").on("change", function (e) {
            partialViewProvincia(e.val);
        });
    }

    function eventoProvincia() {
        $("#MaestroArticuloSucursal_Sucursal_Ciudad_IdProvincia").on("change", function (e) {
            partialViewCiudad(e.val);
        });
    }

    function eventoCiudad() {
        $("#MaestroArticuloSucursal_Sucursal_IdCiudad").on("change", function (e) {
            partialViewSucursal(e.val);
        });
    }

    function eventoSucursal() {
        $("#MaestroArticuloSucursal_IdSucursal").on("change", function (e) {
            partialViewMaestroArticuloSucursal(e.val);
        });
    }

    function partialViewProvincia(idPais) {
        mostrarLoadingPanel("checkout-form", "Cargando provincias...");
        $.ajax({
            url: provinciaSelectResult,
            method: "POST",
            data: { idPais: idPais != null ? idPais : -1 },
            success: function (data) {
                $("#div_provincia").html(data);
                Init_Select2();
            },
            error: function (data) {
                $("#checkout-form").waitMe("hide");
            },
            complete: function (data) {
                eventoProvincia();
                partialViewCiudad($("#MaestroArticuloSucursal_Sucursal_Ciudad_IdProvincia").val());
            }
        });
    }

    function partialViewCiudad(idProvincia) {
        mostrarLoadingPanel("checkout-form", "Cargando ciudades...");
        $.ajax({
            url: ciudadSelectResult,
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
                eventoCiudad();
                partialViewSucursal($("#MaestroArticuloSucursal_Sucursal_IdCiudad").val());
            }
        });
    }

    function partialViewSucursal(idCiudad) {
        mostrarLoadingPanel("checkout-form", "Cargando sucursales...");
        $.ajax({
            url: sucursalSelectResult,
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
                eventoSucursal();
                partialViewMaestroArticuloSucursal($("#MaestroArticuloSucursal_IdSucursal").val());
            }
        });
    }

    function partialViewMaestroArticuloSucursal(idSucursal) {
        mostrarLoadingPanel("checkout-form", "Cargando sucursales...");
        $.ajax({
            url: maestroArticuloSucursal,
            method: "POST",
            data: { idSucursal: idSucursal != null ? idSucursal : -1 },
            success: function (data) {
                $("#div_maestroArticuloSucursal").html(data);
                Init_Select2();
            },
            complete: function (data) {
                $("#checkout-form").waitMe("hide");
            }
        });
    }

    function partialViewTipoArticulo(idTipoArticulo) {
        mostrarLoadingPanel("checkout-form", "Cargando clases de art&iacute;culos...");
        $.ajax({
            url: claseArticuloSucursal,
            method: "POST",
            data: { idTipoArticulo: idTipoArticulo != null ? idTipoArticulo : -1 },
            success: function (data) {
                $("#div_claseArticulo").html(data);
                Init_Select2();
            },
            error: function (data) {
                $("#checkout-form").waitMe("hide");
            },
            complete: function (data) {
                eventoClaseArticulo();
                partialViewClaseArticulo($("#Articulo_SubClaseArticulo_IdClaseArticulo").val());
            }
        });
    }

    function partialViewClaseArticulo(idClaseArticulo) {
        mostrarLoadingPanel("checkout-form", "Cargando subclases de art&iacute;culos...");
        $.ajax({
            url: subClaseArticuloSucursal,
            method: "POST",
            data: { idClaseArticulo: idClaseArticulo != null ? idClaseArticulo : -1 },
            success: function (data) {
                $("#div_subClaseArticulo").html(data);
                Init_Select2();
            },
            complete: function (data) {
                eventoSubClaseArticulo();
                partialViewSubClaseArticulo($("#Articulo_IdSubClaseArticulo").val());
            }
        });
    }

    function partialViewSubClaseArticulo(idSubClaseArticulo) {
        mostrarLoadingPanel("checkout-form", "Cargando art&iacute;culos...");
        $.ajax({
            url: articuloSucursal,
            method: "POST",
            data: { idSubClaseArticulo: idSubClaseArticulo != null ? idSubClaseArticulo : -1 },
            success: function (data) {
                $("#div_Articulo").html(data);
                Init_Select2();
            },
            complete: function (data) {
                $("#checkout-form").waitMe("hide");
            }
        });
    }
});