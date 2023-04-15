$(function () {
    const pickerOptions = {
        onClickOutside: (event) => clickOutsideEmojis(event),
        onEmojiSelect: (emoji) => pasteEmoji(emoji),
        dynamicWidth: true,
        theme: 'light',
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

    $('#mediaModal').on('show.bs.modal', function () {

    });
});

function clickOutsideEmojis(e) {
    var $emojis = $("#emojisDiv");
    var $btnEmojis = $('#btnEmojis');
    if ($emojis.has(e.target).length === 0 && $btnEmojis.has(e.target).length === 0 && !$emojis.hasClass('d-none')) {
        hidenEmojis();
    }
}

function hidenEmojis() {
    if (!$('#emojisDiv').hasClass('d-none')) {
        $('#emojisDiv').addClass("d-none");
    }
}

function showHidenEmojis() {
    if ($('#emojisDiv').hasClass('d-none')) {
        $('#emojisDiv').removeClass("d-none");
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