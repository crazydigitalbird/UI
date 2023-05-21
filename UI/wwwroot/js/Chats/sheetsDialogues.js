let timerLoadingDialoguesId;

$(function () {
    //Загрузка диалогов при разворачивании блока collapse
    $('.accordion-collapse').each(function () {
        this.addEventListener('show.bs.collapse', function () {
            var currentTab = getCurrentTab();
            if (currentTab != 'history') {
                var searchMan = $(`#searchMan-${currentTab}`).val();
                if (!searchMan) {
                    let result = this.id.match(/flush-collapse-(.+)-(.+)/);
                    var tab = result[1];
                    var sheetId = result[2];
                    getDialogues(sheetId, tab, '');
                }
            }
        });
    });

    //Обновление диалогов для раскрытых блоков collapse, при изменении статуса online/offline
    $("[name=isOnlineOnly-active], [name=isOnlineOnly-bookmarked], [name=isOnlineOnly-premium], [name=isOnlineOnly-trash]").on('change', function () {
        var tab = this.id.replace('isOnlineOnly-', '');
        refreshSearchMan(tab);

        $(this).closest('.tab-pane').find('.accordion-collapse.show').each(function () {
            let result = this.id.match(/flush-collapse-(.+)-(.+)/);
            var tab = result[1];
            var sheetId = result[2];
            getDialogues(sheetId, tab, '');
        });
    });

    //Обновление диалогов по таймеру для раскрытых блоков collapse, только на текущей вкладке и если isOnline=true
    timerLoadingDialoguesId = setTimeout(function loadingDialogues() {
        var currentTab = getCurrentTab();
        if (currentTab != 'history') {
            if (isOnline(currentTab)) {
                var searchMan = $(`#searchMan-${currentTab}`).val();
                if (!searchMan) {
                    $('.accordion-collapse.show').each(function () {
                        let result = this.id.match(/flush-collapse-(.+)-(.+)/);
                        var tab = result[1];
                        //Обновление только на текущей вкладке
                        if (tab === currentTab) {
                            var sheetId = result[2];
                            getDialogues(sheetId, tab, '');
                        }
                    });
                }
            }
        }
        timerLoadingDialoguesId = setTimeout(loadingDialogues, 30000);
    }, 30000);
});

function getDialogues(sheetId, currentTab, cursor) {
    var $divDialogues = $(`#dialogues-${currentTab}-${sheetId}`);

    var online = isOnline(currentTab);

    enableSpinnerInCounter(sheetId, currentTab);

    $.post("/Chats/Dialogues", { sheetId: sheetId, criteria: currentTab, online: online, cursor: cursor }, function (data) {
        if (online === isOnline(currentTab)) {
            if (cursor === '') {
                $divDialogues.empty();
            }
            else {
                removeBtnLoadinDialogue(sheetId, currentTab);
            }
            $divDialogues.append(data);
            countDialoguesSheet(sheetId, currentTab);
            updateAllDateHumanize();
        }
        else {
            disableSpinnerInCounter(sheetId, currentTab);
        }
    }).fail(function () {
        disableSpinnerInCounter(sheetId, currentTab);
    });
}

function getCriteria() {
    var criteria = getCurrentTab();
    return criteria;
}

function isOnline(currentTab) {
    return document.getElementById(`isOnlineOnly-${currentTab}`).checked;
}

function removeBtnLoadinDialogue(sheetId, currentTab) {
    $(`#btn-loading-dialogues-${currentTab}-${sheetId}`).remove();
}

function enableSpinnerInCounter(sheetId, currentTab) {
    var element = $(`#count-dialogues-${currentTab}-${sheetId}`);
    var spinner = $(`<div class="spinner-grow spinner-grow-sm" role="status"></div>`);
    element.html(spinner);
}

function disableSpinnerInCounter(sheetId, currentTab) {
    $(`#count-dialogues-${currentTab}-${sheetId}`).empty();
}

function countDialoguesSheet(sheetId, currentTab) {
    var count = $(`#dialogues-${currentTab}-${sheetId}`).find('[name=dialogue]').length;
    $(`#count-dialogues-${currentTab}-${sheetId}`).html(count);
}

function getCurrentTab() {
    var currentTab = $('#sheetsDialogues').find('p[data-bs-toggle="tab"].active').attr('aria-controls');
    return currentTab;
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
            IsPremium: $(`#premium-checkbox-${getCurrentTab()}-${sheetId}-${idInterlocutor}`).is(':checked'),
            IsBookmarked: $(`#bookmarked-checkbox-${getCurrentTab()}-${sheetId}-${idInterlocutor}`).is(':checked'),
            IsTrash: getCurrentTab() === 'trash',
            Name: $divDialogue.find('[name=interlocutorName]').text(),
            Avatar: $divDialogue.find('[name=interlocutorAvatar]').attr('src')
        };

        updateManMessagesMails(owner, interlocutor);
    }
}

function changeBookmark(e) {
    var $input = $(e).find('input').first();
    var idParts = $input[0].id.split('-');
    var sheetId = idParts[3];
    var idRegularUser = idParts[4];
    var addBookmark = $input.is(':checked');
    $.post('/Chats/ChangeBookmark', { sheetId: sheetId, idRegularUser: idRegularUser, addBookmark: addBookmark }, function () {

    }).fail(function () {
        $input.prop('checked', !addBookmark);
    });
}

function changePremium(e) {
    var $input = $(e).find('input').first();
    var idParts = $input[0].id.split('-');
    var sheetId = idParts[3];
    var idRegularUser = idParts[4];
    var addPremium = $input.is(':checked');
    $.post('/Chats/ChangePremium', { sheetId: sheetId, idRegularUser: idRegularUser, addPremium: addPremium }, function () {

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