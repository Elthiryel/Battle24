﻿namespace BattleCrawler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var session = NHibernateHelper.GetCurrentSession();
            Leader Marcin = new Leader();
            Marcin.Name = "marcin";
             
            Belligerent Turcja = new Belligerent();
            Turcja.Name = "turcja";
            Marcin.Belligerent = Turcja;
            Battle newBattle = new Battle();
            Marcin.Battles.Add(newBattle);
            newBattle.Name = "BitwaOWieden";
            newBattle.Leaders.Add(Marcin);
            BattlesBelligerents battleBel = new BattlesBelligerents();
            battleBel.Belligerent = Turcja;
            battleBel.Battle = newBattle;
            newBattle.Belligerents.Add(battleBel);
            session.SaveOrUpdate(newBattle);
        }
    }
}