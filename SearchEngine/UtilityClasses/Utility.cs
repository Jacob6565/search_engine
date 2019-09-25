using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.UtilityClasses
{
    public static class Utility
    {
        public static string GetDomainOfUrl(string url)
        {           
            Uri uri = new Uri(@url);
            string host = uri.GetLeftPart(System.UriPartial.Authority);
            
            return host;
        }

        public static bool IsUrlValid(string url)
        {
            try
            {
                Uri uri = new Uri(@url);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
