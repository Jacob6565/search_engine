using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Indexer
{
    //Indexer på slide 23
    public class IndexCreator
    {
        //term --> list of pageid (aka postings)
        public Dictionary<string, List<int>> index = new Dictionary<string, List<int>>();
        public void AddTermsAndFileToIndex(List<string> terms, int fileId)
        {
            bool newTermAdded = false;
            foreach(string term in terms)
            {
                if (!index.ContainsKey(term))
                {
                    newTermAdded = true;
                    index.Add(term, new List<int>() { fileId });
                }
                else
                {
                    index[term].Add(fileId);
                }

            }

            //we have to have the dictionary sorted by terms (primary) and pageId(secondary)
            //kan dog ikke se hvorfor, for der er alligevel konstant opslagstid i dictionary.
            //slide 35, ved dog ikke hvordan man multilevel sorter.
            if (newTermAdded)
            {
                
            }
        }
    }
}
