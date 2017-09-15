$(document).ready(function () {
    Init_Select2();
    Init_Touchspin();
    eventoTipoActivoFijo();
    eventoClaseActivoFijo();
    eventoMarca();

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

function eventoTipoActivoFijo() {
    $("#SubClaseActivoFijo_ClaseActivoFijo_TipoActivoFijo_IdTipoActivoFijo").on("change", function (e) {
        partialViewTipoActivoFijo(e.val);
    });
}

function eventoClaseActivoFijo() {
    $("#IdClaseActivoFijo").on("change", function (e) {
        partialViewClaseActivoFijo(e.val);
    });
}

function eventoMarca()
{
    $("#Modelo_Marca_IdMarca").on("change", function (e) {
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

function partialViewTipoActivoFijo(idTipoActivoFijo) {
    mostrarLoadingPanel("checkout-form", "Cargando clases de activo fijo...");
    $.ajax({
        url: "/ActivoFijo/ClaseActivoFijo_SelectResult",
        method: "POST",
        data: { idTipoActivoFijo: idTipoActivoFijo != null ? idTipoActivoFijo : -1 },
        success: function (data) {
            $("#div_claseActivoFijo").html(data);
            Init_Select2();
            partialViewClaseActivoFijo($("#IdClaseActivoFijo").val());
            eventoClaseActivoFijo();
        },
        error: function (data) {
            $("#checkout-form").waitMe("hide");
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