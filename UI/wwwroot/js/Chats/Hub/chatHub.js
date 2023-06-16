const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

connection.onreconnecting(error => {
    console.assert(connection.state === signalR.HubConnectionState.Reconnecting);
    console.log(`SignalR Reconnecting. Connection lost due to error "${error}". Reconnecting.`);
});


connection.onreconnected(connectionId => {
    console.assert(connection.state === signalR.HubConnectionState.Connected);
    console.log(`SignalR Reconnected. Connection reestablished. Connected with connectionId "${connectionId}"`);
    initial();
    var sheetId = $('#manMessagesMails').data('sheet-id');
    var idInterlocutor = Number($('#interlocutorIdChatHeader').text());
    if (sheetId && idInterlocutor) {
        AddToGroup(sheetId, idInterlocutor);
    }
    getHistory('');
});

async function start() {
    try {
        await connection.start();
        console.assert(connection.state === signalR.HubConnectionState.Connected);
        console.log('SignalR Connected.');
        await initial();
    } catch (err) {
        console.assert(connection.state === signalR.HubConnectionState.Disconnected);
        console.log(err);
        setTimeout(() => start(), 50000);
    }
};


connection.onclose(error => {
    console.assert(connection.state === signalR.HubConnectionState.Disconnected);
    console.log(`SignalR connection closed due to error "${error}". Try refreshing this page ot restart the connection.`);
});

start();

async function initial() {
    try {
        await connection.invoke("SendInitialAllNewMessagesFromAllMen");  
    } catch (err) {
        console.error(err);
    }
}

async function AddToGroup(sheetId, idInterlocutor) {
    try {
        await connection.invoke("AddToGroup", sheetId, idInterlocutor);
    } catch (err) {
        console.error(err);
    }
}

async function RemoveFromGroup(sheetId, idInterlocutor) {
    try {
        await connection.invoke("RemoveFromGroup", sheetId, idInterlocutor);
    } catch (err) {
        console.error(err);
    }
}

connection.on("ReceiveInitialAllNewMessagesFromAllMen", (data) => {
    initialAllNewMessagesFromAllMan(data);
});

connection.on('AddDialog', function (data) {
    addDialog(data);
});

connection.on('DeleteDialog', function (sheetId, idInterlocutor, idLastMessage) {
    DeleteDialog(sheetId, idInterlocutor, idLastMessage);    
});

connection.on('NewMessage', function (sheetId, idInterlocutor, idNewMessage) {
    LoadNewMessages(sheetId, idInterlocutor, idNewMessage);
});

connection.on('ChangeNumberOfUsersOnline', function (sheetId, numberOfUsersOnline) {
    changeNumberOfUsersOnline(sheetId, numberOfUsersOnline);
});

connection.on('History', function () {
    LoadNewHistory();
});