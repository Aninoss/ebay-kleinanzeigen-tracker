using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;

namespace eBayKleinanzeigenTracker
{
    class Tracker
    {
        private MinimizeToTrayInstance _minimizeToTrayInstance;
        private string _urlLatest = null;
        private Thread thread = null;

        public Tracker(MinimizeToTrayInstance minimizeToTrayInstance)
        {
            this._minimizeToTrayInstance = minimizeToTrayInstance;
            minimizeToTrayInstance.AddClickHandler(HandleBalloonClicked);
        }

        public void Start(int intervalSec)
        {
            if (thread != null) thread.Interrupt();
            thread = new Thread(() => Run(intervalSec));
            thread.IsBackground = true;
            thread.Name = "tracker";
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start();
        }

        private void Run(int intervalSec)
        {
            while (true)
            {
                Console.WriteLine($"Waiting {intervalSec} Seconds...");
                try
                {
                    Thread.Sleep(intervalSec * 1000);
                } catch(ThreadInterruptedException e)
                {
                    return;
                }
                foreach (SearchRequest searchRequest in SearchRequestContainer.GetInstance().SearchRequests)
                {
                    if (!searchRequest.IsEmpty())
                    {
                        Thread t = new Thread(() => ManageSearchRequest(searchRequest));
                        t.IsBackground = true;
                        t.Name = "tracker_" + searchRequest.SearchKey;
                        t.Priority = ThreadPriority.BelowNormal;
                        t.Start();
                    }
                }
            }
        }

        private void ManageSearchRequest(SearchRequest searchRequest)
        {
            try
            {
                string url = GetSearchUrl(searchRequest);
                string webData = SearchRequestGetWebData(url);
                string[] htmlParts = GetOfferStrings(webData);
                List<Offer> offers = ExtractOffers(htmlParts);

                if (searchRequest.NewestIds.Count > 0)
                {
                    List<Offer> newOffers = new List<Offer>();
                    foreach (Offer offer in offers)
                    {
                        if (searchRequest.NewestIds.Contains(offer.Id)) break;
                        newOffers.Add(offer);
                    }
                    Console.WriteLine($"Es gibt {newOffers.Count} neue Ergebnisse zum Suchbegriff \"{searchRequest.SearchKey}\"");
                    ManageNewOffers(newOffers, searchRequest);
                }

                if (offers.Count > 0)
                {
                    List<long> newestIds = new List<long>();
                    foreach (Offer offer in offers)
                    {
                        newestIds.Add(offer.Id);
                    }
                    searchRequest.NewestIds = newestIds;
                }

                searchRequest.Results = offers.Count >= 25 ? ">25" : offers.Count.ToString();
                searchRequest.UpdateDateTime();
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ManageNewOffers(List<Offer> newOffers, SearchRequest searchRequest)
        {
            int n = newOffers.Count;
            if (n == 1)
            {
                Offer offer = newOffers[0];
                _urlLatest = offer.Url;
                _minimizeToTrayInstance.ShowBalloonTip(offer.Title, $"Preis: {offer.Price}\n{offer.Description}");
            } else if (n > 1 && n < 25)
            {
                string searchKey = searchRequest.SearchKey;
                _urlLatest = GetSearchUrl(searchRequest);
                _minimizeToTrayInstance.ShowBalloonTip($"{n} neue Ergebnisse", $"Es gibt {n} neue Ergebnisse zum Suchbegriff \"{searchKey}\"");
            }
        }

        private void HandleBalloonClicked(object sender, EventArgs e)
        {
            if (_urlLatest != null) System.Diagnostics.Process.Start(_urlLatest);
        }

        private List<Offer> ExtractOffers(string[] codeParts)
        {
            List<Offer> offers = new List<Offer>();

            foreach(string code in codeParts)
            {
                string[] urlTitle = StringTools.ExtractGroups(code, " href=\"", "</a>")[0].Replace("\">", ">").Split('>');

                long id = Int64.Parse(StringTools.ExtractGroups(code, "data-adid=\"", "\"")[0]);
                string url = "https://www.ebay-kleinanzeigen.de" + urlTitle[0];

                string[] titleGroups = StringTools.ExtractGroups(urlTitle[1], "data-imgtitle=\"", "\"");
                string title = "";
                if (titleGroups.Length > 0)
                {
                    title = titleGroups[0];
                }

                string desc = StringTools.ExtractGroups(code, "<p class=\"aditem-main--middle--description\">", "</p>")[0];
                string price = StringTools.ExtractGroups(code, "<p class=\"aditem-main--middle--price\">", "</p>")[0].Replace("\n", "").Trim();
                string thumbnail = null;
                if (code.Contains("data-imgsrc=\"")) thumbnail = StringTools.ExtractGroups(code, "data-imgsrc=\"", "\"")[0];

                Offer offer = new Offer(id, title, desc, url, thumbnail, price);
                offers.Add(offer);
            }

            return offers;
        }
        private string[] GetOfferStrings(string htmlCode)
        {
            string start = "<li class=\"ad-listitem lazyload-item   \">", end = "</li>";
            return StringTools.ExtractGroups(htmlCode, start, end);
        }
        private string SearchRequestGetWebData(string url)
        {
            System.Net.WebClient webClient = new System.Net.WebClient();
            webClient.Headers["Content-type"] = "text/xml;charset=utf-8";
            webClient.Headers["User-Agent"] = "Mozilla/5.0 (eBay Kleinanzeigen Tracker) Gecko/20100101 Firefox/74.0";
            webClient.Encoding = System.Text.Encoding.UTF8;
            Console.WriteLine(url);
            return HttpUtility.HtmlDecode(webClient.DownloadString(url));
        }

        private string GetSearchUrl(SearchRequest searchRequest)
        {
            string searchKey = HttpUtility.UrlEncode(searchRequest.SearchKey.ToLower().Replace(" ", "-"));
            return $"https://www.ebay-kleinanzeigen.de/s-anzeige:angebote/preis:{searchRequest.PriceMin}:{searchRequest.PriceMax}/{searchKey}/k0";
        }

    }
}
