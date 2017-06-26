using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using WebSocketServers.Server.Base;

namespace WebSocketServers.Server {

    public class WebSocketClient {

        public Socket Socket { get; private set; }

        public bool Handshaked { get; set; }

        public Stream Stream { get; set; }

        public WebSocketWriter Writer { get; set; }

        public int PingRetries { get; set; }

        public bool Pong { get; set; }

        public WebSocketClient(Socket socket) {

            Socket = socket;
            Handshaked = true;

            PingRetries = 0;
            Pong = true;

        }

    }

}
