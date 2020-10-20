
using Bittrex.Net.Objects;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypoGraph
{
    public class LineOrder
    {
        public OrderTypeV3 OrderType { get; set; }
        public OrderSide OrderSide { get; set; }
        public TimeInForce TimeInForce { get; set; }
        public LineSeries LimitLineSeries { get; set; }
        public List<string> OrderIDs { get; set; }

        public string Start { get; set; }
        public string End { get; set; }

        public string Symbol { get; set; }

        public int OrdersPerHour { get; set; }

        public decimal QuantityPerOrder { get; set; }

        public string LastPlacedOrderId {get; set;}


    }
}
