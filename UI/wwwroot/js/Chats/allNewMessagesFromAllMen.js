//let timerLoadingNewMessages;
//var updateSeconds = 30;
var timers = new Array();

let timerUpdateDateCreatedLastMessage;
let updateDateCreatedLastMessageSeconds = 60 * 1000;
moment.locale('ru');
moment.relativeTimeThreshold('m', 60);

var dialogToLastMessageId = {};

$(function () {

    countNewMessages();
    runTimers();

    //Фильтрация новых сообщений по категориям, от всех мужчин.
    $('#allNewMessagesFromAllMens').find('p[data-bs-toggle="tab"]').each(function () {
        this.addEventListener('shown.bs.tab', (event) => {
            var dataFilters = $(event.target).data('filters');
            filteringMessages(dataFilters);
        });
    });

    //Запуск периодического обновления (1 раз в минуту) даты создания сообщения
    timerUpdateDateCreatedLastMessage = setTimeout(function periodUpdateAllDateHumanize() {
        updateAllDateHumanize();
        timerUpdateDateCreatedLastMessage = setTimeout(periodUpdateAllDateHumanize, updateDateCreatedLastMessageSeconds);
    }, updateDateCreatedLastMessageSeconds);

    ////Запуск периодического обновления новых сообщений
    //timerLoadingNewMessages = setTimeout(function loadingNewMessages() {
    //    $.post('/Chats/AllNewMessagesFromAllMen', {}, function (data) {
    //        $('#allNewMessages').html(data);
    //        var dataFilters = $('#allNewMessagesFromAllMens').find('p.active').data('filters');
    //        filteringMessages(dataFilters);
    //        countNewMessages();
    //        runTimers();
    //    });
    //    timerLoadingNewMessages = setTimeout(loadingNewMessages, updateSeconds * 1000);
    //}, updateSeconds * 1000);
});

function initialAllNewMessagesFromAllMan(content) {
    $('#allNewMessages').html(content);
    var dataFilters = $('#allNewMessagesFromAllMens').find('p.active').data('filters');
    filteringMessages(dataFilters);
    countNewMessages();
    runTimers();
    updateAllDateHumanize();
    initialDictionaryDialogToLastMessageId();
}

function initialDictionaryDialogToLastMessageId() {
    //dialogToLastMessageId = {};
    $('[name=newMessage]').each(function () {
        //Проверяем содержится ли для данного диалога запись в словаре.
        //Если запись содержится проверяем что Id сообщения меньше значения хронящегося в словаре
        //Такие сообщения удаялем
        if (dialogToLastMessageId[this.id] !== undefined && dialogToLastMessageId[this.id] > $(this).data('message-id')) {
            $(this).remove();
            return true;
        }
        dialogToLastMessageId[this.id] = $(this).data('message-id');
    });
}

function updateDialogToLastMessageId(key, idLastMessage) {
    if (dialogToLastMessageId[key] !== undefined && dialogToLastMessageId[key] >= idLastMessage) {
        return true;
    }
    dialogToLastMessageId[key] = idLastMessage;
}

function isNewMessage(key, idLastMessage, messageType) {
    if (messageType) {
        if (dialogToLastMessageId[key] !== undefined && dialogToLastMessageId[key] >= idLastMessage) {
            return false;
        }
    }
    else {
        if (dialogToLastMessageId[key] !== undefined && dialogToLastMessageId[key] > idLastMessage) {
            return false;
        }
    }
    dialogToLastMessageId[key] = idLastMessage;
    return true;
}

function DeleteDialog(sheetId, idInterlocutor, idLastMessage) {
    var $message = $(`#${sheetId}-${idInterlocutor}`); //$(`#${sheetId}-${idInterlocutor}-${idLastMessage}`);
    if ($message.length > 0) {
        var currentIdLasMessage = $message.data('message-id');
        if (idLastMessage >= currentIdLasMessage) {
            if ($message.hasClass('d-none')) {
                $message.remove();
                countNewMessages();
            }
            else {
                //scroll
                $message.animate({ opacity: 0.25 }, 3000, function () {
                    $(this).remove();
                    countNewMessages();
                });
            }
        }
    }
    var key = `${sheetId}-${idInterlocutor}`;
    updateDialogToLastMessageId(key, idLastMessage);
}

function addDialog(messageElement) {
    var $newElement = $(messageElement);

    //Получаем id нового элемента
    var idElement = $newElement.attr('id');

    //Осуществляем поиск существующего элемента с таким же id, как у нового добавляемого элемента
    var $currentElement = $(`#${idElement}`);

    //Проверяем существует ли элемент с данным id на старнице.
    //Если да то организуем дополнительную проверку.
    //Если элемента не существует, то добавляем новый элемент.
    if ($currentElement.length > 0) {

        //Получаем Id сообщения для существующего элемента
        var currentIdLasMessage = $currentElement.data('message-id');

        //Получаем Id сообщения для нового элемента
        var newIdLasMessage = $newElement.data('message-id');

        //Если Id сообщения нового элемента, меньше или равно Id сообщения существующего элемента, вставку нового элемента не осуществляем и завершаем метод.
        //Если Id сообщения нового элемента, больше Id сообщения существующего элемента, то удаляем существующий элемент и продолжаем выполнение метода для вставки нового элемента.
        if (newIdLasMessage <= currentIdLasMessage) {
            return;
        }
        else {
            $currentElement.remove();
        }
    }

    if (!isNewMessage(idElement, $newElement.data('message-id'), $newElement.data('data-message-type'))) {
        return;
    }

    if (isFilteringNewMessage($newElement)) {
        insertElementIntoPosition($newElement);
    }
    else {
        //scroll
        var oldHeight = $('#allNewMessages').height();
        var oldScroll = $('#allNewMessages').scrollTop();
        /*var oldHeight = $('#allNewMessages')[0].scrollHeight;*/
        insertElementIntoPosition($newElement);
        scrollToOldPositionAllNewMessage(oldHeight, oldScroll);
    }
    runTimer($newElement.find('p.timer'));
    dateHumanize($newElement.find('[name=date-created]'));
    countNewMessages();
}

