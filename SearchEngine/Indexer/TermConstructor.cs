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
            List<string> terms = new List<string>();
            //Should also perform some sort of stemming.
            //But for know, I remove stopwords.
            foreach(string token in tokens)
            {
                if (!stopWords.Contains(token))
                {
                    string stemmedToken = getStemmedToken(token);
                   
                    terms.Add(stemmedToken);
                }
            }
            return terms.ToList();
        }
    }
}
