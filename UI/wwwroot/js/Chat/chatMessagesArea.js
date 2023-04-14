var $plugMessages = $('#plugMessages');
var $plugChatMenu = $('#plugChatMenu');

$(function () {
    var freeMessagesAreaHeight = $(window).height() - $('#navMenu').outerHeight(true) - $('#divSearch').outerHeight(true) - $('#messageSend').outerHeight(true) - $('#contentSend').outerHeight(true) - 16 - 48 - 32;
    $plugMessages.css("height", freeMessagesAreaHeight);

    var plugChatMenuHeight = $('#divSearch').outerHeight(true);
    $plugChatMenu.css("height", plugChatMenuHeight);
});

function loadChat(chatId) {
    var sheetId = $('#sheetId').val();

    hidenAllChatMenu();
    hidenAllMessages();
    $plugChatMenu.removeClass('d-none');
    $plugMessages.removeClass('d-none');

    var $messagesDiv = $(`#messages_${chatId}`);
    var $chatMenu = $(`#chatMenu_${chatId}`);

    if (!$messagesDiv.length > 0) {
        $messagesDiv = $(`<div id="messages_${chatId}" name="messages"
                            class="pt-3 pe-3 overflow-auto"
                            data-id-last-message="0" data-is-load-messages="false" data-load-all-messages="false"
                            style="position: relative;" onscroll="loadHistoryChat(this, ${chatId})">
                          </div>`);
        setHeightMessages($messagesDiv);
        $('#messagesArea').prepend($messagesDiv);
        $plugMessages.addClass('d-none');

        var $user = $(`#user_${chatId}`);
        var avatarUrl = $user.find('img').first().attr('src');
        var userName = $user.find('p[name=userName]').first().text().split(',')[0];
        $.post("/Chat/ManMessagesMails", { sheetId: sheetId, idRegularUser: chatId, userName: userName, avatarUrl: avatarUrl }, function (data) {
            $('#messagesArea').prepend(data);
            if ($messagesDiv.hasClass('d-none')) {
                $(`#chatMenu_${chatId}`).addClass('d-none');
            }
            else {
                $plugChatMenu.addClass('d-none');
            }
        });
    }
    else {
        $plugChatMenu.addClass('d-none');
        $chatMenu.removeClass('d-none');

        $plugMessages.addClass('d-none');
        $messagesDiv.removeClass('d-none');
    }
    var isLoadMessages = $messagesDiv.data('is-load-messages');
    if (!isLoadMessages) {
        var idLastMessage = $messagesDiv.data('id-last-message');
        enableSpinnerLoadMessages($messagesDiv);
        $.post("/Chat/LoadMessages", { sheetId: sheetId, chatId: chatId, idLastMessage: idLastMessage }, function (data) {
            $messagesDiv.prepend(data);
            updateDataInMessageDiv($messagesDiv, chatId);
            $messagesDiv.data('is-load-messages', true);
        }).done(function () {
            disableSpinnerLoadMessages();
            scrollToEndMessagesArea(chatId);
        });
    }
}

function setHeightMessages($messagesDiv) {
    var freeMessagesAreaHeight = $(window).height() - $('#navMenu').outerHeight(true) - $('#divSearch').outerHeight(true) - $('#messageSend').outerHeight(true) - $('#contentSend').outerHeight(true) - 16 - 48 - 32 - 8;
    $messagesDiv.css("height", freeMessagesAreaHeight);
}

function loadHistoryChat(e, chatId) {
    var scrollTop = e.scrollTop;
    if (scrollTop == 0) {
        var loadAllMessages = $(e).data('load-all-messages').toLowerCase();
        if (loadAllMessages === 'false') {
            var $messagesDiv = $(`#messages_${chatId}`);
            var idLastMessage = $messagesDiv.data('id-last-message');
            var sheetId = $('#sheetId').val();
            enableSpinnerLoadMessages($messagesDiv);
            $.post("/Chat/LoadMessages", { sheetId: sheetId, chatId: chatId, idLastMessage: idLastMessage }, function (data) {
                var oldHeight = $messagesDiv[0].scrollHeight;
                $messagesDiv.prepend(data);
                scrollToOldPositionMessagesArea(chatId, oldHeight);
                updateDataInMessageDiv($messagesDiv, chatId);
            }).done(function () {
                disableSpinnerLoadMessages();
            });
        }
    }
}

