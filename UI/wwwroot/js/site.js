function errorLoadAvatar(img) {
    img.src = '/image/user.png';
}

function showToast(addClass, removeClass, text) {
    $('#toastBody').html(text)

    var toast = $('#toast');

    if (toast.hasClass(removeClass)) {
        toast.removeClass(removeClass);
    }

    if (!toast.hasClass(addClass)) {
        toast.addClass(addClass);
    }

    bootstrap.Toast.getOrCreateInstance(toast).show();
}