function runTimers() {
    timers.length = 0;
    $('p.timer').each(function () {
         runTimer($(this));
        //var timer = $(this).startTimer({
        //    elementContainer: 'span',
        //    onComplete: function (element) {
        //        $(element).addClass('text-danger');
        //        //Admin danger!!!
        //    }
        //});
        //timer.trigger('start');
        //timers.push(timer);
    });
}

function runTimer($timerElement) {
    if ($timerElement) {
        if (!$timerElement.data('seconds-left')) {
            var sheetId = $timerElement.data('sheetinfo-id');
            var idInterlocutor = $timerElement.data('id-interlocutor');
            var $newMessage = $timerElement.closest('[name=newMessage]');
            var idLastMessage = $newMessage.data('message-id');
            var messageType = $newMessage.data('message-type');
            $.post('/Chats/Timer', { sheetId, idInterlocutor, idLastMessage, messageType }, function (seconds) {
                $timerElement.data('seconds-left', seconds);
                var timer = $timerElement.startTimer({
                    elementContainer: 'span',
                    onComplete: function (element) {
                        $(element).addClass('text-danger');
                        //Admin danger!!!
                    }
                });
                timer.trigger('start');
                timers.push(timer);
            }).fail(function () {
            });
        }
        else {
            var timer = $timerElement.startTimer({
                elementContainer: 'span',
                onComplete: function (element) {
                    $(element).addClass('text-danger');
                    //Admin danger!!!
                }
            });
            timer.trigger('start');
            timers.push(timer);
        }
    }
}

function stopTimer(sheetInfoId, idInterlocutor, idLastMessage) {
    jQuery.each(timers, function () {
        var currentSheetInfoId = $(this[0]).data('sheetinfo-id');
        var currentIdInterlocutor = $(this[0]).data('id-interlocutor');
        var currentIdLastMessage = $(this[0]).data('id-lastMessage');
        if (currentSheetInfoId === sheetInfoId && currentIdInterlocutor === idInterlocutor && currentIdLastMessage === idLastMessage) {
            this.trigger('pause');
            if (!$(this[0]).hasClass('jst-timeout')) {
                $(this[0]).addClass('text-success');
            }
            return false;
        }
    });
}