var arrRecepcionActivoFijoDetalleSeleccionado = [];
var arrRecepcionActivoFijoDetalleTodos = [];
var objAdicional = [];
var mostrarNoSeleccionados = false;
var isPrimeraSeleccion = true;
var cantCheckSeleccionados = 0;
var isComponentes = false;

jQuery(document).ready(function (e) {
    initDataTableFiltrado("tableActivosFijos", []);
});

function inicializarDetallesActivoSeleccion() {
    $.each($(".hiddenIdRecepcionActivoFijoDetalle"), function (index, value) {
        var ids = $(value);
        var arrIds = ids.val().toString().split(",");
        $.each(arrIds, function (index, value) {
            adicionarRecepcionActivoFijoDetalleSeleccionado(value, true);
        });
    });
}

function inicializarIdsArrRecepcionActivoFijoDetalleTodos() {
    arrRecepcionActivoFijoDetalleTodos = [];
    var idsArrRecepcionActivoFijoDetalleTodosAux = $("#hIdsRecepcionActivoFijoDetalleTodos").val();
    if (idsArrRecepcionActivoFijoDetalleTodosAux != null && idsArrRecepcionActivoFijoDetalleTodosAux != "") {
        var idsArrRecepcionActivoFijoDetalleTodos = idsArrRecepcionActivoFijoDetalleTodosAux.split(",");
        for (var i = 0; i < idsArrRecepcionActivoFijoDetalleTodos.length; i++) {
            arrRecepcionActivoFijoDetalleTodos.push(idsArrRecepcionActivoFijoDetalleTodos[i]);
        }
    }
}

function obtenerPosArrRecepcionActivoFijoDetalleTodos(valor)
{
    for (var i = 0; i < arrRecepcionActivoFijoDetalleTodos.length; i++) {
        if (arrRecepcionActivoFijoDetalleTodos[i] == valor)
            return i;
    }
    return -1;
}

function inicializarObjetoAdicional()
{
    $.each(arrRecepcionActivoFijoDetalleSeleccionado, function (index, value) {
        objAdicional.push({
            idRecepcionActivoFijoDetalle: value.idRecepcionActivoFijoDetalle,
            seleccionado: value.seleccionado
        });
    });
}

function adicionarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle, seleccionado)
{
    arrRecepcionActivoFijoDetalleSeleccionado.push({
        idRecepcionActivoFijoDetalle: idRecepcionActivoFijoDetalle,
        seleccionado: seleccionado
    });
}

function abrirVentanaDetallesActivoFijo(id) {
    mostrarLoadingPanel("checkout-form", "Cargando detalles, por favor espere...");
    var btn_detalles = $("#btnDetallesActivoFijo_" + id);
    var arrIds = btn_detalles.data("ids").toString().split(",");

    var arrAux = Array();
    for (var i = 0; i < arrIds.length; i++) {
        var arrIdsSeleccionados = arrIds[i].split("_");
        arrAux.push({
            idRecepcionActivoFijoDetalle: parseInt(arrIdsSeleccionados[0]),
            seleccionado: arrIdsSeleccionados[1].toLowerCase() === "true"
        });
    }

    $.ajax({
        url: btn_detalles.data("url"),
        method: "POST",
        data: { listadoRecepcionActivoFijoDetalleSeleccionado: arrAux, arrConfiguraciones: arrConfiguraciones, mostrarNoSeleccionados: mostrarNoSeleccionados, objAdicional: id },
        success: function (data) {
            Init_BootBox("Listado de Activos Fijos", data, "large", null);
        },
        complete: function (data) {
            initDataTableFiltrado("tableDetallesActivoFijo", []);
            tryMarcarCheckBoxTodos();
            $("#tableDetallesActivoFijo_filter > label > span").prop("style", "height:32px !important;");
            ajustarBootboxPorCiento(80);
            $("#checkout-form").waitMe("hide");
        }
    });
}

