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
            webCrawler.Run();
        }
    }

   
}
