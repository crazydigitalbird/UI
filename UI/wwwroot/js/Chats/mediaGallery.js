var isLoadingGallery = {};

//Отправка медиа файлов.
$('#toSendMedia').on('click', function () {
    var isPost = $(popUpGallery).data('is-post');
    if (isPost) {
        $('.gallery-card.checkedCard').each(function () {
            sendToPopUpPost(this);
        });
        popUpGallery.classList.add('d-none');
    }
    else {
        var count = getCheckedCard();
        if (allowedSendMessages(count)) {
            $('.gallery-card.checkedCard').each(function () {
                sendMedia(this);
            });
            popUpGallery.classList.add('d-none');
        }
    }
});

//Воспроизведение видео
//$('[name=play]').on('click', function () {
//    $(this).prev().removeClass('d-none');
//});

function sendMedia(galleryCard) {
    var url = $(galleryCard).find('img')[0].src;
    var mediaId = $(galleryCard).data('id');
    var message = `${mediaId};${url}`

    var messageType = 'photo';
    if ($(galleryCard).is('.video')) {
        messageType = 'video';
    } else if ($(galleryCard).is('.audio')) {
        messageType = 'audio';
    }
    send(messageType, message);
}

function sendToPopUpPost(galleryCard) {
    var url = $(galleryCard).find('img')[0].src;
    var isVideo = $(galleryCard).is('.video');
    var mediaId = $(galleryCard).data('id');
    var file = $(`<div class="file" data-is-video="${isVideo}">
                                <img src="${url}" alt="">
                                <span class="remove-file" onclick="removeMediaFile(this)">&#x2715</span>
                            </div>`);
    $(post).find('.uploaded-file').append(file);
}

//Отправка выделенных медиа файлов на вкладку отправленные.
$('#markAsSent').on('click', function () {
    $('.gallery-card.checkedCard').each(function () {
        $('#sentMedia').append($(this));
    });
    setCounterSelectedMedia(0);
    setCounterSentMedia();
    unCheckedCard();
});

//Обработчик события достижения прокрутки нижнего положения, с целью подгрузки новых медиа файлов
$('.gallery-box').on('scroll', function () {
    var currentTab = getCurrentTabGallery();
    if (Math.abs(this.scrollHeight - this.clientHeight - this.scrollTop) < 100 && !isLoadingGallery[currentTab]) {
        var sheetId = $('#manMessagesMails').data('sheet-id');
        var idUser = $('#interlocutorIdChatHeader').text
        var isPostPopup = $(popUpGallery).data('is-post');
        getPhotos(sheetId, idUser, false, isPostPopup);
    }
});

$(document).ready(function () {
    setFilterGaller();
});

// filter gallery
function setFilterGaller() {
    $('.gallery-item').hide();
    $('.gallery-item:first-child').show();
    $('.filter__btn').click(function () {
        let pageInfo = $(this).attr('data-radio');
        $('.gallery-item').hide();
        var currentTab = $('.' + pageInfo);
        currentTab.show();
        if (currentTab.is(':empty') && !currentTab.data('cursor')) {
            var sheetId = $('#manMessagesMails').data('sheet-id');
            var idUser = $('#interlocutorIdChatHeader').text();
            getPhotos(sheetId, idUser, true);
        }
    });
}

function showGallery(isPost) {
    var sheetId = $('#manMessagesMails').data('sheet-id');
    var idUser = $('#interlocutorIdChatHeader').text();
    if (sheetId && idUser) {
        var idSheetPopup = $(popUpGallery).data('sheet-id');
        var idUserPopup = $(popUpGallery).data('id-user');
        var isPostPopup = $(popUpGallery).data('is-post');

        if (sheetId === idSheetPopup && idUser === idUserPopup && isPostPopup === isPost) {
            unCheckedCard();
            setCounterSelectedMedia(0);
            popUpGallery.classList.remove('d-none');
        }
        else {
            setHeaderGallery(sheetId, idSheetPopup);
            clearGallery();
            $('.gallery-item').hide();
            $('.gallery-item:first-child').show();
            getPhotos(sheetId, idUser, true, isPost)
        }
    }
}

