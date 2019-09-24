using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webcrawler;

namespace Indexer
{
    public class Program
    {
        static void Main(string[] args)
        {
            PageDB pageDB = new PageDB();
            pageDB.LoadPagesFromFiles();

            
        }
    }
}