function cargarListadoActivosFijosParaSeleccion(objeto, texto) {
    mostrarLoadingPanel("checkout-form", "Cargando listado, por favor espere...");
    $.ajax({
        url: urlListadoActivosFijosSeleccionResult,
        method: "POST",
        data: { listadoRecepcionActivoFijoDetalleSeleccionado: arrRecepcionActivoFijoDetalleSeleccionado, objAdicional: objAdicional },
        success: function (data) {
            if (!texto)
                texto = "Activos Fijos";
            Init_BootBox("Listado de " + texto, data, "large", null);
            try {
                var nombreFuncionCallback = $(objeto).data("funcioncallback");
                eval(nombreFuncionCallback + "()");
            } catch (e) { }
        },
        error: function (errorMessage) {
            mostrarNotificacion("Error", "Ocurrió un error al cargar el formulario.");
        },
        complete: function (data) {
            tryMarcarCheckBoxTodos();
            $("#tableDetallesActivoFijoAltas_filter > label > span").prop("style", "height:32px !important;");
            $("#tableDetallesActivoFijoBajas_filter > label > span").prop("style", "height:32px !important;");
            $("#tableDetallesArticulos_filter > label > span").prop("style", "height:32px !important;");
            ajustarBootboxPorCiento(80);
            $("#checkout-form").waitMe("hide");
        }
    });
}

function obtenerRecepcionActivoFijoDetalleSeleccionado(valor) {
    for (var i = 0; i < arrRecepcionActivoFijoDetalleSeleccionado.length; i++) {
        if (arrRecepcionActivoFijoDetalleSeleccionado[i].idRecepcionActivoFijoDetalle == valor)
            return arrRecepcionActivoFijoDetalleSeleccionado[i];
    }
    return null;
}

function obtenerListadoRecepcionActivoFijoDetalleSeleccionado()
{
    var listaRecepcionActivoFijoDetalleSeleccionado = [];
    for (var i = 0; i < arrRecepcionActivoFijoDetalleSeleccionado.length; i++) {
        if (arrRecepcionActivoFijoDetalleSeleccionado[i].seleccionado)
            listaRecepcionActivoFijoDetalleSeleccionado.push(arrRecepcionActivoFijoDetalleSeleccionado[i]);
    }
    return listaRecepcionActivoFijoDetalleSeleccionado;
}

function eliminarRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle)
{
    for (var i = 0; i < arrRecepcionActivoFijoDetalleSeleccionado.length; i++) {
        if (arrRecepcionActivoFijoDetalleSeleccionado[i].idRecepcionActivoFijoDetalle == idRecepcionActivoFijoDetalle)
        {
            arrRecepcionActivoFijoDetalleSeleccionado.splice(i, 1);
            return;
        }
    }
}

function eventoCheckboxDetallesActivoFijo(checkbox)
{
    var chk = $(checkbox);
    var dataIsSeleccionTodos = chk.data("isselecciontodos");
    if (!dataIsSeleccionTodos)
        dataIsSeleccionTodos = "false";

    var isSeleccionTodos = dataIsSeleccionTodos.toLowerCase() === 'true';
    if (isSeleccionTodos)
        callBackFunctionSeleccionTodos(chk);
    else {
        var seleccionado = chk.prop("checked");
        var idRecepcionActivoFijoDetalle = chk.data("idrecepcionactivofijodetalle");
        if (!isComponentes) {
            try {
                if (idRecepcionActivoFijoDetalle) {
                    var rafdSeleccionado = obtenerRecepcionActivoFijoDetalleSeleccionado(idRecepcionActivoFijoDetalle);
                    rafdSeleccionado.seleccionado = seleccionado;
                }
            } catch (e) { }
        }
        try {
            cantCheckSeleccionados--;
            var nombreFuncionCallback = chk.data("funcioncallback");
            if (nombreFuncionCallback)
                eval(nombreFuncionCallback + "(" + idRecepcionActivoFijoDetalle + "," + seleccionado + ")");
        } catch (e) { }

        if (isComponentes)
        {
            if (idRecepcionActivoFijoDetalle) {
                if (seleccionado)
                    cantComponentesSeleccionados++;
                else
                    cantComponentesSeleccionados--;
            }
            tryMarcarCheckBoxTodosComponentes();
        }
        else
            tryMarcarCheckBoxTodos();
    }
}

