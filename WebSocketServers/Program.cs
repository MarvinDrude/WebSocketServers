using System;
using System.Net;
using WebSocketServers.Server;

namespace WebSocketServers {

    class Program {

        static void Main(string[] args) {
            WebSocketServer server = new WebSocketServer();

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
           
        }

    }

}