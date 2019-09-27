using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using SearchEngine.WebCrawler;

namespace SearchEngine
{
    public class Program
    {
        //Så for at lave det concurrent, skal jeg sikre mig, at jeg håndterer
        //at de ikke tilgå det samme domæne på samme tid i.e. har flere end en 
        //connection åben til et domæne af gangen

        //Derudover kan man benytte sig at concurrency dictionaries osv., som gør
        //at operationerne dertil er atomare. Dog ikke for lists. Men jeg tænker 
        //at man kan lave en ny list klasse, som nedarver fra den normale list klasse.
        //Den nye har så sin egen add funktion (AddNew), som blot er lig den gamles add 
        //funktion blot indkapslet i en semaphor, så man sikrer at kun en adder af gangen:

        //NewAdd(string item)
        //  semaphor-start
        //  base.Add(item)
        //  semaphor-release

        //og det samme kunne man gøre for remove, osv:


        //Derudover, så bliver man nok også nødt til at implementere det system med flere
        //queues. Jeg tror at det du har kørende lige nu kunne virke, men tror det med 
        //queuesne er nemmere at vedligeholde og læse, og sikkert generelt bedre 
        //da det står på slides.

        static void Main(string[] args)
        {
            WebCrawler.WebCrawler webCrawler = new WebCrawler.WebCrawler();
            webCrawler.Initialize();

            Task t1 = new Task( () => webCrawler.Run(1));
            Task t2 = new Task( () => webCrawler.Run(2));
            Task t3 = new Task( () => webCrawler.Run(3));
            Task t4 = new Task( () => webCrawler.Run(4));
            Task t5 = new Task( () => webCrawler.Run(5));

            t1.Start();
            t2.Start();
            t3.Start();
            t4.Start();
            t5.Start();

            t1.Wait();
            t2.Wait();
            t3.Wait();
            t4.Wait();
            t5.Wait();
        }
    }

   
}
