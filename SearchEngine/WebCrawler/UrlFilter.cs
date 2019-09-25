using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using SearchEngine.UtilityClasses;
namespace SearchEngine.WebCrawler
{
    //This is the Url Filter og robotsfilter elementet.

    /// <summary>
    /// Responsible for finding the robot.txt file of a given url. Then read it and evaluate
    /// if the provided crawler can crawl the specified url. Also to find all link of the given site.
    /// </summary>
    public class UrlFilter
    {
        // domain --> crawlerName --> allows
        private Dictionary<string, Dictionary<string, List<string>>> CacheAllowed = new Dictionary<string, Dictionary<string, List<string>>>();

        // domain --> crawlerName --> disallowed
        private Dictionary<string, Dictionary<string, List<string>>> CacheDisallowed = new Dictionary<string, Dictionary<string, List<string>>>();

        // domain --> crawlerName --> delay
        private Dictionary<string, Dictionary<string, double>> CacheCrawlDelay = new Dictionary<string, Dictionary<string, double>>();

        // The domain the current robots.txt file
        private string Domain { get; set; } = "";

        // The URL we are currently looking at.
        private string Url { get; set; } = "";

        // the robot.txt file as a string
        private string robotFile = "";

        public bool AmIAllowedPre(string crawlerName, string url)
        {
            Url = url;
            Domain = Utility.GetDomainOfUrl(Url);

            if (CacheDisallowed.ContainsKey(Domain))//dette virker ikke helt rigtigt, da jeg ender på dr.dk. Så jeg blacklister den bare.
            {
                return AmIAllowed(crawlerName);
            }
            else
            {
               
                bool canLoad = LoadRobotFile();
                if (!canLoad) return false;
                bool foundRules = ParseRobotFile();
                if (foundRules)
                {
                    return AmIAllowed(crawlerName);
                }
                else
                {
                    return true; //no rules, we are allowed
                }
            }
        }

        public bool AmIAllowed(string crawlerName)
        {            
            // I og med at vores crawler går under *, så behøver vi ikke at
            // tjekke for vores crawler har en allowed regel et andet sted.                     
           
            if (CacheDisallowed.ContainsKey(Domain))//dette virker ikke helt rigtigt, da jeg ender på dr.dk. Så jeg blacklister den bare.
            {
                Regex pattern;
                List<string> rules = new List<string>();
                try
                {
                    rules = CacheDisallowed[Domain][crawlerName];
                }
                catch(KeyNotFoundException)
                {
                    //if we have the domain it means we have looked through its
                    //robots.txt file. So if we don't have an entry for crawlerName
                    //it means there are no rules for that crawlerName. So we are allowed.
                    return true;
                }
                foreach (string rule in rules)
                {
                    try
                    {
                        pattern = new Regex(rule);

                    }
                    catch(Exception)
                    {
                        return false; //cant parse rule, we cant go there.
                    }
                    if (pattern.Match(Url).Success)
                    {
                        //fx if url is www.facebook.com/users
                        //and rule is /users, then we should not gain access
                        return false;
                    }
                }
                return true;
            }
            else
            {
                bool shouldnothappen = false;
                return shouldnothappen;
            }
            
           
        }

