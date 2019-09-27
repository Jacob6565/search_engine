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

        public void Initialize(PageDB pageDB, bool justCrawled)
        {
            stopWordsDK = GetStopWords(@"C:\Users\Jacob\Desktop\Index\stopwordsDK.txt");
            this.pageDB = pageDB;
            //if we just crawled, the webpages are in memory
            if(!justCrawled) pageDB.LoadPagesFromFiles();
            tokenizer = new Tokenizer();
            termConstructor = new TermConstructor();
            pageRetriever = new PageRetriever();
            indexCreator = new IndexCreator();
            
        }

        public void Run()
        {
            List<string> files = new List<string>();
            string dummyFile = "Jeg kan ikke lide U.S.A, men jeg tror jeg er up-to-date med global opvarmning. Tror også jeg har en kage til Anders' fødselsdag.";
            string dummyFile1 = "Jeg kan ikke lide citronmåne, men jeg tror jeg er bag ud med lektierne. Tror også jeg har en idé til Anders' bryllup.";
            string dummyFile2 = "Jeg kan godt lide kage, men jeg tror jeg er up-to-date med bagedysten. Tror også jeg har en tale klar til Andreas' indflytterfest.";
            string dummyFile3 = "Jeg kan lide Rusland, men jeg tror jeg er up-to-date med cykling. Tror også jeg har en gave til Anders' begravelse.";
            files.Add(dummyFile);
            files.Add(dummyFile1);
            files.Add(dummyFile2);
            files.Add(dummyFile3);

            //index 0 is page with id 0. 
            List<string> realFiles = pageDB.Webpages.Select(entry => entry.Value).ToList();
            int idOfDummyFile = 0;
            foreach (string file in files)
            {
                List<string> tokens = tokenizer.GetTokens(dummyFile);
                List<string> terms = termConstructor.GetTerms(tokens, stopWordsDK);
                indexCreator.AddTermsAndFileToIndex(terms, idOfDummyFile);
                idOfDummyFile++;
            }           
        }

        public List<string> GetStopWords(string path)
        {
            return System.IO.File.ReadAllLines(path).ToList();
        }

        public void ProcessQuery(List<string> query)
        {
            //Husk det er en boolean query vi laver.
            //men hvor man kun kan sige hvilke ord der skal være der
            //men ikke dem som ikke skal eller alternativer
            //aka du må kun bruge AND.
        }

    }
}
