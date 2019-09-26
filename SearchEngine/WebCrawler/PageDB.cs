using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.WebCrawler
{
    //Fungerer som Doc FP's elementet.

    /// <summary>
    /// Used to contain the webpages in memory. But also to read and write from files.
    /// </summary>
    public class PageDB
    {
        // url --> htmlpage
        public Dictionary<string, string> Webpages = new Dictionary<string, string>();

        public int GetNumOfCrawledPages()
        {
            return Webpages.Count;
        }

        public void LoadPagesFromFiles()
        {
            List<string> files = Directory.GetFiles(@"C:\Users\Jacob\Desktop\WebcrawlerData\Websites\").ToList();
            for (int i = 0; i < files.Count; i += 2)
            {
                string url = System.IO.File.ReadAllText(files[i]);
                string webpage = System.IO.File.ReadAllText(files[i + 1]);
                Webpages.Add(url, webpage);
            }
        }

        public void WritePagesToFiles(int offset)
        {
            int index = 0;
            for (int i = 0; i < 50; i++)
            {
                index = i + (50 * offset);
                string fileName = Webpages.ElementAt(index).Key;
                string webpage = Webpages.ElementAt(index).Value;
                System.IO.File.WriteAllText(WebCrawler.folderPath + $"\\Websites\\{index}-url", fileName);
                System.IO.File.WriteAllText(WebCrawler.folderPath + $"\\Websites\\{index}-webpage", webpage);
            }
        }
    }
}
