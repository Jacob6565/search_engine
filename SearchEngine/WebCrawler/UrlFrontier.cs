using SearchEngine.UtilityClasses;
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

        //sorted according to timespan. So first is the one with the lowest timespan i.e. the oldest.
        List<string> queue = new List<string>();

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

        //Denne er lidt svær at forstå.
        //virker bedst under assumption om, at der findes et link i frontier
        //til alle domæner som befinder sig i queuen. For hvis dette gælder,
        //så vil det næste link taget fra frontier være et link med det domæne,
        //som blev besøgt længst tid siden. MEN hvis vi siger at domæne X er
        //det domæne som blev besøgt for længsttid siden, men som ikke har nogle
        //links i frontieren, så tager vi blot et random link fra frontieren, hvilket
        //jo kunne være et med domænet vi lige har besøgt. Derudover, så registerer
        //vi også at domæne X lige er blevet besøgt (selvom vi ikke gjorde det) blot 
        //for at give den en chance for at få nogle links ind i frontieren med dens domæne.
        //for hvis vi ikke gør dette, så kan vi ende med at vi hver gang forsøger at finde
        //et link med domæne X (da det er jo er den som blev besøgt for længst tid siden), men
        //uden held og vi ender med at tage random fra frontieren.

        //Så kort sagt. Så finder vi det domæne X som blev besøgt for længst tid siden.
        //Tjekker om der er et link i frontieren med dette domæne. Hvis der er, så tager vi
        //og besøger det link og bogfører at vi nu har besøgt domæne X, ved at ligge 
        //det om bagerst i køen.

        //Hvis der så ikke er et link i frontieren med domænet X, så lader vi som om vi besøgte
        //X ved at putte det om bagerst i køen, blot for at undgå at vælge det igen i næste omgang
        //da det jo ikke havde nogle link i frontieren. Så vi giver det lidt en chance for at få
        //nogle links. Nå, men der var ikke et link med domæne X, så vi tager blot et random 
        //link med domæne Y, og returnerer det link. Derudover så bogfører vi så også, at vi 
        //har besøgt domæne Y, ved at ligge det om bagerst i køen, lig bag X som vi tilføjede før.


        public string GetNewUrl1(string currentUrl)
        {
            string domainWeMayChoose = "";
            string newUrl = "";
            try
            {
                //Tjekker om der er et andet domæne som vi kan crawle
                domainWeMayChoose = queue.First(x => x != Utility.GetPartialDomainOfUrl(currentUrl));

                //Hvis der er, så tjekker vi om der er nogle 
                //links i vores frontier med det domæne.
                //PartialDomain, da vi ikke gider at have http og https med
                //da to med samme host, men forskellige sikkerhed vil fremstå som forskellige.
                newUrl = frontier.First(x => Utility.GetPartialDomainOfUrl(x) == domainWeMayChoose);

            }
            catch (Exception)
            {
                //vi fandt et domæne, men inden link i frontier med det domæne
                //så ligger vi det bagerst i queueen, ellers vil vi blot ende med at 
                //tage det først link i frontier indtil der bliver addet 
                //et link til frontier med det domæne.
                if (newUrl == "" && domainWeMayChoose != "")
                {
                    int i = queue.IndexOf(domainWeMayChoose);
                    queue.RemoveAt(i);
                    queue.Add(domainWeMayChoose);
                }
                //Hvis der ikke var et andet domæne, eller der ikke var 
                //et link i frontier med det nye domæne så siger vi blot
                //at vi tager og besøger et random element i frontier og 
                //bogfører at vi har besøgt det, ved at tilføje det domæne bagerst i køen.
                //den eneste der ikke rigtigt bliver bogført 
                //er den aller første url vi besøger.
                Random rd = new Random();
                newUrl = frontier[rd.Next(0, frontier.Count)]; //from 0 til count-1 see docs
                domainWeMayChoose = Utility.GetPartialDomainOfUrl(newUrl);
            }
            int indexToDelete = frontier.FindIndex(x => x == newUrl);
            frontier.RemoveAt(indexToDelete);

            //Adding and element. Or updating one, by deleting it and recreating it,
            //so it comes back in the line.
            int indexOfDomainInQueueIfAny = QueueContainsDomain(domainWeMayChoose);
            if (indexOfDomainInQueueIfAny != -1) //it exists.
            {
                queue.RemoveAt(indexOfDomainInQueueIfAny);
            }            
            queue.Add(domainWeMayChoose);
            return newUrl;
        }

        private int QueueContainsDomain(string domain)
        {
           for(int i = 0; i < queue.Count; i++)
            {
                if (queue[i] == domain)
                {
                    return i;
                }    
            }
            return -1;
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



