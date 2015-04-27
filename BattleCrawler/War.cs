using System;
using System.Text;
using System.Collections.Generic;


namespace BattleCrawler {
    
    public partial class War {
        public virtual int Id { get; set; }
        public virtual War ParentWar { get; set; }
        public virtual string Name { get; set; }
        public virtual string Date { get; set; }
        public virtual string TerritorialChanges { get; set; }
        public virtual string Result { get; set; }
        public virtual string URL { get; set; }
        public virtual IList<Battle> Battles { get; set; }
        public virtual IList<Leader> Leaders { get; set; }
        public virtual IList<Belligerent> Belligerents { get; set; }
    }
}
