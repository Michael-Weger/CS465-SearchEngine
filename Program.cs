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

            DocumentMap documentMap = new DocumentMap("./Environment/documents.txt");
            InvertedIndex index = new InvertedIndex("./Environment/index.txt");
            ParserInverter tokenizer = new ParserInverter(index, documentMap, "./DocumentInput", "./Documents", true, "./Environment/stopWordsLong.txt");

            documentMap.Print();
            index.traverse();

            documentMap.PrintStatistics();
            index.PrintStatistics();

            QueryService service = (QueryService) host.Services.GetService(typeof(QueryService));

            service.Initialize(index, documentMap);

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
