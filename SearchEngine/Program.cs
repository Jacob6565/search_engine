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
        static bool alreadyCrawled = false;
        static void Main(string[] args)
        {
            PageDB pageDB = new PageDB();
            if (!alreadyCrawled)
            {
                WebCrawler.WebCrawler webCrawler = new WebCrawler.WebCrawler();
                webCrawler.Initialize(pageDB);
                webCrawler.Run();
            }
            Indexer.Indexer indexer = new Indexer.Indexer();
            indexer.Initialize(pageDB, alreadyCrawled);
            indexer.Run();
            QueryPages(new Action<List<string>>( (x) => indexer.ProcessQuery(x)));
        }

        public static void QueryPages(Action<List<string>> processQuery)
        {
            List<string> query = new List<string>()
            {
                "rusland"
            };

            processQuery(query);

        }
    }

   
}