        // reads to content of the current domain's robot.txt file and saves
        // information in the dictionaries etc. at the top of the file
        private bool ParseRobotFile()
        {
            List<string> lines = robotFile.Split('\n').ToList();

            // The name of the crawler we are currently fetching rules for
            // changes each time we find "User-agent:"
            string crawlerName = "";

            //If we in our scan of robots.txt found any user agents.
            bool FoundAnyUserAgents = false;

            // allows and disallows of the current crawler
            List<string> allows = new List<string>();
            List<string> disallows = new List<string>();

            double crawlDelay = -1;

            foreach (string line in lines)
            {
                line.Replace("*", ".*"); //converting to regex
                if (line.StartsWith("user-agent:"))
                {
                    FoundAnyUserAgents = true;
                    if (crawlerName != "")
                    {                        
                        if (CacheDisallowed.ContainsKey(Domain))
                        {
                            if (CacheDisallowed[Domain].ContainsKey(crawlerName)) //Håndtere hvis regler for en crawler ikke står samlet. i.e. vi støder på "user-agent: *" to gange.
                            {
                                //Makes the new entry equal to the old one concat with new.
                                CacheDisallowed[Domain][crawlerName] = 
                                    CacheDisallowed[Domain][crawlerName].Concat(disallows).ToList();
                            }
                            else
                            {
                                CacheDisallowed[Domain].Add(crawlerName, disallows);
                            }
                        }
                        else
                        {
                            //First entry for a domain.
                            //Essentially adding the set of rules for
                            //the first crawler on a domain
                            var temp = new Dictionary<string, List<string>>();
                            temp.Add(crawlerName, disallows);
                            CacheDisallowed.Add(Domain, temp);
                        }

                        if (CacheAllowed.ContainsKey(Domain))
                        {
                            if (CacheAllowed[Domain].ContainsKey(crawlerName)) //Håndtere hvis regler for en crawler ikke står samlet. i.e. vi støder på "user-agent: *" to gange.
                            {
                                //Makes the new entry equal to the old one concat with new.
                                CacheAllowed[Domain][crawlerName] = CacheAllowed[Domain][crawlerName].Concat(allows).ToList();
                            }
                            else
                            {
                                CacheAllowed[Domain].Add(crawlerName, allows);
                            }
                        }
                        else
                        {
                            //First entry for a domain.
                            //Essentially adding the set of rules for
                            //the first crawler on a domain
                            var temp = new Dictionary<string, List<string>>();
                            temp.Add(crawlerName, allows);
                            CacheAllowed.Add(Domain, temp);
                        }

                        if (!CacheCrawlDelay.ContainsKey(Domain) && crawlDelay != -1)
                        { 
                            //First entry for a domain.
                            //Essentially adding the set of rules for
                            //the first crawler on a domain
                            var temp = new Dictionary<string, double>();
                            temp.Add(crawlerName, crawlDelay);
                            CacheCrawlDelay.Add(Domain, temp);
                        }

                        // Prepares for next crawler by clearing variables
                        allows.Clear();
                        disallows.Clear();                        
                        crawlerName = getRule(line);
                    }
                    // Cold start case, when we encounter the first "user-agent"
                    if (crawlerName == "")
                    {
                        crawlerName = getRule(line);
                    }
                }
                else if (line.StartsWith("disallow:"))
                {
                    disallows.Add(getRule(line));
                }
                else if (line.StartsWith("allow:"))
                {
                    allows.Add(getRule(line));
                }
                else if (line.StartsWith("crawl-delay:"))
                {
                    crawlDelay = Convert.ToDouble(getRule(line));
                }
            }

            //if we did not find any agents, we need to return here
            //otherwise exceptions are generated belwo.
            if (!FoundAnyUserAgents)  return FoundAnyUserAgents; 

            //When we have currenly looked at the last user-agent. The above doesn't
            //handle it, since it does not tricker another if           
            if (CacheDisallowed.ContainsKey(Domain))
            {
                if (CacheDisallowed[Domain].ContainsKey(crawlerName)) //Håndtere hvis regler for en crawler ikke står samlet. i.e. vi støder på "user-agent: *" to gange.
                {
                    //Makes the new entry equal to the old one concat with new.
                    CacheDisallowed[Domain][crawlerName] =
                        CacheDisallowed[Domain][crawlerName].Concat(disallows).ToList();
                }
                else
                {
                    CacheDisallowed[Domain].Add(crawlerName, disallows);
                }
            }
            else
            {
                //First entry for a domain.
                //Essentially adding the set of rules for
                //the first crawler on a domain
                var temp = new Dictionary<string, List<string>>();
                temp.Add(crawlerName, disallows);
                CacheDisallowed.Add(Domain, temp);
            }

            if (CacheAllowed.ContainsKey(Domain))
            {
                if (CacheAllowed[Domain].ContainsKey(crawlerName)) //Håndtere hvis regler for en crawler ikke står samlet. i.e. vi støder på "user-agent: *" to gange.
                {
                    //Makes the new entry equal to the old one concat with new.
                    CacheAllowed[Domain][crawlerName] = CacheAllowed[Domain][crawlerName].Concat(allows).ToList();
                }
                else
                {
                    CacheAllowed[Domain].Add(crawlerName, allows);
                }
            }
            else
            {
                //First entry for a domain.
                //Essentially adding the set of rules for
                //the first crawler on a domain
                var temp = new Dictionary<string, List<string>>();
                temp.Add(crawlerName, allows);
                CacheAllowed.Add(Domain, temp);
            }

            if (!CacheCrawlDelay.ContainsKey(Domain) && crawlDelay != -1)
            {
                //First entry for a domain.
                //Essentially adding the set of rules for
                //the first crawler on a domain
                var temp = new Dictionary<string, double>();
                temp.Add(crawlerName, crawlDelay);
                CacheCrawlDelay.Add(Domain, temp);
            }
            return FoundAnyUserAgents;
        }

