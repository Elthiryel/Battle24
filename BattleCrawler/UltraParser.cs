using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NHibernate.Linq.ExpressionTransformers;

namespace BattleCrawler
{
    public static class UltraParser
    {
        public static DateTime? ParseDate(string text,bool start = false)
        {
            text = RemoveCitation(text);
            var dividedDates = Divide(text);
            var count = (start) ? 0 : dividedDates.Count -1 ;
            DateTime? date = null;
            try
            {
                date = DateTime.Parse(dividedDates[count]);
                if (date.Value.Year > 1801 || dividedDates[count].Count() < 4)
                    throw new Exception();
            }
            catch (Exception e)
            {
                var matched = Regex.Match(text, @"\d{4}");
                if (matched.Success)
                    date = new DateTime(Convert.ToInt32(matched.Groups[0].ToString()),1,1,0,0,0);
                
            }
            return date;
        }

        public static string ParseCountry(string text)
        {
            text = RemoveCitation(text);

            var results = from result in countries
              where Regex.Match(text, result, RegexOptions.Singleline).Success
              select result;

            return results.FirstOrDefault();
        }

        public static Victory ParseResult(string text, IEnumerable<BattlesBelligerents> belligerents)
        {
            Dictionary<string, BattlesBelligerents> dict = new Dictionary<string, BattlesBelligerents>();
            foreach (var belli in belligerents)
            {
                var shortcut = RemoveHonorific(belli.Belligerent.Name);
                dict.Add(shortcut,belli);
            }
                
            var matched = Regex.Match(text.ToLower(), @"decisive (.*?)victory");
            var sideToMatch = matched.Groups[1].ToString();
            if (sideToMatch != "")
                return GetVictor(sideToMatch, dict, true);
            else
            {
                matched = Regex.Match(text.ToLower(), @"(.*?)victory");
                sideToMatch = matched.Groups[1].ToString();
                if (sideToMatch != "")
                    return GetVictor(sideToMatch, dict, false);
                else
                    return Victory.Inconclusive;
            }
        }

        private static string RemoveHonorific(string text)
        {
            var results = from result in honorifics
                          where Regex.Match(text, result, RegexOptions.Singleline).Success
                          select text.Replace(result,"").Trim();

            return results.FirstOrDefault() ?? text;
        }

        private static Victory GetVictor(string matched, Dictionary<string, BattlesBelligerents> bellis, bool decisive)
        {
            BattlesBelligerents victor = null;
            int iMaxLength = 0;
            foreach (var belli in bellis.Keys)
            {
                for (int i = 0; i < Math.Min(belli.Count(), matched.Count()); i++)
                {
                    if (belli.ToLower()[i] != matched[i])
                    {
                        if (i >= iMaxLength)
                        {
                            iMaxLength = i;
                            victor = bellis[belli];
                        }
                        break;    
                    }
                    if (i == Math.Min(belli.Count(), matched.Count()) - 1)
                    {
                        iMaxLength = i;
                        victor = bellis[belli];
                    }
                        
                }
            }

            if (iMaxLength < 2)
                return Victory.Inconclusive;

            switch (victor.ConflictSide)
            {
                case 0:
                {
                    return (decisive) ? Victory.DecisiveFirst : Victory.First ;
                }
                case 1:
                    return (decisive) ? Victory.DecisiveSecond : Victory.Second;
            }
            return Victory.Inconclusive;
        }

        private static string RemoveCitation(string text)
        {
            var matches = Regex.Matches(text, @"\[(.*?)\]");
            foreach (Match m in matches)
            {
                text = text.Replace(m.Groups[0].ToString(), "");
            }
            return text.Replace("&#160;"," ");
        }

        private static List<string> Divide(string text)
        {
            return Regex.Split(text, "[–-]").ToList();
        }

        private static List<string> honorifics = new List<string>()
        {
            "Kingdom of",
            "Duke of",
            "Crown of",
            "Republic of",
            "Canton of",
            "House of",
            "Forces of",
            "Clan",
            "Monarchy",
            "Army",
            "Navy",
            "Dynasty",
            "County of",
            "Lordship of",
            "Prinicipality",
            "Empire",
            "Emirate of"
        };

        private static List<string> countries = new List<string>()
        {
            "England",
            "France",
            "Poland",
            "Italy",
            "Egypt",
            "Switzerland",
            "Turkey",
            "Spain",
            "Ukraine",
            "Philippines",
            "Sweden",
            "Netherlands",
            "Scotland",
            "Germany",
            "Portugal",
            "Australia",
            "Belgium",
            "India",
            "Czech Republic",
            "Hungary",
            "Moldova",
            "Romania",
            "Bulgaria",
            "Ethiopia",
            "Norway",
            "Denmark",
            "Serbia",
            "Latvia",
            "Belarus",
            "Ireland",
            "Austria",
            "Brazil",
            "Colombia",
            "Ecuador",
            "Japan",
            "China",
            "Iran",
            "Syria",
            "Mexico",
            "Peru",
            "Russia",
            "Korea",
            "Indonesia",
            "Finland",
            "Estonia",
            "Lithuania",
            "Angola",
            "Tunisia",
            "Croatia",
            "Cuba",
            "Malaysia",
            "Ghana",
            "Albania"
        };

        private static Dictionary<String,String> ResultExceptions = new Dictionary<string, string>()
        {
            {"Dutch","Netherlands"}
        };
    }

    public enum Victory
    {
        DecisiveFirst,
        DecisiveSecond,
        First,
        Second,
        Inconclusive
    }
}
