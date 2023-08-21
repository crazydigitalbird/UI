var $mediaShowPopUp = $('#videoPlayer');
var mediaDictionary = {};
var currentKey;

function showMediaInGallery(e) {
    clearMediaShow();

    var $currentGalleryCard = $(e).closest('.gallery-card');
    var currentTabGallery = getCurrentTabGallery();
    $(`#${currentTabGallery}`).find('.gallery-card').each(function (index) {
        var $galleryCard = $(this);
        var urlOriginal = $galleryCard.data('url-original');
        var mediaType;
        if ($galleryCard.hasClass('video')) {
            mediaType = 'video';
        }
        else if ($galleryCard.hasClass('photo')) {
            mediaType = 'photo';
        }
        else if ($galleryCard.hasClass('audio')) {
            mediaType = 'audio';
        }
        mediaDictionary[index + 1] = { mediaType, urlOriginal };
        if ($galleryCard.is($currentGalleryCard)) {
            setMediaToPopUp(urlOriginal, mediaType);
            currentKey = index + 1;
        }
    });

    $mediaShowPopUp.removeClass('d-none');
}

function showProfilePhoto(e) {
    clearMediaShow();

    var $currentPhotoMan = $(e);
    var mediaType = 'photo';

    $('.photo-man').each(function (index) {
        var $photoMan = $(this);
        var urlOriginal = $photoMan.data('url-original');
        mediaDictionary[index + 1] = { mediaType, urlOriginal };
        if ($photoMan.is($currentPhotoMan)) {
            setMediaToPopUp(urlOriginal, mediaType);
            currentKey = index + 1;
        }
    });

    $mediaShowPopUp.removeClass('d-none');
}

function showOneMedia(e, mediaType) {
    clearMediaShow();

    var sheetId = $('#manMessagesMails').data('sheet-id');

    if (mediaType === 'photo') {
        var urlPreview = $(e).attr('src');

        $.get('/Chats/OriginalUrlMedia', { sheetId, urlPreview }, function (urlOriginal) {
            setMediaToPopUp(urlOriginal, mediaType);
            $mediaShowPopUp.removeClass('d-none');
        });
    }
    else if (mediaType === 'video') {
        var idVideo = $(e).data('id-video');

        $.get('/Chats/OriginalUrlVideo', { sheetId, idVideo }, function (urlOriginal) {
            setMediaToPopUp(urlOriginal, mediaType);
            $mediaShowPopUp.removeClass('d-none');
        });
    }
}

function showPhotoBatch(e) {
    clearMediaShow();

    var sheetId = $('#manMessagesMails').data('sheet-id');
    var mediaType = 'photo';
    var promises = [];

    $(e).find('[name="photo_batch"]').each(function (index) {
        var urlPreview = $(this).attr('src');

        promises.push($.get('/Chats/OriginalUrlMedia', { sheetId, urlPreview }, function (urlOriginal) {
            mediaDictionary[index + 1] = { mediaType, urlOriginal };
            if (index === 0) {
                setMediaToPopUp(urlOriginal, mediaType);
                currentKey = index + 1;
            }
        }));
    });

    $.when.apply(undefined, promises).done(function () {
        $mediaShowPopUp.removeClass('d-none');
    }).fail(function () {
    });
}

function showMediaInPost(event) {
    clearMediaShow();

    var sheetId = $('#manMessagesMails').data('sheet-id');
    var $currentMedia = $(event.currentTarget);
    var $currentPost = $currentMedia.closest('.preview-body');
    var promises = [];

    $currentPost.find('img').each(function (index) {
        var $media = $(this);
        var urlPreview = $media.attr('src');
        var mediaType = 'photo';
        if ($media.hasClass('video')) {
            mediaType = 'video';
            var idVideo = $media.data('id-video');
            promises.push($.get('/Chats/OriginalUrlVideo', { sheetId, idVideo }, function (urlOriginal) {
                mediaDictionary[index + 1] = { mediaType, urlOriginal };
                if ($media.is($currentMedia)) {
                    setMediaToPopUp(urlOriginal, mediaType);
                    currentKey = index + 1;
                }
            }));
        }
        else {
            promises.push($.get('/Chats/OriginalUrlMedia', { sheetId, urlPreview }, function (urlOriginal) {
                mediaDictionary[index + 1] = { mediaType, urlOriginal };
                if ($media.is($currentMedia)) {
                    setMediaToPopUp(urlOriginal, mediaType);
                    currentKey = index + 1;
                }
            }));
        }
    });

    $.when.apply(undefined, promises).done(function () {
        $mediaShowPopUp.removeClass('d-none');
    }).fail(function () {
    });
}

