using System;
using System.Collections.Generic;

namespace BattleCrawler
{
    public partial class Battle
    {
        public virtual int Id { get; set; }
        public virtual War War { get; set; }
        public virtual string Name { get; set; }
        public virtual string Date { get; set; }
        public virtual string Location { get; set; }
        public virtual string Result { get; set; }
        public virtual string TerritorialChanges { get; set; }
        public virtual string URL { get; set; }
        public virtual IList<BattlesBelligerents> Belligerents { get; set; }
        public virtual IList<Leader> Leaders { get; set; }
        public virtual DateTime? StartDate { get; set; }
        public virtual DateTime? EndDate { get; set; }
        public virtual string Country { get; set; }
    }
}