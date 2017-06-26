var MWebSocket;

{

    MWebSocket = class MWebSocket {

        constructor() {

            this.address = arguments[0];
            this.port = arguments[1];
            this.secure = arguments[2] || false;
            this.connection = null;
            this.on = { };

        }

        send(id, content) {

            var message = { };
            message["ID"] = id;
            message["Content"] = content;

            this.connection.send(JSON.stringify(message));

        }

        start() {

            var sec = this.secure ? "wss" : "ws";
            var string = sec + "://" + this.address + ":" + this.port;

            this.connection = new WebSocket(string);
            this.init();

        }

        stop() {

            this.send("base::disconnect", []);

        }

        init() {

            var self = this;

            this.connection.onmessage = function(e) {

                var json = JSON.parse(e.data);

                if(self.on[json.ID] != null) {

                    self.on[json.ID](json);

                }

            }

        }

    }

}