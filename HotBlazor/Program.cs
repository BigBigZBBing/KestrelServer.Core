#define HTTPS
#define TCP
#define UNIX
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Connections;

namespace HotBlazor
{
    class Program
    {
        static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args).ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseKestrel();

                webBuilder.ConfigureKestrel(options =>
                {
                    //设置每次请求最大时间
                    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
                    //接收请求包头最大时间
                    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
                    //Http客户端最大连接数
                    options.Limits.MaxConcurrentConnections = 100;
                    //如果协议升级到(WebSocket)
                    options.Limits.MaxConcurrentUpgradedConnections = 100;
                    //最大请求报文体大小限制
                    //协议升级后 控制器的Action里添加RequestSizeLimit特性
                    //[RequestSizeLimit(100000000)]
                    options.Limits.MaxRequestBodySize = null;
                    //最小接收请求报文体的(速率,延时)
                    options.Limits.MinRequestBodyDataRate = new MinDataRate(100, TimeSpan.FromSeconds(10));
                    //最小响应返回数据的(速率,延时)
                    options.Limits.MinResponseDataRate = new MinDataRate(100, TimeSpan.FromSeconds(10));

                    //设置HTTP2每个请求(流)的数量
                    options.Limits.Http2.MaxStreamsPerConnection = 100;
                    //设置HTTP2请求包头大小
                    options.Limits.Http2.HeaderTableSize = 4096;
                    //设置HTTP2连接帧有效负载的最大大小
                    options.Limits.Http2.MaxFrameSize = 16384;
                    //设置HTTP2请求标头大小
                    options.Limits.Http2.MaxRequestHeaderFieldSize = 8192;
                    //设置HTTP2服务器一次性缓存的最大请求主体数据大小
                    options.Limits.Http2.InitialConnectionWindowSize = 131072;
                    //设置HTTP2服务器针对每个请求（流）的一次性缓存的最大请求主体数据大小
                    options.Limits.Http2.InitialStreamWindowSize = 98304;

                    //同步IO
                    options.AllowSynchronousIO = true;
                    //HTTP监听地址和端口
                    options.Listen(IPAddress.Loopback, 5000, listenOptions =>
                    {
                        //不用UseHttps记录明文
                        listenOptions.UseConnectionLogging();
                        //配置协议
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                    });
                    //HTTPS监听地址和端口
#if HTTPS
                    options.Listen(IPAddress.Loopback, 5001, listenOptions =>
                    {
                        //在UseHttps之前记录加密
                        listenOptions.UseConnectionLogging();
                        //设置证书和密码
                        listenOptions.UseHttps("testCert.pfx", "testPassword", config =>
                        {
                            //配置SSL
                            config.SslProtocols = SslProtocols.Tls12;
                        });
                        //在UseHttps之前记录解密
                        listenOptions.UseConnectionLogging();
                    });
#endif

#if UNIX
                    //在Linux使用Nginx的高性能接套字
                    options.ListenUnixSocket("/tmp/kestrel-test.sock");
                    options.ListenUnixSocket("/tmp/kestrel-test.sock", listenOptions =>
                    {
                        listenOptions.UseHttps("testCert.pfx", "testpassword");
                    });
#endif

#if TCP
                    options.Listen(IPAddress.Loopback, 8787, listenOptions =>
                    {
                        //配置协议 不使用HTTP1和HTTP2 就只能接收TCP协议了
                        //listenOptions.Protocols = HttpProtocols.None;
                        listenOptions.UseConnectionHandler<SocketHandler>();
                        listenOptions.UseConnectionLogging("TcpInfo");
                    });
#endif

                    options.ConfigureEndpointDefaults(listenOptions =>
                    {
                        // Configure endpoint defaults
                    });
                });

                webBuilder.Configure(app =>
                {
                    //引用WebSocket中间件 可以接入WebSocket的协议
                    app.UseMiddleware<SocketMode.WebSocketMidWare>();
                });


            }).Build().Run();
        }
    }
}
