using Bittrex.Net;
using Bittrex.Net.Objects;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CrypoGraph
{
    public class LimitConfigViewModel : INotifyPropertyChanged
    {
        private LineSeries limitLine;
        private string Symbol;
        public LineOrder LineOrder;
        public double Slope;
        public double Yintercept;
        private DataPoint Start;
        private DataPoint End;

        public LimitConfigViewModel(DataPoint start, DataPoint end, LineSeries line, string symbol, decimal minTradeSize)
        {
            OrdersPerHourOptions = new List<double>();

            for (double i = 1; i < 61; i++)
            {
                OrdersPerHourOptions.Add(i);

            }


            Start = start;
            End = end;
            OrderPlacedEvent = new ManualResetEvent(false);
            OrdersPerHour = 60;
            LineOrder = null;
            Symbol = symbol;
            OrderType = OrderTypeV3.Limit;
            TimeInForce = TimeInForce.FillOrKill;
            QuantityPerOrder = minTradeSize;
            ErrorMsg = "";
            limitLine = line;
            CalculateSlope(start,end);
            CalculateYIntercept(start, end);
            

            StartString = DateTimeAxis.ToDateTime(start.X).ToString();
            EndString = DateTimeAxis.ToDateTime(end.X).ToString();
            StartStringPrice = start.Y;
            EndStringPrice = end.Y;
            PlaceOrderCommand = new RelayCommand(PlaceOrder, CanPlaceOrder);
            CancelOrderCommand = new RelayCommand(CancelOrder, CanCancel);
        }

        private void CalculateYIntercept(DataPoint start, DataPoint end)
        {
            //y - mx = b
            if(Slope != 0)
            {
                Yintercept = start.Y - (Slope * start.X);
            }
            
        }

        public void CalculateSlope(DataPoint point1, DataPoint point2)
        {
            var slope = (point2.Y - point1.Y ) / (point2.X - point1.X);
            Slope = slope;
        }

        public double CalculateLimitAtSpecificTime(double time)
        { // y-b/ m = x
            return (Slope * time) + Yintercept;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged;


        public OrderSide OrderSide { get; set; }
        public OrderTypeV3 OrderType { get; set; }
        public TimeInForce TimeInForce { get; set; }
        public double OrdersPerHour { get; set; }

        public ICommand PlaceOrderCommand
        {
            get; set;
        }


        private bool CanPlaceOrder()
        {
            return true;
        }
        public ManualResetEvent OrderPlacedEvent;

        private void PlaceOrder()
        {
            if(isSell)
            {
                OrderSide = OrderSide.Sell;
                limitLine.Color = OxyColors.Green;
            }
            else if (isBuy)
            {
                OrderSide = OrderSide.Buy;
                limitLine.Color = OxyColors.Blue;
            }
            else
            {
                ErrorMsg = "You must choose an Order Side";
                return;
            }
            string id;
            AddOrderPoints();
            LineOrder order = new LineOrder() { Symbol = Symbol, TimeInForce = TimeInForce, Start = StartString, End = EndString, LimitLineSeries = limitLine,
                OrderSide = OrderSide, OrderType = OrderType, OrderIDs = new List<string>(), OrdersPerHour = OrdersPerHour, QuantityPerOrder = QuantityPerOrder, Slope = Slope, YIntercept = Yintercept };
            LineOrder = order;
            Screen.Close();
            //decimal? limit = new decimal(0.1500);
            //var client = new BittrexClientV3();
            //var placedOrder = client.PlaceOrder("ADA-USD", OrderSide.Buy, OrderTypeV3.Limit, TimeInForce, 50, limit);
            //var orderInfo = client.GetOrder(placedOrder.Data.Id);


        }

        private void AddOrderPoints()
        {
            List<DataPoint> orderpoints = (List<DataPoint>) limitLine.ItemsSource;
            var start = orderpoints[0];
            orderpoints.RemoveAt(0);
            var last = orderpoints[0];
            orderpoints.Clear();


            //use orders per  hour to calculate points
            var time = DateTime.Now;
            var timedouble = DateTimeAxis.ToDouble(time);
            if(timedouble < start.X)
            {
                //the  first point should be the start point not now.
                timedouble = start.X;
                time = DateTimeAxis.ToDateTime(start.X);
            }
            var first = new DataPoint(timedouble, CalculateLimitAtSpecificTime(timedouble));
            orderpoints.Insert(0,first);
            //find interval value by dividing total line time by orders per hour

            var minutesUntilOrder = 1.0 / (OrdersPerHour / 60.0);

            DateTime timecounter = time;
            int count = 0;
            while(timecounter < DateTimeAxis.ToDateTime(End.X) && count < 50)
            {
                count++;
                timecounter = timecounter.AddMinutes(minutesUntilOrder);

                var nextXCorddouble = DateTimeAxis.ToDouble(timecounter);
                orderpoints.Add(new DataPoint(nextXCorddouble, CalculateLimitAtSpecificTime(nextXCorddouble)));
            }

            orderpoints.Add(last);
            limitLine.ItemsSource = orderpoints;
            
           
        }

        BittrexSocketClientV3 socketClient = new BittrexSocketClientV3();
        CryptoExchange.Net.Objects.WebCallResult<IEnumerable<BittrexSymbolSummary>> marketSummaries1;

        private void Authenticate()
        {
            BittrexClient.SetDefaultOptions(new BittrexClientOptions()
            {
                ApiCredentials = new ApiCredentials("8180a6a91e29425c90c2e2afe349aa71", "e36a6307025c4b4fb1b20a4a00c4c9ef"),
                LogVerbosity = LogVerbosity.Info,
                LogWriters = new List<TextWriter>() { Console.Out }
            });
            BittrexClientV3.SetDefaultOptions(new BittrexClientOptions()
            {
                ApiCredentials = new ApiCredentials("8180a6a91e29425c90c2e2afe349aa71", "e36a6307025c4b4fb1b20a4a00c4c9ef"),
                LogVerbosity = LogVerbosity.Info,
                LogWriters = new List<TextWriter>() { Console.Out }
            });
        }

        public void DoBittrexNetStuff()
        {

            Authenticate();
            //using (var client = new BittrexClient())
            //{
            //    // public
            //    var markets = client.GetSymbols();
            //    var currencies = client.GetCurrencies();
            //    var price = client.GetTicker("BTC-ETH");
            //    var marketSummary = client.GetSymbolSummary("BTC-ETH");
            //    marketSummaries1 = client.GetSymbolSummaries();
            //    var orderbook = client.GetOrderBook("BTC-ETH");
            //    //var marketHistory = client.GetSy("BTC-ETH");

            //    // private
            //    // Commented to prevent accidental order placement



            //    var openOrders = client.GetOpenOrders("BTC-NEO");
            //    var orderHistory = client.GetOrderHistory("BTC-NEO");

            //    var balance = client.GetBalance("NEO");
            //    var balances = client.GetBalances();
            //    var depositAddress = client.GetDepositAddress("BTC");
            //    var withdraw = client.Withdraw("TEST", 1, "TEST", "TEST");
            //    var withdrawHistory = client.GetWithdrawalHistory();
            //    var depositHistory = client.GetDepositHistory();
            //}

            ////placing limit order
            //using (var client = new BittrexClientV3())
            //{
            //    var marketSummary = client.GetSymbolSummary("ADA-USD");


            //    //decimal? limit = new decimal(0.1500);

            //    //var placedOrder = client.PlaceOrder("ADA-USD", OrderSide.Buy, OrderTypeV3.Limit, TimeInForce, 50, limit);
            //    //var orderInfo = client.GetOrder(placedOrder.Data.Id);
            //    //var canceledOrder = client.CancelOrder(placedOrder.Data.Uuid);

            //}


            // Websocket

            //socketClient.SubscribeToMarketSummariesUpdate(data =>
            //{
            //    var eth = data.SingleOrDefault(d => d.MarketName == "BTC-ETH");
            //    if (eth != null)
            //        UpdateLastPrice(eth.Last);
            //});

            //using (var client = new BittrexClient())
            //{
            //    var result = client.GetMarketSummary("BTC-ETH");
            //    UpdateLastPrice(result.Data.Last);
            //    label2.Invoke(new Action(() => { label2.Text = "BTC-ETH Volume: " + result.Data.Volume; }));
            //}

            //var subscription = socketClient.SubscribeToSymbolSummaryUpdatesAsync(summaries =>
            //{

            //});



            //var subscription3 = socketClient.SubscribeToOrderUpdatesAsync(order =>
            //{
            //});


        }


        public ICommand CancelOrderCommand
        {
            get; set;
        }


        private bool CanCancel()
        {
            return true;
        }

        private void CancelOrder()
        {

        }


        private List<double> _OrdersPerHourOptions;
        public List<double> OrdersPerHourOptions
        {
            get
            {
                return _OrdersPerHourOptions;
            }

            set
            {
                if (_OrdersPerHourOptions == value)
                {
                    return;
                }
                _OrdersPerHourOptions = value;
                OnPropertyChanged("OrdersPerHourOptions");
            }

        }

        public double CalculateTimeAtSpecificPrice(double price)
        {//y = mx+b
            return (price - Yintercept) / Slope;
        }

        private double _startstringprice;
        public double StartStringPrice
        {
            get
            {
                return _startstringprice;
            }

            set
            {
                if (_startstringprice == value)
                {
                    return;
                }
                _startstringprice = value;
                if(_startstringprice != 0)
                {
                    StartString = DateTimeAxis.ToDateTime(CalculateTimeAtSpecificPrice(_startstringprice)).ToString();
                }
                
                OnPropertyChanged("StartStringPrice");
            }

        }

        private double _endstringprice;
        public double EndStringPrice
        {
            get
            {
                return _endstringprice;
            }

            set
            {
                if (_endstringprice == value)
                {
                    return;
                }
                _endstringprice = value;
                if (_endstringprice != null)
                {
                    EndString = DateTimeAxis.ToDateTime(CalculateTimeAtSpecificPrice(_endstringprice)).ToString();
                }
                OnPropertyChanged("EndStringPrice");
            }

        }


        private string _startstring;
        public string StartString
        {
            get
            {
                return _startstring;
            }

            set
            {
                if (_startstring == value)
                {
                    return;
                }
                _startstring = value;
                OnPropertyChanged("StartString");
            }

        }


        private bool _isbuy;
        public bool isBuy
        {
            get
            {
                return _isbuy;
            }

            set
            {
                if (_isbuy == value)
                {
                    return;
                }
                _isbuy = value;
                OnPropertyChanged("isBuy");
            }

        }

        private bool _issell;
        public bool isSell
        {
            get
            {
                return _issell;
            }

            set
            {
                if (_issell == value)
                {
                    return;
                }
                _issell = value;
                OnPropertyChanged("isSell");
            }

        }

        private decimal _quantity;
        public decimal QuantityPerOrder
        {
            get
            {
                return _quantity;
            }

            set
            {
                if (_quantity == value)
                {
                    return;
                }
                _quantity = value;
                OnPropertyChanged("QuantityPerOrder");
            }

        }

        private string _error;
        public string ErrorMsg
        {
            get
            {
                return _error;
            }

            set
            {
                if (_error == value)
                {
                    return;
                }
                _error = value;
                OnPropertyChanged("ErrorMsg");
            }

        }

        private string _endstring;
        public string EndString
        {
            get
            {
                return _endstring;
            }

            set
            {
                if (_endstring == value)
                {
                    return;
                }
                _endstring = value;
                OnPropertyChanged("EndString");
            }

        }

        public LimitCondig Screen { get; internal set; }
    }
}
