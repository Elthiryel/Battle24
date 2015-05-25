namespace BattleCrawler
{
    public class DataPreparator
    {
        public NHibernate.ISession Session { get; set; }

        public DataPreparator(NHibernate.ISession sesssion)
        {
            Session = sesssion;
        }

        public void PrepareBattleDates()
        {
            Logger.Log("Preparing battle dates...");
            var batlles = Session.CreateCriteria<Battle>().List<Battle>();
            foreach (var battle in batlles)
            {
                if (!string.IsNullOrEmpty(battle.Date))
                {
                    battle.StartDate = UltraParser.ParseDate(battle.Date, true).Value.AddYears(1000);
                    battle.EndDate = UltraParser.ParseDate(battle.Date).Value.AddYears(1000);
                    Session.Flush();
                    Session.SaveOrUpdate(battle);
                }
            }
            Logger.Log("Finished preparing battle dates.");
        }

        public void PrepareCountries()
        {
            Logger.Log("Preparing countries...");
            var batlles = Session.CreateCriteria<Battle>().List<Battle>();
            foreach (var battle in batlles)
            {
                if (!string.IsNullOrEmpty(battle.Location))
                {
                    battle.Country = UltraParser.ParseCountry(battle.Location);
                    Session.Flush();
                    Session.SaveOrUpdate(battle);
                }
            }
            Logger.Log("Finished preparing countries.");
        }

        public void PrepareResult()
        {
            Logger.Log("Preparing result...");
            var batlles = Session.CreateCriteria<Battle>().List<Battle>();
            //var bellis = Session.CreateCriteria<BattlesBelligerents>().List<BattlesBelligerents>();
            foreach (var battle in batlles)
            {
                try
                {
                    if (!string.IsNullOrEmpty(battle.Result))
                    {
                        var result = UltraParser.ParseResult(battle.Result, battle.Belligerents);
                        foreach (var belli in battle.Belligerents)
                        {
                            belli.Result = GetTranslationForWin(result, belli.ConflictSide);
                            Session.Flush();
                            Session.SaveOrUpdate(belli);
                        }
                    }

                     
                }
                catch
                {
                }
            }
            Logger.Log("Finished preparing result.");
        }

        private string GetTranslationForWin(Victory result, int? conflictSide)
        {
            switch (result)
            {
                case Victory.DecisiveFirst:
                    if (conflictSide == 0)
                        return "Decisive Win";
                    if (conflictSide == 1)
                        return "Decisive Lose";
                    break;
                case Victory.DecisiveSecond:
                    if (conflictSide == 1)
                        return "Decisive Win";
                    if (conflictSide == 0)
                        return "Decisive Lose";
                    break;
                case Victory.First:
                    if (conflictSide == 0)
                        return "Win";
                    if (conflictSide == 1)
                        return "Lose";
                    break;
                case Victory.Second:
                    if (conflictSide == 1)
                        return "Win";
                    if (conflictSide == 0)
                        return "Lose";
                    break;
                case Victory.Inconclusive:
                    return "Inconclusive";
                    break;
            }
            return null;
        }
    }
}
