var previousOldGroupSelect;

$('#addGroup').click(() => {
    $('#formAddGroup').validate().resetForm();
    $('#nameGroup').removeClass('is-invalid is-valid');
    $('#nameGroup').val('');
    $('#modalAddGroup').modal('show');
});

function successAddGroup(data) {
    var group = JSON.parse(data);
    $('#modalAddGroup').modal('hide');
    $('#toastBody').html(`The ${group.name} group has been added successfully`)

    var toast = $('#toast');

    if (toast.hasClass('bg-danger')) {
        toast.removeClass('bg-danger');
    }

    if (!toast.hasClass('bg-success')) {
        toast.addClass('bg-success');
    }

    $('select[name="groupHeader"]').each(function (i, item) {
        $(item).append($('<option>', {
            value: group.id,
            text: group.name
        }));
    });

    $table.bootstrapTable('refresh');

    setTimeout(() => mdb.Toast.getInstance(toast).show(), 500);
}

function failureAddGroup(error) {
    $('#modalAddGroup').modal('hide');

    $('#toastBody').html(`Error adding the ${error.responseText} group`)

    var toast = $('#toast');

    if (toast.hasClass('bg-success')) {
        toast.removeClass('bg-success');
    }

    if (!toast.hasClass('bg-danger')) {
        toast.addClass('bg-danger');
    }

    setTimeout(() => mdb.Toast.getInstance(toast).show(), 500);
}

function getValueSelect(event) {

    previousOldGroupSelect = event.target.value;
}

function changeGroup(event, profileId) {
    var privatePreviousOldGroupSelect = previousOldGroupSelect;
    var rowId = event.target.closest('tr').getAttribute('data-index');
    $.post('/AdminAgency/ChangeGroup',
        {
            profileId: profileId,
            groupId: event.target.value
        },
        function () {
            $('#adminAgencyTable').bootstrapTable('updateCell', {
                index: rowId,
                field: 'group',
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

        event.target.value = privatePreviousOldGroupSelect;

        mdb.Toast.getInstance(toast).show();
    });
}