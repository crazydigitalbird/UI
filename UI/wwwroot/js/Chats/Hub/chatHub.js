const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

connection.onreconnecting(error => {
    console.assert(connection.state === signalR.HubConnectionState.Reconnecting);
    console.log(`Connection lost due to error "${error}". Reconnecting.`);
});


connection.onreconnected(connectionId => {
    console.assert(connection.state === signalR.HubConnectionState.Connected);
    console.log(`Connection reestablished. Connected with connectionId "${connectionId}"`);
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
    console.log(`Connection closed due to error "${error}". Try refreshing this page ot restart the connection.`);
});

start();

async function initial() {
    try {
        await connection.invoke("SendInitialAllNewMessagesFromAllMen");  
    } catch (err) {
        console.error(err);
    }
}

connection.on("ReceiveInitialAllNewMessagesFromAllMen", (data) => {
    $('#allNewMessages').html(data);
});

//connection.on('Recive', function (data) {
//    console.log(data);
//});