using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using WebSocketServers.Server.Base;
using WebSocketServers.Server.Events;
using WebSocketServers.Server.Utils;

namespace WebSocketServers.Server {

    public class WebSocketServer {

        public Socket Socket { get; private set; }
        public bool Running { get; private set; }

        public IPAddress Address { get; private set; }
        public ushort Port { get; private set; }

        public X509Certificate2 SSL { get; private set; }

        public List<WebSocketClient> Clients { get; private set; }
        public Dictionary<string, MessageEventHandler> On { get; private set; }

        public Thread ListenThread { get; private set; }
        public Thread PingThread { get; private set; }

        public int PingInterval { get; set; }
        public int PingMaxRetries { get; set; }

        public delegate void MessageEventHandler(WebSocketClient sender, MessageEventArgs e);

        public delegate void ConnectEventHandler(WebSocketClient sender);
        public event ConnectEventHandler OnConnect;

        public delegate void DisconnectEventHandler(WebSocketClient sender);
        public event DisconnectEventHandler OnDisconnect;

        public WebSocketServer(ushort port = 27789, X509Certificate2 ssl = null) {

            SSL = ssl;
            Port = port;

            Init();

        }

        public bool Start() {

            if(!ListenThread.IsAlive) {

                Running = true;

                ListenThread.Start();
                PingThread.Start();

                return true;

            }

            return false;

        }

        public void Stop() {

            Running = false;

        }

        public void SendMessage(WebSocketClient client, WebSocketMessage message) {

            string json = JsonConvert.SerializeObject(message);
            client.Writer.WriteText(json);

        }

        public bool RemoveClient(WebSocketClient client) {

            try {

                client.Socket.Shutdown(SocketShutdown.Both);

            } catch (Exception e) { }

            OnDisconnect?.Invoke(client);

            return Clients.Remove(client);

        }

        private void Listen() {

            Socket.Listen(500);

            while(Running) {

                Socket socket = Socket.Accept();
                var client = new WebSocketClient(socket);
                
                Thread clientThread = new Thread(() => ListenClient(client));
                clientThread.Start();

            }

        }

        private void ListenClient(WebSocketClient client) {

            using(Stream ns = GetStream(client)) {

                client.Stream = ns;

                WebSocketReader reader = new WebSocketReader();

                string header = HttpUtils.ReadHeader(ns);
                Regex getRegex = new Regex(@"^GET(.*)HTTP\/1\.1", RegexOptions.IgnoreCase);
                Match getRegexMatch = getRegex.Match(header);

                if(getRegexMatch.Success) {

                    DoHandshake(ns, header);
                    client.Writer = new WebSocketWriter(ns);

                    Clients.Add(client);
                    OnConnect?.Invoke(client);

                } else {

                    RemoveClient(client);

                }

                while(Running && Clients.IndexOf(client) != -1) {
                    
                    WebSocketFrame frame = reader.Read(ns, client);

                    if(frame == null || frame.Opcode == WebSocketOpcode.ConnectionCloseFrame) {

                        RemoveClient(client);
                        break;

                    }
                    
                    if(frame.Opcode == WebSocketOpcode.PongFrame) {

                        client.Pong = true;

                    } else if(frame.Opcode == WebSocketOpcode.TextFrame) {

                        try {

                            string json = Encoding.UTF8.GetString(frame.Data);

                            var message = JsonConvert.DeserializeObject<WebSocketMessage>(json);

                            On[message.ID]?.Invoke(client, new MessageEventArgs(message));

                        } catch(Exception e) { }

                    }

                }

            }

        }

        private void Ping() {

            while(Running) {

                for(int i = Clients.Count - 1; i >= 0; i--) {

                    var client = Clients[i];

                    if(!client.Pong) {

                        client.PingRetries++;

                        if(PingMaxRetries < client.PingRetries) {

                            RemoveClient(client);

                        } else {

                            client.Writer.WritePing();

                        }

                    } else {

                        client.PingRetries = 0;
                        client.Pong = false;
                        client.Writer.WritePing();

                    }

                }

                Thread.Sleep(PingInterval);

            }

        }

        private void DoHandshake(Stream ns, string data) {

            string response = "HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                + "Connection: Upgrade" + Environment.NewLine
                + "Upgrade: websocket" + Environment.NewLine
                + "Sec-Websocket-Accept: " + Convert.ToBase64String(
                    SHA1.Create().ComputeHash(
                        Encoding.UTF8.GetBytes(
                            new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                        )
                    )
                ) + Environment.NewLine
                + Environment.NewLine;

            HttpUtils.WriteHeader(ns, response);

        }

        private Stream GetStream(WebSocketClient client) {

            Stream stream = new NetworkStream(client.Socket);

            if(SSL == null) {

                return stream;

            }

            try {

                SslStream sslStream = new SslStream(stream, false);
                var task = sslStream.AuthenticateAsServerAsync(SSL, false, SslProtocols.Tls, true);
                task.Start();
                task.Wait();

                return sslStream;

            } catch(Exception e) {

                return null;

            }

        }

        private void Init() {

            Address = IPAddress.IPv6Any;

            Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            Socket.Bind(new IPEndPoint(Address, Port));

            Running = false;
            Clients = new List<WebSocketClient>();
            On = new Dictionary<string, MessageEventHandler>();

            ListenThread = new Thread(Listen);
            PingThread = new Thread(Ping);

            PingInterval = 2000;
            PingMaxRetries = 3;
            
        }

    }

}
