using System;
using System.Text;
using System.Collections.Generic;


namespace BattleCrawler {
    
    public partial class Treaty {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Date { get; set; }
        public virtual string Summary { get; set; }
        public virtual string URL { get; set; }
    }
}
