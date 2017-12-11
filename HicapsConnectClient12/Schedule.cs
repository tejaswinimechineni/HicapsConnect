using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HicapsConnectClient12
{
    // a schedule class. For serialisation
    public class Schedule
    {
        public Schedule(DateTime expiry, List<Item> items) { Expiry = expiry; Items = items; }
        public Schedule() { }
        public DateTime Expiry { get; set; }
        public List<Item> Items { get; set; }
    }
}