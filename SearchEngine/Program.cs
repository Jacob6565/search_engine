using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using SearchEngine.WebCrawler;

namespace SearchEngine
{
    public class Program
    {
        static bool crawl = true;
        static bool pagesAreInMemory = true;
        static void Main(string[] args)
        {
            PageDB pageDB = new PageDB();
            if (!crawl)
            {
                WebCrawler.WebCrawler webCrawler = new WebCrawler.WebCrawler();
                webCrawler.Initialize(pageDB);
                webCrawler.Run();
            }
            Ranker.Ranker ranker = new Ranker.Ranker();
            Indexer.Indexer indexer = new Indexer.Indexer();
            indexer.Initialize(pageDB, ranker, !pagesAreInMemory, 50);
            indexer.Run();
            List<string> urlsOfMatchedDocuments = QueryPages(indexer);
            if (urlsOfMatchedDocuments.Count == 0)
            {
                Console.WriteLine("No matching pages");
            }
            else
            {
                Console.WriteLine("Url of matching pages:");
                foreach(string url in urlsOfMatchedDocuments)
                {
                    Console.WriteLine(url);
                }
            }
            Console.ReadLine();
        }

        public static List<string> QueryPages(Indexer.Indexer indexer)
        {
            string query = "Jyllands, jyllandsposten, Amerika, Øl, Cirkus, Fastelavn";

            return indexer.ProcessQuery(query);

        }
    }

   
}
