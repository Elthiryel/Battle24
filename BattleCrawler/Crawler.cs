using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using NHibernate;
using NHibernate.Criterion;

namespace BattleCrawler
{
    public class Crawler
    {
        private class LeaderInfo
        {
            public readonly string Name;
            public readonly string Url;
            public readonly bool NameOnly;
            public readonly BelligerentInfo Belligerent;

            private LeaderInfo(string name, string url, bool nameOnly, BelligerentInfo belligerent)
            {
                Name = name;
                Url = url;
                NameOnly = nameOnly;
                Belligerent = belligerent;
            }

            public static LeaderInfo WithUrl(string url, BelligerentInfo belligerent)
            {
                return new LeaderInfo(String.Empty, url, false, belligerent);
            }

            public static LeaderInfo WithoutUrl(string name, BelligerentInfo belligerent)
            {
                return new LeaderInfo(name, String.Empty, true, belligerent);
            }

            protected bool Equals(LeaderInfo other)
            {
                return string.Equals(Name, other.Name) && string.Equals(Url, other.Url) && NameOnly.Equals(other.NameOnly) && 
                    Equals(Belligerent, other.Belligerent);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((LeaderInfo) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = (Name != null ? Name.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ (Url != null ? Url.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ NameOnly.GetHashCode();
                    hashCode = (hashCode*397) ^ (Belligerent != null ? Belligerent.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

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
        private static int _leaderCounter;
        private static int _warCounter;

        private readonly ISession _session;

        private IList<string> _battleList;
        private IDictionary<string, IList<Battle>> _battlesToWars;
        private IDictionary<BelligerentInfo, IList<BattleBelligerentInfo>> _battlesBelligerents;
        private IDictionary<LeaderInfo, IList<string>> _battlesLeaders;

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
            _battlesToWars = new Dictionary<string, IList<Battle>>();
            _battlesBelligerents = new Dictionary<BelligerentInfo, IList<BattleBelligerentInfo>>();
            _battlesLeaders = new Dictionary<LeaderInfo, IList<string>>();

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
                if (liNode.InnerHtml.StartsWith("<a ") ||
                    (liNode.InnerHtml.Length >= 8 && liNode.InnerHtml.Substring(5).StartsWith("<a ")))
                {
                    var aNode = CrawlerHelper.GetNodeByTag(liNode, "a");
                    var url = aNode.GetAttributeValue("href", String.Empty);
                    if (!url.Contains("action=edit") && !url.Contains("#") && !url.StartsWith("http://"))
                        _battleList.Add(url);
                }
            }

            CrawlBattles();
            CrawlWars();
            CrawlBelligerents();
            CrawlLeaders();
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
                            AddBattleToWar(battle, String.Format("{0}{1}", WikiPrefix, warUrl));
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
                            battle.Date = String.IsNullOrEmpty(date) ? null : date;
                        }
                        else if (header.Contains("Location"))
                        {
                            var location = CrawlerHelper.GetStringValueByTag(trNode, "td"); // [BATTLE].Location
                            battle.Location = String.IsNullOrEmpty(location) ? null : location;
                        }
                        else if (header.Contains("Result"))
                        {
                            var result = CrawlerHelper.GetStringValueByTag(trNode, "td"); // [BATTLE].Result
                            battle.Result = String.IsNullOrEmpty(result) ? null : result;
                        }
                        else if (header.Contains("Territorial"))
                        {
                            var territorialChanges = CrawlerHelper.GetStringValueByTag(trNode, "td"); // [BATTLE].TerritorialChanges
                            battle.TerritorialChanges = String.IsNullOrEmpty(territorialChanges) ? null : territorialChanges;
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
                        }
                        else if (header.Contains("Casualties"))
                        {
                            var index = trNodesList.IndexOf(trNode);
                            var casualtiesAndLossesNode = trNodesList[index + 1];
                            casualtiesAndLosses1 = CrawlerHelper.GetStringValueByTag(casualtiesAndLossesNode, "td"); // [BATTLES_BELLIGERENTS].CasualtiesAndLosses
                            casualtiesAndLosses2 = CrawlerHelper.GetStringValueByTag(casualtiesAndLossesNode, "td", 1); // [BATTLES_BELLIGERENTS].CasualtiesAndLosses
                        }
                    }
                }
                var belligerentsByFlags = new Dictionary<string, BelligerentInfo>();
                var currentLeftBelligerents = new LinkedList<BelligerentInfo>();
                var currentRightBelligerents = new LinkedList<BelligerentInfo>();
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
                            CrawlBelligerentsSidePane(firstSideTdNode, true, strength1, casualtiesAndLosses1, battle, 
                                belligerentsByFlags, currentLeftBelligerents);
                            var secondSideTdNode = CrawlerHelper.GetNodeByTag(belligerentsTrNode, "td", 1);
                            CrawlBelligerentsSidePane(secondSideTdNode, false, strength2, casualtiesAndLosses2, battle, 
                                belligerentsByFlags, currentRightBelligerents);
                        }
                    }
                }
                foreach (var trNode in trNodesList)
                {
                    var header = CrawlerHelper.GetStringValueByTag(trNode, "th");
                    if (header != null)
                    {
                        if (header.Contains("Commanders"))
                        {
                            var index = trNodesList.IndexOf(trNode);
                            var leadersTrNode = trNodesList[index + 1];
                            var firstSideTdNode = CrawlerHelper.GetNodeByTag(leadersTrNode, "td");
                            CrawlLeadersSidePane(firstSideTdNode, battle, currentLeftBelligerents); // TODO
                            var secondSideTdNode = CrawlerHelper.GetNodeByTag(leadersTrNode, "td", 1);
                            CrawlLeadersSidePane(secondSideTdNode, battle, currentRightBelligerents); // TODO
                        }
                    }
                }
            }
            _session.Flush();
            _session.SaveOrUpdate(battle);
        }

        private void AddBattleToWar(Battle battle, string fullWarUrl)
        {
            if (_battlesToWars.ContainsKey(fullWarUrl))
                _battlesToWars[fullWarUrl].Add(battle);
            else
                _battlesToWars[fullWarUrl] = new List<Battle> {battle};
        }

        private void CrawlBelligerentsSidePane(HtmlNode sideTdNode, bool firstSide, string strength, 
            string casualtiesAndLosses, Battle battle, IDictionary<string, BelligerentInfo> belligerentsByFlags, 
            LinkedList<BelligerentInfo> currentBelligerents)
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
                            flagUrl = htmlNode.GetAttributeValue("href", String.Empty);
                            break;
                        }
                        if (htmlNode.Name == "span" && htmlNode.GetAttributeValue("class", String.Empty) == "flagicon")
                        {
                            var aNode = htmlNode.ChildNodes.FirstOrDefault(n => n.Name == "a");
                            if (aNode != null)
                            {
                                flagUrl = aNode.GetAttributeValue("href", String.Empty);
                                break;
                            }
                        }
                    }
                    var linkFound = false;
                    foreach (var htmlNode in group)
                    {
                        if (htmlNode.Name == "a" && htmlNode.GetAttributeValue("class", String.Empty) != "image" && 
                            !htmlNode.GetAttributeValue("href", String.Empty).Contains("#"))
                        {
                            url = String.Format("{0}{1}", WikiPrefix, htmlNode.GetAttributeValue("href", String.Empty)); // [BELLIGERENTS].URL
                            linkFound = true;
                            break;
                        }
                    }
                    BelligerentInfo belligerentInfo;
                    if (!linkFound)
                    {
                        var textNode = group.FirstOrDefault(node => node.Name == "#text" &&
                            Regex.Matches(node.InnerText, @"[a-zA-Z]").Count > 0);
                        var name = textNode != null ? textNode.InnerText : String.Empty;
                        belligerentInfo = BelligerentInfo.WithoutUrl(name, flagUrl);
                    }
                    else
                        belligerentInfo = url.Contains("action=edit") ? 
                            BelligerentInfo.WithoutUrl(url.Substring(42, url.IndexOf("&amp;action=edit", 
                                StringComparison.Ordinal) - 42), flagUrl) : 
                            BelligerentInfo.WithUrl(url, flagUrl);
                    if (!String.IsNullOrEmpty(flagUrl))
                        belligerentsByFlags[flagUrl] = belligerentInfo;
                    if (String.IsNullOrEmpty(belligerentInfo.Name) || !belligerentInfo.Name.Contains(":"))
                    {
                        var battleBelligerentInfo = new BattleBelligerentInfo
                        {
                            Battle = battle,
                            Strength = strength, // [BATTLES_BELLIGERENTS].Strength
                            CasualtiesAndLosses = casualtiesAndLosses, // [BATTLES_BELLIGERENTS].CasualtiesAndLosses
                            FirstSide = firstSide // [BATTLES_BELLIGERENTS].ConflictSide
                        };
                        AddBattleBelligerentInfo(belligerentInfo, battleBelligerentInfo);
                        currentBelligerents.AddLast(belligerentInfo);
                    }
                }
            }
            else
            {
                var links = CrawlerHelper.GetAllNodesWithoutClassByTag(sideTdNode, "a").
                    Where(node => !node.GetAttributeValue("href", String.Empty).Contains("#"));
                var linkNodes = links as IList<HtmlNode> ?? links.ToList();
                if (linkNodes.Any())
                {
                    var linkNode = linkNodes.First();
                    var flagNode = CrawlerHelper.GetNodeByTagAndClass(sideTdNode, "a", "image");
                    var flagUrl = flagNode != null ? flagNode.GetAttributeValue("href", String.Empty) : String.Empty;
                    var belligerentInfo = BelligerentInfo.WithUrl(String.Format("{0}{1}", WikiPrefix, 
                        linkNode.GetAttributeValue("href", String.Empty)), flagUrl);
                    if (!String.IsNullOrEmpty(flagUrl))
                        belligerentsByFlags[flagUrl] = belligerentInfo;
                    var battleBelligerentInfo = new BattleBelligerentInfo
                    {
                        Battle = battle,
                        Strength = strength, // [BATTLES_BELLIGERENTS].Strength
                        CasualtiesAndLosses = casualtiesAndLosses, // [BATTLES_BELLIGERENTS].CasualtiesAndLosses
                        FirstSide = firstSide // [BATTLES_BELLIGERENTS].ConflictSide
                    };
                    AddBattleBelligerentInfo(belligerentInfo, battleBelligerentInfo);
                    currentBelligerents.AddLast(belligerentInfo);
                }
                else
                {
                    var name = sideTdNode.InnerText;
                    if (String.IsNullOrEmpty(name) || !name.Contains(":"))
                    {
                        var belligerentInfo = BelligerentInfo.WithoutUrl(name);
                        var battleBelligerentInfo = new BattleBelligerentInfo
                        {
                            Battle = battle,
                            Strength = strength, // [BATTLES_BELLIGERENTS].Strength
                            CasualtiesAndLosses = casualtiesAndLosses, // [BATTLES_BELLIGERENTS].CasualtiesAndLosses
                            FirstSide = firstSide // [BATTLES_BELLIGERENTS].ConflictSide
                        };
                        AddBattleBelligerentInfo(belligerentInfo, battleBelligerentInfo);
                        currentBelligerents.AddLast(belligerentInfo);
                    }
                }
            }
        }

        private BelligerentInfo GetBelligerentInfo(string leaderFlagUrl, ICollection<BelligerentInfo> currentBelligerents)
        {
            BelligerentInfo belligerentInfo = null;
            if (currentBelligerents.Count == 1)
                belligerentInfo = currentBelligerents.First();
            else if (currentBelligerents.Count > 1 && !String.IsNullOrEmpty(leaderFlagUrl))
                belligerentInfo = currentBelligerents.FirstOrDefault(b => b.FlagUrl == leaderFlagUrl);
            return belligerentInfo;
        }

        private void AddBattleBelligerentInfo(BelligerentInfo bInfo, BattleBelligerentInfo bbInfo)
        {
            if (_battlesBelligerents.ContainsKey(bInfo))
                _battlesBelligerents[bInfo].Add(bbInfo);
            else
                _battlesBelligerents[bInfo] = new List<BattleBelligerentInfo> {bbInfo};
        }

        private void CrawlLeadersSidePane(HtmlNode sideTdNode, Battle battle, LinkedList<BelligerentInfo> currentBelligerents)
        {
            if (sideTdNode == null)
                return;
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
                            flagUrl = htmlNode.GetAttributeValue("href", String.Empty);
                            break;
                        }
                        if (htmlNode.Name == "span" && htmlNode.GetAttributeValue("class", String.Empty) == "flagicon")
                        {
                            var aNode = htmlNode.ChildNodes.FirstOrDefault(n => n.Name == "a");
                            if (aNode != null)
                            {
                                flagUrl = aNode.GetAttributeValue("href", String.Empty);
                                break;
                            }
                        }
                    }
                    var linkFound = false;
                    foreach (var htmlNode in group)
                    {
                        if (htmlNode.Name == "a" && htmlNode.GetAttributeValue("class", String.Empty) != "image" &&
                            !htmlNode.GetAttributeValue("href", String.Empty).Contains("#"))
                        {
                            url = String.Format("{0}{1}", WikiPrefix, htmlNode.GetAttributeValue("href", String.Empty));
                            linkFound = true;
                            break;
                        }
                    }
                    LeaderInfo leaderInfo;
                    if (!linkFound)
                    {
                        var textNode = group.FirstOrDefault(node => node.Name == "#text" &&
                                                                    Regex.Matches(node.InnerText, @"[a-zA-Z]").Count > 0);
                        var name = textNode != null ? textNode.InnerText : String.Empty;
                        leaderInfo = LeaderInfo.WithoutUrl(name, GetBelligerentInfo(flagUrl, currentBelligerents));
                    }
                    else
                        leaderInfo = url.Contains("action=edit") ?
                            LeaderInfo.WithoutUrl(url.Substring(42, url.IndexOf("&amp;action=edit", 
                                StringComparison.Ordinal) - 42), GetBelligerentInfo(flagUrl, currentBelligerents)) :
                            LeaderInfo.WithUrl(url, GetBelligerentInfo(flagUrl, currentBelligerents));
                    if (String.IsNullOrEmpty(leaderInfo.Name) || !leaderInfo.Name.Contains(":"))
                        AddLeaderInfo(leaderInfo, battle.URL);
                }
            }
            else
            {
                var links = CrawlerHelper.GetAllNodesWithoutClassByTag(sideTdNode, "a").
                    Where(node => !node.GetAttributeValue("href", String.Empty).Contains("#"));
                var linkNodes = links as IList<HtmlNode> ?? links.ToList();
                if (linkNodes.Any())
                {
                    var linkNode = linkNodes.First();
                    var flagNode = CrawlerHelper.GetNodeByTagAndClass(sideTdNode, "a", "image");
                    var flagUrl = flagNode != null ? flagNode.GetAttributeValue("href", String.Empty) : String.Empty;
                    var leaderInfo = LeaderInfo.WithUrl(String.Format("{0}{1}", WikiPrefix, 
                        linkNode.GetAttributeValue("href", String.Empty)), GetBelligerentInfo(flagUrl, currentBelligerents));
                    AddLeaderInfo(leaderInfo, battle.URL);
                }
                else
                {
                    var name = sideTdNode.InnerText;
                    if (String.IsNullOrEmpty(name) || !name.Contains(":"))
                    {
                        var leaderInfo = LeaderInfo.WithoutUrl(name, GetBelligerentInfo(null, currentBelligerents));
                        AddLeaderInfo(leaderInfo, battle.URL);
                    }
                }
            }
        }

        private void AddLeaderInfo(LeaderInfo lInfo, string bUrl)
        {
            if(_battlesLeaders.ContainsKey(lInfo))
                _battlesLeaders[lInfo].Add(bUrl);
            else
                _battlesLeaders[lInfo] = new List<string> {bUrl};
        }

        private void CrawlWars()
        {
            foreach (var keyValue in _battlesToWars)
            {
                var warUrl = keyValue.Key;
                var doc = GetHtmlDocument(warUrl);
                var war = CrawlWar(doc, warUrl);
                foreach (var battle in keyValue.Value)
                {
                    battle.War = war;
                    if (war.Battles == null)
                        war.Battles = new List<Battle> {battle};
                    else
                        war.Battles.Add(battle);
                    _session.Flush();
                    _session.Save(battle);
                }
            }
        }

        private War CrawlWar(HtmlDocument warDocument, string url, string childWar = null)
        {
            ++_warCounter;
            var war = new War();
            Logger.Log(String.Format("Crawling war #{0}: {1}", _warCounter, url));
            var name = CrawlerHelper.GetStringValueByTagAndClass(warDocument.DocumentNode, "th", "summary") ??
                CrawlerHelper.GetStringValueByTagAndClass(warDocument.DocumentNode, "h1", "firstHeading"); // [WAR].Name
            if (name == childWar)
                return null; // fix for infinite redirection loop
            var existing = _session.CreateCriteria<War>().Add(Restrictions.Eq("Name", name)).List<War>().FirstOrDefault();
            if (existing != null)
                return existing;
            war.Name = name;
            war.URL = url;
            var infoTableNode = CrawlerHelper.GetNodeByTagAndClass(warDocument.DocumentNode, "table", "infobox vevent");
            if (infoTableNode != null)
            {
                var parentTrNode = CrawlerHelper.GetNodeByTag(infoTableNode, "tr", 1);
                var tdNode = CrawlerHelper.GetNodeByTag(parentTrNode, "td");
                if (tdNode.GetAttributeValue("style", String.Empty).Contains("background-color"))
                {
                    var aNode = CrawlerHelper.GetNodeByTag(parentTrNode, "a");
                    if (aNode != null)
                    {
                        var parentUrl = aNode.GetAttributeValue("href", String.Empty);
                        if (!parentUrl.Contains("action=edit") && !parentUrl.Contains("#") && !parentUrl.StartsWith("http://"))
                        {
                            var fullParentUrl = String.Format("{0}{1}", WikiPrefix, parentUrl);
                            var parentDocument = GetHtmlDocument(fullParentUrl);
                            if (parentDocument != null)
                            {
                                var parentWar = CrawlWar(parentDocument, fullParentUrl, name);
                                war.ParentWar = parentWar; // [WAR].ParentWar
                            }
                        }
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
                            var date = CrawlerHelper.GetStringValueByTag(trNode, "td"); // [WAR].Date
                            war.Date = String.IsNullOrEmpty(date) ? null : date;
                        }
                        else if (header.Contains("Result"))
                        {
                            var result = CrawlerHelper.GetStringValueByTag(trNode, "td"); // [WAR].Result
                            war.Result = String.IsNullOrEmpty(result) ? null : result;
                        }
                        else if (header.Contains("Territorial"))
                        {
                            var territorialChanges = CrawlerHelper.GetStringValueByTag(trNode, "td"); // [WAR].TerritorialChanges
                            war.TerritorialChanges = String.IsNullOrEmpty(territorialChanges) ? null : territorialChanges;
                        }
                    }
                }
            }
            // TODO
            _session.Flush();
            _session.SaveOrUpdate(war);
            return war;
        }

        private void CrawlBelligerents()
        {
            foreach (var keyValue in _battlesBelligerents)
            {
                ++_belligerentCounter;
                var belligerentInfo = keyValue.Key;
                Logger.Log(String.Format("Crawling belligerent #{0}: {1}", _belligerentCounter, 
                    belligerentInfo.NameOnly ? belligerentInfo.Name : belligerentInfo.Url));
                if (belligerentInfo.NameOnly && 
                    (String.IsNullOrWhiteSpace(belligerentInfo.Name) || belligerentInfo.Name.Trim().StartsWith("(")))
                    continue;
                var belligerent = new Belligerent
                {
                    FlagURL = String.IsNullOrEmpty(belligerentInfo.FlagUrl) ? null : belligerentInfo.FlagUrl,
                    Name = belligerentInfo.NameOnly ? belligerentInfo.Name : GetArticleName(belligerentInfo.Url),
                    URL = String.IsNullOrEmpty(belligerentInfo.Url) ? null : belligerentInfo.Url
                };
                _session.Flush();
                _session.SaveOrUpdate(belligerent);
                var battleBelligerentInfo = keyValue.Value;
                foreach (var info in battleBelligerentInfo)
                {
                    var battleBelligerent = new BattlesBelligerents
                    {
                        Belligerent = belligerent,
                        CasualtiesAndLosses = String.IsNullOrEmpty(info.CasualtiesAndLosses) ? 
                            null : info.CasualtiesAndLosses,
                        ConflictSide = info.FirstSide ? 0 : 1,
                        Strength = String.IsNullOrEmpty(info.Strength) ? null : info.Strength,
                        Battle = info.Battle
                    };
                    _session.Flush();
                    _session.SaveOrUpdate(battleBelligerent);
                }
            };
        }

        private void CrawlLeaders()
        {
            foreach (var keyValue in _battlesLeaders)
            {
                ++_leaderCounter;
                var leaderInfo = keyValue.Key;
                Logger.Log(String.Format("Crawling leader #{0}: {1}", _leaderCounter, 
                    leaderInfo.NameOnly ? leaderInfo.Name : leaderInfo.Url));
                // TODO maybe filter? (see 673)
                var leader = new Leader
                {
                    Name = leaderInfo.NameOnly ? leaderInfo.Name : GetArticleName(leaderInfo.Url),
                    URL = String.IsNullOrEmpty(leaderInfo.Url) ? null : leaderInfo.Url
                };
                FillLeader(leader, leaderInfo, keyValue.Value);
                _session.SaveOrUpdate(leader);
                _session.Flush();
            }
        }

        private void FillLeader(Leader leader, LeaderInfo leaderInfo, IList<string> battles)
        {
            // belligerent
            Belligerent belligerent = null;
            if (leaderInfo.Belligerent != null)
                if (!leaderInfo.Belligerent.NameOnly)
                    belligerent = _session.CreateCriteria<Belligerent>().
                        Add(Restrictions.Eq("URL", leaderInfo.Belligerent.Url)).List<Belligerent>().FirstOrDefault();
                else
                    belligerent = _session.CreateCriteria<Belligerent>().
                        Add(Restrictions.Eq("Name", leaderInfo.Belligerent.Name)).List<Belligerent>().FirstOrDefault();
            leader.Belligerent = belligerent;
            // battles
            leader.Battles = new List<Battle>(battles.Count);
            foreach (var battleUrl in battles)
            {
                var battle = _session.CreateCriteria<Battle>().Add(Restrictions.Eq("URL", battleUrl)).List<Battle>().
                    FirstOrDefault();
                if (battle != null)
                {
                    leader.Battles.Add(battle);
                    if (battle.Leaders != null)
                        battle.Leaders.Add(leader);
                    else
                        battle.Leaders = new List<Leader> {leader};
                    _session.SaveOrUpdate(battle);
                }
            }
        }

        private string GetArticleName(string url)
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
