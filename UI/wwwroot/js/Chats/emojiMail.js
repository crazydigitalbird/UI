$(function () {
    const pickerOptions = {
        onEmojiSelect: (emoji) => pasteEmojiMail(emoji),
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
    $('#emojisMail').append(picker);
});

function showHidenEmojisMail() {
    $('#emojisMail').toggleClass("d-none");
}

function pasteEmojiMail(emoji) {
    var cursorPos = $('#newMailMessage').prop('selectionStart');
    var v = $('#newMailMessage').val();
    var textBefore = v.substring(0, cursorPos);
    var textAfter = v.substring(cursorPos, v.length);

    $('#newMailMessage').val(textBefore + emoji.native + textAfter);
    $('#newMailMessage').prop('selectionEnd', cursorPos + 2);
    $('#newMailMessage').focus();

    changeMailCounter();
}