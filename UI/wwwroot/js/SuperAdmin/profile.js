$("#deleteBtn").click(function () {

    var profilesId = $("#superAdminTable").bootstrapTable('getSelections');
    var header = $("#modalHeader");
    var body = $("#modalBody");
    var btnSuccess = $("#btnSuccess");

    if (profilesId && profilesId.length > 0) {
        header.html("Confirmation");
        if (profilesId.length == 1) {
            body.html(`Delete 1 profile?`);
        }
        else {
            body.html(`Delete ${profilesId.length} profiles?`);
        }
        if (btnSuccess.hasClass("d-none")) {
            btnSuccess.removeClass("d-none");
        }
    }
    else {
        header.html("Attention");
        body.html("Please select the profiles to delete.");
        if (!btnSuccess.hasClass("d-none")) {
            btnSuccess.addClass("d-none");
        }
    }
    $("#modalDeleteProfiles").modal('show');
})

function deleteProfiles() {
    var profilesId = $.map($("#superAdminTable").bootstrapTable('getSelections'), function (row) {
        return row.id
    });
    $.post("/SuperAdmin/DeleteProfiles", { profilesId: profilesId }, function () {
        $("#superAdminTable").bootstrapTable('remove', {
            field: 'id',
            values: profilesId
        })
    }).fail(function () {
        console.log("fail");
    })
    $("#modalDeleteProfiles").modal('hide');
}