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

            if (FulfillmentThreshold == 0)
            {
                ErrorMsg = "You must choose a threshold for the line to be Fulfilled.  Use -1 if you want to run the line order to completion.";
                return;
            }else if(FulfillmentThreshold  < 0)
            {
                if(FulfillmentThreshold != -1)
                {
                    ErrorMsg = "Invalid Fulfillment Threshold.  Use -1 if you want to run the line order to completion.";
                    return;
                }
            }

            string id;
            AddOrderPoints();



            LineOrder order = new LineOrder()
            {
                Symbol = Symbol,
                TimeInForce = TimeInForce,
                Start = StartString,
                End = EndString,
                LimitLineSeries = limitLine,
                OrderSide = OrderSide,
                OrderType = OrderType,
                OrderIDs = new List<string>(),
                OrdersPerHour = OrdersPerHour,
                QuantityPerOrder = QuantityPerOrder,
                Slope = Slope,
                YIntercept = Yintercept,
                FulfilmentThreshold = FulfillmentThreshold
            };
            LineOrder = order;
            Screen.Close();


       


        }

        private bool AddOrderPoints()
        {
            List<DataPoint> orderpoints = (List<DataPoint>) limitLine.ItemsSource;
            var start = Start;
            var last = End;
            orderpoints.Clear(); //remove the previous start and end points


            //coverride the start time with the current time if it is in the past.     
            var time = DateTime.Now;
            var timedouble = DateTimeAxis.ToDouble(time);
            if(timedouble < start.X)//if the time right now is less than the start time of the line, use the start time of the line, else override it with the current time
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
            while(timecounter < DateTimeAxis.ToDateTime(End.X) && count < 5000)
            {
                count++;
                timecounter = timecounter.AddMinutes(minutesUntilOrder);

                var nextXCorddouble = DateTimeAxis.ToDouble(timecounter);
                orderpoints.Add(new DataPoint(nextXCorddouble, CalculateLimitAtSpecificTime(nextXCorddouble)));
            }
            if(count == 5000)
            {
                new Popup("Too Many Order Points", "Currently, only 5000 orders are allowed per line.  You can shorten the line or decrease the Orders/Hour to avoid this").Show();
            }

            orderpoints.Add(last);
            limitLine.ItemsSource = orderpoints;

            return true;
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

        private bool _preserveLine;
        public bool PreserveLine {

            get 
            {
                return _preserveLine;
            }
            
            
            set
            {
                _preserveLine = value;
                OnPropertyChanged("PreserveLine");

            }
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
                if(_startstringprice != 0 && PreserveLine)
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
                if (_endstringprice != null && PreserveLine)
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
                try
                {
                    var time = DateTime.Parse(value);
                    if(_startstringprice != 0)
                    {

                        var x = DateTimeAxis.ToDouble(time);
                        Start = new DataPoint(x, _startstringprice);

                    }
                }
                catch(Exception e)
                {
                    ErrorMsg = "Invalid value for start time";
                    return;
                }

                OnPropertyChanged("StartString");
            }

        }

        private double _ful;
        public double FulfillmentThreshold
        {
            get
            {
                return _ful;
            }

            set
            {
                if (_ful == value)
                {
                    return;
                }
                _ful = value;
                OnPropertyChanged("FulfillmentThreshold");
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
                try
                {
                    var time = DateTime.Parse(value);
                    if (_endstringprice != 0)
                    {

                        var x = DateTimeAxis.ToDouble(time);
                        End = new DataPoint(x, _endstringprice);

                    }
                }
                catch (Exception e)
                {
                    ErrorMsg = "Invalid value for end time";
                    return;
                }


                OnPropertyChanged("EndString");
            }

        }

        public LimitCondig Screen { get; internal set; }
    }
}
