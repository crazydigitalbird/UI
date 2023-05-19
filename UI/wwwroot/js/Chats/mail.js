function showMail() {
    var sheetId = $('#manMessagesMails').data('sheet-id');
    var idInterlocutor = $('#interlocutorIdChatHeader').text();
    if (sheetId && idInterlocutor) {
        //var idSheetPopup = $(popUpGallery).data('sheet-id');
        //var idUserPopup = $(popUpGallery).data('id-user');

        //if (sheetId === idSheetPopup && idUser === idUserPopup) {

        //}
        //else {
        //    setHeaderGallery(sheetId, idSheetPopup);
        //}
        popUpMail.classList.remove('d-none');
    }
}