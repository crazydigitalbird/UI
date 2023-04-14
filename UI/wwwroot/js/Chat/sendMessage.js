function pressEnter(e, userId) {
    if (e.keyCode == 13) {
        sendMessage(userId);
    }
}

function sendMessage() {
    var message = $('#newMessage').val();
    if (message) {
        send('Message', message);
    }
}

function sendSticker(e, stickerId) {
    var stickerUrl = $(e).siblings('img')[0].src;
    var message = `${stickerId};${stickerUrl}`
    hidenStickers();
    send('Sticker', message);
}

function send(messageType, message) {

    var idRegularUser = $('#usersArea li.active')[0].id.replace('user_', '');

    if (allowedSendMessage(idRegularUser)) {
        var sheetId = $('#sheetId').val();
        var ownerAvatar = $('#ownerAvatar')[0].src;

        $.post('/Chat/SendMessage', { sheetId: sheetId, idRegularUser: idRegularUser, messageType: messageType, message: message, ownerAvatar: ownerAvatar }, function (data) {
            if (messageType === 'Message') {
                $('#newMessage').val('');
            }
            $(`#messages_${idRegularUser}`).append(data);
            scrollToEndMessagesArea(idRegularUser);
            reduceMessagesLeft(idRegularUser)

        }).fail(function () {

        });
    }
}

function allowedSendMessage(idRegularUser) {
    var $messagesLeft = $(`#messagesLeft_${idRegularUser}`)
    if ($messagesLeft) {
        var messagesLeft = Number($messagesLeft.text());
        if (messagesLeft > 0) {
            return true;
        }
    }
    return false;
}

function reduceMessagesLeft(idRegularUser) {
    var $messagesLeft = $(`#messagesLeft_${idRegularUser}`)
    if ($messagesLeft) {
        var messagesLeft = Number($messagesLeft.text());
        var newMessagesLeft = messagesLeft - 1;
        if (newMessagesLeft >= 0) {
            $messagesLeft.text(newMessagesLeft);
        }
    }
}