        private string getRule(string line)
        {
            int i = line.IndexOf(':') + 1;
            int j = line.Length;
            string rule = "";

            for (; i < line.Length; i++)
            {
                rule += line[i];
            }
            string trimmedRule = rule.Trim();
            return trimmedRule;
        }
        private bool LoadRobotFile()
        {
            WebClient client = new WebClient();

            try
            {
                robotFile = client.DownloadString($"{Domain}" + "/robots.txt").Trim().ToLower();
            }
            catch (Exception)
            {
                return false; // could load robot.txt file, lets just skip the page.
            }

            client.Dispose();
            return true;
        }


        //---------For handling links

        public List<string> FindLinks(string webpage)
        {
            List<string> links = new List<string>();
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(webpage);
            var linkTags = doc.DocumentNode.Descendants("link");
            List<string> linkedPages = doc.DocumentNode.Descendants("a")
                                              .Select(a => a.GetAttributeValue("href", null))
                                              .Where(u => !String.IsNullOrEmpty(u) &&
                                                           checkIfLinkIsValid(u)).ToList();
            return linkedPages;
        }        

        public bool checkIfLinkIsValid(string link)
        {
            if (!Utility.IsUrlValid(link)) return false;
            if ((link.Contains("www.") || link.Contains("http")) && 
                notSpecificWebSites(link) && 
                notSameDomainAsCurrent(link) &&
                notOnBlackList(link) &&
                isdkDomain(link))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

       

        private bool notSameDomainAsCurrent(string link)
        {
            return true;
            //hvis denne funktion enables, så skal man sætte Domain.
            //det bliver den ikke lige pt.
            return Utility.GetDomainOfUrl(link) != Domain;
        }
        private bool notSpecificWebSites(string link)
        {
            if (link.Contains("facebook") || link.Contains("globalhagen")|| link.Contains("google") || link.Contains("linkedin") || link.Contains("youtube") || link.Contains("dr.dk"))
            {
                return false;
            }
            return true;
        }

        private bool isdkDomain(string url)
        {
            if (Utility.GetDomainOfUrl(url).Contains(".dk"))
            {
                return true;
            }
            return false;
        }

        private string normalizeUrl(string url)
        {
            
            return url.ToLower();
        }

        private bool notOnBlackList(string url)
        {
            List<string> blackList = new List<string>();
            blackList.Add("https://www.bbc.com/news/world-us-canada");
            foreach(string link in blackList)
            {
                if (url.Contains(link))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
