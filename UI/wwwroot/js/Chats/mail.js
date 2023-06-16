//Сортировка фото и видео путем перетаскивания элементов на новые позиции.
$('#mailUploadedFile').sortable({
    revert: true,
    opacity: 0.5
});

function showMail() {
    var sheetId = $('#manMessagesMails').data('sheet-id');
    var idInterlocutor = $('#interlocutorIdChatHeader').text();
    if (sheetId && idInterlocutor) {
        clearMailPopup();
        enableSpinnerMailHistory();
        //Загружаем историю
        loadHistoryMail(sheetId, idInterlocutor);
        popUpMail.classList.remove('d-none');
    }
}

function loadHistoryMail(sheetId, idRegularUser) {
    $.post('/Chats/HistoryMail', { sheetId, idRegularUser }, function (historyMail) {
        disableSpinnerLoadMailHistory();
        $('#historyMail').html(historyMail);
        updateAllDateHumanize();
    });
}

function enableSpinnerMailHistory() {
    var spinner = $(`<div id="spinnerLoadMailHistory" class="d-flex justify-content-center my-1">
                        <div class="spinner-grow spinner-grow-sm text-white" role="status">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                     </div>`);
    $('#historyMail').prepend(spinner);
}

function disableSpinnerLoadMailHistory() {
    $('#spinnerLoadMailHistory').remove();
}

function sendMail() {
    var $popUpMail = $(popUpMail);
    var sheetId = $('#manMessagesMails').data('sheet-id');
    var idRegularUser = $('#interlocutorIdChatHeader').text();
    var idLastMessage = $('#messages').find('[name=message]').last().data('id-message');
    var text = $('#newMailMessage').val();
    var videos = [];
    var photos = [];

    $('#mailUploadedFile').find('.file').each(function () {
        var isVideo = $(this).data('is-video');
        var url = $(this).find('img')[0].src;
        var id = $(this).data('id');
        if (isVideo) {
            videos.push({ Id: id, Url: url });
        }
        else {
            photos.push({ Id: id, Url: url });
        }
    });

    $.post('/Chats/SendMail', { sheetId, idRegularUser, idLastMessage, text, videos, photos }, function (data) {
        var idRegularUserCurrent = $('#interlocutorIdChatHeader').text();
        var sheetIdCurrent = $('#manMessagesMails').data('sheet-id');
        if (sheetId === sheetIdCurrent && idRegularUser === idRegularUserCurrent) {
            $(`#messages`).append(data);
            scrollToEndMessages();
            reduceMailsLeft();
        }
    }).fail(function () {

    });
    $popUpMail.addClass('d-none');
}

//Проверка доступности отправки почтовых сообщений
function allowedSendMeils(count) {
    var $mailsLeft = $('#mailsLeft');
    if ($mailsLeft) {
        var mailsLeft = Number($mailsLeft.text());
        if ((mailsLeft - count) >= 0) {
            return true;
        }
    }
    return false;
}

//Уменьшаем счетчик доступных почтовых сообщений
function reduceMailsLeft() {
    var $mailsLeft = $('#messagesLeft');
    if ($mailsLeft) {
        var mailsLeft = Number($mailsLeft.text());
        var newMailsLeft = mailsLeft - 1;
        if (newMailsLeft >= 0) {
            $mailsLeft.text(newMailsLeft);
        }
        if (newMailsLeft === 0) {
            $mailsLeft.addClass('large-block-header-timer-text-opacity');
        }
    }
}

//Обновление счетчика размера текстового сообщения в TextArea
function changeMailCounter() {
    var count = $('#newMailMessage').val().length;
    var mailCounter = $('#mailCounter');
    mailCounter.text(count);

    checkSendMail(count);
}

function checkSendMail(textLength) {
    if (allowedSendMeils(1) && textLength >= 200 && textLength <= 3500) {
        if ($('#sendMail').hasClass('disabled')) {
            $('#sendMail').removeClass('disabled');
        }
    }
    else {
        if (!$('#sendMail').hasClass('disabled')) {
            $('#sendMail').addClass('disabled')
        }
    }
}

function clearMailPopup() {
    //Проверяем возможность отправки почтового сообщения, если отправка невозможна, блокируем кнопку отправки почтовых сообщений в модальном окне
    //В противном случае данную кнопку разблокируем
    if (allowedSendMeils(1)) {
        $('#sendMail').removeClass('disabled');
    }
    else {
        $('#sendMail').addClass('disabled');
    }

    $('#newMailMessage').val('');
    changeMailCounter();

    $('#mailUploadedFile').empty();
    $('#historyMail').empty();

    if (!$('#emojisMail').hasClass("d-none")) {
        $('#emojisMail').addClass("d-none");
    }
}
