﻿$(function () {
    const pickerOptions = {
        onClickOutside: (event) => clickOutsideEmojis(event),
        onEmojiSelect: (emoji) => pasteEmoji(emoji),
        dynamicWidth: false,
        theme: 'dark',
        emojiButtonRadius: '6px',
        emojiButtonColors: [
            'rgba(155,223,88,.7)',
            'rgba(149,211,254,.7)',
            'rgba(247,233,34,.7)',
            'rgba(238,166,252,.7)',
            'rgba(255,213,143,.7)',
            'rgba(211,209,255,.7)',
        ],
    };
    const picker = new EmojiMart.Picker(pickerOptions);
    $('#emojis').append(picker);
});

function clickOutsideEmojis(e) {
    var $emojis = $("#emojis");
    var $btnEmojis = $('#btnEmojis');
    if (!$btnEmojis.is(e.target) && !$emojis.hasClass('d-none')) {
        hidenEmojis();
    }
}

function hidenEmojis() {
    if (!$('#emojis').hasClass('d-none')) {
        $('#emojis').addClass("d-none");
    }
}

function showHidenEmojis() {
    if ($('#emojis').hasClass('d-none')) {
        $('#emojis').removeClass("d-none");
    } else {
        hidenEmojis();
    }
}

function pasteEmoji(emoji) {
    var cursorPos = $('#newMessage').prop('selectionStart');
    var v = $('#newMessage').val();
    var textBefore = v.substring(0, cursorPos);
    var textAfter = v.substring(cursorPos, v.length);

    $('#newMessage').val(textBefore + emoji.native + textAfter);

    hidenEmojis();
}