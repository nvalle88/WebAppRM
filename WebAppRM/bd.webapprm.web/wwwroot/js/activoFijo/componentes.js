﻿var arrRecepcionActivoFijoDetalleComponentes = Array();
var idFilaDatoEspecificoActual = -1;
jQuery(document).ready(function () {
    $.each($(".btnComponentesDatosEspecificos"), function (index, value) {
        var btnComponente = $(value);
        var idFilaDatoEspecifico = btnComponente.data("idfila");
        addComponenteToArray(idFilaDatoEspecifico, btnComponente.data("idorigen"), $("#hComponentes_" + idFilaDatoEspecifico).val().split(","));
    });
});

function addComponenteToArray(idFilaDatoEspecifico, idRecepcionActivoDetalleOrigen, arrIdsComponentes)
{
    arrRecepcionActivoFijoDetalleComponentes.push({
        idFilaDatoEspecifico: idFilaDatoEspecifico,
        idRecepcionActivoDetalleOrigen: idRecepcionActivoDetalleOrigen,
        arrIdsComponentes: arrIdsComponentes
    });
}

function cargarFormularioComponentesDatosEspecificos(idFila) {
    mostrarLoadingPanel("modalContentComponente", "Cargando componentes, por favor espere...");
    $("#btnAdicionarComponentes").data("idFila", idFila);
    idFilaDatoEspecificoActual = idFila;
    $.ajax({
        url: urlModalComponentesResult,
        method: "POST",
        data: { componentesActivo: obtenerRecepcionActivoFijoDetalleComponente(), idsComponentesExcluir: obtenerIdsComponentesExcluir(idFila) },
        success: function (data) {
            $("#modalBodyComponente").html(data);
            initDataTableFiltrado("tableDetallesActivoFijo", [15, 16, 17, 18, 19, 20, 21]);
        },
        error: function (errorMessage) {
            mostrarNotificacion("Error", "Ocurrió un error al cargar el formulario.");
        },
        complete: function (data) {
            $("#modalContentComponente").waitMe("hide");
        }
    });
}

function cargarListadoActivosFijosParaComponentes()
{
    mostrarLoadingPanel("modalContentComponenteActivoFijo", "Cargando listado de activos fijos, por favor espere...");
    var idFila = $("#btnAdicionarComponentes").data("idFila");

    var componentesActivo = obtenerRecepcionActivoFijoDetalleComponente();
    var idsComponentesExcluir = obtenerIdsComponentesExcluir(idFila);
    idsComponentesExcluir.push(componentesActivo.idRecepcionActivoDetalleOrigen);
    
    $.ajax({
        url: urlComponentesActivosFijos,
        method: "POST",
        data: { componentesActivo: componentesActivo, idsComponentesExcluir: idsComponentesExcluir },
        success: function (data) {
            $("#modalBodyComponenteActivoFijo").html(data);
            initDataTableFiltrado("tableDetallesActivoFijoComponentes", [16, 17, 18, 19, 20, 21, 22]);
            eventoCheckboxDetallesActivoFijo();
        },
        error: function (errorMessage) {
            mostrarNotificacion("Error", "Ocurrió un error al cargar el formulario.");
        },
        complete: function (data) {
            $("#modalContentComponenteActivoFijo").waitMe("hide");
        }
    });
}

function obtenerIdsComponentesExcluir(idFila)
{
    var arrIdsComponentesExcluir = Array();
    for (var i = 0; i < arrRecepcionActivoFijoDetalleComponentes.length; i++) {
        if (arrRecepcionActivoFijoDetalleComponentes[i].idFilaDatoEspecifico != idFila)
        {
            var arrIdsComponentes = arrRecepcionActivoFijoDetalleComponentes[i].arrIdsComponentes;
            for (var j = 0; j < arrIdsComponentes.length; j++)
                arrIdsComponentesExcluir.push(arrIdsComponentes[j]);
        }
    }
    return arrIdsComponentesExcluir;
}

