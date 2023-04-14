$(function () {
    const pickerOptions = {};
    const picker = new EmojiMart.Picker(pickerOptions);
    $('#emojis').append(picker);

    $('#mediaModal').on('show.bs.modal', function () {

    });
});