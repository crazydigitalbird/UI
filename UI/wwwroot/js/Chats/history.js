const $history = $('#history');
    /*updateDateCreatedHistorySeconds = 60 * 1000;*/

//let timerUpdateDateCreatedHistory;

$('#hisoty-tab').on('click', function () {
    getHistory('');
})

//$(function () {
//    //Запуск периодического обновления (1 раз в минуту) даты создания сообщения в истории
//    timerUpdateDateCreatedHistory = setTimeout(function periodUpdateDateHistoryHumanize() {
//        updateHistoryDateHumanize();
//        timerUpdateDateCreatedHistory = setTimeout(periodUpdateDateHistoryHumanize, updateDateCreatedHistorySeconds);
//    }, updateDateCreatedHistorySeconds);
//});


function getHistory(cursor) {
    var initialized = $history.data('initialized');
    if (!initialized || cursor) {
        enableSpinnerHistory();

        $.post("/Chats/History", { cursor }, function (data) {
            if (!initialized) {
                $history.data('initialized', true);
            }
            disableSpinnerHistory();
            if ($('#btn-loading-history')) {
                $('#btn-loading-history').remove();
            }
            $history.append(data);
            updateAllDateHumanize();
        }).fail(function () {
            disableSpinnerHistory();
        });
    }
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