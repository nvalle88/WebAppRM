$(document).ready(function () {
    asignarCodigoBarras();

    $("#ActivoFijo_CodigoActivoFijo_CodigoBarras").bind('keydown keyup', function (event) {
        asignarCodigoBarras();
    });
});

function asignarCodigoBarras()
{
    try {
        var codigoBarras = document.getElementById("ActivoFijo_CodigoActivoFijo_CodigoBarras").value;
        if (codigoBarras != "") {
            Asignar_Codigo_Barras(codigoBarras);
            document.getElementById("barcode1").style.display = "";
        }
        else
            document.getElementById("barcode1").style.display = "none";
    } catch (e) { }
}