using System;
using System.Text;
using System.Collections.Generic;


namespace BattleCrawler {
    
    public partial class BattlesBelligerents {
        public virtual int Id { get; set; }
        public virtual Belligerent Belligerent { get; set; }
        public virtual Battle Battle { get; set; }
        public virtual string Strength { get; set; }
        public virtual string CasualtiesAndLosses { get; set; }
        public virtual int? ConflictSide { get; set; }
    }
}
