var $messagesDiv = $('#messages');

function loadMessages(sheetId, idInterlocutor, newLoading) {
    var loadAllMessages = $messagesDiv.data('load-all-messages').toLowerCase();
    if (loadAllMessages === 'false') {
        var idLastMessage = $messagesDiv.data('id-last-message');
        enableSpinnerLoadMessages();
        $.post("/Chats/LoadMessages", { sheetId: sheetId, idInterlocutor: idInterlocutor, idLastMessage: idLastMessage }, function (data) {
            if (CheckCurrentChat(sheetId, idInterlocutor)) {
                if (newLoading) {
                    $messagesDiv.html(data);
                    scroll(null);
                }
                else {
                    var oldHeight = $messagesDiv[0].scrollHeight;
                    $messagesDiv.prepend(data);
                    scroll(oldHeight);
                }
                updateDataInMessageDiv();
            }
        }).done(function () {
            disableSpinnerLoadMessages();
        });
    }
}

function loadHistoryChat(e) {
    var idLastMessage = $messagesDiv.data('id-last-message');
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
    $messagesDiv.prepend(spinner);
}

function disableSpinnerLoadMessages() {
    $('#spinnerLoadMessages').remove();
}

function updateDataInMessageDiv() {
    var $inputTemp = $(`#temp_chat`);
    if ($inputTemp) {
        var dataIdLastMessage = $inputTemp.data('id-last-message');
        var dataLoadAllMessage = $inputTemp.data('load-all-messages');
        $messagesDiv.data('id-last-message', dataIdLastMessage);
        $messagesDiv.data('load-all-messages', dataLoadAllMessage);
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
    $messagesDiv.animate({ scrollTop: $messagesDiv.outerHeight(true) + 1000 }, 0);
    setTimeout(function () {
        $messagesDiv.animate({ scrollTop: $messagesDiv.outerHeight(true) + 1000 }, 0);
    }, 1000);
    //$messagesDiv.scrollTop = $messagesDiv[].scrollHeight;
    //setTimeout(function () {
    //    $messagesDiv.scrollTop = $messagesDiv.scrollHeight;
    //}, 1000);
}

function scrollToOldPositionMessages(oldHeight) {
    $messagesDiv[0].scrollTo(0, ($messagesDiv[0].scrollHeight - oldHeight));
}

function clearMessageDiv() {
    $messagesDiv.empty();
    $messagesDiv.data('load-all-messages', 'false');
    $messagesDiv.data('id-last-message', '');
}