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

            Task one = new Task( () => webCrawler.Run(1));
            Task two = new Task( () => webCrawler.Run(2));
            Task tre = new Task( () => webCrawler.Run(3));
            Task fire = new Task( () => webCrawler.Run(4));
            Task fem = new Task( () => webCrawler.Run(5));

            one.Start();
            two.Start();
            tre.Start();
            fire.Start();
            fem.Start();

        }
    }

   
}
