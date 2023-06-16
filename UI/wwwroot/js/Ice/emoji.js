$(function () {
    const pickerChatOptions = {
        onEmojiSelect: (emoji) => pasteEmojiChat(emoji),
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

    const pickerMailOptions = {
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

    const pickerChat = new EmojiMart.Picker(pickerChatOptions);
    const pickerMail = new EmojiMart.Picker(pickerMailOptions);
    $('#emojisIceChat').append(pickerChat);
    $('#emojisIceMail').append(pickerMail);
});

function showHidenEmojisIce(id) {
    $(`#${id}`).toggleClass("d-none");
}

function pasteEmojiChat(emoji) {
    pasteEmoji(emoji, $('#chatIceTextArea'));
    changeChatIceCounter();
}

function pasteEmojiMail(emoji) {
    pasteEmoji(emoji, $('#mailIceTextArea'));
    changeMailIceCounter();
}

function pasteEmoji(emoji, $textArea) {
    var cursorPos = $textArea.prop('selectionStart');
    var v = $textArea.val();
    var textBefore = v.substring(0, cursorPos);
    var textAfter = v.substring(cursorPos, v.length);

    $textArea.val(textBefore + emoji.native + textAfter);
    $textArea.prop('selectionEnd', cursorPos + 2);
    $textArea.focus();
}