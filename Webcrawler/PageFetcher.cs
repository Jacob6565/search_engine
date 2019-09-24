using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Webcrawler
{
    //This is the Fetch element

    /// <summary>
    /// Responsible for downloading the a webpage.
    /// </summary>
    public class PageFetcher
    {

        
        /// Fetch side, send den til parseren, som finder alle links på den side, og 
        /// laver en standardiseret repræsentation af en side, nok som en string.
        /// send den string til duplicatePagechecker som tjekker om denne side allerede 
        /// er iblandt nogle af de sider vi har fetchet.
        /// Hvis ikke så send url'en til pagen til robothandler, som tjekker om vi må crawle
        /// det page. Hvis vi må så send urlen til duplicateURLchecker som tjekker at vi ikke 
        /// allerede har en url som denne i vores frontier. Hvis vi ikke må, så blot smid urlen
        /// ud. Hvis vi ikke har denne url i frontieren, så send den til frontier, ellers smid den
        /// ud. Så frontieren så sende en ny url til fetcheren, eller fetcheren skal tage en ny
        /// url fra frontieren. Og er vi tilbage ved start.
        ///
        
                        
        public string Fetch(string url)
        {
            string webpage = "";
            WebClient client = new WebClient();
            try
            {
                webpage = client.DownloadString(url).Trim();
            }
            catch (Exception e)
            {
                return "error";
            }

            client.Dispose();

            return webpage;
        }
    }
}
