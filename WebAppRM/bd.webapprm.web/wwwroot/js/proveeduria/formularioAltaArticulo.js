jQuery(document).ready(function () {
    Init_Select2();
    var ID = '@Model.IdRecepcionArticulos';
    $.ajax({
        url: urlCargarTablaFacturasExcluidas,
        data: { ID: ID },
        success: function (data) {
            $('#facturasExluidas').html(data);
        }
    });

    $.ajax({
        url: urlCargarTablaFacturasIncluidas,
        data: { ID: ID },
        success: function (data) {
            $('#facturasIncluidas').html(data);
        }
    });

    $("#acreditacion").on("change", function (e) {
        var valor = $("#acreditacion").val();
        if (valor == "Facturas") {
            $("#divDocumentos").addClass("hide");
            $("#camposFactura").removeClass("hide");
        }
        else {
            $("#camposFactura").addClass("hide");
            $("#divDocumentos").removeClass("hide");
        }
    });
});

function IncluirFacturas(idFactura) {
    var idFact = idFactura;
    $.ajax({
        url: urlRefrescarTablaExcluidos,
        data: { idFactura: idFact },
        success: function (data) {
            $('#facturasExluidas').html(data);
        }
    });

    $.ajax({
        url: urlIncluirFacturasEnAlta,
        data: { idFactura: idFact },
        success: function (data) {
            $('#facturasIncluidas').html(data);
        }
    });
};

function ExcluirFacturas(idFactura) {
    var idFact = idFactura;

    $.ajax({
        url: urlRefrescarTablaIncluidos,
        data: { idFactura: idFact },
        success: function (data) {
            $('#facturasIncluidas').html(data);
        }
    });

    $.ajax({
        url: urlExcluirFacturasEnAlta,
        data: { idFactura: idFact },
        success: function (data) {
            $('#facturasExluidas').html(data);
        }
    });
};