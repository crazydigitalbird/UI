const $history = $('#history');

var loadNewHistory = false;
/*updateDateCreatedHistorySeconds = 60 * 1000;*/

//let timerUpdateDateCreatedHistory;

//$('#hisoty-tab').on('click', function () {
//    getHistory('');
//})

//$(function () {
//    //Запуск периодического обновления (1 раз в минуту) даты создания сообщения в истории
//    timerUpdateDateCreatedHistory = setTimeout(function periodUpdateDateHistoryHumanize() {
//        updateHistoryDateHumanize();
//        timerUpdateDateCreatedHistory = setTimeout(periodUpdateDateHistoryHumanize, updateDateCreatedHistorySeconds);
//    }, updateDateCreatedHistorySeconds);
//});

$(function () {
    getHistory('');
})

// Загрузка новой истории
function LoadNewHistory() {
    if (loadNewHistory) {
        return;
    }

    loadNewHistory = true;
    var idLastMessage = $('#history').find('.accordion-item-button').first().data('bs-target').replace('#flush-collapse-history-', '');
    $.post("/Chats/History", { isNew: true, idLastMessage }, function (history) {
        //scroll
        var oldHeight = $('#history').height();
        var oldScroll = $('#history').scrollTop();

        $history.prepend(history);
        updateAllDateHumanize();

        scrollToOldPositionHistory(oldHeight, oldScroll);
        loadNewHistory = false;
    }).fail(function () {
        loadNewHistory = false;
    });
}

function getHistory(cursor) {
    //var initialized = $history.data('initialized');
    //if (!initialized || cursor) {

    if (!cursor) {
        $history.empty();
    }

    enableSpinnerHistory();

    $.post("/Chats/History", { cursor }, function (data) {
        //if (!initialized) {
        //    $history.data('initialized', true);
        //}
        disableSpinnerHistory();
        if (!cursor) {
            $history.html(data);
        }
        else {
            if ($('#btn-loading-history')) {
                $('#btn-loading-history').remove();
            }
            $history.append(data);
        }
        updateAllDateHumanize();
    }).fail(function () {
        disableSpinnerHistory();
    });
    //}
}

function clickMessageHistory(event) {
    if ($(event.target).is('.btn-show-all-message') || $(event.target).is('.btn-show-all-message img')) {
        showHistoryMessage(event.currentTarget);
    }
    else {
        goToChat($(event.currentTarget).find('.accordion-item-button')[0]);
    }
}

function showHistoryMessage(e) {
    $(e).find('.btn-show-all-message img').toggleClass('img-down-revers');
    $(e).find('.accordion-collapse').collapse('toggle');
}

function enableSpinnerHistory() {
    var $btnLoadingHistory = $('#btn-loading-history');
    var spinner = $(`<div id="spinnerHistory" class="spinner-grow spinner-grow-sm" role="status"></div>`);
    if ($btnLoadingHistory.length > 0) {
        $btnLoadingHistory.find('button').append(spinner);
    }
    else {
        $history.append(spinner);
    }
}

function disableSpinnerHistory() {
    $('#spinnerHistory').remove();
}

function scrollToOldPositionHistory(oldHeight, oldScroll) {
    if (oldScroll > 0) {
        $('#history').scrollTop(oldScroll + $('#history').height() - oldHeight);
    }
}