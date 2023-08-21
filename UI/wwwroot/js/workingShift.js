$('#btnWorkingShift').on('click', function () {
    if ($('#btnWorkingShift').hasClass('btn-success')) {
        $.post('/WorkingShift/Start', function () {
            $('#btnWorkingShift').text('Закончить смену');
            $('#btnWorkingShift').removeClass('btn-success');
            $('#btnWorkingShift').addClass('btn-danger');
        });
    }
    else {
        $.post('/WorkingShift/Stop', function () {
            $('#btnWorkingShift').text('Начать смену');
            $('#btnWorkingShift').removeClass('btn-danger');
            $('#btnWorkingShift').addClass('btn-success');
        });
    }
})