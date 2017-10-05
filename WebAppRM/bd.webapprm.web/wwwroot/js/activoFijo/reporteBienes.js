jQuery(document).ready(function () {
    var table = $('#tbReporte').DataTable({
        "columnDefs": [
            { "visible": false, "targets": [0, 1, 2, 3, 4] }
        ],
        "order": [[0, 'asc'], [1, 'asc'], [2, 'asc'], [3, 'asc'], [4, 'asc']],
        "displayLength": 25,
        "drawCallback": function (settings) {
            var api = this.api();
            var rows = api.rows({ page: 'current' }).nodes();
            var last = null;
            var groupadmin = [];

            crearGrupo(api, rows, last, groupadmin, 0, "Pa&iacute;s", 0);
            crearGrupo(api, rows, last, groupadmin, 1, "Provincia", 6);
            crearGrupo(api, rows, last, groupadmin, 2, "Ciudad", 12);
            crearGrupo(api, rows, last, groupadmin, 3, "Sucursal", 18);
            crearGrupo(api, rows, last, groupadmin, 4, "Funcionario", 24);
        }
    });

    var tableTools = new $.fn.dataTable.TableTools(table, {
        "aButtons": ["copy", "csv", "xls", "pdf", "print"],
        'sSwfPath': '/js/plugin/datatables/swf/copy_csv_xls_pdf.swf'
    });
    $(tableTools.fnContainer()).insertBefore('#tbReporte');
});

function crearGrupo(api, rows, last, groupadmin, idColumna, textoLugar, paddingLeft) {
    api.column(idColumna, { page: 'current' }).data().each(function (group, i) {
        if (last !== group) {
            $(rows).eq(i).before(
                '<tr class="group" id="' + i + '"><td colspan="16" style="font-weight:bold;">' + "<i class='fa fa-angle-down' style='font-weight:bold;padding-left:" + paddingLeft + "px;'></i> " + textoLugar + ": " + group + '</td ></tr > '
            );
            groupadmin.push(i);
            last = group;
        }
    });
}