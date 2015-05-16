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
        public virtual string Result { get; set; }
        public virtual int InfantryStrength { get; set; }
        public virtual int CavalryStrength { get; set; }
        public virtual int ArtilleryStrength { get; set; }
        public virtual int NavyStrength { get; set; }
        public virtual int OtherStrength { get; set; }
        public virtual int AllStrength { get; set; }
        public virtual int Killed { get; set; }
        public virtual int Wounded { get; set; }
        public virtual int Captured { get; set; }
        public virtual int OtherLosses { get; set; }
        public virtual int AllLosses { get; set; }
        public virtual int ShipsLost { get; set; }
    }
}
