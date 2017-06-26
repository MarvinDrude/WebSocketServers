using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketServers.Server.Events {

    public class MessageEventArgs : EventArgs {

        public WebSocketMessage Message { get; private set; }

        public MessageEventArgs(WebSocketMessage message) {

            Message = message;

        }

    }

}
