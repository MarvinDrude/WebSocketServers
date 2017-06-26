# WebSocketServers
Lightweight WebSocket server in Net Core

Usage:

```C#

WebSocketServer server = new WebSocketServer(27789, null);

server.OnConnect += (src) => {

    Console.WriteLine("Someone connected! IP: " + ((IPEndPoint)src.Socket.LocalEndPoint).Address.MapToIPv4());

    var message = new WebSocketMessage("try");
    message.Content["test"] = 123;

        server.SendMessage(src, message);

    };

    server.OnDisconnect += (src) => {

        Console.WriteLine("Someone disconnected! IP: " + ((IPEndPoint)src.Socket.LocalEndPoint).Address.MapToIPv4());

    };

    server.On["test"] = (src, e) => {

         Console.WriteLine("test send:");
         Console.WriteLine(e.Message.Content["name"]);

    };

    server.Start();

```

For Javascript client you need my MWebSocket.js

Usage:
```JavaScript

var socket;

window.onload = function() {

    socket = new MWebSocket("127.0.0.1", 27789);

    socket.on["try"] = function(e) {

        console.log(e);

        var message = e.Content;

        socket.send("test", {"name": message.test});

    }

    socket.start();


};

```
