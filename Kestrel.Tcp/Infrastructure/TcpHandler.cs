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
            Console.WriteLine($"欢迎用户{connection.RemoteEndPoint}加入");

            IDuplexPipe pipe = connection.Transport;
            PipeReader pipeReader = pipe.Input;

            while (true)
            {
                try
                {
                    ReadResult readResult = await pipeReader.ReadAsync();
                    ReadOnlySequence<byte> readResultBuffer = readResult.Buffer;

                    if (readResult.IsCompleted)
                    {
                        Console.WriteLine($"用户{connection.RemoteEndPoint}退出");
                        break;
                    }

                    string messageSegment = Encoding.UTF8.GetString(readResultBuffer.FirstSpan);
                    Console.WriteLine(messageSegment);

                    //await pipe.Output.WriteAsync(segment);

                    pipeReader.AdvanceTo(readResultBuffer.End, readResultBuffer.End);
                }
                catch
                {
                    Console.WriteLine($"用户{connection.RemoteEndPoint}退出");
                    break;
                }
            }
        }
    }
}
