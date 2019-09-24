using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Webcrawler
{
    //This is the Url Filter og robotsfilter elementet.

    /// <summary>
    /// Responsible for finding the robot.txt file of a given url. Then read it and evaluate
    /// if the provided crawler can crawl the specified url. Also to find all link of the given site.
    /// </summary>
    public class UrlFilter
    {
        //Everything is loaded and read from these dictionaries.
        // crawlerName --> list of allows
        private Dictionary<string, List<string>> Allowed = new Dictionary<string, List<string>>();

        // crawlerName --> list of allows
        private Dictionary<string, List<string>> Disallowed = new Dictionary<string, List<string>>();

        // crawlerName --> delay
        private Dictionary<string, int> CrawlDelay = new Dictionary<string, int>();

        // The domain the current robots.txt file and content relates to.
        private string Domain { get; set; } = "";

        // the robot.txt file as a string
        private string robotFile = "";

               

        private string GetRootOfUrl()
        {

            List<string> elements = Domain.Split('.').ToList();
            string root = "";
            for (int i = 0; ; i++)
            {
                if (elements[i].Contains("www"))
                {
                    root = elements[i] + "." +
                            elements[i + 1] + "." +
                             elements[i + 2].Split('/').First();
                    break;
                }
            }
            return root;
        }

        public bool AmIAllowed(string crawlerName, string url)
        {            
            Domain = url;
            bool canLoad = LoadRobotFile(GetRootOfUrl());
            if (!canLoad) return false;
            ParseRobotFile();
            foreach (string rule in Disallowed[crawlerName])
            {
                if (url.Contains(rule))
                {
                    //fx if url is www.facebook.com/users
                    //and rule is /users, then we should not gain access
                    clearData(); //we still loaded its robots.txt file, so we need to clear
                    return false;
                }
            }

            //So the rules for the current robots.txt file gets 
            //removed/cleared before the rules for the next robots.txt file.
            clearData();
            return true;
        }

        private void clearData()
        {
            Allowed.Clear();
            Disallowed.Clear();
            CrawlDelay.Clear();
        }

        // reads to content of the current domain's robot.txt file and saves
        // information in the dictionaries etc. at the top of the file
        private void ParseRobotFile()
        {
            List<string> lines = robotFile.Split('\n').ToList();

            // The name of the crawler we are currently fetching rules for
            // changes each time we find "User-agent:"
            string crawlerName = "";

            // allows and disallows of the current crawler
            List<string> allows = new List<string>();
            List<string> disallows = new List<string>();

            int crawlDelay = -1;

            foreach (string line in lines)
            {
                if (line.StartsWith("User-agent:"))
                {
                    if (crawlerName != "")
                    {
                        //Save the information
                        Allowed.Add(crawlerName, allows);
                        Disallowed.Add(crawlerName, disallows);
                        CrawlDelay.Add(crawlerName, crawlDelay);

                        // Prepares for next crawler by clearing variables
                        allows.Clear();
                        disallows.Clear();
                        crawlerName = line.Split(':').Last().Trim();
                    }
                    // Cold start case, when we encounter the first "user-agent"
                    if (crawlerName == "")
                    {
                        crawlerName = line.Split(':').Last().Trim();
                    }
                }
                else if (line.StartsWith("Disallow:"))
                {
                    disallows.Add(line.Split(':').Last().Trim());
                }
                else if (line.StartsWith("Allow:"))
                {
                    allows.Add(line.Split(':').Last().Trim());
                }
                else if (line.StartsWith("Crawl-delay:"))
                {
                    crawlDelay = Convert.ToInt32(line.Split(':').Last().Trim());
                }
            }

            //When we have currenly looked at the last user-agent. The above doesn't
            //handle it, since it does not tricker another if
            Allowed.Add(crawlerName, allows);
            Disallowed.Add(crawlerName, disallows);
            CrawlDelay.Add(crawlerName, crawlDelay);

        }

        private bool LoadRobotFile(string rootUrl)
        {
            WebClient client = new WebClient();

            try
            {
                robotFile = client.DownloadString($"{rootUrl}" + "/robots.txt").Trim();
            }
            catch (Exception e)
            {
                return false; // could load robot.txt file, lets just skip the page.
            }

            client.Dispose();
            return true;
        }



        //---------For handling links

        public List<string> FindLinks(string webpage)
        {
            List<string> linksFound = new List<string>();
            List<string> lines = webpage.Split('\n').ToList();
            foreach (string line in lines)
            {
                if (line.Contains("href") && line.Contains("<a"))
                {
                    int indexOfHref = line.IndexOf("href");
                    bool startReached = false;
                    string link = "";
                    for (int i = indexOfHref; ; i++)
                    {
                        if (line[i] == '"' && !startReached)
                        {
                            startReached = true;
                        }
                        else if (line[i] == '"' && startReached)
                        {
                            break;
                        }
                        else if (startReached)
                        {
                            link += line[i];
                        }
                    }
                    if (checkIfLinkIsValid(link))
                    {
                        linksFound.Add(normalizeUrl(link));
                    }
                }
            }
            return linksFound;

        }

        public bool checkIfLinkIsValid(string link)
        {
            if (link.Contains("www.") && notSpecificWebSites(link)) //facebook got that Applebot which is declared twice
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool notSpecificWebSites(string link)
        {
            if(link.Contains("facebook") || link.Contains("linkedin") || link.Contains("youtube"))
            {
                return false;
            }
            return true;
        }

        private string normalizeUrl(string url)
        {
            return url.ToLower();
        }

    }
}
