﻿using System;
using System.Text;
using System.Collections.Generic;


namespace BattleCrawler
{

    public partial class Belligerent
    {
        public Belligerent()
        {

        }
        public virtual int Id { get; set; }
        public virtual string FlagURL { get; set; }
        public virtual string Name { get; set; }
        public virtual string URL { get; set; }
        public virtual IList<War> Wars { get; set; }
    }
}
