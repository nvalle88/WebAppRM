jQuery(document).ready(function () {
    var table = $('#tbReporteHojaVidaActivo').DataTable({ "displayLength": 25});

    var tableTools = new $.fn.dataTable.TableTools(table, {
        "aButtons": ["copy", "csv", "xls", "pdf", "print"],
        'sSwfPath': '/js/plugin/datatables/swf/copy_csv_xls_pdf.swf'
    });
    $(tableTools.fnContainer()).insertBefore('#tbReporteHojaVidaActivo');
});