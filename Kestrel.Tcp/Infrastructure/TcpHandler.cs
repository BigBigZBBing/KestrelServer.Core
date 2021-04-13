using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kestrel.Tcp.Infrastructure
{
    public class TcpHandler : ConnectionHandler
    {
        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            var connectionId = connection.ConnectionId;
            var endpoint = connection.RemoteEndPoint;
            Console.WriteLine($"欢迎用户{connection.RemoteEndPoint}加入");
            IDuplexPipe pipe = connection.Transport;
            PipeReader pipeReader = pipe.Input;
            ReadResult readResult = await pipeReader.ReadAsync();
            ReadOnlySequence<byte> readResultBuffer = readResult.Buffer;
            var BytesTransferred = readResultBuffer.FirstSpan.Length;
            await pipe.Output.WriteAsync(Encoding.UTF8.GetBytes("收到" + connectionId));

        }
    }
}
