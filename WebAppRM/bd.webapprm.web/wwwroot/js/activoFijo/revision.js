$(document).ready(function () {
    $.each($(".imgBarCode"), function (index, value) {
        var imgBarCode = $(value);
        Asignar_Codigo_Barras("barcode_" + imgBarCode.data("idrecepcionactivofijodetalle"), imgBarCode.data("codigosecuencial"));
    });
});