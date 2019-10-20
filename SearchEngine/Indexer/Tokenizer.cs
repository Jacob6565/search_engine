using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Indexer
{
    //Tokenizer på slide 23.
    public class Tokenizer
    {

        private List<int> getIndexOfCharacters(string text, char character)
        {
            List<int> indexes = new List<int>();
            for(int i = 0; i < text.Length; i++)
            {
                if (text[i] == character)
                {
                    indexes.Add(i);
                }
            }

            return indexes;
        }
        public List<string> GetTokens (string file)
        {
            List<string> tokens = new List<string>();

            tokens = file.Split(' ').ToList();
            List<string> modifiedTokens = new List<string>();
            for (int i = 0; i < tokens.Count; i++)
            {
                //Gider ikke at tage forbehold for upper lower osv.
                tokens[i] = tokens[i].ToLower();
                //så vores token bliver lig sig selv, blot kun de characters som enten er whitespace eller letterordigit
                //på denne måde fjerner man .,-
                tokens[i] = new string(tokens[i].Where(c => char.IsWhiteSpace(c) || char.IsLetterOrDigit(c)).ToArray());
                                
                

                modifiedTokens.Add(tokens[i]);

            }

            //the above does not filter away empty tokens.
            modifiedTokens.RemoveAll(x => x == "");
            //do not think any terms of length < 2 is worth indexing.
            modifiedTokens.RemoveAll(x => x.Length < 2);

            return modifiedTokens;
        }
    }
}
