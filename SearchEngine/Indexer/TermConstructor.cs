using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Indexer
{
    //(linguistic) feature constructor modules på slide 23
    public class TermConstructor
    {
        private string getStemmedToken(string token)
        {
            return token;
        }
        public List<string> GetTerms(List<string> tokens, List<string> stopWords)
        {
            HashSet<string> terms = new HashSet<string>();
            //Should also perform some sort of stemming.
            //But for know, I remove stopwords and duplicates.
            foreach(string token in tokens)
            {
                if (!stopWords.Contains(token))
                {
                    string stemmedToken = getStemmedToken(token);
                    if (!terms.Contains(stemmedToken))
                    {
                        terms.Add(stemmedToken);
                    }
                }
            }
            return terms.ToList();
        }
    }
}
