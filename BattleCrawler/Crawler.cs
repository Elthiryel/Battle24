using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using HtmlAgilityPack;
using NHibernate;

namespace BattleCrawler
{
    public class Crawler
    {
        private class BelligerentInfo
        {
            public readonly string Name;
            public readonly string Url;
            public readonly bool NameOnly;
            public readonly string FlagUrl; // not included in equality methods

            private BelligerentInfo(string name, string url, bool nameOnly, string flagUrl = null)
            {
                Name = name;
                Url = url;
                NameOnly = nameOnly;
                FlagUrl = flagUrl;
            }

            public static BelligerentInfo WithUrl(string url, string flagUrl = null)
            {
                return new BelligerentInfo(String.Empty, url, false, flagUrl);
            }

            public static BelligerentInfo WithoutUrl(string name, string flagUrl = null)
            {
                return new BelligerentInfo(name, String.Empty, true, flagUrl);
            }

            protected bool Equals(BelligerentInfo other)
            {
                return string.Equals(Name, other.Name) && string.Equals(Url, other.Url) && NameOnly.Equals(other.NameOnly);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((BelligerentInfo) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (Name != null ? Name.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ (Url != null ? Url.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ NameOnly.GetHashCode();
                    return hashCode;
                }
            }
        }

        private class BattleBelligerentInfo
        {
            public Battle Battle;
            public string Strength;
            public string CasualtiesAndLosses;
            public bool FirstSide;
        }

        private const string HomePage = "http://en.wikipedia.org/wiki/List_of_battles_1301%E2%80%931800";
        private const string WikiPrefix = "http://en.wikipedia.org";

        private static int _battleCounter;
        private static int _belligerentCounter;

        private readonly ISession _session;

        private IList<string> _battleList;
        private IDictionary<string, IList<string>> _battlesToWars;
        private IDictionary<BelligerentInfo, IList<BattleBelligerentInfo>> _battlesBelligerents; 

        public Crawler(ISession session)
        {
            _session = session;
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
                if (liNode.InnerHtml.StartsWith("<a ") || (liNode.InnerHtml.Length >= 8 && liNode.InnerHtml.Substring(5).StartsWith("<a ")))
                {
                    var aNode = CrawlerHelper.GetNodeByTag(liNode, "a");
                    var url = aNode.GetAttributeValue("href", String.Empty);
                    if (!url.Contains("action=edit") && !url.Contains("#") && !url.StartsWith("http://"))
                        _battleList.Add(url);
                }
            }

            CrawlBattles();
            // CrawlWars();
            CrawlBelligerents();
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
            var battle = new Battle();
            Logger.Log(String.Format("Crawling battle #{0}: {1}", _battleCounter, url));
            var name = CrawlerHelper.GetStringValueByTagAndClass(battleDocument.DocumentNode, "th", "summary") ?? 
                CrawlerHelper.GetStringValueByTagAndClass(battleDocument.DocumentNode, "h1", "firstHeading"); // [BATTLE].Name
            battle.Name = name;
            battle.URL = url;
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
                            battle.Date = date;
                        }
                        else if (header.Contains("Location"))
                        {
                            var location = CrawlerHelper.GetStringValueByTag(trNode, "td"); // [BATTLE].Location
                            battle.Location = location;
                        }
                        else if (header.Contains("Result"))
                        {
                            var result = CrawlerHelper.GetStringValueByTag(trNode, "td"); // [BATTLE].Result
                            battle.Result = result;
                        }
                        else if (header.Contains("Territorial"))
                        {
                            var territorialChanges = CrawlerHelper.GetStringValueByTag(trNode, "td"); // [BATTLE].TerritorialChanges
                            battle.TerritorialChanges = territorialChanges;
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
                // TODO leaders (flags)
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
                            CrawlBelligerentsSidePane(firstSideTdNode, true, strength1, casualtiesAndLosses1, battle);
                            var secondSideTdNode = CrawlerHelper.GetNodeByTag(belligerentsTrNode, "td", 1);
                            CrawlBelligerentsSidePane(secondSideTdNode, false, strength2, casualtiesAndLosses2, battle);
                        }
                    }
                }
                // TODO leaders

            }
            _session.Flush();
            _session.SaveOrUpdate(battle);
        }

        private void AddBattleToWar(string fullBattleUrl, string fullWarUrl)
        {
            if (_battlesToWars.ContainsKey(fullWarUrl))
                _battlesToWars[fullWarUrl].Add(fullBattleUrl);
            else
                _battlesToWars[fullWarUrl] = new List<string> {fullBattleUrl};
        }

        private void CrawlBelligerentsSidePane(HtmlNode sideTdNode, bool firstSide, string strength, string casualtiesAndLosses, Battle battle)
        {
            var children = sideTdNode.ChildNodes.ToList();
            var hasBrNodes = children.Any(childNode => childNode.Name == "br");
            if (hasBrNodes)
            {
                var groups = new List<List<HtmlNode>>();
                var start = true;
                var i = 0;
                foreach (var childNode in children)
                {
                    if (start)
                    {
                        groups.Add(new List<HtmlNode> {childNode});
                        start = false;
                    }
                    else
                    {
                        if (childNode.Name == "br")
                        {
                            ++i;
                            groups.Add(new List<HtmlNode>());
                        }
                        else
                            groups[i].Add(childNode);
                    }
                }
                foreach (var group in groups)
                {
                    var flagUrl = String.Empty;
                    var url = String.Empty;
                    foreach (var htmlNode in group)
                    {
                        if (htmlNode.Name == "a" && htmlNode.GetAttributeValue("class", String.Empty) == "image")
                        {
                            flagUrl = htmlNode.GetAttributeValue("src", String.Empty); // [BELLIGERENTS].FlagURL
                            break;
                        }
                    }
                    var linkFound = false;
                    foreach (var htmlNode in group)
                    {
                        if (htmlNode.Name == "a" && htmlNode.GetAttributeValue("class", String.Empty) == String.Empty && !htmlNode.GetAttributeValue("href", String.Empty).Contains("#"))
                        {
                            url = String.Format("{0}{1}", WikiPrefix, htmlNode.GetAttributeValue("href", String.Empty)); // [BELLIGERENTS].URL
                            linkFound = true;
                            break;
                        }
                    }
                    BelligerentInfo belligerentInfo;
                    if (!linkFound)
                    {
                        var textNode = @group.FirstOrDefault(node => node.Name == "#text");
                        var name = textNode != null ? textNode.InnerText : String.Empty;
                        belligerentInfo = BelligerentInfo.WithoutUrl(name, flagUrl);
                    } else
                        belligerentInfo = BelligerentInfo.WithUrl(url, flagUrl);
                    var battleBelligerentInfo = new BattleBelligerentInfo
                    {
                        Battle = battle,
                        Strength = strength, // [BATTLES_BELLIGERENTS].Strength
                        CasualtiesAndLosses = casualtiesAndLosses, // [BATTLES_BELLIGERENTS].CasualtiesAndLosses
                        FirstSide = firstSide // [BATTLES_BELLIGERENTS].ConflictSide
                    };
                    AddBattleBelligerentInfo(belligerentInfo, battleBelligerentInfo);
                }
            }
            else
            {
                var links = CrawlerHelper.GetAllNodesWithoutClassByTag(sideTdNode, "a").Where(node => !node.GetAttributeValue("href", String.Empty).Contains("#"));
                var linkNodes = links as IList<HtmlNode> ?? links.ToList();
                if (linkNodes.Any())
                {
                    var linkNode = linkNodes.First();
                    var flagUrl = CrawlerHelper.GetStringValueByTagAndClass(sideTdNode, "a", "image");
                    var belligerentInfo = BelligerentInfo.WithUrl(String.Format("{0}{1}", WikiPrefix, linkNode.GetAttributeValue("href", String.Empty)), flagUrl);
                    var battleBelligerentInfo = new BattleBelligerentInfo
                    {
                        Battle = battle,
                        Strength = strength, // [BATTLES_BELLIGERENTS].Strength
                        CasualtiesAndLosses = casualtiesAndLosses, // [BATTLES_BELLIGERENTS].CasualtiesAndLosses
                        FirstSide = firstSide // [BATTLES_BELLIGERENTS].ConflictSide
                    };
                    AddBattleBelligerentInfo(belligerentInfo, battleBelligerentInfo);
                }
                else
                {
                    var name = sideTdNode.InnerText;
                    var belligerentInfo = BelligerentInfo.WithoutUrl(name);
                    var battleBelligerentInfo = new BattleBelligerentInfo
                    {
                        Battle = battle,
                        Strength = strength, // [BATTLES_BELLIGERENTS].Strength
                        CasualtiesAndLosses = casualtiesAndLosses, // [BATTLES_BELLIGERENTS].CasualtiesAndLosses
                        FirstSide = firstSide // [BATTLES_BELLIGERENTS].ConflictSide
                    };
                    AddBattleBelligerentInfo(belligerentInfo, battleBelligerentInfo);
                }
            }
        }

        private void AddBattleBelligerentInfo(BelligerentInfo bInfo, BattleBelligerentInfo bbInfo)
        {
            if (_battlesBelligerents.ContainsKey(bInfo))
                _battlesBelligerents[bInfo].Add(bbInfo);
            else
                _battlesBelligerents[bInfo] = new List<BattleBelligerentInfo> {bbInfo};
        }

        private void CrawlWars()
        {
            // TODO
        }

        private void CrawlBelligerents()
        {
            foreach (var keyValue in _battlesBelligerents)
            {
                ++_belligerentCounter;
                var belligerentInfo = keyValue.Key;
                Logger.Log(String.Format("Crawling belligerent #{0}: {1}", _belligerentCounter, belligerentInfo.NameOnly ? belligerentInfo.Name : belligerentInfo.Url));
                var belligerent = new Belligerent
                {
                    FlagURL = belligerentInfo.FlagUrl,
                    Name = belligerentInfo.NameOnly ? belligerentInfo.Name : GetBelligerentName(belligerentInfo.Url),
                    URL = belligerentInfo.Url
                };
                _session.Flush();
                _session.SaveOrUpdate(belligerent);
                var battleBelligerentInfo = keyValue.Value;
                foreach (var info in battleBelligerentInfo)
                {
                    var battleBelligerent = new BattlesBelligerents
                    {
                        Belligerent = belligerent,
                        CasualtiesAndLosses = info.CasualtiesAndLosses,
                        ConflictSide = info.FirstSide ? 0 : 1,
                        Strength = info.Strength,
                        Battle = info.Battle
                    };
                    _session.Flush();
                    _session.SaveOrUpdate(battleBelligerent);
                }
            };
        }

        private string GetBelligerentName(string url)
        {
            var name = String.Empty;
            var doc = GetHtmlDocument(url);
            if (doc != null)
            {
                name = CrawlerHelper.GetStringValueByTagAndClass(doc.DocumentNode, "h1", "firstHeading");
            }
            return name;
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
