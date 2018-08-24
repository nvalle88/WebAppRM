jQuery(document).ready(function () {
    try {
        var codigoBarras = $("#codigoSecuencial").html();
        AsignarCodigoBarrasPorClase(codigoBarras);
    } catch (e) { }
    initDataTableFiltrado("tableMantenimiento", []);
    initDataTableFiltrado("tableTransferenciasDetalles", []);
    initDataTableFiltrado("tableCustodios", []);
    initDataTableFiltrado("tableProcesosJudiciales", []);
});