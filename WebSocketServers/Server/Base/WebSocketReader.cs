using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WebSocketServers.Server.Utils;

namespace WebSocketServers.Server.Base {

    public class WebSocketReader {

        public byte[] Buffer { get; private set; }

        public WebSocketReader() {

            Buffer = new byte[98304];

        }

        public WebSocketFrame Read(Stream stream, WebSocketClient client) {

            byte first;

            try {

                first = (byte)stream.ReadByte();

            } catch(Exception e) {

                return null;

            }

            if(!WebSocketUtils.IsClientConnected(client)) {

                return null;

            }

            byte bitFinFlag = 0x80;
            byte opcodeFlag = 0x0F;

            bool bitFinSet = (first & bitFinFlag) == bitFinFlag;
            WebSocketOpcode opcode = (WebSocketOpcode)(first & opcodeFlag);

            byte bitMaskFlag = 0x80;
            byte second = (byte)stream.ReadByte();

            bool bitMaskSet = (second & bitMaskFlag) == bitMaskFlag;
            uint length = ReadLength(stream, second);

            if(length != 0) {
                
                byte[] decoded;

                if(bitMaskSet) {

                    byte[] key = WebSocketReaderWriter.Read(stream, 4);
                    byte[] encoded = WebSocketReaderWriter.Read(stream, length);

                    decoded = new byte[length];

                    for(int i = 0; i < encoded.Length; i++) {

                        decoded[i] = (byte)(encoded[i] ^ key[i % 4]);

                    }

                } else {

                    decoded = WebSocketReaderWriter.Read(stream, length);

                }

                WebSocketFrame frame = new WebSocketFrame(opcode, decoded);

                return frame;

            }

            return null;

        }

        public uint ReadLength(Stream stream, byte second) {

            byte dataLengthFlag = 0x7F;
            uint length = (uint)(second & dataLengthFlag);

            if(length == 126) {

                length = WebSocketReaderWriter.ReadUShort(stream, false);

            } else if(length == 127) {

                length = (uint)WebSocketReaderWriter.ReadULong(stream, false);

                //Max 500MB
                if(length < 0 || length > 536870912) {

                    return 0;

                }

            }

            return length;

        }

    }

}
