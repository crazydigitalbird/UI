function showHidenStickers() {
    var $stickers = $('#stickers');
    if ($stickers.data('load')) {
        if ($stickers.hasClass('d-none')) {
            $('#stickers').removeClass('d-none');
        }
        else {
            hidenStickers();
        }
    }
    else {
        var sheetId = $('#manMessagesMails').data('sheet-id');
        $.post('/Chats/Stickers', {sheetId}, function (data) {
            $('#stickers').data('load', true);
            $('#stickers').html(data);
            $('#stickers').removeClass('d-none');
        });
    }
}

function hidenStickers() {
    $('#stickers').addClass('d-none');
}

//Hiden the stickers when clicked outside of it 
$(document).on('click', function (e) {
    var $stickers = $('stickers');
    var $btnStickers = $('#btnStickers');
    if (!$btnStickers.is(e.target) && !$stickers.hasClass('d-none')) {
        hidenStickers();
    }
})

function sendSticker(e, stickerId) {
    var stickerUrl = $(e).find('img')[0].src;
    var message = `${stickerId};${stickerUrl}`
    send('Sticker', message);
}