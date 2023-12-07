var previousShiftSelect;

function getOldValueShift(event) {

    previousShiftSelect = event.target.value;
}

function changeShift(event, sheetId) {
    var privatePreviousShiftSelect = previousShiftSelect;
    var rowId = event.target.closest('tr').getAttribute('data-index');
    $.post('/AdminAgency/ChangeShift',
        {
            sheetId: sheetId,
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

        if (toast.hasClass('bg-cornflower_blue')) {
            toast.removeClass('bg-cornflower_blue');
        }

        if (!toast.hasClass('bg-danger')) {
            toast.addClass('bg-danger');
        }

        event.target.value = privatePreviousShiftSelect;

        bootstrap.Toast.getOrCreateInstance(toast).show();
    });
}