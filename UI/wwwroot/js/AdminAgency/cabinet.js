var previousCabinetSelect;

function getOldValueCabinet(event) {

    previousCabinetSelect = event.target.value;
}

function changeCabinet(event, sheetId) {
    var privatePreviousCabinetSelect = previousCabinetSelect;
    var rowId = event.target.closest('tr').getAttribute('data-index');
    $.post('/AdminAgency/ChangeCabinet',
        {
            sheetId: sheetId,
            cabinetId: event.target.value
        },
        function () {
            $('#adminAgencyTable').bootstrapTable('updateCell', {
                index: rowId,
                field: 'cabinet',
                value: event.target.value,
                reinit: false
            });
        }
    ).fail(function (error) {
        showToast('bg-danger', 'bg-cornflower_blue', error.responseText);
        event.target.value = privatePreviousCabinetSelect;
    });
}