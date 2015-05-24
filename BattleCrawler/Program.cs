using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;

namespace BattleCrawler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var session = NHibernateHelper.GetCurrentSession();
//            DataPreparator preparator = new DataPreparator(session);
//            preparator.PrepareBattleDates();
//            preparator.PrepareCountries();
//            preparator.PrepareResult();
//
            var crawler = new Crawler(session);
            crawler.Crawl();

//            var persister = new ParsedDataPersister(session);
//            persister.PersistParsedData("battles.csv", "belligerents.csv", "battles_belligerents.csv");

            Console.ReadLine();
        }
    }
}
