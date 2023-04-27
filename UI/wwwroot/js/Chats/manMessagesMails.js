function updateManMessagesMails(owner, interlocutor) {
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

    $('#bookmark').toggleClass('symbol-checked', interlocutor.IsBookmarked);
    $('#premium').toggleClass('symbol-checked', interlocutor.IsPinned);

    $.post('/Chats/ManMessagesMails', { sheetId: owner.SheetId, idRegularUser: interlocutor.Id }, function (data) {
        $('#messagesLeft').text(data.messagesLeft);
        $('#mailsLeft').text(data.mailsLeft);
    });    
}

function changeBookmarkChat(e) {    
    var sheetId = $('#manMessagesMails').data('sheet-id');
    var idRegularUser = $('#interlocutorIdChatHeader').text();
    var addBookmark = !$('#bookmark').hasClass('symbol-checked');
    $.post('/Chats/ChangeBookmark', { sheetId: sheetId, idRegularUser: idRegularUser, addBookmark: addBookmark }, function () {
        $('#bookmark').toggleClass('symbol-checked');
    }).fail(function () {
        console.log('fail change bookmark')
    });
}

function changePremiumChat(e) {
    var sheetId = $('#manMessagesMails').data('sheet-id');
    var idRegularUser = $('#interlocutorIdChatHeader').text();
    var addPin = !$('#premium').hasClass('symbol-checked');
    $.post('/Chats/ChangePin', { sheetId: sheetId, idRegularUser: idRegularUser, addPin: addPin }, function () {
        $('#premium').toggleClass('symbol-checked');
    }).fail(function () {
        console.log('fail change pin')
    });  
}

function changeBlockChat(e) {
    //$('#block').toggleClass('symbol-checked');
}

function changeTrashChat(e) {
    //$('#trash').toggleClass('symbol-checked');
}