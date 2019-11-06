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
            //det gøres på helt samme måde som vi gjorde det på papir.
            //blot hvor i stedet for at betragte hver document og dermed
            //hver row og col i P matricen, så betragter vi her blot 
            //kun de documenter som matchede querien og dermed kun
            //de row og col i P matricen som svarer til id'erne for de dokumenter.

            this.termsInQuery = query;
            List<int> idOfDocsMatchingQuery = new List<int>();

            //we are only interested in the documents matching a query
            //ved faktisk ikke om man burde gøre det for alle documents.
            //men lige pt udregner vi også kun tfidf-values for de docs som matcher
            //queryen. De andre har vil bare en tfidf-value på 0. 
            foreach(string term in termsInQuery)
            {
                //retrieve postings for term.
                List<Tuple<int, int>> postings = new List<Tuple<int, int>>();
                try
                {
                    postings = indexer.indexCreator.index[term];
                }
                catch (Exception)
                {
                    //så hvis der ikke er nogle dokumenter som indeholder termen.
                    //så får vi en fejl og vi prøver blot med næste term
                    continue;
                }

                foreach (Tuple<int, int> docTfPair in postings)
                {
                    //hvis det er første gang vi støder på documentet.
                    if (!idOfDocsMatchingQuery.Contains(docTfPair.Item1))
                    {
                        idOfDocsMatchingQuery.Add(docTfPair.Item1);
                    }
                }
            }


            int numberOfPages = pageDB.GetNumOfCrawledPages();

            //NOTE: Hvis du ikke forstår hvorfor alt er foreach
            //så er det blot fordi vi kun kigger på de docs som matchede querien
            //så istedet for at udregne pagerank for alle pages.
            //så laver vi stadig en P matrix og en q som om vi gjorde det.
            //men vi tilgår bare kun de indgange for de id's af de documenter som 
            //matchede querien. Så meget af P matricen er ikke blevet initialiseret.
            //og det samme med q. Man kunne også bare have udregnet det for alle
            //og så nede i udregning af total score så bare kun gøre det for dem
            //som havde matchede querien, men så har vi jo udregnet pagerank for
            //nogle unødvendige, så vi gør dette i stedet.

            //make list of all links
            Dictionary<int, List<string>> idToLinks = new Dictionary<int, List<string>>();

            foreach(int i in idOfDocsMatchingQuery)
            {
                string iUrl = pageDB.IdToUrl[i];
                string iWebpage = pageDB.UrlToWebpage[iUrl];
                List<string> iLinks = urlFilter.FindLinks(iWebpage, iUrl);
                idToLinks.Add(i, iLinks);
            }

            //calculate normal P-matrix
            //since we only consider all the ids of the pages that
            //matches many indexes in this matrix are never used.
            double[,] P = new double[numberOfPages, numberOfPages];
            foreach (int row in idOfDocsMatchingQuery)
            {
                List<string> iLinks = idToLinks[row];
                foreach (int col in idOfDocsMatchingQuery)
                {
                    string jUrl = pageDB.IdToUrl[col];
                    if (iLinks.Contains(jUrl))
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
            Dictionary<int, double> sumOfEachRow = new Dictionary<int, double>();

            foreach (int row in idOfDocsMatchingQuery)
            {
                double sum = 0;
                foreach (int col in idOfDocsMatchingQuery)
                {
                    sum += P[row, col];
                }
                //for hver row gemmer vi dens sum
                sumOfEachRow.Add(row, sum);
            }


            foreach (int row in idOfDocsMatchingQuery)
            {
                foreach (int col in idOfDocsMatchingQuery)
                {
                    if (P[row, col] == 1)
                    {
                        P[row, col] = P[row, col] / sumOfEachRow[row];
                    }       
                }
            }

            //tilføj alt det med alpha osv
            double alpha = 0.1;
            double teleportProb = 1.0 / (double)numberOfPages;

            //udregner PPagerank
            foreach (int row in idOfDocsMatchingQuery)
            {
                foreach (int col in idOfDocsMatchingQuery)
                {
                    double temp = (1 - alpha) * P[row, col];
                    double temp1 = (alpha * teleportProb);
                    P[row, col] =  ((1 - alpha) * P[row, col]) + (alpha * teleportProb);
                    
                }
            }

            //assume we start on page 0
            double[] q = new double[numberOfPages];
            foreach (int i in idOfDocsMatchingQuery)
            {
                q[i] = 0;
            }

            q[idOfDocsMatchingQuery[0]] = 1;
            

            //calculating new q

            //ineffective iteration over arrays
            //loop hvor man udregner qi indtil den ikke afviger særlig meget
            int iterations = 20; //ifølge slides kunne man også nøjes med mindre (slide 47)
            for (int i = 0; i < iterations; i++)
            {
                foreach (int col in idOfDocsMatchingQuery)
                {
                    double sum = 0;
                    foreach (int row in idOfDocsMatchingQuery)
                    {
                        sum += P[row, col] * q[row];
                    }

                    q[col] = sum;
                }
            }

            //add pagerank værdier til dictionary.
            foreach (int i in idOfDocsMatchingQuery)
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
            List<int> idOfMatchedDocuments = tfidfvalues.Keys.ToList();
            foreach(int i in idOfMatchedDocuments)
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
            //essentially implements the algoritm from the book.

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
                //we calculate the score it gets for this term and accumulates it.
                foreach (Tuple<int, int> docTfPair in postings)
                {
                    //hvis det er første gang vi støder på documentet.
                    if (!tfidfvalues.ContainsKey(docTfPair.Item1))
                    {
                        tfidfvalues.Add(docTfPair.Item1, 0);
                    }

                    double Dtfidf = CalculateTfIdfForDocument(docTfPair, numberOfDocuments, df);

                    //accumulates the tf-idf value for each term the document
                    //has in common with the query. So at the end
                    //we have the total tf-idf-value for a document.
                    tfidfvalues[docTfPair.Item1] += Qtfidf * Dtfidf;
                    
                }
            }

            //Go through all terms and their postings
            //to calculate the vector of weights for each document
            //that matched the query in some form or another.
            foreach(var entry in indexer.indexCreator.index) //dette er dictionarien der gør fra term til posting
            {
                //gets the posting for current entry in the index
                List<Tuple<int, int>> postings = entry.Value;
                int df = postings.Count;
                //go through the entries for the posting of the terms
                foreach (Tuple<int,int> docTfPair in postings)
                {
                    //if the document matched the query in general
                    //and not just regarding a specific term. 
                    //altså vi kigger jo kun på de docs som matchede querien.
                    //hvilket er dem som findes i tfidfvalues-dictionarien.
                    if (tfidfvalues.ContainsKey(docTfPair.Item1))
                    {
                        double Dtfidf = CalculateTfIdfForDocument(docTfPair, numberOfDocuments, df);

                        //if first time.
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

            //had to do the .Tolist(), otherwise something went wrong #refencetypes.
            //calculate the length of each vector.
            //husk vi gør det stadig kun for de docs som matchede querien
            //aka them som findes i tfidfvalues-dictionarien.
            foreach(int key in tfidfvalues.Keys.ToList())
            {
                double sumOfVectorElementsSquared = 0;
                //listen af tfidfsvalues for alle terms i givent dokument.
                List<double> vector = Length[key].ToList();
                foreach(double element in vector)
                {
                    //det skal jo være ^2.
                    sumOfVectorElementsSquared += (element*element);
                }

                //normalize by dividing the tf-idfs value for document i with the length
                //of the weight vector for document i.
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