function callBackFunctionSeleccionTodos(chk)
{
    var isSeleccionado = chk.prop("checked");
    if (isSeleccionado) {
        var arrIds = [];
        for (var i = 0; i < arrRecepcionActivoFijoDetalleTodos.length; i++) {
            if (isComponentes) {
                if (!existeIdComponente(arrRecepcionActivoFijoDetalleTodos[i]))
                    arrIds.push(arrRecepcionActivoFijoDetalleTodos[i]);
            }
            else {
                var rafd = obtenerRecepcionActivoFijoDetalleSeleccionado(arrRecepcionActivoFijoDetalleTodos[i]);
                if (rafd == null || !rafd.seleccionado)
                    arrIds.push(arrRecepcionActivoFijoDetalleTodos[i]);
            }
        }
        cantCheckSeleccionados = arrIds.length;
        for (var i = 0; i < arrIds.length; i++) {
            $("#chkDetalleActivoFijo_" + arrIds[i]).click();
            isPrimeraSeleccion = false;
        }
    }
    else {
        for (var i = 0; i < arrRecepcionActivoFijoDetalleTodos.length; i++) {
            $("#chkDetalleActivoFijo_" + arrRecepcionActivoFijoDetalleTodos[i]).click();
        }
    }
    isPrimeraSeleccion = true;
}

function tryMarcarCheckBoxTodos()
{
    var listadoRecepcionActivoFijoDetalleSeleccionado = obtenerListadoRecepcionActivoFijoDetalleSeleccionado();
    if (listadoRecepcionActivoFijoDetalleSeleccionado.length == arrRecepcionActivoFijoDetalleTodos.length)
        $("#chkDetalleActivoFijo_0").prop("checked", "checked");
    else
        $("#chkDetalleActivoFijo_0").prop("checked", "");
}

function obtenerArrColumnasVisible(idTable)
{
    var table = $('#' + idTable).DataTable();
    var iColumns = $('#' + idTable).dataTable().fnSettings().aoColumns.length;
    var arrColumnas = [];
    for (var i = 0; i < iColumns; i++) {
        arrColumnas.push(i);
    }
    var arrColumnasVisible = table.columns(arrColumnas).visible();
    return arrColumnasVisible;
}

function mostrarOcultarColumnas(idTable, arrObj) {
    var otable = $('#' + idTable).dataTable();
    for (var i = 0; i < arrObj.length; i++) {
        otable.fnSetColumnVis(i, arrObj[i]);
    }
}

function ocultarColumnasPorArray(idTable, arrObj) {
    var otable = $('#' + idTable).dataTable();
    for (var i = 0; i < arrObj.length; i++) {
        if (arrObj[i] == false)
            otable.fnSetColumnVis(i, false);
    }
}

function mostrarOcultarColumnasPorArray(idTable, mostrarOcultar)
{
    var table = $('#' + idTable).DataTable();
    var iColumns = $('#' + idTable).dataTable().fnSettings().aoColumns.length;
    for (var i = 0; i < iColumns; i++) {
        table.column(i).visible(mostrarOcultar);
    }
}

function obtenerArrValores(idTableCopiando, idRecepcionActivoFijoDetalle, arrCeldasCopiando, isOpcionesUltimaColumna)
{
    var arrValores = [];
    for (var i = 0; i < arrCeldasCopiando.length; i++) {
        if (i == (arrCeldasCopiando.length - 1)) {
            if (isOpcionesUltimaColumna) {
                arrValores.push(arrCeldasCopiando[i]);
            }
            else
                arrValores.push($("#" + idTableCopiando + idRecepcionActivoFijoDetalle + arrCeldasCopiando[i]).html());
        }
        else
            arrValores.push($("#" + idTableCopiando + idRecepcionActivoFijoDetalle + arrCeldasCopiando[i]).html());
    }
    return arrValores;
}

