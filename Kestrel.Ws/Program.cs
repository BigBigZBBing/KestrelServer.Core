using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;

namespace Kestrel.Ws
{
    class Program
    {
        static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args).ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, 5000, listenOptions =>
                    {
                        listenOptions.UseConnectionLogging();
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                    });
                });

                webBuilder.Configure(app =>
                {
                    var options = new WebSocketOptions()
                    {
                        //长连接检测间隔
                        KeepAliveInterval = TimeSpan.FromMinutes(2)
                    };
                    //设置跨域地址
                    options.AllowedOrigins.Add("*");
                    app.UseWebSockets(options);
                    app.UseMiddleware<SocketMode.WebSocketMidWare>();
                });
            }).Build().Run();
        }
    }
}
