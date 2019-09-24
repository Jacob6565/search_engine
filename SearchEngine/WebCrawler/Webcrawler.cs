using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.WebCrawler
{
    public class WebCrawler
    {   
        public static string folderPath = @"C:\Users\Jacob\Desktop\WebcrawlerData";
        public void Run()
        {
            //TODO: flere crawler threads og håndtering af det.
            //TODO: frontier bliver unødvendig stor, hvilket gør, at når vi når til 
            //at have crawled page 2500, med frontier på ca 2600, så tager det lang tid 
            //at crawle videre, da det tager lang tid at tjekke om et link allerede har været
            //i frontier.


            //Starts as seed
            string currentUrl = "";// "https://www.dr.dk/";//"https://starwars.fandom.com/wiki/Luke_Skywalker";
            List<string> seeds = new List<string>()
            {
                "https://www.tv2.dk",
                //"https://www.dr.dk/",
                "https://www.jyllands-posten.dk/",
                "https://www.politiken.dk",
                "https://www.bt.dk/"
            };
            //Tænker at det skal køre i et while true loop,
            //for hvis det blot er de forskellige funktioner
            //som kalder hinanden igen og igen, så får vi nok
            //stack overflow på et tidspunkt. Så vi skal istedet
            //lade dem returnere og give svaret tilbage for så
            //at kalde den næste funktion med svaret.
            PageFetcher fetcher = new PageFetcher();
            PageParser parser = new PageParser();
            PageDB pageDB = new PageDB();
            DuplicatePageChecker DPC = new DuplicatePageChecker(pageDB);
            UrlFilter urlFilter = new UrlFilter();
            DuplicateURLChecker DUC = new DuplicateURLChecker();
            UrlFrontier urlFrontier = new UrlFrontier();

            bool recover = false;
            if (recover)
            {
                //så længe page duplication er dissabled, så burde dette være nok
                //ellers bliver jeg også nødt til at gemme og loade 
                //cachen af shingelse osv., da den nu antager at 
                //den allerede har det på filerne i db. 
                pageDB.LoadPagesFromFiles();
                DUC.LoadAllLinksAddedToFrontier();
                urlFrontier.LoadState();

            }
            else
            {
                foreach (string seed in seeds)
                {
                    urlFrontier.AddUrl(seed);
                }

            }


            //one iteration consists of fetching a page
            //check if is duplicate, if not the page is stored as a crawled page
            //find all links on that page 
            //and then for each link check if we are allowed to crawl it
            //or if we already have added it to the frontier
            //if this is good, add the link to the frontier
            //when all this has happened for all links on the page
            //we choose one of the links as the next page to crawl
            //by doing this we only crawl the pages we are allowed to
            //since all the others are never added to the frontier
            while (true)
            {
                Console.Clear();
                if (pageDB.GetNumOfCrawledPages() % 50 == 0 && pageDB.GetNumOfCrawledPages() / 50 >= 1)
                {
                    int offset = pageDB.Webpages.Count / 50;
                    Console.WriteLine("Writing to files");
                    pageDB.WritePagesToFiles(offset - 1); //-1 because index is 0-indexed, but count is not.
                    urlFrontier.SaveState();
                    DUC.SaveAllLinksAddedToFrontier();
                    Console.WriteLine("Done writing");

                }
                currentUrl = urlFrontier.GetNewUrl();

                Console.WriteLine("Url: " + currentUrl);
                Console.WriteLine("Frontier: " + urlFrontier.Size());
                Console.WriteLine("Crawled: " + pageDB.GetNumOfCrawledPages());

                if (pageDB.GetNumOfCrawledPages() >= 2000)
                {
                    break;
                }

                T("fetch");
                string webpage = fetcher.Fetch(currentUrl);
                if (webpage == "error") continue;
                T("parse");
                string parsedWebpage = parser.ParseWebPage(webpage, currentUrl);
                T("duplicate page");
                bool isDuplicatePage = DPC.IsDuplicate(currentUrl, parsedWebpage); //bottleneck, man kunne gemme hashværdierne for hver side, så når man skal sammenligne, så skal man kun udregne hash for den nye.

                if (!isDuplicatePage)//TODO: if duplication checking is done, this should be removed. So that even though it is a duplicate we can still take the links from it, we just dont save the duplicate page.
                {
                    T("find links");
                    List<string> urls = urlFilter.FindLinks(parsedWebpage, currentUrl);

                    T("AmIAllowed and duplicate link check");
                    foreach (string url in urls)
                    {
                        bool allowed = urlFilter.AmIAllowed("*", url);  //bottleneck, tilføj cache af robots.txt, dictionary fra domæne --> allows, disallows,crawltime (tuple af lists, måske)  Der er ikke rigtig nogen grund til at hente robots.txt filen for et domæne på ny hver gang du besøger det, nu hvor du laver en one-shot crawler samt det kun er 1000 pages.        
                        if (allowed)
                        {

                            bool isDuplicateUrl = DUC.IsDuplicateUrl(url); //denne kan sikkert godt blive en bottleneck, når den implementeres.
                            if (!isDuplicateUrl)
                            {
                                DUC.AddToTotalListOfUrls(url);
                                urlFrontier.AddUrl(url);
                            }
                            else
                            {
                                //ignore url, since we already visited it
                                //or are planing on doing it
                            }
                        }
                        else
                        {
                            //ignore that url, since we aren't allowed to crawl that page
                        }
                    }
                }
                else
                {
                    //ignore this page, since it is a duplicate
                }
            }
            Console.WriteLine("done crawling");
        }
        public static void T(string value)
        {
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString().Substring(0, 8) + ": " + value);
        }
    }
}
