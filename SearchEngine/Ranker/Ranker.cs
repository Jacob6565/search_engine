using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchEngine.Indexer;
using SearchEngine.WebCrawler;

namespace SearchEngine.Ranker
{
    public class Ranker
    {
        //docID --> score
        public Dictionary<int, double> Score = new Dictionary<int, double>();
        public Indexer.Indexer indexer;
        public List<int> IdsOfMatchedDocuments;
        public List<string> termsInQuery;
        public PageDB pageDB;

        public List<int> Top10Documents(Indexer.Indexer indexer,                                        
                                        List<string> query,
                                        PageDB pageDB)
        {
            this.indexer = indexer;
            this.termsInQuery = query;
            this.pageDB = pageDB;

            CalculateDocumentScores(query);
            //have en foreach, hvor man extracter vægtene for alle terms i doci, 
            //men kun de terms som også er i query t, de andre er jo irrelevante.
            //og med vægte mener jeg, at man udregner tf-idf value ud fra tf og df værdierne
            //for term t og document i

            //det samme gør man så for querien, altså udregner tf-idf værdien 
            //for de terms der indgår i den querien.
            return Score.OrderByDescending(x => x.Value).Select(x=> x.Key).Take(10).ToList();
        }

        public void CalculateDocumentScores(List<string> termsFromQuery)
        {
            //length of file interms of how many terms it contains
            int numberOfDocuments = pageDB.GetNumOfCrawledPages();
            
            foreach (string term in termsFromQuery)
            {
                Console.WriteLine("term");
                //retrieve postings for term.
                List<Tuple<int, int>> postings = new List<Tuple<int, int>>();
                try
                {
                    postings = indexer.indexCreator.index[term];
                }
                catch(Exception)
                {
                    //så hvis der ikke er nogle dokumenter som indeholder termen.
                    continue;
                }

                //df is equal to the length of the postings.
                int df = postings.Count;
                double Qtfidf = CalculateTfIdfForQuery(term, termsFromQuery, numberOfDocuments, df);

                //for each documents in that postings, i.e. the documents containing the term
                //we calculate the score it gets for this term
                foreach (Tuple<int, int> docTfPair in postings)
                {
                    if (!Score.ContainsKey(docTfPair.Item1))
                    {
                        Score.Add(docTfPair.Item1, 0);
                    }

                    double Dtfidf = CalculateTfIdfForDocument(docTfPair, numberOfDocuments, df);

                    Score[docTfPair.Item1] += Qtfidf * Dtfidf;
                    int numberOftermsInDoc = indexer.IdToTerms[docTfPair.Item1].Count;
                    Score[docTfPair.Item1] = Score[docTfPair.Item1] / numberOftermsInDoc;
                }
            }
        }


        private double CalculateTfIdfForDocument(Tuple<int, int> docTfPair,
                                              int numberOfDocuments, int df)
        {
            int tf = docTfPair.Item2;
            double idf = Math.Log10(numberOfDocuments / df);
            return tf * idf;

        }

        private double CalculateTfIdfForQuery(string term, List<string> termsFromQuery,
                                           int numberOfDocuments, int df)
        {
            int tf = termsFromQuery.Where(x => x == term).Count();
            double idf = Math.Log10(numberOfDocuments / df);

            return tf*idf;
        }
    }
}
