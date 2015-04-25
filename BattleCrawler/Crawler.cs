using System.IO;
using System.Net;
using System.Threading;
using HtmlAgilityPack;

namespace BattleCrawler
{
    public class Crawler
    {
        private const string HomePage = "http://en.wikipedia.org/wiki/List_of_battles_1301%E2%80%931800";

        public Crawler()
        {

        }

        public void Crawl()
        {
            Logger.Log("Starting crawler.");

            var doc = GetHtmlDocument(HomePage);
            if (doc == null)
                return;


        }

        private static HtmlDocument GetHtmlDocument(string url)
        {
            try
            {
                Stream data = null;

                var repeat = true;
                while (repeat)
                {
                    repeat = false;
                    try
                    {
                        var request = WebRequest.Create(url);
                        var response = request.GetResponse();
                        data = response.GetResponseStream();
                    }
                    catch (WebException e)
                    {
                        if (e.Message.Contains("503"))
                        {
                            Logger.Log(e, "Connection error, sleeping...");
                            repeat = true;
                            Thread.Sleep(60000);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                var doc = new HtmlDocument();

                string html;
                using (var sr = new StreamReader(data))
                {
                    html = sr.ReadToEnd();
                }
                doc.LoadHtml(html);
                return doc;
            }
            catch (WebException e)
            {
                Logger.Log(e, "Error retrieving HTML document.");
                return null;
            }
        }
    }
}
