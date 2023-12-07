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

        $('#toastBody').html(error.responseText)

        var toast = $('#toast');

        if (toast.hasClass('bg-cornflower_blue')) {
            toast.removeClass('bg-cornflower_blue');
        }

        if (!toast.hasClass('bg-danger')) {
            toast.addClass('bg-danger');
        }

        event.target.value = privatePreviousCabinetSelect;

        bootstrap.Toast.getOrCreateInstance(toast).show();
    });
}