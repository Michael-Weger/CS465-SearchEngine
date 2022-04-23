using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CS465_SearchEngine.Source.InvertedIndex;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CS465_SearchEngine
{
    public class Program
    {
        public static void Main(string[] args)
        {
            InvertedIndex index = new InvertedIndex("./Environment/index.txt");
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
