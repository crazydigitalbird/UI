﻿function updateManMessagesMails(owner, interlocutor) {
    var sheetId = $('#manMessagesMails').data('sheet-id');
    var idInterlocutor = Number($('#interlocutorIdChatHeader').text());
    if (sheetId != owner.SheetId || idInterlocutor != interlocutor.Id) {
        $('#manMessagesMails').data('sheet-id', owner.SheetId);

        $('#ownerIdChatHeader').text(owner.Id);
        $('#ownerNameChatHeader').text(owner.Name);
        $('#ownerAvatarChatHeader').attr('src', owner.Avatar);

        $('#interlocutorIdChatHeader').text(interlocutor.Id);
        $('#interlocutorNameChatHeader').text(interlocutor.Name);
        $('#interlocutorAvatarChatHeader').attr('src', interlocutor.Avatar);

        if ($('#chatDropdown').hasClass('disabled')) {
            $('#chatDropdown').removeClass('disabled');
        }

        $('#bookmark-svg').toggleClass('symbol-checked', interlocutor.IsBookmarked);
        $('#premium-svg').toggleClass('symbol-checked', interlocutor.IsPremium);
        $('#trash-svg').toggleClass('symbol-checked', interlocutor.IsTrash);

        $.post('/Chats/ManMessagesMails', { sheetId: owner.SheetId, idRegularUser: interlocutor.Id }, function (data) {
            $('#messagesLeft').text(data.messagesLeft || 0);
            $('#mailsLeft').text(data.mailsLeft || 0);
        });

        checkGifts(owner.SheetId, interlocutor.Id);
        checkPost(owner.SheetId, interlocutor.Id);
    }

    clearMessageDiv();
    loadMessages(owner.SheetId, interlocutor.Id, true);
}

function changeBookmarkChat(e) {
    var sheetId = $('#manMessagesMails').data('sheet-id');
    var idRegularUser = $('#interlocutorIdChatHeader').text();
    var addBookmark = !$('#bookmark').hasClass('symbol-checked');
    $.post('/Chats/ChangeBookmark', { sheetId: sheetId, idRegularUser: idRegularUser, addBookmark: addBookmark }, function () {
        $('#bookmark-svg').toggleClass('symbol-checked');
    }).fail(function () {
        console.log('fail change bookmark')
    });
}

function changePremiumChat(e) {
    var sheetId = $('#manMessagesMails').data('sheet-id');
    var idRegularUser = $('#interlocutorIdChatHeader').text();
    var addPremium = !$('#premium-svg').hasClass('symbol-checked');
    $.post('/Chats/ChangePremium', { sheetId: sheetId, idRegularUser: idRegularUser, addPremium: addPremium }, function () {
        $('#premium-svg').toggleClass('symbol-checked');
    }).fail(function () {
        console.log('fail change pin')
    });
}

function changeBlockChat(e) {
    //$('#block-svg').toggleClass('symbol-checked');
}

function changeTrashChat(e) {
    var sheetId = $('#manMessagesMails').data('sheet-id');
    var idRegularUser = $('#interlocutorIdChatHeader').text();
    var addTrash = !$('#trash-svg').hasClass('symbol-checked');
    $.post('/Chats/ChangeTrash', { sheetId: sheetId, idRegularUser: idRegularUser, addTrash: addTrash }, function () {
        $('#trash-svg').toggleClass('symbol-checked');
    }).fail(function () {
        console.log('fail change trash')
    });
}

function copyId(e) {
    var id = e.textContent;
    navigator.clipboard.writeText(id);
}