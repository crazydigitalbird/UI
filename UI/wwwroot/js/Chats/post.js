var isLoadingHistoryPosts = false;
const $post = $('#post'),
    previewBtn = document.querySelector('#previewPost'),
    $btnPost = $('#btnPost');

//Сортировка фото и видео путем перетаскивания элементов на новые позиции.
$post.find('.uploaded-file').sortable({
    revert: true,
    opacity: 0.5
});

//Отображает окно создания эксклюзивного поста
function showHidenPost() {
    if ($btnPost.hasClass('textarea-icon')) {
        var sheetId = $('#manMessagesMails').data('sheet-id');
        var idInterlocutor = $('#interlocutorIdChatHeader').text();
        if (sheetId && idInterlocutor) {
            var idSheetPopup = $post.data('sheet-id');
            var idInterlocutorPopup = $post.data('id-user');            
            if (sheetId === idSheetPopup && idInterlocutor === idInterlocutorPopup) {

            }
            else {
                createExclusivePostClear();
                historyPostsClear();
                getHistoryPosts(sheetId, idInterlocutor, 0);
            }
            $post.removeClass('d-none');
        }
    }
}

//Загрузка истории экслюзивных постов
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
    //var $post = $(`#${idPost}`);
    //var date = $post.data('date');
    //var text = $post.find('.history-item__text').text();
    //var mediaString = decodeURIComponent(escape(atob($post.data('media'))));
    //var media = JSON.parse(mediaString);

    idPost = idPost.replace('post_', '');
    var sheetId = $('#manMessagesMails').data('sheet-id');
    var idInterlocutor = $('#interlocutorIdChatHeader').text();

    $.post('/Chats/PreviewPost', { sheetId, idInterlocutor, idPost }, function (data) {
        var media = [];
        for (var i = 0; i < data.photos.length; i++) {
            media.push({ Url: data.photos[i].url, IsVideo: false });
        }
        for (var i = 0; i < data.videos.length; i++) {
            media.push({ Url: data.videos[i].url, IsVideo: true });
        }
        previewPost(data.dateSent, data.text, media);
    }).fail(function () {

    });
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
    var stringDate = moment(date).format('HH:mm DD.MM.YYYY');
    $(popUpPreviewPost).find('.date').text(stringDate);

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
        checkSendPost();
    });
}

function previewPostClear() {
    $(popUpPreviewPost).find('.avatar__img').attr('src', '');
    $(popUpPreviewPost).find('.man__name').text('');
    $(popUpPreviewPost).find('.date').text('');
    $(popUpPreviewPost).find('.preview__text').text('');
    $(popUpPreviewPost).find('.preview-body-media').empty();
}

function historyPostsClear() {
    $('#historyPosts').empty();
}

function createExclusivePostClear() {
    $(post).find('textarea').val('');
    $(post).find('.uploaded-file').empty();
    $(`#sentPost`).addClass('disabled');
}

//<----- Send Post ----->
$('#sentPost').on('click', sendPost);
$(post).find('textarea').on('input', checkSendPost);


function checkSendPost() {
    var textLength = $(post).find('textarea').val().length;
    var countVideo = 0;
    var countPhoto = 0;

    var textPostLength = $('#textPostLength');
    textPostLength.text(textLength);

    $(post).find('.uploaded-file .file').each(function () {
        var isVideo = $(this).data('is-video');
        if (isVideo) {
            countVideo = countVideo + 1;
        }
        else {
            countPhoto = countPhoto + 1;
        }
    });
    
    if (allowedSendMessages(1) && textLength >= 200 && textLength <= 3500 && (countVideo >= 1 || countPhoto >= 4)) {
        if($('#sentPost').hasClass('disabled'))
        {
            $('#sentPost').removeClass('disabled')
        }
    }
    else{
        if (!$('#sentPost').hasClass('disabled')) {
            $('#sentPost').addClass('disabled')
        }
    }
}

function sendPost() {
    var sheetId = $('#manMessagesMails').data('sheet-id');
    var idRegularUser = $('#interlocutorIdChatHeader').text();
    var idLastMessage = $('#messages').find('[name=message]').last().data('id-message');
    var text = $(post).find('textarea').val();
    var videos = [];
    var photos = [];

    $(post).find('.uploaded-file .file').each(function () {
        var isVideo = $(this).data('is-video');
        var url = $(this).find('img')[0].src;
        var id = $(this).data('id');
        if (isVideo) {
            videos.push({Id: id, Url: url});
        }
        else {
            photos.push({ Id: id, Url: url });
        }
    });

    $.post('/Chats/SendPost', { sheetId, idRegularUser, idLastMessage, text, videos, photos }, function (data) {

        //Timer
        stopTimer(sheetId, idRegularUser, idLastMessage);

        var idRegularUserCurrent = $('#interlocutorIdChatHeader').text();
        var sheetIdCurrent = $('#manMessagesMails').data('sheet-id');
        if (sheetId === sheetIdCurrent && idRegularUser === idRegularUserCurrent) {
            $(`#messages`).append(data);
            scrollToEndMessages();
            reduceMessagesLeft()
        }
        createExclusivePostClear();
    }).fail(function () {

    });
    $post.addClass('d-none');
}
