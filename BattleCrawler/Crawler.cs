using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using HtmlAgilityPack;

namespace BattleCrawler
{
    public class Crawler
    {
        private class BelligerentInfo
        {
            public readonly string Name;
            public readonly string Url;
            public readonly bool NameOnly;
            public readonly bool FirstSide;

            private BelligerentInfo(string name, string url, bool nameOnly, bool firstSide)
            {
                Name = name;
                Url = url;
                NameOnly = nameOnly;
                FirstSide = firstSide;
            }

            public static BelligerentInfo WithUrl(string url, bool firstSide)
            {
                return new BelligerentInfo(String.Empty, url, false, firstSide);
            }

            public static BelligerentInfo WithoutUrl(string name, bool firstSide)
            {
                return new BelligerentInfo(name, String.Empty, true, firstSide);
            }
        }

        private const string HomePage = "http://en.wikipedia.org/wiki/List_of_battles_1301%E2%80%931800";
        private const string WikiPrefix = "http://en.wikipedia.org";

        private static int _battleCounter;

        private IList<string> _battleList;
        private IDictionary<string, IList<string>> _battlesToWars; 

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
            _battlesToWars = new Dictionary<string, IList<string>>();

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
                var fullUrl = String.Format("{0}{1}", WikiPrefix, battleUrl);
                var battleDoc = GetHtmlDocument(fullUrl);
                if (battleDoc != null)
                    CrawlBattle(battleDoc, fullUrl);
            }
        }

        private void CrawlBattle(HtmlDocument battleDocument, string url)
        {
            ++_battleCounter;
            Logger.Log(String.Format("Crawling battle #{0}: {1}", _battleCounter, url));
            var name = CrawlerHelper.GetStringValueByTagAndClass(battleDocument.DocumentNode, "th", "summary") ?? 
                CrawlerHelper.GetStringValueByTagAndClass(battleDocument.DocumentNode, "h1", "firstHeading"); // [BATTLE].Name
            var infoTableNode = CrawlerHelper.GetNodeByTagAndClass(battleDocument.DocumentNode, "table", "infobox vevent");
            if (infoTableNode != null)
            {
                var warTrNode = CrawlerHelper.GetNodeByTag(infoTableNode, "tr", 1);
                var tdNode = CrawlerHelper.GetNodeByTag(warTrNode, "td");
                if (tdNode.GetAttributeValue("style", String.Empty).Contains("background-color"))
                {
                    var aNode = CrawlerHelper.GetNodeByTag(warTrNode, "a");
                    if (aNode != null)
                    {
                        var warUrl = aNode.GetAttributeValue("href", String.Empty);
                        if (!warUrl.Contains("action=edit") && !warUrl.Contains("#") && !warUrl.StartsWith("http://"))
                            AddBattleToWar(url, String.Format("{0}{1}", WikiPrefix, warUrl));
                    }
                }
                var trNodes = CrawlerHelper.GetAllNodesByTag(infoTableNode, "tr");
                var trNodesList = trNodes as IList<HtmlNode> ?? trNodes.ToList();
                foreach (var trNode in trNodesList)
                {
                    var header = CrawlerHelper.GetStringValueByTag(trNode, "th");
                    if (header != null)
                    {
                        if (header.Contains("Date"))
                        {
                            var date = CrawlerHelper.GetStringValueByTag(trNode, "td"); // [BATTLE].Date
                        }
                        else if (header.Contains("Location"))
                        {
                            var location = CrawlerHelper.GetStringValueByTag(trNode, "td"); // [BATTLE].Location
                        }
                        else if (header.Contains("Result"))
                        {
                            var result = CrawlerHelper.GetStringValueByTag(trNode, "td"); // [BATTLE].Result
                        }
                        else if (header.Contains("Territorial"))
                        {
                            var territorialChanges = CrawlerHelper.GetStringValueByTag(trNode, "td"); // [BATTLE].TerritorialChanges
                        }
                        else if (header.Contains("Belligerents"))
                        {
                            var index = trNodesList.IndexOf(trNode);
                            var belligerentsTrNode = trNodesList[index + 1];
                            var firstSideTdNode = CrawlerHelper.GetNodeByTag(belligerentsTrNode, "td");
                            CrawlBelligerentsSidePane(firstSideTdNode, true);
                            var secondSideTdNode = CrawlerHelper.GetNodeByTag(belligerentsTrNode, "td", 1);
                            CrawlBelligerentsSidePane(secondSideTdNode, false);
                        }
                        // TODO
                    }
                }

            }
        }

        private void AddBattleToWar(string fullBattleUrl, string fullWarUrl)
        {
            if (_battlesToWars.ContainsKey(fullWarUrl))
                _battlesToWars[fullWarUrl].Add(fullBattleUrl);
            else
                _battlesToWars[fullWarUrl] = new List<string> {fullBattleUrl};
        }

        private void CrawlBelligerentsSidePane(HtmlNode sideTdNode, bool firstSide)
        {
            // TODO
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
