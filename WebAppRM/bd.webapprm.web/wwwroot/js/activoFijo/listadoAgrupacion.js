var arrAgrupacionColumnas = [];
var nombreTablaActual = "";
var mostrarBtnAgrupar = true;

function eventoWndAgrupar(btn) {
    var btnAgrupar = $(btn);
    var idTabla = btnAgrupar.data("idtabla");
    nombreTablaActual = idTabla;

    mostrarLoadingPanel("content", "Cargando ventana de columnas...");
    $.ajax({
        url: urlAgruparFilas,
        method: "POST",
        data: { arrColumnas: obtenerArrColumnas(idTabla), idTabla: idTabla },
        success: function (data) {
            Init_BootBox("Columnas", data, "large");
        },
        error: function (errorMessage) {
            mostrarNotificacion("Error", "Ocurrió un error al cargar la ventana de columnas, inténtelo nuevamente.");
        },
        complete: function (errorMessage) {
            $("#content").waitMe("hide");
        }
    });
}

function obtenerArrColumnas(idTabla) {
    var arrColumnas = [];
    var columnas = $('#' + idTabla).dataTable().fnSettings().aoColumns;
    for (var i = 0; i < columnas.length; i++) {
        var nombreColumna = $(columnas[i].nTh).prop("id").replace(idTabla, "").replace("th", "");
        var textoColumna = $(columnas[i].nTh).html();

        var columnaSeleccionNombre = obtenerColumnaSeleccionPorNombre(idTabla, nombreColumna);
        var seleccionado = columnaSeleccionNombre != null ? columnaSeleccionNombre.seleccionado : false;

        arrColumnas.push({
            PropiedadValor: {
                Propiedad: nombreColumna,
                Valor: textoColumna
            },
            Seleccionado: seleccionado
        });
    }
    return arrColumnas;
}

function obtenerTextoColumna(idTabla, nombreColumna) {
    var arrColumnas = obtenerArrColumnas(idTabla);
    for (var i = 0; i < arrColumnas.length; i++) {
        if (arrColumnas[i].PropiedadValor.Propiedad == nombreColumna)
            return arrColumnas[i].PropiedadValor.Valor;
    }
    return "";
}

function obtenerColumnaSeleccion(idTabla) {
    for (var i = 0; i < arrAgrupacionColumnas.length; i++) {
        if (arrAgrupacionColumnas[i].idTabla == idTabla)
            return arrAgrupacionColumnas[i];
    }
    return null;
}

function obtenerColumnaSeleccionPorNombre(idTabla, nombreColumna) {
    var columnaSeleccion = obtenerColumnaSeleccion(idTabla);
    if (columnaSeleccion != null) {
        var arrColumnasNombreSeleccionado = columnaSeleccion.arrColumnasNombreSeleccionado;
        for (var i = 0; i < arrColumnasNombreSeleccionado.length; i++) {
            if (arrColumnasNombreSeleccionado[i].nombreColumna == nombreColumna)
                return arrColumnasNombreSeleccionado[i];
        }
    }
    return null;
}

function obtenerColumnaSeleccionPorNombreSeleccionado(idTabla) {
    var arrColumnasNombreSeleccionado = [];
    var columnaSeleccion = obtenerColumnaSeleccion(idTabla);
    if (columnaSeleccion != null) {
        var arrColumnasNombre = columnaSeleccion.arrColumnasNombreSeleccionado;
        for (var i = 0; i < arrColumnasNombre.length; i++) {
            if (arrColumnasNombre[i].seleccionado) {
                arrColumnasNombreSeleccionado.push(arrColumnasNombre[i].nombreColumna);
            }
        }
    }
    return arrColumnasNombreSeleccionado;
}

function obtenerArrFinalColumnaSeleccionPorNombreSeleccionado(arrColumnasNombreSeleccionado) {
    var arrFinalColumnaSeleccionPorNombre = [];
    for (var i = 0; i < arrColumnasNombreSeleccionado.length; i++) {
        arrFinalColumnaSeleccionPorNombre.push({
            propiedad: arrColumnasNombreSeleccionado[i], valor: obtenerTextoColumna(nombreTablaActual, arrColumnasNombreSeleccionado[i]).replace(":", "")
        });
    }
    return arrFinalColumnaSeleccionPorNombre;
}

function gestionarArrAgrupacionColumnas(idTabla, nombreColumna, seleccionado) {
    var columnaSeleccion = obtenerColumnaSeleccion(idTabla);
    if (columnaSeleccion == null) {
        arrAgrupacionColumnas.push({
            idTabla: idTabla,
            arrColumnasNombreSeleccionado: []
        });
        columnaSeleccion = arrAgrupacionColumnas[arrAgrupacionColumnas.length - 1];
    }

    var columnaSeleccionNombre = obtenerColumnaSeleccionPorNombre(idTabla, nombreColumna);
    if (columnaSeleccionNombre == null) {
        columnaSeleccion.arrColumnasNombreSeleccionado.push({
            nombreColumna: nombreColumna,
            seleccionado: seleccionado
        });
    }
    else
        columnaSeleccionNombre.seleccionado = seleccionado;
}

function callBackFunctionSeleccionColumna(nombreColumna, seleccionado) {
    nombreColumna = nombreColumna.replace(nombreTablaActual, "");
    gestionarArrAgrupacionColumnas(nombreTablaActual, nombreColumna, seleccionado);

    var arrColumnasNombreSeleccionado = obtenerColumnaSeleccionPorNombreSeleccionado(nombreTablaActual);
    var arrFinalColumnaSeleccionPorNombre = obtenerArrFinalColumnaSeleccionPorNombreSeleccionado(arrColumnasNombreSeleccionado);

    changeDrawDataTableFiltrado(nombreTablaActual, function () {
        crearGrupo(nombreTablaActual, arrFinalColumnaSeleccionPorNombre);
    });
}