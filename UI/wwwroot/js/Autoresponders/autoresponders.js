const option = document.querySelectorAll('.select-info__list'),
    selects = document.querySelectorAll('.select-title');

option.forEach(list => {
    list.addEventListener('click', () => {
        let selectText = list.parentElement.parentElement.children[0].children[0]
        selectText.textContent = list.textContent
        list.parentElement.parentElement.classList.toggle('showSelect')
    });
});

selects.forEach(select => {
    select.addEventListener('click', () => {
        select.parentElement.classList.toggle('showSelect')
    });
});

function settingRemove() {
    $('table').each(function () {
        let findFirst = false;
        $($(this).find('tr').get().reverse()).each(function () {
            if (!findFirst && $(this).find('[name=message]').text()) {
                $(this).find('.autoanswer-delete').removeClass('disabled');
                findFirst = true;
            }
            else {
                $(this).find('.autoanswer-delete').addClass('disabled');
            }
        });
    });
}

function autoresponders(e, sheetId) {
    $(`[name=sheet]`).each(function () {
        if ($(this).hasClass('active')) {
            $(this).removeClass('active');
        }
    });

    $(e).addClass('active');

    addSpinner();
    $.get('/Autoresponders/SheetAutoresponders', { sheetId }, function (data) {
        $('.auto-replies').html(data);
        settingRemove(); 
    }).fail(function () {
        removeSpinner('spinnerAutoresponders');
    });
}

function addSpinner() {
    var spinner = $(`<div id="spinnerAutoresponders" class="spinner-grow spinner-grow-sm text-white" role="status"></div>`);
    $('.auto-replies').html(spinner);
}

function removeSpinner(id) {
    $(`#${id}`).remove();
}

const autoresponderPopUp = document.querySelector('.pop-up.autoresponder');
let $tr;

function showEditPopUp(e) {
    $tr = $(e).closest('tr');

    var $activeSheet = $('[name=sheet].active');

    $('#avatarPopUp').attr('src', $activeSheet.find('img').attr('src'));

    if ($activeSheet.find('[name=status]').hasClass('online')) {
        $('#statusPopUp').removeClass('offline');
        $('#statusPopUp').addClass('online');
    }
    else {
        $('#statusPopUp').removeClass('online');
        $('#statusPopUp').addClass('offline');
    }

    $('#namePopUp').text($activeSheet.find('.user__name').text());
    $('#idPopUp').text($activeSheet.find('.pd').text());

    $('#intervalStart').val($tr.find('[name=intervalStart]').text());
    $('#intervalFinish').val($tr.find('[name=intervalFinish]').text());
    $('#limitMessage').val($tr.find('[name=limitMessage]').text());
    $('#textAutoresponder').val($tr.find('[name=message]').text());

    if ($tr.find('[name=counter]').text() === '1') {
        $('#intervalStart').attr('readonly', true);
        $('#intervalFinish').attr('readonly', true);
    }
    else {
        $('#intervalStart').attr('readonly', false);
        $('#intervalFinish').attr('readonly', false);
    }

    changeCounter();

    autoresponderPopUp.classList.remove('d-none');
}

function changeCounter() {
    var count = $('#textAutoresponder').val().length;
    var counter = $('#counter');
    counter.text(count);
}

function save() {
    let sheetId = $('#idPopUp').text();
    let stackType = $tr.closest('table').data('stacktype');
    let autoresponderMessages = $tr.find('[name=counter]').text();
    let message = $('#textAutoresponder').val();
    let intervalStart = $('#intervalStart').val();
    let intervalFinish = $('#intervalFinish').val();
    let limitMessage = $('#limitMessage').val();

    $.post('/Autoresponders/Update', { sheetId, stackType, autoresponderMessages, message, intervalStart, intervalFinish, limitMessage }, function () {
        $tr.find('[name=message]').text(message);
        $tr.find('[name=intervalStart]').text(intervalStart);
        $tr.find('[name=intervalFinish]').text(intervalFinish);
        $tr.find('[name=limitMessage]').text(limitMessage);
        settingRemove();
        closeAutoresponderPopUp();
    });
}

function deleteAutoresponder(e) {
    if (!$(e).hasClass('disabled')) {
        let $activeSheet = $('[name=sheet].active');
        let sheetId = $activeSheet.find('.pd').text();
        let stackType = $(e).closest('table').data('stacktype');
        let autoresponderMessages = $(e).closest('tr').find('[name=counter]').text();

        $.post('/Autoresponders/Clear', { sheetId, stackType, autoresponderMessages }, function () {
            let $tr = $(e).closest('tr');
            $tr.find('[name=message]').text('');
            $tr.find('[name=intervalStart]').text('');
            $tr.find('[name=intervalFinish]').text('');
            $tr.find('[name=limitMessage]').text('');
            settingRemove(); 
        });
    }
}

//Close pop-up addUser
$(document).on('click', '.pop-up__close', function (event) {
    closeAutoresponderPopUp()
});

window.addEventListener('click', (e) => {
    if (e.target == autoresponderPopUp) {
        closeAutoresponderPopUp()
    }
});

function closeAutoresponderPopUp() {
    autoresponderPopUp.classList.add('d-none');
}
