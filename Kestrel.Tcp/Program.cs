using Kestrel.Tcp.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore;

namespace Kestrel.Tcp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new WebHostBuilder()
            .UseKestrel()
            .ConfigureKestrel(kestrelOption =>
            {
                kestrelOption.Listen(IPAddress.Loopback, 8001, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.None;
                    listenOptions.UseConnectionHandler<TcpHandler>();
                });
            })
            .Configure(app => { })
            .Build().Run();
        }
    }
}
