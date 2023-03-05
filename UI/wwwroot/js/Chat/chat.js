$(function () {
    var $firstLiUsersArea = $('#usersArea').find('li').first();
    $firstLiUsersArea.addClass('list-group-item active');
    $('#messagesArea').find('div[name=messages]').each(function (i) {
        if (i != 0) {
            $(this).addClass('d-none');
        }
    });

    setHeight();

    $(window).resize(function () {
        setHeight();
    });

    var activeChatId = Number($firstLiUsersArea[0].id.replace('user_', ''));
    readAllMessages(activeChatId);
    scrollToEndMessagesArea(activeChatId);
})

function setHeight() {
    var freeAreaHeight = $(window).height() - $('#navMenu').outerHeight(true) - $('#divSearch').outerHeight(true) - 16 - 48 - 32;
    $("#usersArea").css("max-height", freeAreaHeight);
    $('#messagesArea').find('div[name=messages]').each(function () {
        $(this).css("height", freeAreaHeight);
    });
}

function searchChat(e) {
    var search = e.value.toLowerCase();
    $('#usersArea').find('li').each(function () {
        var chatId = this.id.replace('user_', '').toLowerCase();
        var userName = $(this).find('p[name=userName]').first().text().toLowerCase();
        if (search && !(userName.includes(search) || chatId.includes(search))) {
            $(this).addClass('d-none');
        }
        else {
            $(this).removeClass('d-none');
        }
    });
}

function activatingChat(chatId) {
    var $liUsersArea = $(`#user_${chatId}`).closest('li');

    if (!$liUsersArea.hasClass('active')) {

        $('#usersArea').find('li').each(function () {
            if ($(this).hasClass('active')) {
                $(this).removeClass('list-group-item active');
            }
        });

        $liUsersArea.addClass('list-group-item active');

        $('#messagesArea').find('div[name=messages]').each(function () {
            if (!$(this).hasClass('d-none')) {
                $(this).addClass('d-none');
            }
        });

        $(`#messages_${chatId}`).removeClass('d-none');

        readAllMessages(chatId);
        scrollToEndMessagesArea(chatId);
    }
}

function readAllMessages(chatId) {
    var $spanNumberOfUnreadMessages = $(`#numberOfUnreadMessages_${chatId}`);
    if (!$spanNumberOfUnreadMessages.hasClass('d-none')) {
        $spanNumberOfUnreadMessages.fadeOut(2000, function () {
            $spanNumberOfUnreadMessages.text('');

            // HTTP POST Read all messages
        });
    }
}

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

function scrollToEndMessagesArea(chatId) {
    var currentMessagesArea = document.getElementById(`messages_${chatId}`);
    currentMessagesArea.scrollTo(0, $(currentMessagesArea).outerHeight());
}

function pressEnter(e, userId) {
    if (e.keyCode == 13) {
        sendMessage(userId);
    }
}

function sendMessage(userId) {
    var chatId = $('#usersArea li.active')[0].id.replace('user_', '');
    var $inputNewMessage = $('#newMessage');
    var message = $inputNewMessage.val();

    if (message) {
        $.post('Chat/SendMessage', { userId: userId, chatId: chatId, message: message }, function (data) {

            $inputNewMessage.val('');
            var ownerAwatarHtml = $('#ownerAvatar')[0].outerHTML;

            $(`#messages_${chatId}`).append(`<div class="d-flex flex-row justify-content-start">
                                                ${ownerAwatarHtml}
                                                <div>
                                                    <p class="small p-2 ms-3 mb-1 rounded-3 text-black" style="background-color: #e3ebf7;">
                                                        ${message}
                                                    </p>
                                                    <p class="small ms-3 mb-3 rounded-3 text-muted float-end">${data}</p>
                                                </div>
                                              </div>`);
            scrollToEndMessagesArea(chatId);

        }).fail(function () {

        });
    }
}
