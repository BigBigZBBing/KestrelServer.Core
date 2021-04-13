using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kestrel.Ws
{
    public static class SocketMode
    {
        static List<WebSocket> sockets = new List<WebSocket>();

        public static async Task<string> Receive(this WebSocket webSocket, CancellationToken cancellationToken)
        {
            var buffer = new ArraySegment<byte>(new byte[1024 * 4]);
            using MemoryStream stream = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                result = await webSocket.ReceiveAsync(buffer, cancellationToken);
                stream.Write(buffer.Array, buffer.Offset, result.Count - buffer.Offset);
            } while (!result.EndOfMessage);
            stream.Seek(0, SeekOrigin.Begin);
            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static async Task Send(this WebSocket webSocket, string msg)
        {
            CancellationToken cancellation = default(CancellationToken);
            var buf = Encoding.UTF8.GetBytes(msg);
            var segment = new ArraySegment<byte>(buf);
            await webSocket.SendAsync(segment, WebSocketMessageType.Text, true, cancellation);
        }

        public class WebSocketMidWare
        {
            private readonly RequestDelegate _next;

            public WebSocketMidWare(RequestDelegate next)
            {
                _next = next;
            }

            public async Task Invoke(HttpContext context)
            {
                if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
                {
                    var socket = await context.WebSockets.AcceptWebSocketAsync();
                    sockets.Add(socket);
                    socket.Send($"欢迎{context.Connection.Id}");
                    string message = await socket.Receive(CancellationToken.None);
                }
                else
                {
                    await _next(context);
                }
            }
        }

    }

}
