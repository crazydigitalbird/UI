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

function sendGift(e, giftId) {
    var giftUrl = $(e).find('img')[0].src;
    var message = `${giftId};${giftUrl}`
    send('Gift', message);
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