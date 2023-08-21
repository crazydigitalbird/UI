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

    //Запуск немедленного обновления/загрузки диалогов, для всех анкет, при переход на новую вкладку, кроме вкладки "История".
    //IsOnline должент иметь значение true
    $('#sheetsDialogues').find('p[data-bs-toggle="tab"]').each(function () {
        this.addEventListener('shown.bs.tab', (event) => {
            var currentTab = getCurrentTab();
            if (currentTab != 'history' && isOnline(currentTab)) {
                if (currentTab != 'active') {
                    $('.accordion-collapse').each(function () {
                        let result = this.id.match(/flush-collapse-(.+)-(.+)/);
                        var tab = result[1];
                        //Обновление только на текущей вкладке
                        if (tab === currentTab) {
                            var sheetId = result[2];
                            getDialogues(sheetId, tab, '');
                        }
                    });
                }
                else {
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
        });
    });
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

//Устанавливаем для анкеты кол-во пользователей находящихся в онлайн
function changeNumberOfUsersOnline(sheetId, numberOfUsersOnline) {
    //Проверяем, что Online на вкладке "Все" в положении true
    if (isOnline('active')) {
        //Получаем элемент, в который будет записываться кол-во пользователей в онлайн
        var $countDialogues = $(`#count-dialogues-active-${sheetId}`);
        if ($countDialogues) {
            //Получаем родительский элемент, что бы проверить, что он не раскрыт. Рвскрытые элементы collapse онбнавляются не SignalR, а каждые 30 секунд со стороны клиента.
            var $accordioncollapse = $countDialogues.closest('.accordion-collapse');
            if ($accordioncollapse && !$accordioncollapse.hasClass('show')) {
                $countDialogues.html(numberOfUsersOnline);
            }
        }
    }
}

function onlineStatusSheet(sheetId, isOnline) {
    $(`[name="online-status-${sheetId}"]`).each(function () {
        if (isOnline && $(this).hasClass('status-circle-red')) {
            $(this).removeClass('status-circle-red');
            $(this).addClass('status-circle-green');
        }
        else if (!isOnline && $(this).hasClass('status-circle-green')) {
            $(this).removeClass('status-circle-green');
            $(this).addClass('status-circle-red');
        }
    });
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