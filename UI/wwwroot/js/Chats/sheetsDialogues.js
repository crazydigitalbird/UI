let timerLoadingDialoguesId;

$(function () {
    $('#sheetsDialogues').find('p[data-bs-toggle="tab"]').each(function () {
        this.addEventListener('shown.bs.tab', (event) => {
            var $currentTab = $(event.target);
            var nameTab = $currentTab.attr('aria-controls');
            switch (nameTab) {
                case 'history':
                    $('#sheetsDialoguesSearch').addClass('d-none');
                    break;

                case 'all':
                case 'bookmarked':
                case 'premium':
                case 'trash':
                    $('#sheetsDialoguesSearch').removeClass('d-none');
                    var oldNameTab = $('#sheetsDialoguesTabContent').data('old-tab');
                    if (oldNameTab != nameTab) {
                        $('#searchMan').val('');

                        $('[name=dialogues]').each(function () {
                            var sheetId = this.id.replace('dialogues-', '');
                            getDialogues(sheetId, '', false);
                        });

                        $('#sheetsDialoguesTabContent').data('old-tab', nameTab);
                    }
                    break;
            }
        });
    });

    $('#isOnlineOnly').on('change', function () {
        $('#searchMan').val('');

        $('[name=dialogues]').each(function () {
            var sheetId = this.id.replace('dialogues-', '');
            getDialogues(sheetId, '', false);
        });
    });

    $('[name=dialogues]').each(function () {
        var sheetId = this.id.replace('dialogues-', '');
        countDialoguesSheet(sheetId);
    })

    timerLoadingDialoguesId = setTimeout(function loadingDialogues() {
        if (isOnline()) {
           /* $('[name=btn-loading-dialogues] button').each(function () {*/
                var searchMan = $('#searchMan').val();
                if (!searchMan) {
                    $('[name=dialogues]').each(function () {
                        var sheetId = this.id.replace('dialogues-', '');
                        getDialogues(sheetId, '', false);
                    });
                    /*$(this).click();*/
                }
            /*});*/
        }
        timerLoadingDialoguesId = setTimeout(loadingDialogues, 30000);
    }, 30000);
});

function getDialogues(sheetId, cursor, newLoading) {
    var $divDialogues = $(`#dialogues-${sheetId}`);

    var criteria = getCriteria();
    var online = isOnline();

    var $btnLoadDialogues = $(`#btn-loading-dialogues-${sheetId}`).find("span");
    enableSpinnerAll($btnLoadDialogues, sheetId);

    $.post("/Chats/Dialogues", { sheetId: sheetId, criteria: criteria, online: online, cursor: cursor }, function (data) {
        if (criteria === getCriteria() && online === isOnline()) {
            if (cursor === '') {
                $divDialogues.empty();
            }
            else {
                removeBtnLoadinDialogue(sheetId);
            }
            $divDialogues.append(data);
            countDialoguesSheet(sheetId);
        }
    }).done(function () {
        disableSpinnerAll(sheetId);
    });
}

function getCriteria() {
    var criteria = $('#sheetsDialogues').find('.active').attr('aria-controls');
    return criteria;
}

function isOnline() {
    return document.getElementById('isOnlineOnly').checked;
}

function removeBtnLoadinDialogue(sheetId) {
    $(`#btn-loading-dialogues-${sheetId}`).remove();
}

function enableSpinnerAll($e, sheetId) {
    if (!$(`#spinnerSheetDialogue${sheetId}`).length) {
        var spinner = $(`<div id="spinnerSheetDialogue${sheetId}" class="spinner-grow spinner-grow-sm ms-2" role="status">
                            <span class="visually-hidden">Loading...</span>
                        </div>`);
        $e.append(spinner);
    }
}

function disableSpinnerAll(sheetId) {
    $(`#spinnerSheetDialogue${sheetId}`).remove();
}

function countDialoguesSheet(sheetId) {
    var count = $(`#dialogues-${sheetId}`).find('[name=dialogue]').length;
    $(`#count-dialogues-sheet-${sheetId}`).text(count);
}

function goToChatFromSheetDialogues(event) {
    if ($(event.target).closest('span[name="bookmark"], [name="premium"]').length == 0) {
        var $divDialogue = $(event.currentTarget);
        var $divOwner = $divDialogue.closest('.accordion-item');
        var sheetId = $divOwner.data('sheet-id');
        var idInterlocutor = $divDialogue.data('id-interlocutor');

        var owner = {
            Id: $divOwner.find('[name=ownerId]').text(),
            SheetId: sheetId,
            Name: $divOwner.find('[name=ownerName]').text(),
            Avatar: $divOwner.find('[name=ownerAvatar]').attr('src')
        };

        var interlocutor = {
            Id: idInterlocutor,
            IsPinned: $(`#premium-checkbox-${sheetId}-${idInterlocutor}`).is(':checked'),
            IsBookmarked: $(`#bookmarked-checkbox-${sheetId}-${idInterlocutor}`).is(':checked'),
            Name: $divDialogue.find('[name=interlocutorName]').text(),
            Avatar: $divDialogue.find('[name=interlocutorAvatar]').attr('src')
        };

        updateManMessagesMails(owner, interlocutor);
    }
}

function changeBookmark(e) {
    var $input = $(e).find('input').first();
    var idParts = $input[0].id.split('-');
    var sheetId = idParts[2];
    var idRegularUser = idParts[3];
    var addBookmark = $input.is(':checked');
    $.post('/Chats/ChangeBookmark', { sheetId: sheetId, idRegularUser: idRegularUser, addBookmark: addBookmark }, function () {

    }).fail(function () {
        $input.prop('checked', !addBookmark);
    });
}

function changePin(e) {
    var $input = $(e).find('input').first();
    var idParts = $input[0].id.split('-');
    var sheetId = idParts[2];
    var idRegularUser = idParts[3];
    var addPin = $input.is(':checked');
    $.post('/Chats/ChangePin', { sheetId: sheetId, idRegularUser: idRegularUser, addPin: addPin }, function () {

    }).fail(function () {
        $input.prop('checked', !addPin);
    });
}

//function enableSpinner($e, name) {
//    var spinner = $(`<div id="spinnerNavTabs" class="d-flex justify-content-center my-1">
//                        <div id="spinner${name}" class="spinner-grow spinner-grow-sm ms-2" role="status">
//                            <span class="visually-hidden">Loading...</span>
//                        </div>
//                     </div>`);
//    $e.append(spinner);
//}

//function disableSpinner(name) {
//    $(`#spinner${name}`).remove();
//}