using System.Collections.Generic;

namespace BattleCrawler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var session = NHibernateHelper.GetCurrentSession();
            Crawler crawler = new Crawler(session);
            crawler.Crawl();
        }
    }
}
