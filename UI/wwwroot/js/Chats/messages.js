// Загрузка новых сообщений
function LoadNewMessages(sheetId, idInterlocutor, idNewMessage) {
    // проверяем что пользователь за время запроса осталься в том же диалоге
    if (CheckCurrentChat(sheetId, idInterlocutor)) {
        // Получаем id самого нового сообщения
        var idLastMessage = $('#messages').find('[name=message]').last().data('id-message');

        //Проверяем что id нового сообщения больше самого нового сообщения в окне чата 
        if (idNewMessage > idLastMessage) {
            $.post("/Chats/LoadMessages", { sheetId, idInterlocutor, idLastMessage, isNew: true }, function (messages) {
                // проверяем что пользователь за время запроса осталься в том же диалоге и наличию новых сообщений в ответе от сервера
                if (CheckCurrentChat(sheetId, idInterlocutor) && messages) {
                    // Получаем id самого нового сообщения
                    var currentIdLastMessage = $('#messages').find('[name=message]').last().data('id-message');
                    // Сверяем id самого нового сообщения до загрузки и после загрузки новых сообщений
                    // Если id совпадают то пользователь не осуществлял действий, которые могли привести к обновлению окна переписки
                    if (idLastMessage === currentIdLastMessage) {
                        $('#messages').append(messages);
                        updateAllDateHumanize();
                        scroll();
                        ManMessagesMailsLeft(sheetId, idInterlocutor);
                    }
                }
            }).fail(function (error) {
            });
        }
    }
}

function loadMessages(sheetId, idInterlocutor, newLoading) {
    var loadAllMessages = $('#messages').data('load-all-messages').toLowerCase();
    if (loadAllMessages === 'false') {
        var idLastMessage = $('#messages').data('id-last-message');
        enableSpinnerLoadMessages();
        $.post("/Chats/LoadMessages", { sheetId: sheetId, idInterlocutor: idInterlocutor, idLastMessage: idLastMessage }, function (data) {
            if (CheckCurrentChat(sheetId, idInterlocutor)) {
                if (newLoading) {
                    $('#messages').html(data);
                    scroll(null);
                }
                else {
                    var oldHeight = $('#messages')[0].scrollHeight;
                    $('#messages').prepend(data);
                    scroll(oldHeight);
                }
                updateDataInMessageDiv();
                updateAllDateHumanize();
            }
        }).done(function () {
            disableSpinnerLoadMessages();
        });
    }
}

function loadHistoryChat(e) {
    var idLastMessage = $('#messages').data('id-last-message');
    if (idLastMessage) {
        var scrollTop = e.scrollTop;
        if (scrollTop == 0) {
            var sheetId = $('#manMessagesMails').data('sheet-id');
            var idInterlocutor = $('#interlocutorIdChatHeader').text();
            loadMessages(sheetId, idInterlocutor, false);
        }
    }
}

function CheckCurrentChat(sheetId, idInterlocutor) {
    var currentSheetId = $('#manMessagesMails').data('sheet-id');
    var currentIdInterlocutor = $('#interlocutorIdChatHeader').text();
    if (sheetId === currentSheetId && currentIdInterlocutor === idInterlocutor.toString()) {
        return true;
    }
    else {
        return false;
    }
}

function enableSpinnerLoadMessages() {
    var spinner = $(`<div id="spinnerLoadMessages" class="d-flex justify-content-center my-1">
                        <div class="spinner-grow spinner-grow-sm text-white" role="status">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                     </div>`);
    $('#messages').prepend(spinner);
}

function disableSpinnerLoadMessages() {
    $('#spinnerLoadMessages').remove();
}

function updateDataInMessageDiv() {
    var $inputTemp = $(`#temp_chat`);
    if ($inputTemp) {
        var dataIdLastMessage = $inputTemp.data('id-last-message');
        var dataLoadAllMessage = $inputTemp.data('load-all-messages');
        $('#messages').data('id-last-message', dataIdLastMessage);
        $('#messages').data('load-all-messages', dataLoadAllMessage);
        $inputTemp.remove();
    }
}

function scroll(oldHeight) {
    if (oldHeight) {
        scrollToOldPositionMessages(oldHeight);
    }
    else {
        scrollToEndMessages();
    }
}

function scrollToEndMessages() {
    $('#messages').animate({ scrollTop: $('#messages')[0].scrollHeight }, 0);
    setTimeout(function () {
        $('#messages').animate({ scrollTop: $('#messages')[0].scrollHeight }, 0);
    }, 1000);

    //$('#messages').animate({ scrollTop: $('#messages').outerHeight(true) + 1000 }, 0);
    //setTimeout(function () {
    //    $('#messages').animate({ scrollTop: $('#messages').outerHeight(true) + 1000 }, 0);
    //}, 1000);

    //$('#messages').scrollTop = $('#messages')[].scrollHeight;
    //setTimeout(function () {
    //    $('#messages').scrollTop = $('#messages').scrollHeight;
    //}, 1000);
}

function scrollToOldPositionMessages(oldHeight) {
    $('#messages')[0].scrollTo(0, ($('#messages')[0].scrollHeight - oldHeight));
}

function clearMessageDiv() {
    $('#messages').empty();
    $('#messages').data('load-all-messages', 'false');
    $('#messages').data('id-last-message', '');
}