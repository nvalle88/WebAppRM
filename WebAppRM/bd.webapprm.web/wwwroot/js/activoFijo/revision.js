$(document).ready(function () {
    $.each($(".imgBarCode"), function (index, value) {
        var imgBarCode = $(value);
        AsignarCodigoBarrasPorId("barcode_" + imgBarCode.data("idrecepcionactivofijodetalle"), imgBarCode.data("codigosecuencial"));
    });
});