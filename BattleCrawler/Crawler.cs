using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using HtmlAgilityPack;

namespace BattleCrawler
{
    public class Crawler
    {
        private const string HomePage = "http://en.wikipedia.org/wiki/List_of_battles_1301%E2%80%931800";
        private const string WikiPrefix = "http://en.wikipedia.org";

        private IList<string> _battleList; 

        public Crawler()
        {

        }

        public void Crawl()
        {
            Logger.Log("Starting crawler.");

            var doc = GetHtmlDocument(HomePage);
            if (doc == null)
                return;

            _battleList = new List<string>();

            // 14th century battles
            var tableNodes = CrawlerHelper.GetAllNodesByTagAndClass(doc.DocumentNode, "table", "wikitable");
            foreach (var tableNode in tableNodes)
            {
                var trNodes = CrawlerHelper.GetAllNodesByTag(tableNode, "tr");
                foreach (var trNode in trNodes)
                {
                    var tdNode = CrawlerHelper.GetNodeByTag(trNode, "td", 1);
                    if (tdNode != null)
                    {
                        var aNode = CrawlerHelper.GetNodeByTag(tdNode, "a");
                        if (aNode != null)
                        {
                            var url = aNode.GetAttributeValue("href", String.Empty);
                            if (!url.Contains("action=edit") && !url.Contains("#") && !url.StartsWith("http://"))
                                _battleList.Add(url);
                        }
                    }
                }
            }

            // all remaining battles
            var contentDivNode = CrawlerHelper.GetNodeByTagAndClass(doc.DocumentNode, "div", "mw-content-ltr");
            var liNodes = CrawlerHelper.GetAllNodesWithoutClassByTag(contentDivNode, "li");
            foreach (var liNode in liNodes)
            {
                if (liNode.InnerHtml.StartsWith("<a "))
                {
                    var aNode = CrawlerHelper.GetNodeByTag(liNode, "a");
                    var url = aNode.GetAttributeValue("href", String.Empty);
                    if (!url.Contains("action=edit") && !url.Contains("#") && !url.StartsWith("http://"))
                        _battleList.Add(url);
                }
            }

            CrawlBattles();
        }

        private void CrawlBattles()
        {
            foreach (var battleUrl in _battleList)
            {
                var battleDoc = GetHtmlDocument(String.Format("{0}{1}", WikiPrefix, battleUrl));
                if (battleDoc != null)
                    CrawlBattle(battleDoc);
            }
        }

        private void CrawlBattle(HtmlDocument battleDocument)
        {
            
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
