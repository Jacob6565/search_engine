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
        //Remember, df for term t is just the lenght of t's postings.
        // term --> list of (pageid, tf)
        public Dictionary<string, List<Tuple<int, int>>> index = new Dictionary<string, List<Tuple<int, int>>>();

        //terms er alle terms i filen med fileId, inklusiv duplicates.
        public void AddTermsAndFileToIndex(List<string> terms, int fileId)
        {
            bool newTermAdded = false;
            foreach(string term in terms)
            {
                if (!index.ContainsKey(term))
                {
                    //if this is the first time we encounter the term at all among all files.
                    //we add the first element to the postings.
                    newTermAdded = true;
                    index.Add(term, new List<Tuple<int, int>>() { new Tuple<int, int>(fileId, 1) });
                    
                }
                else if (index.ContainsKey(term) && 
                        index[term].Any(entry => entry.Item1 == fileId))
                {
                    //vi har set termen før, også i current stream af tokens fra denne fil.
                    //så vi increaser tf.
                    Tuple<int, int> oldTuple = index[term].Find(entry => entry.Item1 == fileId);
                    int indexOfOldTuple = index[term].FindIndex(entry => entry.Item1 == fileId);

                    index[term][indexOfOldTuple] = new Tuple<int, int>(oldTuple.Item1, oldTuple.Item2 + 1);
                }
                else if (index.ContainsKey(term) &&
                        !index[term].Any(entry => entry.Item1 == fileId))
                {
                    //vi har termen i index (fra gamle filer vi har løbet igennem),
                    //men current file er ikke i dens postings, aka det er første
                    //gang vi støder på denne term for current file.
                    index[term].Add(new Tuple<int, int>(fileId, 1));
                }
                

            }

            //we have to have the dictionary sorted by terms (primary) and pageId(secondary)
            //kan dog ikke se hvorfor, for der er alligevel konstant opslagstid i dictionary.
            //slide 35, ved dog ikke hvordan man multilevel sorter.
            if (newTermAdded)
            {
                //han sagde dog idag, at ens postings blot skal være sorted efter docID.
            }
        }
    }
}
