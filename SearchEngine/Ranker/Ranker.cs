using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchEngine.Indexer;

namespace SearchEngine.Ranker
{
    public class Ranker
    {
        //docID --> score
        public Dictionary<int, int> Score = new Dictionary<int, int>();
        public Indexer.Indexer indexer;

        public Ranker(Indexer.Indexer indexer)
        {
            //vi skal jo have adgang til indexet.
            this.indexer = indexer;
        }


        public List<int> Top10Documents()
        {

            //have en foreach, hvor man extracter vægtene for alle terms i doci, 
            //men kun de terms som også er i query t, de andre er jo irrelevante.
            //og med vægte mener jeg, at man udregner tf-idf value ud fra tf og df værdierne
            //for term t og document i

            //det samme gør man så for querien, altså udregner tf-idf værdien 
            //for de terms der indgår i den querien.
            return Score.Values.OrderByDescending(x => x).Take(10).ToList(); 
        } 

    }
}
