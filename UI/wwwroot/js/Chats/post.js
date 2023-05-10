var isLoadingHistoryPosts = false;
const $post = $('#post'),
    previewBtn = document.querySelector('#previewPost');
$btnPost = $('#btnPost');


function showHidenPost() {
    if ($btnPost.hasClass('textarea-icon')) {
        var sheetId = $('#manMessagesMails').data('sheet-id');
        var idInterlocutor = $('#interlocutorIdChatHeader').text();
        if (sheetId && idInterlocutor) {
            var idSheetPopup = $post.data('sheet-id');
            var idInterlocutorPopup = $post.data('id-user');
            //createExclusivePostClear();
            if (sheetId === idSheetPopup && idInterlocutor === idInterlocutorPopup) {

            }
            else {
                getHistoryPosts(sheetId, idInterlocutor, 0);
            }
            $post.removeClass('d-none');
        }
    }
}

function getHistoryPosts(sheetId, idInterlocutor, idLastMessage) {
    enableSpinnerInHistoryPosts();
    isLoadingHistoryPosts = true;
    $.post('/Chats/Posts', { sheetId: sheetId, idInterlocutor: idInterlocutor, idLastMessage: idLastMessage }, function (data) {
        var currentSheetId = $('#manMessagesMails').data('sheet-id');
        var currentIdInterlocutor = $('#interlocutorIdChatHeader').text();
        if (currentSheetId == sheetId && currentIdInterlocutor === idInterlocutor) {
            $('#historyPosts').append(data);
        }
        isLoadingHistoryPosts = false;
        disableSpinnerInHistoryPosts();
    }).fail(function () {
        isLoadingHistoryPosts = false;
        disableSpinnerInHistoryPosts();
    });
}

//Обработчик события достижения прокрутки нижнего положения, с целью подгрузки новых постов
$('.history-posts').on('scroll', function () {
    if (Math.abs(this.scrollHeight - this.clientHeight - this.scrollTop) < 1 && !isLoadingHistoryPosts) {
        var sheetId = $('#manMessagesMails').data('sheet-id');
        var idInterlocutor = $('#interlocutorIdChatHeader').text();
        var idLastMessage = $('#historyPosts').find('.history-item').last().data('id-message');
        getHistoryPosts(sheetId, idInterlocutor, idLastMessage);
    }
});

//Инициализирует spinner. Сигнализирующий о загрузке данных.
function enableSpinnerInHistoryPosts() {
    var spinner = $(`#spinner-hostory-posts`);
    spinner.removeClass('d-none');
}

//Скрывает spinner. По окнончании загрузки данных вне зависимости от результата.
function disableSpinnerInHistoryPosts() {
    var spinner = $(`#spinner-hostory-posts`);
    spinner.addClass('d-none');
}

//<----- Check Post ----->
function checkPost(sheetId, idInterlocutor) {
    disabledPost();
    $.post('/Chats/CheckPost', { sheetId, idInterlocutor }, function (data) {
        if (data) {
            enablePost();
        }
    });
}

function enablePost() {
    if ($btnPost.hasClass('textarea-icon-disabled')) {
        $btnPost.removeClass('textarea-icon-disabled');
        $btnPost.addClass('textarea-icon');
    }
}

function disabledPost() {
    if ($btnPost.hasClass('textarea-icon')) {
        $btnPost.removeClass('textarea-icon');
        $btnPost.addClass('textarea-icon-disabled');
    }
}


//<----- Preview Post ----->

//Preview Post
previewBtn.addEventListener('click', () => {
    var text = $(post).find('textarea').val();
    var media = [];
    $(post).find('.uploaded-file .file').each(function () {
        var url = $(this).find('img').attr('src');
        var isVideo = $(this).data('is-video');
        media.push({ Url: url, IsVideo: isVideo });
    });
    previewPost('', text, media);
});

//Preview History Post
function previewHistoryPost(idPost) {
    var $post = $(`#${idPost}`);
    var date = $post.data('date');
    var text = $post.find('.history-item__text').text();
    var mediaString = decodeURIComponent(escape(atob($post.data('media'))));
    var media = JSON.parse(mediaString);
    previewPost(date, text, media);
}

function previewPost(date, text, media) {
    previewPostClear();

    //Avatar owner
    var ownerAvatarSrc = $('#ownerAvatarChatHeader').attr('src');
    $(popUpPreviewPost).find('.avatar__img').attr('src', ownerAvatarSrc);

    //Name interlocutor
    var manName = $('#interlocutorNameChatHeader').text();
    $(popUpPreviewPost).find('.man__name').text(manName);

    //Date sent
    $(popUpPreviewPost).find('.date').text(date);

    //Text
    $(popUpPreviewPost).find('.preview__text').text(text);

    //media
    if (media) {
        for (var i = 0; i < media.length; i++) {
            var img = document.createElement('img');
            img.src = media[i].Url;

            var previewCard = document.createElement('div');
            previewCard.classList.add('preview-card');
            if (media[i].IsVideo) {
                previewCard.classList.add('video');
            }

            previewCard.appendChild(img);
            $(popUpPreviewPost).find('.preview-body-media').append(previewCard);
        }
    }

    popUpPreviewPost.classList.remove('d-none');
}

function removeMediaFile(e) {
    $(e).closest('div .file').hide(1000, function () {
        $(this).remove();
    });
}

function previewPostClear() {
    $(popUpPreviewPost).find('.avatar__img').attr('src', '');
    $(popUpPreviewPost).find('.man__name').text('');
    $(popUpPreviewPost).find('.date').text('');
    $(popUpPreviewPost).find('.preview__text').text('');
    $(popUpPreviewPost).find('.preview-body-media').empty();
}

function createExclusivePostClear() {
    $(post).find('textarea').val('');
    $(post).find('.uploaded-file').empty();
    $(`#sentPost`).addClass('disabled');
    $(`#previewPost`).addClass('disabled');
}