$("#deleteBtn").click(function () {

    var sheetsId = $("#adminAgencyTable").bootstrapTable('getSelections');
    var header = $("#modalHeader");
    var body = $("#modalBody");
    var btnSuccess = $("#btnSuccess");

    if (sheetsId && sheetsId.length > 0) {
        header.html("Confirmation");
        if (sheetsId.length == 1) {
            body.html(`Delete 1 sheet?`);
        }
        else {
            body.html(`Delete ${sheetsId.length} sheets?`);
        }
        if (btnSuccess.hasClass("d-none")) {
            btnSuccess.removeClass("d-none");
        }
    }
    else {
        header.html("Attention");
        body.html("Please select the sheets to delete.");
        if (!btnSuccess.hasClass("d-none")) {
            btnSuccess.addClass("d-none");
        }
    }
    $("#modalDeleteSheets").modal('show');
})

function deleteSheets() {
    var sheetsId = $.map($("#adminAgencyTable").bootstrapTable('getSelections'), function (row) {
        return row.id
    });
    $.post("/AdminAgency/DeleteSheets", { sheetsId: sheetsId }, function (errorDeletingSheets) {
        if (errorDeletingSheets.length > 0) {
            $('#toastBody').html(`Error deleting sheets: ${errorDeletingSheets.join(", ")}.`)

            var toast = $('#toast');

            if (toast.hasClass('bg-success')) {
                toast.removeClass('bg-success');
            }

            if (!toast.hasClass('bg-danger')) {
                toast.addClass('bg-danger');
            }

            mdb.Toast.getInstance(toast).show();

            sheetsId = sheetsId.filter(function (id) { return !errorDeletingSheets.includes(id); });
        }

        $("#adminAgencyTable").bootstrapTable('remove', {
            field: 'id',
            values: sheetsId
        });

        setHeight();
    }).fail(function () {
        $('#toastBody').html("Error deleting sheets.")

        var toast = $('#toast');

        if (toast.hasClass('bg-success')) {
            toast.removeClass('bg-success');
        }

        if (!toast.hasClass('bg-danger')) {
            toast.addClass('bg-danger');
        }

        mdb.Toast.getInstance(toast).show();
    })
    $("#modalDeleteSheets").modal('hide');
}