function eliminarIdComponente(idRecepcionActivoFijoDetalleEliminar, rafdComponente)
{
    for (var i = 0; i < rafdComponente.arrIdsComponentes.length; i++) {
        if (rafdComponente.arrIdsComponentes[i] == idRecepcionActivoFijoDetalleEliminar) {
            rafdComponente.arrIdsComponentes.splice(i, 1);
            return;
        }
    }
}

function eliminarComponente(idFilaDatoEspecifico)
{
    for (var i = 0; i < arrRecepcionActivoFijoDetalleComponentes.length; i++) {
        if (arrRecepcionActivoFijoDetalleComponentes[i].idFilaDatoEspecifico == idFilaDatoEspecifico) {
            arrRecepcionActivoFijoDetalleComponentes.splice(i, 1);
            return;
        }
    }
}

function obtenerRecepcionActivoFijoDetalleComponente()
{
    for (var i = 0; i < arrRecepcionActivoFijoDetalleComponentes.length; i++) {
        if (arrRecepcionActivoFijoDetalleComponentes[i].idFilaDatoEspecifico == idFilaDatoEspecificoActual) {
            return arrRecepcionActivoFijoDetalleComponentes[i];
        }
    }
    return null;
}

function callBackFunctionSeleccionComponente(idRecepcionActivoFijoDetalle, seleccionado)
{
    if (seleccionado) {
        var btnEliminarComponente = "<div id='divEliminarComponente_" + idRecepcionActivoFijoDetalle + "' class='btnEliminarComponentes' style='display:inline;'><a href='javascript: void(0);' id='btnEliminarComponente_" + idRecepcionActivoFijoDetalle + "' onclick=abrirVentanaConfirmacion('btnEliminarComponente_" + idRecepcionActivoFijoDetalle + "') data-funcioncallback=callBackEliminarComponente('" + idRecepcionActivoFijoDetalle + "') data-titulo='Eliminar' data-descripcion='&#191; Desea eliminar el Componente... ?'>Eliminar</a></div>";
        addRowDetallesActivosFijos("tableDetallesActivoFijo", "tableDetallesActivoFijoComponentes", idRecepcionActivoFijoDetalle, ['Codigosecuencial', 'TipoActivoFijo', 'ClaseActivoFijo', 'SubclaseActivoFijo', 'NombreActivoFijo', 'Marca', 'Modelo', 'Serie', 'NumeroChasis', 'NumeroMotor', 'Placa', 'NumeroClaveCatastral', 'Sucursal', 'Bodega', 'Empleado', 'Proveedor', 'MotivoAlta', 'FechaRecepcion', 'OrdenCompra', 'FondoFinanciamiento', 'FechaAlta', 'NumeroFactura', btnEliminarComponente], true);
        var rafdComponente = obtenerRecepcionActivoFijoDetalleComponente();
        rafdComponente.arrIdsComponentes.push(idRecepcionActivoFijoDetalle);
    }
    else
        callBackEliminarComponente(idRecepcionActivoFijoDetalle);
    putValuesComponentesenCampoHidden();
}

function callBackEliminarComponente(idRecepcionActivoFijoDetalle)
{
    deleteRowDetallesActivosFijos("tableDetallesActivoFijo", idRecepcionActivoFijoDetalle);
    var rafdComponente = obtenerRecepcionActivoFijoDetalleComponente();
    eliminarIdComponente(idRecepcionActivoFijoDetalle, rafdComponente);
    putValuesComponentesenCampoHidden();
}

function putValuesComponentesenCampoHidden()
{
    var hComponentes = $("#hComponentes_" + idFilaDatoEspecificoActual);
    var rafdComponente = obtenerRecepcionActivoFijoDetalleComponente();
    var idsComponentes = rafdComponente.arrIdsComponentes.join(',');
    hComponentes.val(idsComponentes);
}