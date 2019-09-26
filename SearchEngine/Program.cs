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