function getPhotos(sheetId, idUser, newLoading, isPost) {
    popUpGallery.classList.remove('d-none');
    var currentTab = getCurrentTabGallery();
    if (!isLoadingGallery[currentTab]) {
        var cursor = $(`#${currentTab}`).data('cursor');

        // Если новая загрузка или cursor не пустой, то отправляем запросо на получения фото
        if (newLoading || cursor) {
            isLoadingGallery[currentTab] = true;
            enableSpinnerInGallery(currentTab);
            $.post(`/Chats/Media${currentTab}`, { sheetId: sheetId, idUser: idUser, exclusivePost: isPost, cursor: cursor }, function (data) {
                isLoadingGallery[currentTab] = false;
                var currentSheetId = $('#manMessagesMails').data('sheet-id');
                var currentIdUser = $('#interlocutorIdChatHeader').text();
                if (currentSheetId === sheetId && currentIdUser === idUser) {
                    $(popUpGallery).data('sheet-id', sheetId);
                    $(popUpGallery).data('id-user', idUser);
                    $(popUpGallery).data('is-post', isPost);
                    disableSpinnerInGallery(currentTab);
                    $(`#${currentTab}`).append(data);
                    setDataCursor(currentTab);
                    addEventListenerToCard();
                }
            }).fail(function () {
                isLoadingGallery[currentTab] = false;
                disableSpinnerInGallery(currentTab);
            });
        }
    }
}

//Заполенине заголовка галереи. Устанавливается аватарка и имя владелицы
function setHeaderGallery(currentSheetId, lastSheetId) {
    if (currentSheetId != lastSheetId) {
        var avatarOwnerSrc = $('#ownerAvatarChatHeader').attr('src');
        $(popUpGallery).find('.avatar__img').attr('src', avatarOwnerSrc);

        var ownerName = $('#ownerNameChatHeader').text();
        $(popUpGallery).find('.user__name').text(ownerName);
    }
}

//Метод определяет активную вкладку в галереи: Фотографии, Видео, Аудио, Отправленные
function getCurrentTabGallery() {
    var currentTab;
    $('.gallery-item').each(function () {
        if ($(this).css('display') != 'none') {
            currentTab = this.id;
        }
    });
    return currentTab;
}

//Устанвливает data-cursor для подгрузки новых медиа файлов. Для гаждой вкладки в галереи свой курсор.
function setDataCursor(currentTab) {
    var newCursor = $(`#new-cursor-${currentTab}`).val();
    $(`#new-cursor-${currentTab}`).remove();
    $(`#${currentTab}`).data('cursor', newCursor);
}

//Добавление обработчика событий при клике на медиа файле. Функционал выделения.
function addEventListenerToCard() {
    let cards = document.querySelectorAll('.gallery-card');
    $('[name=play]').click(function () {
        console.log('click svg');
    });
    cards.forEach(card => {
        $(card).unbind('click');
        $(card).click(function (event) {
            if ($(event.target).parent().is('svg') || $(event.target).is('svg')) {
                var videoUrl = $(event.target).closest('.video').data('video-url');
                $videoPlayer = $('#videoPlayer');
                $videoPlayer.find('source').attr('src', videoUrl);
                $videoPlayer.find('video')[0].load();
                $videoPlayer.removeClass('d-none');
            }
            else {
                $(this).toggleClass('checkedCard');
                var count = getCheckedCard();
                setCounterSelectedMedia(count);
            }
        });
    });
}

//Очищаем галерею для нового пользователя. Устанавливает активную владку Фотою Очищает тело всех вкладок от медиа данных и data-cursor. Обнуляет счетчик на владке Отправленные.
function clearGallery() {
    document.getElementById('photo').checked = true;
    $('.gallery-item').each(function () {
        $(this).empty();
        $(this).data('cursor', '');
    });
    setCounterSentMedia();
}

//Устанавливаем заначение счетчика выделенных медиа файлов
function setCounterSelectedMedia(count) {
    $('#counter-selected-media').text(count);
}

//Устанавливаем заначение счетчика показывающего количество файлов находящихся на вкладке 'Отправленные'
function setCounterSentMedia() {
    var countCard = $(".filter_4.gallery-item").find('.gallery-card').length;
    $('#counter-send-media').text(`(${countCard})`);
}

//Возвращаем количество выделенных медиа файлов
function getCheckedCard() {
    return $('.gallery-card.checkedCard').length;
}

//снимаем выделение с выбранных медиа файлов
function unCheckedCard() {
    $('.gallery-card').each(function () {
        if ($(this).hasClass('checkedCard')) {
            $(this).removeClass('checkedCard');
        }
    });
}

//Инициализирует spinner. Сигнализирующий о загрузке данных.
function enableSpinnerInGallery(currentTab) {
    var spinner = $(`#spinner-gallery-${currentTab}`);
    var svg = $(`#svg-gallery-${currentTab}`);
    spinner.removeClass('d-none');
    svg.addClass('d-none');
}

//Скрывает spinner. По окнончании загрузки данных вне зависимости от результата.
function disableSpinnerInGallery(currentTab) {
    var spinner = $(`#spinner-gallery-${currentTab}`);
    var svg = $(`#svg-gallery-${currentTab}`);
    spinner.addClass('d-none');
    svg.removeClass('d-none');
}