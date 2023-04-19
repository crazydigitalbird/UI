//const hubConnection = new signalR.HubConnectionBuilder()
//    .withUrl("/dialogues")
//    .build();

//hubConnection.on("Dialogues", function (data) {
//    $listUsers.empty();
//    $('#listUsers').append(data);
//    setCursor($listUsers);
//});
//hubConnection.start();

setInterval(getDialoguesSignalR, 30000);

function getDialoguesSignalR() {
    var $listUsers = $('#listUsers');

    $('#search').val('');
    var cursor = '';
    $listUsers.data('is-search', false);
        $listUsers.data('all-load', false);
    if (!$listUsers.data('all-load')) {
        var sheetId = $('#sheetId').val();

        var criteria = getCriteria();

        $.post("/Chat/Dialogues", { sheetId: sheetId, criteria: criteria, cursor: cursor }, function (data) {
            if (criteria === getCriteria()) {
                $listUsers.html(data);
                setCursor($listUsers);
                console.log('updating');
            }
        }).done(function () {
            additionalLoad = false;
        });
    }
}
