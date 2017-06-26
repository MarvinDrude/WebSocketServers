using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebSocketServers.Server.Base {

    public class WebSocketWriter {

        public Stream Stream { get; private set; }

        public WebSocketWriter(Stream stream) {

            Stream = stream;

        }

        public void WritePing() {

            Write(WebSocketOpcode.PingFrame, new byte[] { 1 });

        }

        public void WriteText(string text) {

            byte[] data = Encoding.UTF8.GetBytes(text);
            Write(WebSocketOpcode.TextFrame, data);

        }

        public void Write(WebSocketOpcode opcode, byte[] data) {

            using(MemoryStream ms = new MemoryStream()) {

                byte bitFin = 0x80;
                byte first = (byte)(bitFin | (byte)opcode);
                
                ms.WriteByte(first);
                byte second;

                if(data.Length <= 125) {

                    second = (byte)data.Length;
                    ms.WriteByte(second);

                } else if(data.Length <= 65535) {

                    second = 126;
                    ms.WriteByte(second);
                    WebSocketReaderWriter.WriteNumber(ms, (ushort)data.Length, false);

                } else {

                    second = 127;
                    ms.WriteByte(second);
                    WebSocketReaderWriter.WriteNumber(ms, (ulong)data.Length, false);

                }

                ms.Write(data, 0, data.Length);
                byte[] buffer = ms.ToArray();
                Stream.Write(buffer, 0, buffer.Length);

            }

        }

    }

}
