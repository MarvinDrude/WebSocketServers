using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebSocketServers.Server.Utils {

    public static class HttpUtils {

        public static void WriteHeader(Stream stream, string content) {

            byte[] buffer = Encoding.UTF8.GetBytes(content);
            stream.Write(buffer, 0, buffer.Length);

        }

        public static string ReadHeader(Stream stream) {

            int len = 32768;
            int read = 0;
            byte[] buffer = new byte[len];

            read = stream.Read(buffer, 0, buffer.Length);

            string header = Encoding.UTF8.GetString(buffer);

            if(header.Contains("\r\n\r\n")) {

                return header;

            }

            return null;

        }

    }

}
