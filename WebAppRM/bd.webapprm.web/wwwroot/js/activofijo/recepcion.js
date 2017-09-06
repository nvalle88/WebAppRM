$(document).ready(function () {
    Init_Select2();
});

$("#IdTipoActivoFijo").on("change", function (e) {
    $.ajax({
        url: urlClaseActivoFijo,
        method: "POST",
        data: { idTipoActivoFijo: e.val },
        success: function (data)
        {
            $("#div_claseActivoFijo").html(data);
            Init_Select2();
        }
    });
});