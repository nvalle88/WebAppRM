function generarNumeroConsecutivo() {
    var numeroConsecutivo = $("#CodigoActivoFijo_Consecutivo").val();
    $("#spanNumeroConsecutivo").html(numeroConsecutivo);
}

function eventoSpinnerNumeroConsecutivo() {
    $(".spinnerNumeroConsecutivo").spinner('delay', 200).spinner('changed', function (e, newVal, oldVal) {
        asignarNumeroConsecutivoCodigoBarras();
    });
}

function asignarCodigoBarras()
{
    try {
        var codigoBarras = $("#spanCodigoSecuencial").html() + $("#spanNumeroConsecutivo").html();
        if (codigoBarras != "") {
            AsignarCodigoBarrasPorClase(codigoBarras);
            document.getElementById("barcode1").style.display = "";
        }
        else
            document.getElementById("barcode1").style.display = "none";
    } catch (e) { }
}

function asignarNumeroConsecutivoCodigoBarras()
{
    generarNumeroConsecutivo();
    asignarCodigoBarras();
}