using SearchEngine.Indexer;
using SearchEngine.WebCrawler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    public static class DI
    {

        //Ranker namespace
        public static Ranker.Ranker ranker;

        //Indexer namespace
        public static Indexer.Indexer indexer;
        public static UrlFilter urlFilter;
        public static UrlFrontier urlFrontier;
        public static IndexCreator indexCreator;
        public static PageRetriever pageRetriever;
        public static TermConstructor termContructor;
        public static Tokenizer tokenizer;

        //Webcrawler namespace
        public static WebCrawler.WebCrawler webCrawler;
        public static PageDB pageDB;
        public static DuplicatePageChecker DPC;
        public static DuplicateURLChecker DUC;
        public static PageFetcher pageFetcher;
        public static PageParser pageParser;
        public static void Initialize()
        {
            ranker = new Ranker.Ranker();
            indexer = new Indexer.Indexer();
            urlFilter = new UrlFilter();
            urlFrontier = new UrlFrontier();
            indexCreator = new IndexCreator();
            pageRetriever = new PageRetriever();
            termContructor = new TermConstructor();
            tokenizer = new Tokenizer();
            webCrawler = new WebCrawler.WebCrawler();
            pageDB = new PageDB();
            DPC = new DuplicatePageChecker();
            DUC = new DuplicateURLChecker();
            pageFetcher = new PageFetcher();
            pageParser = new PageParser();
        }

        public static void GiveDependencies()
        {
            //kalde alle initalize.
            webCrawler.Initialize();
            DPC.Initialize();
            ranker.Initialize();
            indexer.Initialize(false, 50);

        }
    }
}
