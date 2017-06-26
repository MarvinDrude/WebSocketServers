using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketServers.Server {

    public class WebSocketMessage {

        public string ID { get; private set; }

        public Dictionary<string, object> Content { get; private set; }

        public WebSocketMessage(string id) {

            ID = id;
            Content = new Dictionary<string, object>();

        }

    }

}
