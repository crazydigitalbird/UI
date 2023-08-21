const option = document.querySelectorAll('.select-info__list'),
    selectText = document.querySelector('.select-title__text'),
    select = document.querySelector('.select-title'),
    viewTrashBtn = document.querySelector('.view-trash'),
    addBtn = document.querySelector('.add__btn'),
    popUpTrash = document.querySelector('.pop-up.trash'),
    popUpChatAndMail = document.querySelector('.pop-up.chatAndMail'),
    popUpClose = document.querySelectorAll('.pop-up__close');
var currentSheetId = undefined;
var $sheet = undefined;

$(function () {
    //Для всех элементов с name равным sheet подписываеся на события click, в результате которого осуществляется загрузка списка ice для выбранной анкеты. 
    $('[name=sheet]').each(function () {
        this.addEventListener('click', function () {
            resetFltersIce();
            clearBoxIce();
            resetSortIce();
            addSpinnersIce();

            $sheet = $(this);
            var sheetId = $sheet.data('sheet-id');
            currentSheetId = sheetId;
            $.get('/Ice/Ices', { sheetId }, function (iceList) {
                $('.middle-box').html(iceList);
            }).fail(function () {
                removeSpinnerIce('spinnerIces');
            });
            $.get('/Ice/Progress', { sheetId }, function (ProgressIces) {
                $('.progress-box').html(ProgressIces);
            }).fail(function () {
                removeSpinnerIce('spinnerProgress');
            });
        });
    });
});

option.forEach(list => {
    list.addEventListener('click', () => {
        selectText.textContent = list.textContent;
        select.parentElement.classList.toggle('showSelect');
        sortIces(list.id);
    });
});

function sortIces(criterion) {
    switch (criterion) {
        case 'data':
            $('.middle-box .middle-item').sort(function (a, b) {
                var dataA = Date.parse($(a).data('created'));
                var dataB = Date.parse($(b).data('created'));
                return dataB - dataA;
            }).appendTo('.middle-box');
            break;

        case 'efficiency':
            $('.middle-box .middle-item').sort(function (a, b) {
                return $(b).data('efficiency') - $(a).data('efficiency');
            }).appendTo('.middle-box');
            break;

        case 'numberOfMessage':
            $('.middle-box .middle-item').sort(function (a, b) {
                var typeA = $(a).data('type');
                var typeB = $(b).data('type');
                if (typeA === typeB) {
                    var numberOfSentA = $(a).data('number-of-sent');
                    var numberOfSentB = $(b).data('number-of-sent');
                    return numberOfSentB - numberOfSentA;
                }
                else if (typeB === 'Message') {
                    return 1;
                }
                return -1;
            }).appendTo('.middle-box');
            break;

        case 'numberOfMail':
            $('.middle-box .middle-item').sort(function (a, b) {
                var typeA = $(a).data('type');
                var typeB = $(b).data('type');
                if (typeA === typeB) {
                    var numberOfSentA = $(a).data('number-of-sent');
                    var numberOfSentB = $(b).data('number-of-sent');
                    return numberOfSentB - numberOfSentA;
                }
                else if (typeB === 'Mail') {
                    return 1;
                }
                return -1;
            }).appendTo('.middle-box');
            break;
    }
}

select.addEventListener('click', () => {
    select.parentElement.classList.toggle('showSelect');
});

popUpClose.forEach(close => {
    close.addEventListener('click', () => {
        close.parentElement.parentElement.classList.add('d-none');
    })
});

viewTrashBtn.addEventListener('click', () => {
    if (currentSheetId) {
        var $trash = $('.trash-box');
        $trash.empty();
        addSpinnerIceTrash();

        $.get('/Ice/Trash', { sheetId: currentSheetId }, function (ices) {
            $('.trash-box').html(ices);
        }).fail(function () {
            removeSpinnerIce('spinnerTrash');
        });
        popUpTrash.classList.remove('d-none');
    }
});

addBtn.addEventListener('click', () => {
    if (currentSheetId) {
        if (!$('#emojisIceMail').hasClass("d-none")) {
            $('#emojisIceMail').addClass("d-none");
        }
        if (!$('#emojisIceChat').hasClass("d-none")) {
            $('#emojisIceChat').addClass("d-none");
        }
        popUpChatAndMail.classList.remove('d-none');
    }
});

window.addEventListener('click', (e) => {
    if (e.target == popUpTrash) {
        popUpTrash.classList.add('d-none');
    }
    if (e.target == popUpChatAndMail) {
        popUpChatAndMail.classList.add('d-none');
    }
});

$('#sendToModeration').on('click', function () {
    var $chatTextArea = $('#chatIceTextArea');
    var $mailTextArea = $('#mailIceTextArea');

    $.post('/Ice/Create', { sheetId: currentSheetId, content: $chatTextArea.val(), iceType: 'Message' }, function () {
        $chatTextArea.val('');
    });
    $.post('/Ice/Create', { sheetId: currentSheetId, content: $mailTextArea.val(), iceType: 'Mail' }, function () {
        $mailTextArea.val('');
    });
    popUpChatAndMail.classList.add('d-none')
});

function changeChatIceCounter() {
    var count = $('#chatIceTextArea').val().length;
    var chatIceCounter = $('#chatIceCounter');
    chatIceCounter.text(count);
}

