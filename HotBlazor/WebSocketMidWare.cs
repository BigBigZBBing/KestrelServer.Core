using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HotBlazor
{
    public static class SocketMode
    {
        public static async Task<string> RecvAsync(this WebSocket webSocket, CancellationToken cancellationToken)
        {
            var ms = new MemoryStream();
            var buffer = new ArraySegment<byte>(new byte[1024 * 8]);

            WebSocketReceiveResult result;
            do
            {
                result = await webSocket.ReceiveAsync(buffer, cancellationToken);
                ms.Write(buffer.Array, buffer.Offset, result.Count - buffer.Offset);
            } while (!result.EndOfMessage);
            ms.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(ms);
            var s = reader.ReadToEnd();
            reader.Dispose();
            ms.Dispose();
            return s;
        }

        public static async Task SendAsync(this WebSocket webSocket, string msg)
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
                }
                else
                {
                    await _next(context);
                }
            }
        }

    }

}
