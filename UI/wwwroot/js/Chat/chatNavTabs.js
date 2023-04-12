var additionalLoad = false;

$(function () {
    setHeightUsersArea();

    $(window).resize(function () {
        setHeightUsersArea();
    });

    var $listUsers = $('#listUsers');
    setCursor($listUsers);

    var $firstLiUsersArea = $('#usersArea').find('li').first();
    $firstLiUsersArea.addClass('list-group-item active');

    var activeChatId = Number($firstLiUsersArea[0].id.replace('user_', ''));
    loadChat(activeChatId);
})

function setHeightUsersArea() {
    var freeAreaHeight = $(window).height() - $('#navMenu').outerHeight(true) - $('#divSearch').outerHeight(true) - $('#onlineOnly').outerHeight(true) - $('#tabsNavs').outerHeight(true) - 16 - 48 - 32;
    $("#usersArea").css("height", freeAreaHeight);
}

function activatingDialogue(chatId) {
    var $liUsersArea = $(`#user_${chatId}`).closest('li');

    if (!$liUsersArea.hasClass('active')) {

        $('#usersArea').find('li').each(function () {
            if ($(this).hasClass('active')) {
                $(this).removeClass('list-group-item active');
            }
        });

        $liUsersArea.addClass('list-group-item active');

        loadChat(chatId);
    }
}

function getDialogues(newLoading) {
    var $listUsers = $('#listUsers');

    $('#search').val('');
    $listUsers.data('is-search', false);

    var cursor = '';
    if (newLoading) {
        $listUsers.empty();
        $listUsers.data('all-load', false);
        MessagesAreaClear();
    }
    else {
        cursor = $listUsers.data('cursor');
    }
    if (!$listUsers.data('all-load')) {
        var sheetId = $('#sheetId').val();

        var criteria = getCriteria();


        enableSpinnerNavTabs($listUsers);

        $.post("/Chat/Dialogues", { sheetId: sheetId, criteria: criteria, cursor: cursor }, function (data) {
            if (criteria === getCriteria()) {
                $listUsers.append(data);
                setCursor($listUsers);
            }
        }).done(function () {
            disableSpinnerNavTabs();
            additionalLoad = false;
        });
    }
}

function getCriteria() {
    var criteria = $('#tabsNavs').find('.active').data('type');
    if (document.getElementById('isOnlineOnly').checked) {
        criteria = criteria + ',online'
    }
    return criteria;
}

function setCursor($listUsers) {
    var $cursor = $('#cursor');
    $listUsers.data('cursor', $cursor.val());
    if ($cursor.val() === '') {
        $listUsers.data('all-load', true);
    }
    $cursor.remove();
}

function additionalLoadDialogues(e) {
    if (e.scrollTop != 0 && e.scrollHeight - e.scrollTop === e.clientHeight && !additionalLoad) {
        additionalLoad = true;
        getDialogues(false);
    }
}

function enableSpinnerNavTabs($tab) {
    var spinner = $(`<div id="spinnerNavTabs" class="d-flex justify-content-center my-1">
                        <div class="spinner-grow spinner-grow-sm text-success" role="status">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                     </div>`);
    $tab.append(spinner);
}

function disableSpinnerNavTabs() {
    $('#spinnerNavTabs').remove();
}

function MessagesAreaClear() {
    $('#messagesArea').find('div[name=messages]').each(function () {
        $(this).remove()
    });
    $('#messagesArea').find('div[name=chatMenu]').each(function () {
        $(this).remove();
    });
    $('#plugMessages').removeClass('d-none');
    $('#plugChatMenu').removeClass('d-none');
}

function searchDialogue(e) {
    var search = e.value.toLowerCase();
    var find = false;

    var $listUsers = $('#listUsers');
    if (search === '' && $listUsers.data('is-search')) {
        getDialogues(true);
    }

    $('#usersArea').find('li').each(function () {
        var chatId = this.id.replace('user_', '').toLowerCase();
        var userName = $(this).find('p[name=userName]').first().text().toLowerCase();
        if (search && !(userName.includes(search) || chatId.includes(search))) {
            $(this).addClass('d-none');
        }
        else {
            $(this).removeClass('d-none');
            find = true;
        }
    });

    if (!find && search.length === 8 && isNumeric(search)) {
        var sheetId = $('#sheetId').val();
        
        $listUsers.empty();
        $listUsers.data('is-search', true);
        MessagesAreaClear();

        enableSpinnerNavTabs($listUsers);

        $.post('/Chat/FindDialogueById', { sheetId: sheetId, idRegularUser: search }, function (data) {
            if (search === $('#search').val().toLowerCase()) {
                $listUsers.append(data);
            }
        }).done(function () {
            disableSpinnerNavTabs();
        });
    }
}

function isNumeric(value) {
    return /^\d+$/.test(value);
}