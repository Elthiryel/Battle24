using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Criterion;

namespace BattleCrawler
{
    public class ParsedDataPersister
    {
        private readonly ISession _session;

        public ParsedDataPersister(ISession session)
        {
            _session = session;
        }

        public void PersistParsedData(string battlesFilename, string belligerentsFilename, string battlesBelligerentsFilename)
        {
            var battlesDictionary = new Dictionary<int, string>(); // BATTLES.ID -> BATTLES.URL
            using (var battlesFile = new StreamReader(battlesFilename))
            {
                while (true)
                {
                    var line = battlesFile.ReadLine();
                    if (String.IsNullOrEmpty(line))
                        break;
                    var splitted = line.Split(';');
                    var id = Int32.Parse(splitted[0]);
                    var url = splitted[1];
                    battlesDictionary[id] = url;
                }
            }

            var belligerentsDictionary = new Dictionary<int, Tuple<string, string>>();
            using (var belligerentsFile = new StreamReader(belligerentsFilename))
            {
                while (true)
                {
                    var line = belligerentsFile.ReadLine();
                    if (String.IsNullOrEmpty(line))
                        break;
                    var splitted = line.Split(';');
                    var id = Int32.Parse(splitted[0]);
                    var name = splitted[1];
                    var url = splitted[2] == "NULL" ? null : splitted[2];
                    belligerentsDictionary[id] = new Tuple<string, string>(name, url);
                }
            }

            // BelligerentID, BattleID, InfantryStrength, CavalryStrength, ArtilleryStrength, NavyStrength, AllStrength, OtherStrength, Killed, Wounded, Captured, AllLosses, OtherLosses, ShipsLost
            using (var battlesBelligerentsFile = new StreamReader(battlesBelligerentsFilename))
            {
                while (true)
                {
                    var line = battlesBelligerentsFile.ReadLine();
                    if (String.IsNullOrEmpty(line))
                        break;

                    var splitted = line.Split(';');
                    var belligerentId = Int32.Parse(splitted[0]);
                    var battleId = Int32.Parse(splitted[1]);
                    var battle = _session.CreateCriteria<Battle>().Add(Restrictions.Eq("URL", battlesDictionary[battleId])).List<Battle>().FirstOrDefault();
                    var battleBelligerentList = _session.QueryOver<BattlesBelligerents>().JoinQueryOver(bb => bb.Battle).Where(b => b.Id == battle.Id).List<BattlesBelligerents>();
                    var belligerentName = belligerentsDictionary[belligerentId].Item1;
                    var belligerentUrl = belligerentsDictionary[belligerentId].Item2;

                    BattlesBelligerents battlesBelligerents = null;
                    var isExactName = false;
                    if (!String.IsNullOrEmpty(belligerentUrl))
                    {
                        foreach (var bb in battleBelligerentList)
                        {
                            if (bb.Belligerent.URL == belligerentUrl)
                            {
                                battlesBelligerents = bb;
                                isExactName = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (var bb in battleBelligerentList)
                        {
                            if (bb.Belligerent.Name == belligerentName)
                            {
                                battlesBelligerents = bb;
                                isExactName = true;
                                break;
                            }
                        }
                        if (!isExactName)
                        {
                            foreach (var bb in battleBelligerentList)
                            {
                                if (bb.Belligerent.Name.Contains(belligerentName))
                                {
                                    battlesBelligerents = bb;
                                    break;
                                }
                            }
                        }
                    }

                    if (battlesBelligerents != null)
                    {
                        if (!isExactName)
                            Logger.Log(String.Format("PARTIALY MISSED! BattleID = {0}, BelligerentID = {1}", battleId, belligerentId));

                        var infantryStrength = String.IsNullOrEmpty(splitted[2]) ? null : (int?) Int32.Parse(splitted[2]);
                        var cavalryStrength = String.IsNullOrEmpty(splitted[3]) ? null : (int?)Int32.Parse(splitted[3]);
                        var artilleryStrength = String.IsNullOrEmpty(splitted[4]) ? null : (int?)Int32.Parse(splitted[4]);
                        var navyStrength = String.IsNullOrEmpty(splitted[5]) ? null : (int?)Int32.Parse(splitted[5]);
                        var allStrength = String.IsNullOrEmpty(splitted[6]) ? null : (int?)Int32.Parse(splitted[6]);
                        var otherStrength = String.IsNullOrEmpty(splitted[7]) ? null : splitted[7];
                        var killed = String.IsNullOrEmpty(splitted[8]) ? null : (int?)Int32.Parse(splitted[8]);
                        var wounded = String.IsNullOrEmpty(splitted[9]) ? null : (int?)Int32.Parse(splitted[9]);
                        var captured = String.IsNullOrEmpty(splitted[10]) ? null : (int?)Int32.Parse(splitted[10]);
                        var allLosses = String.IsNullOrEmpty(splitted[11]) ? null : (int?)Int32.Parse(splitted[11]);
                        var otherLosses = String.IsNullOrEmpty(splitted[12]) ? null : splitted[12];
                        var shipsLost = String.IsNullOrEmpty(splitted[13]) ? null : (int?)Int32.Parse(splitted[13]);

                        battlesBelligerents.InfantryStrength = infantryStrength;
                        battlesBelligerents.CavalryStrength = cavalryStrength;
                        battlesBelligerents.ArtilleryStrength = artilleryStrength;
                        battlesBelligerents.NavyStrength = navyStrength;
                        battlesBelligerents.AllStrength = allStrength;
                        battlesBelligerents.OtherStrength = otherStrength;
                        battlesBelligerents.Killed = killed;
                        battlesBelligerents.Wounded = wounded;
                        battlesBelligerents.Captured = captured;
                        battlesBelligerents.AllLosses = allLosses;
                        battlesBelligerents.OtherLosses = otherLosses;
                        battlesBelligerents.ShipsLost = shipsLost;

                        _session.Update(battlesBelligerents);
                        _session.Flush();
                    }
                    else
                        Logger.Log(String.Format("MISSED! BattleID = {0}, BelligerentID = {1}", battleId, belligerentId));
                }
            }

        }
    }
}
