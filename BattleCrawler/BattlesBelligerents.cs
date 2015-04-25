using System;
using System.Text;
using System.Collections.Generic;


namespace BattleCrawler {
    
    public partial class BattlesBelligerents {
        public virtual int Id { get; set; }
        public virtual Belligerent Belligerents { get; set; }
        public virtual Battle Battle { get; set; }
        public virtual string Strength { get; set; }
        public virtual string Casualtiesandlosses { get; set; }
        public virtual int? Conflictside { get; set; }
    }
}
