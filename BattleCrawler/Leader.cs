using System;
using System.Text;
using System.Collections.Generic;


namespace BattleCrawler
{

    public partial class Leader
    {
        public Leader()
        {
            Wars = new List<War>();
        }
        public virtual int Id { get; set; }
        public virtual Belligerent Belligerent { get; set; }
        public virtual string Name { get; set; }
        public virtual string URL { get; set; }
        public virtual IList<Battle> Battles { get; set; }
        public virtual IList<War> Wars { get; set; }
    }
}
