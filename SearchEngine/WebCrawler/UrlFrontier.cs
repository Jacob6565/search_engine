using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.WebCrawler
{
    //This is the URL Frontier element.

    /// <summary>
    /// Responsible for handling the set of urls.
    /// </summary>
    public class UrlFrontier
    {
        List<string> frontier = new List<string>();

        public void AddUrl(string url)
        {
            frontier.Add(url);
        }

        public int Size()
        {
            return frontier.Count;
        }

        public string GetNewUrl()
        {
            string url = frontier[0];
            frontier.RemoveAt(0);
            return url;
        }

        public void SaveState()
        {
            System.IO.File.WriteAllLines(WebCrawler.folderPath + @"\Frontier\state", frontier);

        }

        public void LoadState()
        {
            frontier = System.IO.File.ReadAllLines(WebCrawler.folderPath + @"\Frontier\state").ToList();
        }

    }
}



