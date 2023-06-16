const $gifts = $('#gifts'),
    $btnGifts = $('#btnGifts');


function showHidenGifts() {
    if ($btnGifts.hasClass('textarea-icon')) {
        if ($gifts.data('load')) {
            $gifts.toggleClass('d-none');
        }
        else {
            var sheetId = $('#manMessagesMails').data('sheet-id');
            $.post('/Chats/Gifts', { sheetId }, function (data) {
                $gifts.data('load', true);
                $gifts.html(data);
                $gifts.removeClass('d-none');
            });
        }
    }
}

//Hiden the gifts when clicked outside of it 
$(document).on('click', function (e) {
    var $btnGifts = $('#btnGifts');
    if (!$btnGifts.is(e.target) && !$gifts.hasClass('d-none')) {
        $gifts.addClass('d-none');
    }
})

function createGift(e, giftId, giftName) {
    var giftUrl = $(e).find('img')[0].src;

    var $createGift = $('#createGift');
    $createGift.data('gift-id', giftId);
    $createGift.find('.giftUrl').attr('src', giftUrl);
    $createGift.find('.giftName').text(giftName);
    $createGift.find('textarea').val('');

    $createGift.removeClass('d-none');
}

function sendGift() {
    var $createGift = $('#createGift');
    var giftId = $createGift.data('gift-id');
    var giftUrl = $createGift.find('.giftUrl').attr('src');
    var giftText = $createGift.find('textarea').val();

    var message = `${giftId};${giftUrl};${giftText}`;
    send('Virtual_Gift', message);

    $createGift.addClass('d-none');
}

function checkGifts(sheetId, idInterlocutor) {
    disabledGifts();
    $.post('/Chats/CheckGifts', { sheetId, idInterlocutor }, function (data) {
        if (data) {
            enableGifts();
        }
    });
}

function enableGifts() {
    if ($btnGifts.hasClass('textarea-icon-disabled')) {
        $btnGifts.removeClass('textarea-icon-disabled');
        $btnGifts.addClass('textarea-icon');
    }
}

function disabledGifts() {
    if ($btnGifts.hasClass('textarea-icon')) {
        $btnGifts.removeClass('textarea-icon');
        $btnGifts.addClass('textarea-icon-disabled');
    }
}