function addRowDetallesActivosFijos(idTableACopiar, idTableCopiando, idRecepcionActivoFijoDetalle, arrCeldasCopiando, isOpcionesUltimaColumna) {
    if (isPrimeraSeleccion) {
        arrColumnasVisibleTableACopiar = obtenerArrColumnasVisible(idTableACopiar);
        arrColumnasVisibleTableACopiando = obtenerArrColumnasVisible(idTableCopiando);

        mostrarOcultarColumnasPorArray(idTableACopiar, true);
        mostrarOcultarColumnasPorArray(idTableCopiando, true);
    }

    var arrValores = obtenerArrValores(idTableCopiando, idRecepcionActivoFijoDetalle, arrCeldasCopiando, isOpcionesUltimaColumna);
    addRowDetallesActivosFijosPorArray(idTableACopiar, idRecepcionActivoFijoDetalle, arrCeldasCopiando, arrValores, isOpcionesUltimaColumna);
    
    if (cantCheckSeleccionados <= 0) {
        ocultarColumnasPorArray(idTableACopiar, arrColumnasVisibleTableACopiar);
        ocultarColumnasPorArray(idTableCopiando, arrColumnasVisibleTableACopiando);

        cantCheckSeleccionados = 0;
        arrColumnasVisibleTableACopiar = [];
        arrColumnasVisibleTableACopiando = [];
    }
}

function addRowDetallesActivosFijosPorArray(idTableACopiar, idRecepcionActivoFijoDetalle, arrCeldasCopiando, arrValores, isOpcionesUltimaColumna)
{
    var table = $('#' + idTableACopiar).DataTable();
    var rowNode = table.row.add(arrValores).draw().node();
    var childrens = $(rowNode).children();
    $(rowNode).prop("id", idTableACopiar + idRecepcionActivoFijoDetalle);
    $.each(childrens, function (index, value) {
        if (index == (childrens.length - 1)) {
            if (!isOpcionesUltimaColumna)
                $(value).prop("id", idTableACopiar + idRecepcionActivoFijoDetalle + arrCeldasCopiando[index]);
        }
        else
            $(value).prop("id", idTableACopiar + idRecepcionActivoFijoDetalle + arrCeldasCopiando[index]);
    });
}

function deleteRowDetallesActivosFijos(idTableEliminar, idRecepcionActivoFijoDetalle) {
    var row = $("#" + idTableEliminar + idRecepcionActivoFijoDetalle);
    $('#' + idTableEliminar).dataTable().fnDeleteRow(row);
}

function addRowCheckBox(idRecepcionActivoFijoDetalle, isChecked, callbackFunctionCheckBox, isConfiguracionSeleccionDisabled, callbackFunctionAddRowDetalleTodos, callbackFunctionRemoveRowDetalleTodos, isSeleccionTodos)
{
    return '<div class="checkbox"><label><input class="checkbox style-0 chkDetallesActivoFijo"' + (isChecked ? 'checked="checked"' : '') + ' type="checkbox" onchange="eventoCheckboxDetallesActivoFijo(this)" data-funcioncallback="' + callbackFunctionCheckBox + '" data-functioncallbackselectiontodos="' + callbackFunctionAddRowDetalleTodos + '" data-functioncallbackremovetodos="' + callbackFunctionRemoveRowDetalleTodos + '" data-isselecciontodos="' + isSeleccionTodos + '" data-idrecepcionactivofijodetalle="' + idRecepcionActivoFijoDetalle + '" id= "chkDetalleActivoFijo_' + idRecepcionActivoFijoDetalle + '"' + (isConfiguracionSeleccionDisabled ? 'disabled="disabled"' : '') + '><span>&nbsp;</span></label ></div>';
}

function agregarDashValorEmpty(valor)
{
    if (valor == "" || valor == null)
        valor = "-";

    return valor;
}