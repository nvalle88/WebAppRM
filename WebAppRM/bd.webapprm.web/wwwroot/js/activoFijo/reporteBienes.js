jQuery(document).ready(function () {
    Init_Select2();
    eventoRadioDatosEspecificos();
    eventoBodega();
    eventoEmpleado();
    partialViewDetallesActivosFijos();
});

function eventoRadioDatosEspecificos() {
    $(".radioDatosEspecificos").on("change", function (e) {
        var ubicacion = $(e.currentTarget).data("ubicacion");
        switch (ubicacion) {
            case "Bodega": {
                $("#divDatosEspecificosEmpleado").addClass("hide");
                $("#divDatosEspecificosBodega").removeClass("hide");
                break;
            }
            default: {
                $("#divDatosEspecificosBodega").addClass("hide");
                $("#divDatosEspecificosEmpleado").removeClass("hide");
                break;
            }
        }
        partialViewDetallesActivosFijos();
    });
}

function eventoBodega() {
    $("#IdBodega").on("change", function (e) {
        partialViewDetallesActivosFijos();
    });
}

function eventoEmpleado() {
    $("#IdEmpleado").on("change", function (e) {
        partialViewDetallesActivosFijos();
    });
}

function partialViewDetallesActivosFijos() {
    mostrarLoadingPanel("content", "Cargando datos...");
    var isBodega = $("#radioBodegaDatosEspecificos").prop("checked");
    var idBodegaEmpleado = isBodega == true ? $("#IdBodega").val() : $("#IdEmpleado").val();

    $("#isBodega").val(isBodega);
    $("#idBodegaEmpleado").val(idBodegaEmpleado);

    $.ajax({
        url: urlListadoActivosFijosBodegaEmpleadoResult,
        method: "POST",
        data: { isBodega: isBodega, idBodegaEmpleado: idBodegaEmpleado },
        success: function (data) {
            $("#divDetallesActivosFijos").html(data);
        },
        error: function (errorMessage) {
            mostrarNotificacion("Error", "Ocurrió un error al cargar los datos, inténtelo nuevamente.");
        },
        complete: function (data) {
            arrAgrupacionColumnas = [];
            var arrAgrupacion = isBodega == true ? [{ propiedad: thClassName.bodega, valor: "Bodega" }] : [{ propiedad: thClassName.empleado, valor: "Custodio" }];
            gestionarArrAgrupacionColumnas("tableDetallesActivoFijoBajas", arrAgrupacion[0].propiedad, true);
            initDataTableFiltrado("tableDetallesActivoFijoBajas", [], function () {
                crearGrupo("tableDetallesActivoFijoBajas", arrAgrupacion);
            });
            $("#content").waitMe("hide");
        }
    });
}