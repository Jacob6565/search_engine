using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace SearchEngine.WebCrawler
{
    //This is the Content Seen? element.

    /// <summary>
    /// Responsible for determining if two pages are duplicates
    /// </summary>
    public class DuplicatePageChecker
    {
        private PageDB pageDB;

        //temp storage hash of webpage to list of shingles
        private Dictionary<string, List<string>> tempWebPageHashToShingles = new Dictionary<string, List<string>>();

        //temp storage Hash of webpage to list of hashed shingles
        private Dictionary<string, List<int>> tempWebPageHashToHashShingles = new Dictionary<string, List<int>>();

        //hash of webpage to list of shingles
        private Dictionary<string, List<string>> webPageHashToShingles = new Dictionary<string, List<string>>();

        //Hash of webpage to list of hashed shingles
        private Dictionary<string, List<int>> webPageHashToHashShingles = new Dictionary<string, List<int>>();


        public DuplicatePageChecker(PageDB pageDB)
        {
            this.pageDB = pageDB;
        }        
               
        public bool IsDuplicate(string url, string webpage, int shingleSize = 3)
        {            
            pageDB.AddPageToPages(url, webpage);            
            return false;
            
            
            CalculateHashValuesOfPage(webpage);
            foreach (string webpageFromDB in pageDB.UrlToWebpage.Values) //TODO: lave cache for hash-værdier, så de kun skal udregnes 1 gang og ikke hver gang.
            {
                bool isDuplicate = AreNearDuplicates(webpage, webpageFromDB);
                if (isDuplicate)
                {
                    //if duplicate, we shouldnt save its hashvalues.
                    tempWebPageHashToHashShingles.Clear();
                    tempWebPageHashToShingles.Clear();
                    return true;
                }
            }


            pageDB.UrlToWebpage.Add(url, webpage);


            webPageHashToHashShingles.Add(webpage, tempWebPageHashToHashShingles[webpage]);
            webPageHashToShingles.Add(webpage, tempWebPageHashToShingles[webpage]);
            tempWebPageHashToHashShingles.Clear();
            tempWebPageHashToShingles.Clear();
            return false;

        }

        public void CalculateHashValuesOfPage(string webpage, int shingleSize = 3)
        {
            
            List<string> words = webpage.Split(' ').ToList();
            List<string> shingles = new List<string>();
            int shinglesHash1 = words.Select(x => Hash1(x)).Min();
            int shinglesHash2 = words.Select(x => Hash2(x)).Min();
            int shinglesHash3 = words.Select(x => Hash3(x)).Min();

            List<int> shingleHash = new List<int> { shinglesHash1, shinglesHash2, shinglesHash3 };

            for (int i = 0; i < words.Count - shingleSize + 1; i++)
            {
                string result = "";
                for (int j = 0; j < shingleSize; j++)
                {
                    result += words[i + j];
                }
                shingles.Add(result);
            }

            tempWebPageHashToShingles.Add(webpage, shingles); //ERROR: somehow we end up with two pages that have the same hashvalue and thereby two identical keys ==> error
            tempWebPageHashToHashShingles.Add(webpage, shingleHash);


        }


        public bool AreNearDuplicates(string one, string two, int shingleSize = 3)
        {
            List<string> first1 = tempWebPageHashToShingles[one];
            List<string> second1 = webPageHashToShingles[two];
            List<int> first2 = tempWebPageHashToHashShingles[one];
            List<int> second2 = webPageHashToHashShingles[two];

            return (Jaccard(first1, second1) + JaccardHash(first2, second2)) / 2 > 0.9;
        }
        

        // checks if the shingles overlaps
        public double Jaccard(List<string> first, List<string> second)
        {
            double overlap = 0;
            double union = 0;

            first = first.Distinct().ToList();
            second = second.Distinct().ToList();

            foreach (string stFirst in first)
            {
                foreach (string stSecond in second)
                {
                    if (stFirst == stSecond)
                        overlap++;
                }
            }

            List<string> unionList = first;
            unionList.AddRange(second);
            unionList = unionList.Distinct().ToList();
            union = unionList.Count;


            return overlap / union;
        }

        // checks if the hashed shingles overlaps
        public static double JaccardHash(List<int> first, List<int> second)
        {
            double overlap = 0;
            for (int i = 0; i < first.Count; i++)
            {
                if (first[i] == second[i])
                    overlap++;
            }
            //Console.WriteLine(overlap / first.Count);
            return overlap / first.Count;
        }

        public int Hash1(string str)
        {
            int result = 0;

            for (int i = 0; i < str.Length; i++)
            {
                result += str[i].GetHashCode();
            }

            return result % 256;
            //return (result / str.Length) % 256;
        }

        public int Hash2(string str)
        {
            int result = 0;

            result = str.GetHashCode();


            return result % 256;
        }

        public int Hash3(string str)
        {
            int result = 0;

            for (int i = 0; i < str.Length; i++)
            {
                result += str[i].GetHashCode();
            }

            return result % 256;
        }

    }
}