using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using SearchEngine.WebCrawler;

namespace SearchEngine
{
    //TODO: Gjort sådan, at alle klasse blot laves som en statisk public variable,
    //istedet for at de forskellige instanser rundt, men så kan de forskellige
    //klasser blot tilgå dem. Ligesom i Giraf. De bruger alligevel alle de samme instanser
    //som dem der laves her i main.

    // ifht linked based ranking:
    //lave en dictionary fra docID -> pagerank og så efter at have udregnet deres tf-idfs-values
    //så læg deres pagerank til deres endelige score og så tag 10 top ud fra den score.

    //ifht pagerank
    /*
     * lav en matrix med dimensionerne nxn, hvor n er antallet af webpages. Hvor row 1 så
     * er den vektor som beskriver de links der går ud fra webpage med id 1. Så går man blot
     * igennem hver page og trekker om den har links til nogle af de andre sider, hvis fx page i har
     * link til page j, så bliver index i (row) , j (col) lig 1. Efter man så har 
     * for hele page j, så kan der jo være fx 51 1-taller, hvis den har 51 links til
     * nogle andre sider indexet. Men summen skal jo være 1, så blot del hvert 1-tal med 51
     * aka del alle 1 taller med summen af rækken (antal 1-taller). Så nu har du den initielle 
     * matrix. Så vælg alpha til at være lig 0.1 og så gang denne matrix med (1-alpha). Dernæst
     * så skal vi også plusse hver indgang med alpha*(1/n), hvor n er antal webpages. Og så har 
     * vi den rigtige matrix. Og så starter vi blot med q0 osv. og så når den stabiliserer sig
     * så vil entry i være lig pagerank for page i. Og så bliver den overall score lig dens
     * tf-idfs-value * pagerank, da dem med høj pagerank så vil få boosted deres score. 
         */
                 
    public class Program
    {
        static bool crawl = true;
        static void Main(string[] args)
        {
            DI.Initialize();
            DI.GiveDependencies();
            if (!crawl)
            {
                WebCrawler.WebCrawler webCrawler = DI.webCrawler;                
                webCrawler.Run();
            }
            Indexer.Indexer indexer = DI.indexer;
            indexer.Run();
            List<string> urlsOfMatchedDocuments = QueryPages(indexer);
            if (urlsOfMatchedDocuments.Count == 0)
            {
                Console.WriteLine("No matching pages");
            }
            else
            {
                Console.WriteLine("Url of matching pages:");
                foreach(string url in urlsOfMatchedDocuments)
                {
                    Console.WriteLine(url);
                }
            }
            Console.ReadLine();
        }

        public static List<string> QueryPages(Indexer.Indexer indexer)
        {
            string query = "Jyllands, jyllandsposten, Amerika, Øl, Cirkus, Fastelavn";

            return indexer.ProcessQuery(query);

        }
    }

   
}
