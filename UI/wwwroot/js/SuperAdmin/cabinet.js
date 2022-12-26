var previousCabinetSelect;

function getOldValueCabinet(event) {

    previousCabinetSelect = event.target.value;
}

function changeCabinet(event, profileId) {
    var privatePreviousCabinetSelect = previousCabinetSelect;
    var rowId = event.target.closest('tr').getAttribute('data-index');
    $.post('/SuperAdmin/ChangeCabinet',
        {
            profileId: profileId,
            cabinetId: event.target.value
        },
        function () {
            $('#superAdminTable').bootstrapTable('updateCell', {
                index: rowId,
                field: 'cabinet',
                value: event.target.value,
                reinit: false
            });
        }
    ).fail(function (error) {

        $('#toastBody').html(error.responseText)

        var toast = $('#toast');

        if (toast.hasClass('bg-success')) {
            toast.removeClass('bg-success');
        }

        if (!toast.hasClass('bg-danger')) {
            toast.addClass('bg-danger');
        }

        event.target.value = privatePreviousCabinetSelect;

        mdb.Toast.getInstance(toast).show();
    });
}