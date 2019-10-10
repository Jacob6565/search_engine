using SearchEngine.WebCrawler;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Indexer
{
    public class Indexer
    {
        public List<string> stopWordsDK;
        public PageDB pageDB;
        public Tokenizer tokenizer;
        public TermConstructor termConstructor;
        public PageRetriever pageRetriever;
        public IndexCreator indexCreator;
        public Ranker.Ranker ranker;

        //docID --> terms
        public Dictionary<int, List<string>> IdToTerms = new Dictionary<int, List<string>>();

        public void Initialize(PageDB pageDB, Ranker.Ranker ranker, 
                               bool pagesAreInMemory, int loadCount)
        {
            stopWordsDK = GetStopWords(@"C:\Users\Jacob\Desktop\Index\stopwordsDK.txt");
            this.pageDB = pageDB;
            this.ranker = ranker;
            //if we just crawled, the webpages are in memory, and we dont read from file
            if(!pagesAreInMemory) pageDB.LoadPagesFromFiles(loadCount);
            tokenizer = new Tokenizer();
            termConstructor = new TermConstructor();
            pageRetriever = new PageRetriever();
            indexCreator = new IndexCreator();            
        }

        public void Run()
        {
            //index 0 is page with id 0. 
            List<string> files = pageDB.UrlToWebpage.Select(entry => entry.Value).ToList();
           
            int idOfFile = 0; //could use for-loop instead. But the case is simply that
            //file at index 0 has id 0, so this is fine.
            foreach (string file in files)
            {
                List<string> tokens = tokenizer.GetTokens(file);
                List<string> terms = termConstructor.GetTerms(tokens, stopWordsDK);
                IdToTerms.Add(idOfFile, terms);
                indexCreator.AddTermsAndFileToIndex(terms, idOfFile);
                idOfFile++;
            }           
        }

        public List<string> GetStopWords(string path)
        {
            return System.IO.File.ReadAllLines(path).ToList();
        }

        
        public List<string> ProcessQuery(string query)
        {

            //we also have to process the query the same way as the webpages.            
            List<string> tokensFromQuery = tokenizer.GetTokens(query);
            //getting the terms from the tokens.
            //tror ikke jeg eliminere duplicates længere, da jeg skal udregne tf for 
            //en term i en query, men det vil vel forårsage at dokumenter som indeholder
            //en term med duplicates får dobbelt score.
            List<string> termsFromQuery = termConstructor.GetTerms(tokensFromQuery, stopWordsDK);                              
            
            List<int> Top10 = ranker.Top10Documents(this, termsFromQuery, pageDB);

            List<string> urlsOfMatchedDocuments = new List<string>();
            foreach(int id in Top10)
            {
                urlsOfMatchedDocuments.Add(pageDB.IdToUrl[id]);
            }

            return urlsOfMatchedDocuments;

        }
        
        private List<int> IntersectAllLists(List<List<Tuple<int, int>>> listOfPostings)
        {
            List<Tuple<int,int>> result = listOfPostings[0];
            for(int i = 1; i < listOfPostings.Count; i++)
            {
                //intersect first list with second. Then intersect that with third list, and so forth.
                result = result.Intersect(listOfPostings[i]).ToList();
            }

            return result.Select(entry => entry.Item1).ToList();
        }
    }
}
