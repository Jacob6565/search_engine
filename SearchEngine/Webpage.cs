using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    public class Webpage
    {
        public int ID { get; set; }
        public string Rawpage { get; set; }
        public string Text { get; set; }
        public string Url { get; set; }
        public List<string> Links { get; set; }
    }
}
