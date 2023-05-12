var isSearchManTab = {};

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
    if (!isSearchManTab[tab]) {
        $(`#searchMan-${tab}`).attr('readonly', true);
        isSearchManTab[tab] = true;
        var promises = [];

        var searchManId = $(`#searchMan-${tab}`).val().toLowerCase();
        if (isNumeric(searchManId) || !searchManId) {
            $('.tab-pane.active').find('.accordion-item').each(function () {
                var find = false;
                var $sheet = $(this);
                $(this).removeClass('d-none');

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

                        enableSpinnerInCounter(sheetId, tab);

                        promises.push($.post('/Chats/FindDialogueById', { sheetId: sheetId, idRegularUser: searchManId, criteria: tab }, function (data) {
                            if (searchManId === $(`#searchMan-${tab}`).val().toLowerCase()) {
                                if (data) {
                                    $(`#dialogues-${tab}-${sheetId}`).prepend(data);
                                }
                                else {
                                    $sheet.addClass('d-none');
                                }
                            }
                        }).done(function () {
                            disableSpinnerInCounter(sheetId, tab);
                        }).fail(function () {
                            $sheet.addClass('d-none');
                            disableSpinnerInCounter(sheetId, tab);
                        }));
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

        $.when.apply(undefined, promises).done(function () {
            isSearchManTab[tab] = false;
            $(`#searchMan-${tab}`).attr('readonly', false);
        }).fail(function () {
            isSearchManTab[tab] = false;
            $(`#searchMan-${tab}`).attr('readonly', false);
        });
    }
}

function searchManKeyPress(event, tab) {
    if (event.key === 'Enter' && !isSearchManTab[tab]) {
        searchMan(tab);
    }
}

function clearSearchMan(element, tab) {
    if (!element.value) {
        isSearchManTab[tab] = false;
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