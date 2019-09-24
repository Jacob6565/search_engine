using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webcrawler
{
    //This is the Dup URL elim element and the url set element
    public class DuplicateURLChecker
    {
        //Dictionary fra domain til hashset.
        //så key er domain og hashsettet indeholder så alle de urls 
        //der er fundet med det domæne. Så er det nemmere at tjekke 
        //om man allerede har fundet en url, da i stedet for at tjekke 
        //igennem alle links, så tjekker man kun igennem de links som har 
        //samme domæne. Så man fx ikke skal tjekke de 1000 urls fra wikipedia
        //for at vurdere om en af de links er ens med det youtube url man kigger på.
        HashSet<string> urlsPreviouslyAddedToFrontier = new HashSet<string>();
        public bool IsDuplicateUrl(string url)
        {
            if (urlsPreviouslyAddedToFrontier.Contains(url))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddToTotalListOfUrls(string url)
        {
            urlsPreviouslyAddedToFrontier.Add(url);
        }


        public void SaveAllLinksAddedToFrontier()
        {
            System.IO.File.WriteAllLines(Program.folderPath + @"\Frontier\total", urlsPreviouslyAddedToFrontier);

        }

        public void LoadAllLinksAddedToFrontier()
        {
            List<string> links = System.IO.File.ReadAllLines(Program.folderPath + @"\Frontier\total").ToList();
            foreach(string link in links)
            {
                urlsPreviouslyAddedToFrontier.Add(link);
            }
        }
    }
}
