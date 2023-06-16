function showComments() {
    stopAnimationCommentBtn();
    var sheetId = $('#manMessagesMails').data('sheet-id');
    var idRegularUser = $('#interlocutorIdChatHeader').text();
    if (sheetId && idRegularUser) {
        $.post('Chats/Comments', { sheetId, idRegularUser }, function (comments) {
            $('#commentPopUpBody').html(comments);
            $('#comment').find('textarea').val('');
            //$('#modalComments').find('.modal-tittle').text(`${row['name']} ${row['lastName']}`);
            if ($('#comment').hasClass('d-none')) {
                $('#comment').removeClass('d-none');
            }
        }).fail(function (error) {
        });
    }
}

function addComment() {
    var $textAreaComment = $('#comment').find('textarea');
    if ($textAreaComment.val()) {
        var sheetId = $('#manMessagesMails').data('sheet-id');
        var idRegularUser = $('#interlocutorIdChatHeader').text();
        $.post('Chats/AddComment', { sheetId, idRegularUser, text: $textAreaComment.val() }, function (comment) {
            if (!$('#comment').hasClass('d-none')) {
                $('#comment').addClass('d-none');
            }
        }).fail(function (error) {
        });
    }
}

function checkNewComments(sheetId, idRegularUser) {
    stopAnimationCommentBtn();

    $.post('Chats/GetNewDialogueCommentsCount', { sheetId, idRegularUser }, function (count) {
        if (count > 0) {
            if (!$('#commentBtn').hasClass('flicker')) {
                $('#commentBtn').addClass('flicker')
            }
        }
    });
}

function stopAnimationCommentBtn() {
    if ($('#commentBtn').hasClass('flicker')) {
        $('#commentBtn').removeClass('flicker')
    }
}