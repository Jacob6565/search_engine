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
            Indexer.Indexer indexer = new Indexer.Indexer();
            indexer.Initialize(pageDB, !pagesAreInMemory);
            indexer.Run();
            List<string> matchedPages = QueryPages(new Func<List<string>, List<string>>( (x) => indexer.ProcessQuery(x)));
        }

        public static List<string> QueryPages(Func<List<string>, List<string>> processQuery)
        {
            List<string> query = new List<string>()
            {
                "abekatland"
            };

            return processQuery(query);

        }
    }

   
}
