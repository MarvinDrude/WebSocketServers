﻿using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketServers.Server.Base {

    public class WebSocketFrame {

        public WebSocketOpcode Opcode { get; private set; }

        public byte[] Data { get; private set; }

        public WebSocketFrame(WebSocketOpcode opcode, byte[] data) {

            Opcode = opcode;
            Data = data;

        }

    }

}
