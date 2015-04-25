using System;

namespace BattleCrawler
{
    public class Logger
    {
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }

        public static void Log(Exception exception, string message = null)
        {
            Console.WriteLine("Exception thrown.");
            if (message != null)
                Console.WriteLine(message);
            Console.WriteLine(exception.Message);
            Console.WriteLine(exception.StackTrace);
        }
    }
}