function changeMailIceCounter() {
    var count = $('#mailIceTextArea').val().length;
    var mailIceCounter = $('#mailIceCounter');
    mailIceCounter.text(count);
}

function switchOff(iceId) {
    var $iceProgress = $('.progress-box').find(`[name=${iceId}]`);
    $iceProgress.addClass('d-none');

    $.post("/Ice/Switch", { sheetId: currentSheetId, iceId, status: "off" }, function () {
        var typeIceProgress = $iceProgress.data('type');
        $iceProgress.remove();
        $sheet.find(`[name="${typeIceProgress}"]`).addClass('opacity-50');
    }).fail(function () {
        $iceProgress.removeClass('d-none');
    });
}

function switchOn(iceId) {
    if ($('.progress-box').find(`[name=${iceId}]`).length > 0) {
        return;
    }

    var $newIceProcess = $('.middle-box').find(`[name=${iceId}]`).clone();
    $newIceProcess.find('.progress-item__close').removeClass('d-none');
    $newIceProcess.find('.delate').addClass('d-none');
    $newIceProcess.find('.play').addClass('d-none');

    var typeNewIceProgress = $newIceProcess.data('type');
    var $oldIceProcess = $('.progress-box').find(`[data-type="${typeNewIceProgress}"]`);
    if ($oldIceProcess.length > 0) {
        $oldIceProcess.addClass('d-none');
    }
    $('.progress-box').append($newIceProcess);

    $.post("/Ice/Switch", { sheetId: currentSheetId, iceId, status: "on" }, function () {
        if ($oldIceProcess.length === 0) {
            $sheet.find(`[name="${typeNewIceProgress}"]`).removeClass('opacity-50');
        }
        else {
            $oldIceProcess.remove();
        }
    }).fail(function () {
        if ($oldIceProcess.length > 0) {
            $oldIceProcess.removeClass('d-none');
        }
        $newIceProcess.remove();
    });
}

function replyIce(iceId) {
    var $trashIce = $('.trash-box').find(`[name=${iceId}]`);
    var $newIceProcess = $trashIce.clone();
    $newIceProcess.find('.delate').removeClass('d-none');
    $newIceProcess.find('.play').removeClass('d-none');
    $newIceProcess.find('.reply').addClass('d-none');
    $trashIce.addClass('d-none');
    $('.middle-box').prepend($newIceProcess);

    $.post('/Ice/Reply', { sheetId: currentSheetId, iceId }, function () {
        $trashIce.remove();
    }).fail(function () {
        $trashIce.removeClass('d-none');
        $newIceProcess.remove();
    });
}

function filterIce(e, filterType) {
    if ($sheet) {
        $('.middle-box').find(`[data-type="${filterType}"]`).each(function () {
            $(this).toggleClass('d-none');
        });
        $(e).toggleClass('opacity-50');
    }
}

function resetFltersIce() {
    $('.middle-head-left').find('svg').each(function () {
        if ($(this).hasClass('opacity-50')) {
            $(this).removeClass('opacity-50');
        }
    });
}

function resetSortIce() {
    selectText.textContent = option[0].textContent;
}

function clearBoxIce() {
    $('.middle-box').empty();
    $('.progress-box').empty();
}

function addSpinnersIce() {
    var spinnerIces = $(`<div id="spinnerIces" class="spinner-grow spinner-grow-sm text-white" role="status"></div>`);
    var spinnerProgress = $(`<div id="spinnerProgress" class="spinner-grow spinner-grow-sm text-white" role="status"></div>`);
    $('.middle-box').html(spinnerIces);
    $('.progress-box').html(spinnerProgress);
}

function addSpinnerIceTrash() {
    var spinnerTrash = $(`<div id="spinnerTrash" class="spinner-grow spinner-grow-sm text-white" role="status"></div>`);
    $('.trash-box').html(spinnerTrash);
}

function removeSpinnerIce(id) {
    $(`#${id}`).remove();
}

function searchSheet(e) {
    var searchSheetIdentity = e.value.toLowerCase();
    $('.wrapper-box [name="sheet"]').each(function () {
        var sheetIdentity = $(this).find('[name="sheetId"]').text();
        if (!sheetIdentity.includes(searchSheetIdentity)) {
            $(this).addClass('d-none');
        }
        else {
            $(this).removeClass('d-none');
        }
    });
}

function searchOperator(e) {
    var searchOperatorIdentity = e.value.toLowerCase();
    $('.wrapper-box [name="sheet"]').each(function () {
        var operatorIdentity = $(this).find('[name="operatorId"]').text();
        if (!operatorIdentity.includes(searchOperatorIdentity)) {
            $(this).addClass('d-none');
        }
        else {
            $(this).removeClass('d-none');
        }
    });
}

function deleteIce(iceId) {
    var $ice = $(`.middle-box [name="${iceId}"]`);
    $ice.addClass('d-none');
    $.post('/Ice/Delete', { sheetId: currentSheetId, iceId }, function () {
        $(`.middle-box [name="${iceId}"]`).remove();
        if ($(`.progress-box [name="${iceId}"]`).length > 0) {
            switchOff(iceId);
        }
    }).fail(function () {
        $ice.removeClass('d-none');
    });
}
