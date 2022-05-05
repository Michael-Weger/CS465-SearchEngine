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

// Michael Weger
// CS465, S22, Project #1

namespace CS465_SearchEngine
{
    public class Program
    {
        /// <summary>
        /// Main entry to the program.
        /// </summary>
        /// <param name="args">Command line arguments, $documentsPath, $indexPath, $stopWordsFile</documents></param>
        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();

            DocumentMap documentMap = new DocumentMap("./Environment/documents.txt"); // Document mapping. Maps document IDs to file paths.
            InvertedIndex index = new InvertedIndex("./Environment/index.txt"); // The inverted index for this IR system.
            ParserInverter tokenizer = new ParserInverter(index, documentMap, "./DocumentInput", "./Documents", true, "./Environment/stopWordsLong.txt"); // File parser and inverter

            // Print information
            documentMap.Print();
            index.traverse();

            // Display statistics
            documentMap.PrintStatistics();
            index.PrintStatistics();

            // Initalize the service to allow users to send queries.
            QueryService service = (QueryService) host.Services.GetService(typeof(QueryService));
            service.Initialize(index, documentMap);

            // Launch the web host.
            host.Run();
        }

        /// <summary>
        /// Auto generated with Blazor project. Creates the web host.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
