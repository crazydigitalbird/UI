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
            showToast('bg-danger', 'bg-cornflower_blue', `Error deleting sheets: ${errorDeletingSheets.join(", ")}.`);
            sheetsId = sheetsId.filter(function (id) { return !errorDeletingSheets.includes(id); });
        }

        $("#adminAgencyTable").bootstrapTable('remove', {
            field: 'id',
            values: sheetsId
        });

        setHeight();
    }).fail(function () {
        showToast('bg-danger', 'bg-cornflower_blue', "Error deleting sheets.");
    })
    $("#modalDeleteSheets").modal('hide');
}

$("#addSheet").click(function () {
    document.getElementById('formAddSheet').reset();
    hidenAlert();
    $("#modalAddSheet").modal('show');
});

function successAddSheet(sheet) {
    hidenAlert();

    var userInfo = JSON.parse(sheet.info);

    var fullName = '';
    if (userInfo.Personal.Name) {
        fullName = userInfo.Personal.Name;
    }
    if (userInfo.Personal.LastName) {
        fullName = `${fullName} ${userInfo.Personal.LastName}`;
    }

    //let $newRow = { id: 1, name: '123' };
    //$('#adminAgencyTable').bootstrapTable('append', $newRrow);

    var totalRows = $('#adminAgencyTable').bootstrapTable('getOptions').totalRows;

    $('#adminAgencyTable').bootstrapTable('insertRow', {
        index: totalRows,
        row: {
            number: totalRows + 1,
            status: '<i class="fa-solid fa-circle text-success"></i>',
            name: userInfo.Personal.Name,
            lastName: userInfo.Personal.LastName,
            sheetId: userInfo.Id,
            id: sheet.id,
            balance: `<div class="row justify-content-center">
                        <div class="col-6 text-end">
                            0$
                        </div>
                        <div class="col-auto ps-0">
                            <i class="fa-solid fa-signal fa-signal-gradient"></i>
                        </div>
                      </div>`,
            media: 'media',
            operators: 0,
            group: '',
            shift: '',
            cabinet: ''
        }
    });

    $("#modalAddSheet").modal('hide');

    $('#adminAgencyTable').bootstrapTable('scrollTo', 'bottom');

    showToast('bg-cornflower_blue', 'bg-danger', `The  ${fullName} sheet has been added successfully`);
}

function failureAddSheet(error) {
    showAlert('alert-danger', 'alert-success', error.responseText);
}

function showAlert(addClass, removeClass, text) {
    var $alert = $('#alertAddSheet');
    if (!$alert.hasClass(addClass)) {
        $alert.addClass(addClass);
    }
    if ($alert.hasClass(removeClass)) {
        $alert.removeClass(removeClass);
    }
    if ($alert.hasClass('d-none')) {
        $alert.removeClass('d-none');
    }
    $alert.text(text);
}

function hidenAlert() {
    var $alert = $('#alertAddSheet');
    $alert.text('');

    if (!$alert.hasClass('d-none')) {
        $alert.addClass('d-none');
    }
}