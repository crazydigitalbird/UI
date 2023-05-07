const hubConnection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

hubConnection.on('Recive', function (data) {
    console.log(data);
});

hubConnection.start();