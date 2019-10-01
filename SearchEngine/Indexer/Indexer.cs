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

        public void Initialize(PageDB pageDB, bool pagesAreInMemory)
        {
            stopWordsDK = GetStopWords(@"C:\Users\Jacob\Desktop\Index\stopwordsDK.txt");
            this.pageDB = pageDB;
            //if we just crawled, the webpages are in memory, and we dont read from file
            if(!pagesAreInMemory) pageDB.LoadPagesFromFiles();
            tokenizer = new Tokenizer();
            termConstructor = new TermConstructor();
            pageRetriever = new PageRetriever();
            indexCreator = new IndexCreator();            
        }

        public void Run()
        {
            //index 0 is page with id 0. 
            //List<string> files = pageDB.UrlToWebpage.Select(entry => entry.Value).ToList();
            List<string> files = new List<string>();
            files.Add("How many beers can X drink?");
            files.Add("How many colas can X drink? colas");
            int idOfFile = 0; //could use for-loop instead. But the case is simply that
            //file at index 0 has id 0, so this is fine.
            foreach (string file in files)
            {
                List<string> tokens = tokenizer.GetTokens(file);
                List<string> terms = termConstructor.GetTerms(tokens, stopWordsDK);               
                indexCreator.AddTermsAndFileToIndex(terms, idOfFile);
                idOfFile++;
            }           
        }

        public List<string> GetStopWords(string path)
        {
            return System.IO.File.ReadAllLines(path).ToList();
        }

        /*
        public List<string> ProcessQuery(List<string> query)
        {

            //we also have to process the query the same way as the webpages.
            //making each query word into its token
            for(int i = 0; i < query.Count; i++)
            {
                //vi ved jo at da hvert element i query er 1 ord, så kommer der også kun 1 token ud.
                query[i] = tokenizer.GetTokens(query[i]).First();
            }

            //getting the terms from the tokens.
            query = termConstructor.GetTerms(query, stopWordsDK);

            List<List<int>> listOfPostings = new List<List<int>>();
            foreach(string term in query)
            {
                listOfPostings.Add(indexCreator.index[term]);
            }
            
            List<int> idsOfMatchedDocuments = IntersectAllLists(listOfPostings);

            List<string> urlsOfMatchedDocumnets = new List<string>();
            foreach(int id in idsOfMatchedDocuments)
            {
                urlsOfMatchedDocumnets.Add(pageDB.IdToUrl[id]);
            }

            return urlsOfMatchedDocumnets;

        }
        */
        private List<int> IntersectAllLists(List<List<int>> listOfPostings)
        {
            List<int> result = listOfPostings[0];
            for(int i = 1; i < listOfPostings.Count; i++)
            {
                //intersect first list with second. Then intersect that with third list, and so forth.
                result = result.Intersect(listOfPostings[i]).ToList();
            }

            return result;
        }
    }
}
