using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypoGraph
{
    public class UpcomingOrder
    {
        public string TimeDisplay { get; set; }
        public DateTime Time { get; set; }

        public string Price { get; set; }

        public string Side { get; set; }

        public LineOrder Order { get; set; }

        public bool StatusCompleted { get; set; }
    }
}
