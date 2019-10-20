using HtmlAgilityPack;
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
        public Dictionary<string, string> UrlToWebpage = new Dictionary<string, string>();


        //id --> url
        //bruges til når vi skal slå op i index, og får pageId, så skal vi kunne finde
        //det url til den pageId, og igennem det, selve pagen.
        public Dictionary<int, string> IdToUrl = new Dictionary<int, string>();

        int nextPageId = 0;


        public void AddPageToPages(string url, string webpage)
        {
            UrlToWebpage.Add(url, webpage);
            IdToUrl.Add(nextPageId, url);
            nextPageId++;
        }

        public int GetNumOfCrawledPages()
        {
            return UrlToWebpage.Count;
        }

        public void LoadPagesFromFiles(int count)
        {
          
            //we assume that when we load, we currently have 0 pages cached.
            nextPageId = 0;
            List<string> files = Directory.GetFiles(@"C:\Users\Jacob\Desktop\WebcrawlerData\Websites\").ToList();
            if (count == 0)
            {
                count = files.Count / 2;
            }

            for (int i = 0; i < count*2; i += 2)
            {
                string url = File.ReadAllText(files[i], Encoding.UTF8);
                string webpage = File.ReadAllText(files[i + 1], Encoding.UTF8);
                AddPageToPages(url, webpage);               
            }
        }

        public void WritePagesToFilesTextEtd(int offset)
        {
            int index = 0;
            for (int i = 0; i < 50; i++)
            {
                
                index = i + (50 * offset);
                string fileName = UrlToWebpage.ElementAt(index).Key;
                string webpage = GetAllTextFromWebpage(UrlToWebpage.ElementAt(index).Value);
                System.IO.File.WriteAllText(WebCrawler.folderPath + $"\\Websites\\{index}-url", fileName, Encoding.UTF8);
                System.IO.File.WriteAllText(WebCrawler.folderPath + $"\\Websites\\{index}-webpage", webpage, Encoding.UTF8);
            }

        }


        public void WritePagesToFilesRaw(int offset)
        {
            int index = 0;
            for (int i = 0; i < 50; i++)
            {

                index = i + (50 * offset);
                string fileName = UrlToWebpage.ElementAt(index).Key;
                string webpage = UrlToWebpage.ElementAt(index).Value;
                System.IO.File.WriteAllText(WebCrawler.folderPath + $"\\Websites\\{index}-url", fileName, Encoding.UTF8);
                System.IO.File.WriteAllText(WebCrawler.folderPath + $"\\Websites\\{index}-webpage", webpage, Encoding.UTF8);
            }

        }

        public string GetAllTextFromWebpage(string webpage)
        {
            string text = "";
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(webpage);
            //for at undgå alt whitespace, så kan man lige tjekke at innerText.Trim ikke kun indeholder "".
            foreach (HtmlNode node in doc.DocumentNode.Descendants().Where(n =>
                                                                           n.NodeType == HtmlNodeType.Text &&
                                                                           n.ParentNode.Name != "script" &&
                                                                           n.ParentNode.Name != "style"))
            {
                text += node.InnerText.Trim() + " ";//mellemrum imellem text elementer.
            }
            return text;
        }

        
    }
}
