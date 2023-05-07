function searchSheet(e) {
    var searchSheetIdentity = e.value.toLowerCase();
    if (isNumeric(searchSheetIdentity) || !searchSheetIdentity) {
        $('.tab-pane.active').find('.accordion-item').each(function () {
            var sheetIdentity = $(this).data('sheet-identity').toString();
            if (!sheetIdentity.includes(searchSheetIdentity)) {
                $(this).addClass('d-none');
            }
            else {
                $(this).removeClass('d-none');
            }
        });
    }
}

function searchMan(tab) {
    var searchManId = $(`#searchMan-${tab}`).val().toLowerCase();
    if (isNumeric(searchManId) || !searchManId) {
        $('.tab-pane.active').find('.accordion-item').each(function () {
            var find = false;
            var $sheet = $(this);
            $(this).find('[name=dialogue]').each(function () {
                var idInterlocutor = $(this).data('id-interlocutor').toString();
                if (searchManId && !idInterlocutor.includes(searchManId)) {
                    $(this).addClass('d-none');
                }
                else {
                    $(this).removeClass('d-none');
                    find = true;
                }
            });
            if (!find && searchManId) {
                if (searchManId.length > 5) {
                    var sheetId = $sheet.data('sheet-id');

                    //var $btnLoadDialogues = $(`#btn-loading-dialogues-${tab}-${sheetId}`).find("span");
                    //enableSpinnerAll($btnLoadDialogues, sheetId);

                    $.post('/Chats/FindDialogueById', { sheetId: sheetId, idRegularUser: searchManId, criteria: tab }, function (data) {
                        if (data && searchManId === $(`#searchMan-${tab}`).val().toLowerCase()) {
                            $(`#dialogues-${tab}-${sheetId}`).prepend(data);
                        }
                        else {
                            $sheet.addClass('d-none');
                        }
                    }).done(function () {
                        disableSpinnerAll(sheetId, tab);
                    });
                }
                else {
                    $sheet.addClass('d-none');
                }
            }
            else {
                $sheet.removeClass('d-none');
            }
        });
    }
}

function searchManKeyPress(event, tab) {
    if (event.key === 'Enter') {
        searchMan(tab);
    }
}

function clearSearchMan(element, tab) {
    if (!element.value) {
        searchMan(tab);
    }
}

function refreshSearchMan(tab) {
    if ($(`#searchMan-${tab}`).val()) {
        $(`#searchMan-${tab}`).val('');
        searchMan(tab);
    }
}

function isNumeric(value) {
    return /^\d+$/.test(value);
}