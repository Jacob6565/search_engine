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
            for(int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] == "")//hvis der er flere mellemrum efter hinanden, så gør split på ' ' at der kommer et element som blot er "", og dem gider vi ikke at have.
                {
                    continue;
                }
                //blot køre igennem stregen og så tage at erstatte alle
                // de chars der ligger uden for ascii rangen for bogstaver med ingenting.
                tokens[i] = tokens[i].ToLower();
                
                if (tokens[i].Contains('.'))
                {
                    tokens[i] = tokens[i].Replace(".", "");
                }

                if (tokens[i].Contains('-'))
                {
                    tokens[i] = tokens[i].Replace("-", "");
                }

                if (tokens[i].Contains(','))
                {
                    tokens[i] = tokens[i].Replace(",", "");
                }

                if (tokens[i].Contains("'"))
                {
                    tokens[i] = tokens[i].Replace("'", "");
                }

                modifiedTokens.Add(tokens[i]);

            }

            return modifiedTokens;
        }
    }
}