function showMediaInMail(e) {
    clearMediaShow();
    var $currentMailFile = $(e);
    var $divMailFiles = $currentMailFile.closest('[name="mailFile"]')
    var mediaType = '';
    var urlOriginal = '';
    var promises = [];

    $divMailFiles.find('.file').each(function (index) {
        var $mailFile = $(this);
        if ($mailFile.data('is-video') === true) {
            mediaType = 'video';
            var sheetId = $('#manMessagesMails').data('sheet-id');
            var idVideo = $mailFile.data('id-video');

            promises.push($.get('/Chats/OriginalUrlVideo', { sheetId, idVideo }, function (urlOriginal) {
                mediaDictionary[index + 1] = { mediaType, urlOriginal };
            }));
        }
        else {
            mediaType = 'photo';
            urlOriginal = $mailFile.data('url-original');
            mediaDictionary[index + 1] = { mediaType, urlOriginal };
        }
        if ($mailFile.is($currentMailFile)) {
            currentKey = index + 1;
        }
    });

    $.when.apply(undefined, promises).done(function () {
        var currentMediaOriginal = mediaDictionary[currentKey];
        setMediaToPopUp(currentMediaOriginal.urlOriginal, currentMediaOriginal.mediaType);
        $mediaShowPopUp.removeClass('d-none');
    }).fail(function () {
    });
}

function setMediaToPopUp(urlOriginal, mediaType) {
    clearMediaShowPopUp();
    if (mediaType === 'video') {
        $mediaShowPopUp.find('source').attr('src', urlOriginal);
        $mediaShowPopUp.find('video').removeClass('d-none');
        $mediaShowPopUp.find('video')[0].load();
    }
    else if (mediaType === 'photo') {
        $mediaShowPopUp.find('img').attr('src', urlOriginal);
        $mediaShowPopUp.find('img').removeClass('d-none');
    }
    else if (mediaType === 'audio') {

    }
}

function clearMediaShow() {
    mediaDictionary = {};
    currentKey = '';

    clearMediaShowPopUp();
}

function clearMediaShowPopUp() {
    $mediaShowPopUp.find('video').addClass('d-none');
    $mediaShowPopUp.find('source').attr('src', '');

    $mediaShowPopUp.find('img').addClass('d-none');
    $mediaShowPopUp.find('img').attr('src', '');
}

function nextMedia() {
    var media = next(mediaDictionary, currentKey);
    if (media) {
        setMediaToPopUp(media.urlOriginal, media.mediaType);
    }
}

function backMedia() {
    var media = back(mediaDictionary, currentKey);
    if (media) {
        setMediaToPopUp(media.urlOriginal, media.mediaType);
    }
}

var next = function (db, key) {
    var keys = Object.keys(db);
    var i = keys.indexOf(key.toString());
    if (keys[i + 1]) {
        if (db[key].mediaType === 'video') {
            $mediaShowPopUp.find('video')[0].pause();
        }
        currentKey = keys[i + 1];
    }
    return i !== -1 && keys[i + 1] && db[keys[i + 1]];
}

var back = function (db, key) {
    var keys = Object.keys(db);
    var i = keys.indexOf(key.toString());
    if (keys[i - 1]) {
        if (db[key].mediaType === 'video') {
            $mediaShowPopUp.find('video')[0].pause();
        }
        currentKey = keys[i - 1];
    }
    return i !== -1 && keys[i - 1] && db[keys[i - 1]];
}