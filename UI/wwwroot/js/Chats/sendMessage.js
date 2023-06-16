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
        var date = Date.now();

        if (messageType === 'Message') {
            var ownerAvatar = $('#ownerAvatarChatHeader').attr('src');
            var tempMessage = $(`<div id="tempMessage-${date}-${idLastMessage}" class="large-block__body-item mt-2 me-1">
                                    <div class="large-block__body-item-top d-flex align-items-end flex-row-reverse">
                                        <div class="large-block__body-item-image position-relative ms-2">
                                            <div class="standart-image-block position-relative">
                                                <img src="${ownerAvatar}" alt="avatar" class="small-image rounded">
                                            </div>
                                        </div>
                                        <div class="large-block__body-item-message-answer rounded-start d-flex align-items-center justify-content-start p-2 rounded-top">
                                            <p class="text-white large-block__body-item-text">
                                                ${message}
                                                <span class="ms-1">
                                                    <svg width="10px" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512">
                                                        <path fill="white" d="M256 0a256 256 0 1 1 0 512A256 256 0 1 1 256 0zM232 120V256c0 8 4 15.5 10.7 20l96 64c11 7.4 25.9 4.4 33.3-6.7s4.4-25.9-6.7-33.3L280 243.2V120c0-13.3-10.7-24-24-24s-24 10.7-24 24z"/>
                                                    </svg>
                                                </span>
                                            </p>
                                        </div>
                                    </div>
                                </div>`);

            $('#newMessage').val("");
            changeCounter();
            $(`#messages`).append(tempMessage);
            scrollToEndMessages();
        }

        $.post('/Chats/SendMessage', { sheetId: sheetId, idRegularUser: idRegularUser, messageType: messageType, message: message, idLastMessage: idLastMessage }, function (data) {

            //Timer
            stopTimer(sheetId, idRegularUser, idLastMessage);

            var idRegularUserCurrent = $('#interlocutorIdChatHeader').text();
            var sheetIdCurrent = $('#manMessagesMails').data('sheet-id');
            if (idRegularUser === idRegularUserCurrent && sheetId === sheetIdCurrent) {
                if (messageType === 'Message') {
                    $(`#tempMessage-${date}-${idLastMessage}`).replaceWith(data);
                }
                else {
                    $(`#messages`).append(data);
                }
                reduceMessagesLeft();
                updateAllDateHumanize();
                scrollToEndMessages();
            }

            ////SignalR
            //replyToNewMessage(sheetId, Number(idRegularUser), idLastMessage);

            ////Remove
            //$(`#${sheetId}-${idRegularUser}-${idLastMessage}`).animate({ opacity: 0.25 }, 3000, function () {
            //    $(this).remove();
            //});

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
        if (newMailsLeft === 0) {
            $messagesLeft.addClass('large-block-header-timer-text-opacity');
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
        event.preventDefault();
    }
}