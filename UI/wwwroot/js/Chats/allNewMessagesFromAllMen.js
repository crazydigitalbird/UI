let timerLoadingNewMessages;
var timers = new Array();
var updateSeconds = 30;

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

    //Запуск периодического обновления новых сообщений
    timerLoadingNewMessages = setTimeout(function loadingNewMessages() {
        $.post('/Chats/AllNewMessagesFromAllMen', {}, function (data) {
            $('#allNewMessages').html(data);
            var dataFilters = $('#allNewMessagesFromAllMens').find('p.active').data('filters');
            filteringMessages(dataFilters);
            countNewMessages();
            runTimers();
        });
        timerLoadingNewMessages = setTimeout(loadingNewMessages, updateSeconds * 1000);
    }, updateSeconds * 1000);
});

function filteringMessages(dataFilters) {
    if (!dataFilters) {
        $('#allNewMessages').find('[name=newMessage]').each(function () {
            if ($(this).hasClass('d-none')) {
                $(this).removeClass('d-none')
            }
        });
    }
    else if (dataFilters == 'Online') {
        $('#allNewMessages').find('[name=newMessage]').each(function () {
            var online = $(this).data('online');
            if (online === 'Online') {
                if ($(this).hasClass('d-none')) {
                    $(this).removeClass('d-none')
                }
            }
            else {
                if (!$(this).hasClass('d-none')) {
                    $(this).addClass('d-none')
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
                    $(this).removeClass('d-none')
                }
            }
            else {
                if (!$(this).hasClass('d-none')) {
                    $(this).addClass('d-none')
                }
            }
        });
    }
    $('#allNewMessages').scrollTop(0);
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

function runTimers() {
    timers.length = 0;
    $('p.timer').each(function () {
        var timer = $(this).startTimer({
            elementContainer: 'span',
            onComplete: function (element) {
                $(element).addClass('text-danger');
                //Admin danger!!!
            }
        });
        timer.trigger('start');
        timers.push(timer);
    })
}

function stopTimer(sheetInfoId, IdInterlocutor) {
    jQuery.each(timers, function () {
        var currentSheetInfoId = $(this[0]).data('sheetinfo-id');
        var currentIdInterlocutor = $(this[0]).data('id-interlocutor');
        if (currentSheetInfoId === sheetInfoId && currentIdInterlocutor === IdInterlocutor) {
            this.trigger('pause');
            if (!$(this[0]).hasClass('jst-timeout')) {
                $(this[0]).addClass('text-success');
            }
        }
    })
}

function goToChat(e) {
    var owner = {
        Id: $(e).data('owner-id'),
        SheetId: $(e).data('sheet-id'),
        Name: $(e).find('[name=ownerName]').text(),
        Avatar: $(e).find('[name=ownerAvatar]').attr('src')
    };

    var interlocutor = {
        Id: $(e).find('[name=interlocutorId]').text(),
        IsPinned: $(e).data('is-pinned').toLowerCase() === 'true',
        IsBookmarked: $(e).data('is-bookmarked').toLowerCase() === 'true',
        Name: $(e).find('[name=interlocutorName]').text(),
        Avatar: $(e).find('[name=interlocutorAvatar]').attr('src')
    };

    updateManMessagesMails(owner, interlocutor);
}
