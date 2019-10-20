using SearchEngine.UtilityClasses;
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
        private PageFetcher fetcher;
        private PageParser parser;
        private PageDB pageDB;
        private DuplicatePageChecker DPC;
        private UrlFilter urlFilter;
        private DuplicateURLChecker DUC;
        private UrlFrontier urlFrontier;

        public void Initialize(PageDB pageDB)
        {
            //TODO: implemetér near-duplicate checking af pages.

            //TODO: Synes det lidt rodet med de globale variabler "Domain" og "URL" inde i 
            //UrlFiler.cs. De burde måske ikke være derinde, men i denne fil stedet. Det
            //er blot det at de bliver sat forskellige steder, hvilket er lidt uoverskueligt.

            List<string> danskeSeeds = new List<string>()
            {
                "https://jyllands-posten.dk/",
                "https://tv2.dk",
                "https://politiken.dk",
                "https://bt.dk/",
                "https://berlingske.dk",
                "https://ekstrabladet.dk/",
                "http://lokalavisen.dk/",
                "https://foedevarestyrelsen.dk/"
            };

            List<string> engelskeSeeds = new List<string>()
            {
                "https://www.bbc.com/",
                "https://www.nytimes.com",
                "https://www.gartner.com/",
                "https://www.forbes.com",
                "https://www.cbsnews.com",
                "https://www.theinformation.com/"
            };
            //Tænker at det skal køre i et while true loop,
            //for hvis det blot er de forskellige funktioner
            //som kalder hinanden igen og igen, så får vi nok
            //stack overflow på et tidspunkt. Så vi skal istedet
            //lade dem returnere og give svaret tilbage for så
            //at kalde den næste funktion med svaret.
            fetcher = new PageFetcher();
            parser = new PageParser();
            this.pageDB = pageDB;
            DPC = new DuplicatePageChecker(this.pageDB);
            urlFilter = new UrlFilter();
            DUC = new DuplicateURLChecker();
            urlFrontier = new UrlFrontier();

            bool recover = false;
            if (recover)
            {
                //så længe page duplication er dissabled, så burde dette være nok
                //ellers bliver jeg også nødt til at gemme og loade 
                //cachen af shingelse osv., da den nu antager at 
                //den allerede har det på filerne i db. 
                pageDB.LoadPagesFromFiles(0);
                DUC.LoadAllLinksAddedToFrontier();
                urlFrontier.LoadState();

            }
            else
            {
                foreach (string seed in danskeSeeds)
                {
                    urlFrontier.AddUrl(seed);
                    urlFrontier.queue.Add(Utility.GetPartialDomainOfUrl(seed));//så der er flere at vælge fra i starten. Ellers ender den ofte med at tage det samme domæne igen og igen.
                    DUC.AddToTotalListOfUrls(seed);
                }

            }
        }

        public void Run()
        {
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
            string currentUrl = urlFrontier.GetNewUrl1("");
            while (true)
            {
                Console.Clear();
                if (pageDB.GetNumOfCrawledPages() % 50 == 0 && pageDB.GetNumOfCrawledPages() / 50 >= 1)
                {
                    int offset = pageDB.GetNumOfCrawledPages() / 50;
                    Console.WriteLine("Writing to files");
                    pageDB.WritePagesToFilesRaw(offset - 1); //-1 because index is 0-indexed, but count is not.
                    urlFrontier.SaveState();
                    DUC.SaveAllLinksAddedToFrontier();
                    Console.WriteLine("Done writing");
                }

                T("Get url");                
                currentUrl = urlFrontier.GetNewUrl1(currentUrl);
                T("done getting");
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
                int linksAddedFromThisPage = 0;
                if (!isDuplicatePage)//TODO: if duplication checking is done, this should be removed. So that even though it is a duplicate we can still take the links from it, we just dont save the duplicate page.
                {
                    T("find links");
                    List<string> urls = urlFilter.FindLinks(parsedWebpage, currentUrl);

                    T("AmIAllowed and duplicate link check");
                    foreach (string url in urls)
                    {
                        T("links added: " + linksAddedFromThisPage);
                        //before I had this in findlinks, so it would only return X links
                        //but those X links could be invalid, thus 0 getting added. 
                        //this way, we let all links be tested. So if there are X valid
                        //links (in terms of not being duplicative and allowed to access)
                        //on the webpage, X links would be added.
                        if (linksAddedFromThisPage >= 10)
                        {
                            break;
                        }

                        bool isDuplicateUrl = DUC.IsDuplicateUrl(url); //denne kan sikkert godt blive en bottleneck, når den implementeres.
                        if (!isDuplicateUrl)
                        {
                            bool allowed = urlFilter.AmIAllowedPre("*", url);       
                            if (allowed)
                            {
                                DUC.AddToTotalListOfUrls(url);
                                urlFrontier.AddUrl(url);
                                linksAddedFromThisPage++;
                            }
                            else
                            {
                                //ignore that url, since we aren't allowed to crawl that page
                            }
                        }
                        else
                        {
                            //ignore url, since we already visited it
                            //or are planing on doing it
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
            //Console.WriteLine(DateTime.Now.TimeOfDay.ToString().Substring(0, 8) + ": " + value);
        }
    }
}