function hidenAllMessages() {
    $('#messagesArea').find('div[name=messages]').each(function () {
        if (!$(this).hasClass('d-none')) {
            $(this).addClass('d-none');
        }
    });
}

function hidenAllChatMenu() {
    $('#messagesArea').find('div[name=chatMenu]').each(function () {
        if (!$(this).hasClass('d-none')) {
            $(this).addClass('d-none');
        }
    });
}

function enableSpinnerLoadMessages($messagesDiv) {
    var spinner = $(`<div id="spinnerLoadMessages" class="d-flex justify-content-center my-1">
                        <div class="spinner-grow spinner-grow-sm text-success" role="status">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                     </div>`);
    $messagesDiv.prepend(spinner);
}

function disableSpinnerLoadMessages() {
    $('#spinnerLoadMessages').remove();
}

function updateDataInMessageDiv(messageDiv, chatId) {
    var $inputTemp = $(`#temp_${chatId}`);
    if ($inputTemp) {
        var dataIdLastMessage = $inputTemp.data('id-last-message');
        var dataLoadAllMessage = $inputTemp.data('load-all-messages');
        messageDiv.data('id-last-message', dataIdLastMessage);
        messageDiv.data('load-all-messages', dataLoadAllMessage);
        $inputTemp.remove();
    }
}

function copyIdInterlocutor(e) {
    var idInterlocutor = e.text;
    navigator.clipboard.writeText(idInterlocutor);
}

function scrollToEndMessagesArea(chatId) {
    var currentMessagesArea = document.getElementById(`messages_${chatId}`);
    currentMessagesArea.scrollTop = currentMessagesArea.scrollHeight;
    setTimeout(function () {
        currentMessagesArea.scrollTop = currentMessagesArea.scrollHeight;
    }, 1000);
}

function scrollToOldPositionMessagesArea(chatId, oldHeight) {
    var currentMessagesArea = document.getElementById(`messages_${chatId}`);
    currentMessagesArea.scrollTo(0, (currentMessagesArea.scrollHeight - oldHeight));
}







function showHidenStickers() {
    var $stickers = $('#stickers');
    if ($stickers.hasClass('d-none')) {
        $('#stickers').removeClass('d-none');
    }
    else {
        hidenStickers();
    }
}

function hidenStickers() {
    $('#stickers').addClass('d-none');
}

//Hiden the stickers when clicked outside of it 
$(document).on('click', function (e) {
    var $stickers = $("#stickers");
    var $btnStickers = $('#btnStickers');
    if ($stickers.has(e.target).length === 0 && $btnStickers.has(e.target).length === 0 && !$stickers.hasClass('d-none')) {
        hidenStickers();
    }
})




//function readAllMessages(chatId) {
//    var $spanNumberOfUnreadMessages = $(`#numberOfUnreadMessages_${chatId}`);
//    if (!$spanNumberOfUnreadMessages.hasClass('d-none')) {
//        $spanNumberOfUnreadMessages.fadeOut(2000, function () {
//            $spanNumberOfUnreadMessages.text('');

//            // HTTP POST Read all messages
//        });
//    }
//}

function addNewMessages(chatId, numberOfNewMessages) {
    var $spanNumberOfUnreadMessages = $(`#numberOfUnreadMessages_${chatId}`);
    if ($spanNumberOfUnreadMessages.css('display') == 'none') {
        $spanNumberOfUnreadMessages.text(numberOfNewMessages);
        $spanNumberOfUnreadMessages.fadeIn(2000);
    }
    else {
        allNewMessages = Number($spanNumberOfUnreadMessages.text()) + numberOfNewMessages;
        $spanNumberOfUnreadMessages.text(allNewMessages);
    }
}