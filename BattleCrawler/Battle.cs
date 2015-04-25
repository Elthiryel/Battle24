using System;
using System.Collections.Generic;

namespace BattleCrawler
{
    public partial class Battle
    {
        public Battle()
        {
            Belligerents = new List<BattlesBelligerents>();
            Leaders = new List<Leader>();
        }
        public virtual int Id { get; set; }
        public virtual War War { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime? Date { get; set; }
        public virtual string Location { get; set; }
        public virtual string Result { get; set; }
        public virtual string Territorialchanges { get; set; }
        public virtual string Url { get; set; }
        public virtual DateTime? Startdate { get; set; }
        public virtual DateTime? Enddate { get; set; }
        public virtual IList<BattlesBelligerents> Belligerents { get; set; }
        public virtual IList<Leader> Leaders { get; set; }
    }
}