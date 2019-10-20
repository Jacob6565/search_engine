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
        //docID --> tf-idfs-value
        public Dictionary<int, double> tfidfvalues = new Dictionary<int, double>();

        //docID --> weight-vector.
        public Dictionary<int, List<double>> Length = new Dictionary<int, List<double>>();

        //docID --> pagerank
        public Dictionary<int, double> pagerank = new Dictionary<int, double>();

        //docID --> totalscore (pagerank * tfidfvalues)
        public Dictionary<int, double> Score = new Dictionary<int, double>();

        public Indexer.Indexer indexer;
        public List<int> IdsOfMatchedDocuments;
        public List<string> termsInQuery;
        public PageDB pageDB;
        public UrlFilter urlFilter;


        public void Initialize()
        {
            this.indexer = DI.indexer;
            this.pageDB = DI.pageDB;
            this.urlFilter = DI.urlFilter;
        }

        public void CalcuatePageRank(List<string> query)
        {
            this.termsInQuery = query;
            int numberOfPages = pageDB.GetNumOfCrawledPages();

            //calculate normal P-matrix
            double[,] P = new double[numberOfPages, numberOfPages];
            for (int row = 0; row < numberOfPages; row++)
            {
                for (int col = 0; col < numberOfPages; col++)
                {
                    if (iContainsRefToj(row, col))
                    {
                        P[row, col] = 1;
                    }
                    else
                    {
                        P[row, col] = 0;
                    }
                }
            }
            //normalize each entry
            List<double> sumOfEachRow = new List<double>();

            for (int row = 0; row < numberOfPages; row++)
            {
                double sum = 0;
                for (int col = 0; col < numberOfPages; col++)
                {
                    sum += P[row, col];
                }
                sumOfEachRow.Add(sum);
            }


            for (int row = 0; row < numberOfPages; row++)
            {                
                for (int col = 0; col < numberOfPages; col++)
                {
                    if (P[row, col] == 1)
                    {
                        P[row, col] = P[row, col] / sumOfEachRow[row];
                    }       
                }
            }

            //tilføj alt det med alpha osv
            double alpha = 0.1;
            double teleportProb = 1 / numberOfPages;

            //udregner PPagerank
            for (int row = 0; row < numberOfPages; row++)
            {
                for (int col = 0; col < numberOfPages; col++)
                {
                    P[row, col] =  ((1 - alpha) * P[row, col]) + (alpha * teleportProb);
                    
                }
            }

            //assume we start on page 0
            double[] q = new double[numberOfPages];
            for(int i = 0; i < numberOfPages; i++)
            {
                if (i == 0)
                {
                    q[i] = 1;
                }
                else
                {
                    q[i] = 0;
                }
            }

            //calculating new q

            //ineffective iteration over arrays
            //loop hvor man udregner qi indtil den ikke afviger særlig meget
            int iterations = 20; //ifølge slides kunne man også nøjes med mindre (slide 47)
            for (int i = 0; i < iterations; i++)
            {
                for (int col = 0; col < numberOfPages; col++)
                {
                    double sum = 0;
                    for (int row = 0; row < numberOfPages; row++)
                    {
                        sum += P[row, col] * q[row];
                    }

                    q[col] = sum;
                }
            }

            //add pagerank værdier til dictionary.
            for(int i = 0; i < numberOfPages; i++)
            {
                pagerank.Add(i, q[i]);
            }
        }

        private bool iContainsRefToj(int i, int j)
        {
            string jUrl = pageDB.IdToUrl[j];
            string iUrl = pageDB.IdToUrl[i];
            string iWebpage = pageDB.UrlToWebpage[iUrl];
            List<string> iLinks = urlFilter.FindLinks(iWebpage, iUrl);

            return iLinks.Contains(jUrl);
        }



        public void CalculateTfidfvalues(List<string> query)
        {
            this.termsInQuery = query;
            CalculateDocumentScores(query);
            //have en foreach, hvor man extracter vægtene for alle terms i doci, 
            //men kun de terms som også er i query t, de andre er jo irrelevante.
            //og med vægte mener jeg, at man udregner tf-idf value ud fra tf og df værdierne
            //for term t og document i

            //det samme gør man så for querien, altså udregner tf-idf værdien 
            //for de terms der indgår i den querien.
        }

        public void CalculateTotalScore()
        {
            int numberOfPages = pageDB.GetNumOfCrawledPages();
            for(int i = 0; i < numberOfPages; i++)
            {
                double tfidfvalue = tfidfvalues[i];
                double cpagerank = pagerank[i];
                Score.Add(i, tfidfvalue * cpagerank);
            }
        }

        public List<int> TopNDocuments(int n)
        {
            //skal lige ændres til scores når pagerank er blevet implementeret.
            return Score.OrderByDescending(x => x.Value).Select(x => x.Key).Take(n).ToList();
        }

        private void CalculateDocumentScores(List<string> termsFromQuery)
        {
            //length of file interms of how many terms it contains
            int numberOfDocuments = pageDB.GetNumOfCrawledPages();
            
            foreach (string term in termsFromQuery)
            {
                //retrieve postings for term.
                List<Tuple<int, int>> postings = new List<Tuple<int, int>>();
                try
                {
                    postings = indexer.indexCreator.index[term];
                }
                catch(Exception)
                {
                    //så hvis der ikke er nogle dokumenter som indeholder termen.
                    //så får vi en fejl og vi prøver blot med næste term
                    continue;
                }

                //df is equal to the length of the postings.
                int df = postings.Count;
                double Qtfidf = CalculateTfIdfForQuery(term, termsFromQuery, numberOfDocuments, df);

                //for each documents in that postings, i.e. the documents containing the term
                //we calculate the score it gets for this term
                foreach (Tuple<int, int> docTfPair in postings)
                {
                    //hvis det er første gang vi støder på documentet.
                    if (!tfidfvalues.ContainsKey(docTfPair.Item1))
                    {
                        tfidfvalues.Add(docTfPair.Item1, 0);
                    }

                    double Dtfidf = CalculateTfIdfForDocument(docTfPair, numberOfDocuments, df);


                    tfidfvalues[docTfPair.Item1] += Qtfidf * Dtfidf;
                    //int numberOftermsInDoc = indexer.IdToTerms[docTfPair.Item1].Count;
                    //Score[docTfPair.Item1] = Score[docTfPair.Item1] / numberOftermsInDoc;
                }
            }

            //Go through all terms and their postings
            //to calculate the vector of weights for each document
            //that matched the query in some form or another.
            foreach(var entry in indexer.indexCreator.index)
            {
                List<Tuple<int, int>> postings = entry.Value;
                int df = postings.Count;
                //go through the entries for the posting of the terms
                foreach (Tuple<int,int> docTfPair in postings)
                {
                    //if the document matched the query in general
                    //and not just regarding a specific term.
                    if (tfidfvalues.ContainsKey(docTfPair.Item1))
                    {
                        double Dtfidf = CalculateTfIdfForDocument(docTfPair, numberOfDocuments, df);

                        if (!Length.ContainsKey(docTfPair.Item1))
                        {
                            //vektoren er en liste af alle tfidf værdier for alle terms
                            //i et dokument og ikke blot dem den matchede querien med.
                            Length.Add(docTfPair.Item1, new List<double>() { Dtfidf });
                        }
                        else
                        {
                            Length[docTfPair.Item1].Add(Dtfidf);
                        }
                    }
                }
            }

            //calculate the length of each vector.
            foreach(int key in tfidfvalues.Keys.ToList())
            {
                double sumOfVectorElementsSquared = 0;
                List<double> vector = Length[key].ToList();
                foreach(double element in vector)
                {
                    sumOfVectorElementsSquared += (element*element);
                }

                tfidfvalues[key] = tfidfvalues[key] / (Math.Sqrt(sumOfVectorElementsSquared));                                
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
