function sendMessage() {
    var message = $('#newMessage').val();
    if (message) {
        send('Message', message);
    }
}

function send(messageType, message) {
    if (allowedSendMessages(1)) {
        var idRegularUser = $('#interlocutorIdChatHeader').text();
        var sheetId = $('#manMessagesMails').data('sheet-id');
        var idLastMessage = $('#messages').find('[name=message]').last().data('id-message');
        //var ownerAvatar = $('#ownerAvatarChatHeader').attr('src');
        $.post('/Chats/SendMessage', { sheetId: sheetId, idRegularUser: idRegularUser, messageType: messageType, message: message, idLastMessage: idLastMessage }, function (data) {
            var idRegularUserCurrent = $('#interlocutorIdChatHeader').text();
            var sheetIdCurrent = $('#manMessagesMails').data('sheet-id');
            if (idRegularUser === idRegularUserCurrent && sheetId === sheetIdCurrent) {
                if (messageType === 'Message') {
                    $('#newMessage').val('');
                    changeCounter();
                }
                $(`#messages`).append(data);
                scrollToEndMessages();
                reduceMessagesLeft()
            }
        }).fail(function () {

        });
    }
}

//Проверка доступности отправки сообщений
function allowedSendMessages(count) {
    var $messagesLeft = $('#messagesLeft');
    if ($messagesLeft) {
        var messagesLeft = Number($messagesLeft.text());
        if ((messagesLeft - count) >= 0) {
            return true;
        }
    }
    return false;
}

//Уменьшаем счетчик доступных сообщений
function reduceMessagesLeft() {
    var $messagesLeft = $('#messagesLeft');
    if ($messagesLeft) {
        var messagesLeft = Number($messagesLeft.text());
        var newMessagesLeft = messagesLeft - 1;
        if (newMessagesLeft >= 0) {
            $messagesLeft.text(newMessagesLeft);
        }
    }
}

//Обновление счетчика размера текстового сообщения в TextArea
function changeCounter() {
        var count = $('#newMessage').val().length;
        var messageCounter = $('#messageLength');
        messageCounter.text(count);
}

function sendEnter(event) {
    if (event.keyCode == 13) {
        sendMessage();
    }
}