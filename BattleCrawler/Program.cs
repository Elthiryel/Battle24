using System;

namespace BattleCrawler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var session = NHibernateHelper.GetCurrentSession();

            var crawler = new Crawler(session);
            crawler.Crawl();

            var persister = new ParsedDataPersister(session);
            persister.PersistParsedData("../../../Data/battles.csv", "../../../Data/belligerents.csv", "../../../Data/battles_belligerents.csv");

            var preparator = new DataPreparator(session);
            preparator.PrepareBattleDates();
            preparator.PrepareCountries();
            preparator.PrepareResult();

            Console.ReadLine();
        }
    }
}
