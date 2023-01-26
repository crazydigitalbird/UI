var previousShiftSelect;

function getOldValueShift(event) {

    previousShiftSelect = event.target.value;
}

function changeShift(event, profileId) {
    var privatePreviousShiftSelect = previousShiftSelect;
    var rowId = event.target.closest('tr').getAttribute('data-index');
    $.post('/AdminAgency/ChangeShift',
        {
            profileId: profileId,
            shiftId: event.target.value
        },
        function () {
            $('#adminAgencyTable').bootstrapTable('updateCell', {
                index: rowId,
                field: 'shift',
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

        event.target.value = privatePreviousShiftSelect;

        mdb.Toast.getInstance(toast).show();
    });
}