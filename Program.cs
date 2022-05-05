using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CS465_SearchEngine.Source.Index;
using CS465_SearchEngine.Source.Web;
using Microsoft.Extensions.DependencyInjection;

namespace CS465_SearchEngine
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();

            QueryService service = (QueryService) host.Services.GetService(typeof(QueryService)); //host.Services..ApplicationServices.GetRequiredService<QueryService>();
            service.Initialize("./Environment/index.txt");

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
