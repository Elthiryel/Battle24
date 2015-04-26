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
            public readonly string FlagUrl; // not included in equality methods

            private BelligerentInfo(string name, string url, bool nameOnly, bool firstSide, string flagUrl = null)
            {
                Name = name;
                Url = url;
                NameOnly = nameOnly;
                FirstSide = firstSide;
                FlagUrl = flagUrl;
            }

            public static BelligerentInfo WithUrl(string url, bool firstSide, string flagUrl = null)
            {
                return new BelligerentInfo(String.Empty, url, false, firstSide, flagUrl);
            }

            public static BelligerentInfo WithoutUrl(string name, bool firstSide, string flagUrl = null)
            {
                return new BelligerentInfo(name, String.Empty, true, firstSide, flagUrl);
            }
        }

        private class BattleBelligerentInfo
        {
            public string FullBattleUrl;
            public string Strength;
            public string CasualtiesAndLosses;
        }

        private const string HomePage = "http://en.wikipedia.org/wiki/List_of_battles_1301%E2%80%931800";
        private const string WikiPrefix = "http://en.wikipedia.org";

        private static int _battleCounter;

        private IList<string> _battleList;
        private IDictionary<string, IList<string>> _battlesToWars;
        private IDictionary<BelligerentInfo, IList<BattleBelligerentInfo>> _battlesBelligerents; 

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
            _battlesBelligerents = new Dictionary<BelligerentInfo, IList<BattleBelligerentInfo>>();

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
                    }
                }
                var strength1 = String.Empty;
                var strength2 = String.Empty;
                var casualtiesAndLosses1 = String.Empty;
                var casualtiesAndLosses2 = String.Empty;
                foreach (var trNode in trNodesList)
                {
                    var header = CrawlerHelper.GetStringValueByTag(trNode, "th");
                    if (header != null)
                    {
                        if (header.Contains("Strength"))
                        {
                            var index = trNodesList.IndexOf(trNode);
                            var strengthTrNode = trNodesList[index + 1];
                            strength1 = CrawlerHelper.GetStringValueByTag(strengthTrNode, "td"); // [BATTLES_BELLIGERENTS].Strength
                            strength2 = CrawlerHelper.GetStringValueByTag(strengthTrNode, "td", 1); // [BATTLES_BELLIGERENTS].Strength
                        } else if (header.Contains("Casualties"))
                        {
                            var index = trNodesList.IndexOf(trNode);
                            var casualtiesAndLossesNode = trNodesList[index + 1];
                            casualtiesAndLosses1 = CrawlerHelper.GetStringValueByTag(casualtiesAndLossesNode, "td"); // [BATTLES_BELLIGERENTS].CasualtiesAndLosses
                            casualtiesAndLosses2 = CrawlerHelper.GetStringValueByTag(casualtiesAndLossesNode, "td", 1); // [BATTLES_BELLIGERENTS].CasualtiesAndLosses
                        }
                    }
                }
                //var belligerentsByFlags = new Dictionary<string, >
                foreach (var trNode in trNodesList)
                {
                    var header = CrawlerHelper.GetStringValueByTag(trNode, "th");
                    if (header != null)
                    {
                        if (header.Contains("Belligerents"))
                        {
                            var index = trNodesList.IndexOf(trNode);
                            var belligerentsTrNode = trNodesList[index + 1];
                            var firstSideTdNode = CrawlerHelper.GetNodeByTag(belligerentsTrNode, "td");
                            CrawlBelligerentsSidePane(firstSideTdNode, true, strength1, casualtiesAndLosses1, url);
                            var secondSideTdNode = CrawlerHelper.GetNodeByTag(belligerentsTrNode, "td", 1);
                            CrawlBelligerentsSidePane(secondSideTdNode, false, strength2, casualtiesAndLosses2, url);
                        }
                    }
                }
                // TODO

            }
        }

        private void AddBattleToWar(string fullBattleUrl, string fullWarUrl)
        {
            if (_battlesToWars.ContainsKey(fullWarUrl))
                _battlesToWars[fullWarUrl].Add(fullBattleUrl);
            else
                _battlesToWars[fullWarUrl] = new List<string> {fullBattleUrl};
        }

        private void CrawlBelligerentsSidePane(HtmlNode sideTdNode, bool firstSide, string strength, string casualtiesAndLosses, string battleUrl)
        {
            var flagNode = CrawlerHelper.GetNodeByTagAndClass(sideTdNode, "a", "image");
            var flagUrl = String.Empty;
            if (flagNode != null)
            {
                flagUrl = flagNode.GetAttributeValue("src", String.Empty); // [BELLIGERENTS].FlagURL
            }
            var belligerentNodes = CrawlerHelper.GetAllNodesWithoutClassByTag(sideTdNode, "a");
            var belligerentNodesList = belligerentNodes as IList<HtmlNode> ?? belligerentNodes.ToList();
            if (belligerentNodesList.Any())
            {
                foreach (var belligerentNode in belligerentNodesList)
                {
                    var url = String.Format("{0}{1}", WikiPrefix, belligerentNode.GetAttributeValue("href", String.Empty)); // [BELLIGERENTS].URL
                    var belliferentInfo = BelligerentInfo.WithUrl(url, firstSide, flagUrl);
                    var battleBelligerentInfo = new BattleBelligerentInfo
                    {
                        FullBattleUrl = battleUrl,
                        Strength = strength, // [BATTLES_BELLIGERENTS].Strength
                        CasualtiesAndLosses = casualtiesAndLosses // [BATTLES_BELLIGERENTS].CasualtiesAndLosses
                    };
                    AddBattleBelligerentInfo(belliferentInfo, battleBelligerentInfo);
                }
            }
            else
            {
                var name = sideTdNode.InnerText;
                var belligerentInfo = BelligerentInfo.WithoutUrl(name, firstSide, flagUrl);
                var battleBelligerentInfo = new BattleBelligerentInfo
                {
                    FullBattleUrl = battleUrl,
                    Strength = strength,
                    CasualtiesAndLosses = casualtiesAndLosses
                };
                AddBattleBelligerentInfo(belligerentInfo, battleBelligerentInfo);
            }
        }

        private void AddBattleBelligerentInfo(BelligerentInfo bInfo, BattleBelligerentInfo bbInfo)
        {
            if (_battlesBelligerents.ContainsKey(bInfo))
                _battlesBelligerents[bInfo].Add(bbInfo);
            else
                _battlesBelligerents[bInfo] = new List<BattleBelligerentInfo> {bbInfo};
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
