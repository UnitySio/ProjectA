using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.Net_Core_Http_RestAPI_Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.Limits.MaxConcurrentConnections = null;
                        serverOptions.Limits.MaxConcurrentUpgradedConnections = null;
                        serverOptions.Limits.MaxRequestBodySize = 1073741824; //1GB
                        /*
                        serverOptions.Limits.MinRequestBodyDataRate =
                            new MinDataRate(bytesPerSecond: 100,
                                gracePeriod: TimeSpan.FromSeconds(10));
                        serverOptions.Limits.MinResponseDataRate =
                            new MinDataRate(bytesPerSecond: 100,
                                gracePeriod: TimeSpan.FromSeconds(10));
                        serverOptions.Listen(IPAddress.Loopback, 5000);
                        serverOptions.Listen(IPAddress.Loopback, 5001,
                            listenOptions =>
                            {
                                listenOptions.UseHttps("testCert.pfx",
                                    "testPassword");
                            });
                        serverOptions.Limits.KeepAliveTimeout =
                            TimeSpan.FromMinutes(2);
                        serverOptions.Limits.RequestHeadersTimeout =
                            TimeSpan.FromMinutes(1);

                        serverOptions.Limits.Http2.MaxStreamsPerConnection = 10000;
                        serverOptions.Limits.Http2.MaxFrameSize = 32768;*/
                    });
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls(WAS_Config.getWAS_URLInfo());
                });
    }
}