function insertElementIntoPosition($element) {
    var dateCreated = $element.find(`[name=date-created]`).data('date');
    var insert = false;
    $('[name=newMessage]').each(function (i) {
        var dateCreatedExistElement = $(this).find(`[name=date-created]`).data('date');
        // Проверяем условие если дата создания существующего элемента более старая, чем дата создания добавляемого сообщения, то вставляем новый элемент перед текущим.
        if (!moment(dateCreatedExistElement).isAfter(dateCreated)) {
            $element.insertBefore($(this));
            insert = true;
            return false;
        }
    });
    if (!insert) {
        $('#allNewMessages').append($element);
    }
}

function scrollToOldPositionAllNewMessage(oldHeight, oldScroll) {
    if (oldScroll > 0) {
        $('#allNewMessages').scrollTop(oldScroll + $('#allNewMessages').height() - oldHeight);
        /*$('#allNewMessages')[0].scrollTo(0, ($('#allNewMessages')[0].scrollHeight - oldHeight));*/
    }
}

function filteringMessages(dataFilters) {
    if (!dataFilters) {
        $('#allNewMessages').find('[name=newMessage]').each(function () {
            if ($(this).hasClass('d-none')) {
                $(this).removeClass('d-none');
            }
        });
    }
    else if (dataFilters == 'Online') {
        $('#allNewMessages').find('[name=newMessage]').each(function () {
            var online = $(this).data('online');
            if (online === 'Online') {
                if ($(this).hasClass('d-none')) {
                    $(this).removeClass('d-none');
                }
            }
            else {
                if (!$(this).hasClass('d-none')) {
                    $(this).addClass('d-none');
                }
            }
        });
    }
    else {
        var filters = dataFilters.split(';');
        $('#allNewMessages').find('[name=newMessage]').each(function () {
            var messageType = $(this).data('message-type');
            if (filters.includes(messageType)) {
                if ($(this).hasClass('d-none')) {
                    $(this).removeClass('d-none');
                }
            }
            else {
                if (!$(this).hasClass('d-none')) {
                    $(this).addClass('d-none');
                }
            }
        });
    }
    $('#allNewMessages').scrollTop(0);
}

function isFilteringNewMessage(newMessages) {
    var dataFilters = $('#allNewMessagesFromAllMens').find('p.active').data('filters');
    if (dataFilters == 'Online') {
        var online = newMessages.data('online');
        if (online != 'Online') {
            newMessages.addClass('d-none');
            return true;
        }
    }
    else if (dataFilters) {
        var filters = dataFilters.split(';');
        var messageType = newMessages.data('message-type');
        if (!filters.includes(messageType)) {
            newMessages.addClass('d-none');
            return true;
        }
    }
    return false;
}

function updateAllDateHumanize() {
    $('[name=date-created]').each(function () {
        dateHumanize($(this));
    });
}

function dateHumanize($element) {
    var utc = $element.data('date');
    var utcHumanize = moment(utc).fromNow(true);
    $element.text(utcHumanize);
}

//Подсчет количества сообщений по категориям: все; чат; реакции; системные; онлайн.
function countNewMessages() {
    var countAll = 0;
    var countChats = 0;
    var countReactions = 0;
    var countSystems = 0;
    var countOnline = 0;

    $('#allNewMessages').find('[name=newMessage]').each(function () {
        countAll++;
        var online = $(this).data('online');
        if (online === 'Online') {
            countOnline++;
        }
        var messageType = $(this).data('message-type');
        switch (messageType) {
            case 'Message':
            case 'Sticker':
            case 'Photo':
            case 'Photo_batch':
            case 'Video':
            case 'Post':
            case 'Virtual_Gift':
                countChats++
                break;

            case 'LikePhoto':
            case 'Like_NewsFeed_Post':
            case 'Wink':
                countReactions++
                break;

            case 'System':
                countSystems++;
                break;
        }
    });

    $('#countAllNewMessages').text(countAll);
    $('#countChatNewMessages').text(countChats);
    $('#countReactionNewMessages').text(countReactions);
    $('#countSystemNewMessages').text(countSystems);
    $('#countOnlineNewMessages').text(countOnline);
}

function goToChat(e) {
    var owner = {
        Id: $(e).data('owner-id'),
        SheetId: $(e).data('sheet-id'),
        Name: $(e).find('[name=ownerName]').text(),
        Avatar: $(e).find('[name=ownerAvatar]').attr('src')
    };

    var interlocutor = {
        Id: Number($(e).find('[name=interlocutorId]').text()),
        //IsPremium: $(e).data('is-pinned').toLowerCase() === 'true',
        //IsBookmarked: $(e).data('is-bookmarked').toLowerCase() === 'true',
        IsTrash: false,
        Name: $(e).find('[name=interlocutorName]').text(),
        Avatar: $(e).find('[name=interlocutorAvatar]').attr('src')
    };

    updateManMessagesMails(owner, interlocutor);
}
