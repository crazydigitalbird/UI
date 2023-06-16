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

function showMediaInMail(e) {
    clearMediaShow();
    var $currentMailFile = $(e);
    var mediaType = '';
    var urlOriginal = '';

    $('#mailFile').find('.file').each(function (index) {
        var $mailFile = $(this);
        if ($mailFile.data('is-video') === true) {
            var id = $mailFile.data('id');
            //send Post
            mediaType = 'video';
        }
        else {
            urlOriginal = $mailFile.data('url-original');
            mediaType = 'photo';
        }
        mediaDictionary[index + 1] = { mediaType, urlOriginal };
        if ($mailFile.is($currentMailFile)) {
            setMediaToPopUp(urlOriginal, mediaType);
            currentKey = index + 1;
        }
    });

    $mediaShowPopUp.removeClass('d-none');
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