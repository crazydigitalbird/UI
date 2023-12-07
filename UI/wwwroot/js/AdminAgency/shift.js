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
        showToast('bg-danger', 'bg-cornflower_blue', error.responseText);
        event.target.value = privatePreviousShiftSelect;
    });
}