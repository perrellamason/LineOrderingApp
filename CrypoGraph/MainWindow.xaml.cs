using Bittrex.Net;
using Bittrex.Net.Objects;
using Bittrex.Net.Objects.V3;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CrypoGraph
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private PlotModel plotmodel;

        private Axis XAxis;
        private Axis YAxis;
        private DataPoint StartPoint;
        private DataPoint EndPoint;
        private LineSeries LimitLineSeries;
        private string CurrentLoadedSymbol;
        private CandleInterval CurrentInterval;
        private MarketSummary[] marketSummaries;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            if (name == "ActiveLineOrders")
            {
                showAllbtn_Checked(null, null);

            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private List<string> _listofcoins;
        public List<string> ListOfCoins
        {
            get
            {
                return _listofcoins;
            }
            set
            {
                _listofcoins = value;
                OnPropertyChanged("ListOfCoins");
            }
        }

        private string _balance;
        public string AccountBalance
        {
            get
            {
                return _balance;
            }
            set
            {
                _balance = value;
                OnPropertyChanged("AccountBalance");
            }
        }

       
        private decimal _currentPricehigh;
        public decimal CurrentPriceHigh
        {
            get
            {
                return _currentPricehigh;
            }
            set
            {
                _currentPricehigh = value;
                OnPropertyChanged("CurrentPriceHigh");
            }
        }
        private decimal _currentPricelow;
        public decimal CurrentPriceLow
        {
            get
            {
                return _currentPricelow;
            }
            set
            {
                _currentPricelow = value;
                OnPropertyChanged("CurrentPriceLow");
            }
        }

        private decimal _midTrade;
        public decimal CurrentMinTradeSize
        {
            get
            {
                return _midTrade;
            }
            set
            {
                _midTrade = value;
                OnPropertyChanged("CurrentMinTradeSize");
            }
        }


        private string _ActionHistory;
        public string ActionHistory
        {
            get
            {
                return _ActionHistory;
            }
            set
            {
                _ActionHistory = value;
                OnPropertyChanged("ActionHistory");
            }
        }

        private BittrexOrderBookV3 _orderbook;
        public BittrexOrderBookV3 OrderBook
        {
            get
            {
                return _orderbook;
            }
            set
            {
                _orderbook = value;
                OnPropertyChanged("OrderBook");
            }
        }


        private List<string> _intervals;
        public List<string> CandleIntervals
        {
            get
            {
                return _intervals;
            }
            set
            {
                _intervals = value;
                OnPropertyChanged("CandleIntervals");
            }
        }



              private ObservableCollection<LineOrder> _FulfilledOrders;
        public ObservableCollection<LineOrder> FulfilledOrders
        {
            get
            {
                return _FulfilledOrders;
            }
            set
            {
                _FulfilledOrders = value;

                OnPropertyChanged("FulfilledOrders");
            }
        }
        
        private ObservableCollection<LineOrder> _actives;
        public ObservableCollection<LineOrder> ActiveLineOrders
        {
            get
            {
                return _actives;
            }
            set
            {
                _actives = value;

                OnPropertyChanged("ActiveLineOrders");
            }
        }
        private ObservableCollection<LineOrder> _ActiveLineOrdersFiltered;
        public ObservableCollection<LineOrder> ActiveLineOrdersFiltered
        {
            get
            {
                return _ActiveLineOrdersFiltered;
            }
            set
            {
                _ActiveLineOrdersFiltered = value;
                OnPropertyChanged("ActiveLineOrdersFiltered");
            }
        }


        private ObservableCollection<BittrexOrderV3> _orderhistory;
        public ObservableCollection<BittrexOrderV3> ClosedOrders
        {
            get
            {
                return _orderhistory;
            }
            set
            {
                _orderhistory = value;
                OnPropertyChanged("ClosedOrders");
            }
        }

        private ObservableCollection<BittrexOrderV3> _openorders;
        public ObservableCollection<BittrexOrderV3> OpenOrders
        {
            get
            {
                return _openorders;
            }
            set
            {
                _openorders = value;
                OnPropertyChanged("OpenOrders");
            }
        }

        private ObservableCollection<UpcomingOrder> _upcoming;
        public ObservableCollection<UpcomingOrder> UpcomingOrders
        {
            get
            {
                return _upcoming;
            }
            set
            {
                _upcoming = value;
                OnPropertyChanged("UpcomingOrders");
            }
        }


        BittrexSocketClientV3 socketClient = new BittrexSocketClientV3();
        CryptoExchange.Net.Objects.WebCallResult<IEnumerable<BittrexSymbolSummary>> marketSummaries1;

        private void Authenticate()
        {
            BittrexClient.SetDefaultOptions(new BittrexClientOptions()
            {

               // ApiCredentials = new ApiCredentials("8180a6a91e29425c90c2e2afe349aa71", "e36a6307025c4b4fb1b20a4a00c4c9ef"),
                ApiCredentials = new ApiCredentials(Key, Secret),
                LogVerbosity = LogVerbosity.Info,
                LogWriters = new List<TextWriter>() { Console.Out }
            });
            BittrexClientV3.SetDefaultOptions(new BittrexClientOptions()
            {
                ApiCredentials = new ApiCredentials(Key, Secret),
               // ApiCredentials = new ApiCredentials("8180a6a91e29425c90c2e2afe349aa71", "e36a6307025c4b4fb1b20a4a00c4c9ef"),
               // ApiCredentials = new ApiCredentials("8180a6a91e29425c90c2e2afe349aa71", "e36a6307025c4b4fb1b20a4a00c4c9ef"),
                LogVerbosity = LogVerbosity.Info,
                LogWriters = new List<TextWriter>() { Console.Out }
            });
        }
        BittrexClientV3 client = new BittrexClientV3();
        public void DoBittrexNetStuff()
        {

            Authenticate();


            var subscription = socketClient.SubscribeToSymbolSummaryUpdatesAsync(summaries =>
            {
                //var update = summaries.Deltas.FirstOrDefault(l => l.Symbol == CurrentLoadedSymbol);

                //Dispatcher.Invoke(() =>
                //{
                //    LoadChart(CurrentLoadedSymbol, CurrentInterval);
                //});
            });



            var subscription3 = socketClient.SubscribeToOrderUpdatesAsync(order =>
            {
                var d = client.GetClosedOrders(CurrentLoadedSymbol);
                if (d.Data != null)
                {
                    ClosedOrders = new ObservableCollection<BittrexOrderV3>(d.Data);
                }


                var o = client.GetOpenOrders(CurrentLoadedSymbol);
                if (o.Data != null)
                {
                    OpenOrders = new ObservableCollection<BittrexOrderV3>(o.Data);
                }

                var balance = client.GetBalance(CurrentLoadedSymbol.Substring(0,3));

                if (balance.Data != null)
                {
                    AccountBalance = balance.Data.Total.ToString();
                }

              
           
            });


        }

        public double CalculateLimitAtSpecificTime(double time, LineOrder order)
        { // y-b/ m = x
            return (order.Slope * time) + order.YIntercept;
        }

        public double CalculateTimeAtSpecificPrice(double price, LineOrder order)
        {//y = mx+b
            return (price - order.YIntercept) / order.Slope;
        }



        public void MakeOrder(LineOrder order)
        {
            DateTime currenttime;
            decimal currentlimitprice;
            WebCallResult<BittrexOrderV3> linePointOrder;


            currenttime = DateTime.Now;
            var limit = CalculateLimitAtSpecificTime(DateTimeAxis.ToDouble(currenttime), order);

            //cancel the previous order if needed
            if (order.TimeInForce == TimeInForce.GoodTillCancelled)
            {
                if (order.LastPlacedOrderId != null)//cancel previous open order
                {
                    var or = client.GetOrder(order.LastPlacedOrderId);
                    if(or.Error == null)
                    {
                        if(or.Data.Status == OrderStatus.Closed)
                        {
                            order.ActiveTotal += double.Parse(or.Data.Proceeds.ToString()); //increase the active total for this order based on the previous orders proceeds
                            if (order.ActiveTotal > order.FulfilmentThreshold)
                            {
                                FulfilledOrders.Add(order);
                                ActiveLineOrders.Remove(order);
                                ActionHistory += DateTime.Now.TimeOfDay.ToString() + " - " + (order.OrderSide == OrderSide.Buy ? "Buy Line" : "Sell Line") + order.Symbol + " Fulfilled" + "\n";
                            }
                            else
                            {
                                ActiveLineOrders.Add(order);
                            }
                        }
                       
                    }

                    //cancel order
                    var res = client.CancelConditionalOrder(order.LastPlacedOrderId);
                    if (res.Error != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ActionHistory += DateTime.Now.TimeOfDay.ToString() + " - " + "Error: " + " Failed to Cancel Order" + res.Error.Message+"\n";
                        });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ActionHistory += DateTime.Now.TimeOfDay.ToString() + " - " + "Order Cancelled: ID = " +order.LastPlacedOrderId + "\n";
                        });
                    }
                }


            }

            if (limit != 0)
            {

                currentlimitprice = Convert.ToDecimal(limit);
                currentlimitprice = Convert.ToDecimal(currentlimitprice.ToString("F5"));
                //place order
                linePointOrder = client.PlaceOrder(CurrentLoadedSymbol, order.OrderSide, order.OrderType, order.TimeInForce, order.QuantityPerOrder, currentlimitprice);

                if (linePointOrder.Error != null)
                {//error occured, log it
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ActionHistory += DateTime.Now.TimeOfDay.ToString() + " - " + "Error: " + linePointOrder.Error.Message + "\n";
                    });
                  
                }
                else
                {
                    order.OrderIDs.Add(linePointOrder.Data.Id);
                    order.LastPlacedOrderId = linePointOrder.Data.Id;
                    if (linePointOrder.Data.Status == OrderStatus.Closed)
                    {
                        order.ActiveTotal += double.Parse(linePointOrder.Data.Proceeds.ToString()); //increase the active total for this order based on the previous orders proceeds
                    }
                   
                    //log the order
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ActionHistory += DateTime.Now.TimeOfDay.ToString() + " - " + (order.OrderSide == OrderSide.Buy ? "Buy" : "Sell") + order.Symbol + " at $" + currentlimitprice + "\n";
                        //new Popup("Order Placed", order.Symbol + " at $" + currentlimitprice).Show();
                        if (order.ActiveTotal > order.FulfilmentThreshold)
                        {
                            FulfilledOrders.Add(order);
                            ActiveLineOrders.Remove(order);
                            ActionHistory += DateTime.Now.TimeOfDay.ToString() + " - " + (order.OrderSide == OrderSide.Buy ? "Buy Line" : "Sell Line") + order.Symbol + " Fulfilled"+ "\n";
                        }
                        else
                        {
                            ActiveLineOrders.Add(order);
                        }
                    });

                }
            }
            else
            {
  
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ActionHistory += DateTime.Now.TimeOfDay.ToString() + " - " + "Limit price could not be calculated" + "\n";
                  
                });
                OnPropertyChanged("FulfilledOrders");

                Thread.Sleep(10000); //keep running every 10 seconds
            }
        }

        private void UpdateChart(BittrexSymbolSummaryV3 update)
        {
            var candles = (CandleStickSeries)plotmodel.Series[0];

        }
        private Thread upcomingThread;
        private Thread executeThread;

        private string Key;
        private string Secret;

        public MainWindow()
        {

            this.DataContext = this;
            this.Closed += MainWindow_Closed;
            InitializeComponent();
            SignIn signin = new SignIn();
            signin.ShowDialog();
            Key = signin.Key;
            Secret = signin.Secret;
            if (Key == null || Secret == null)
            {
                this.Close();
                return;
            }

            Properties.Settings.Default.Secret = Secret;
            Properties.Settings.Default.Key = Key;
            Properties.Settings.Default.Save();

            //if (ConfigurationManager.AppSettings.Count == 0)
            //{
            //    ConfigurationManager.AppSettings.Add("Key", Key);
            //    ConfigurationManager.AppSettings.Add("Secret", Secret);
            //}
            //else
            //{
            //    ConfigurationManager.AppSettings["Key"] = Key;
            //    ConfigurationManager.AppSettings["Secret"] = Secret;
            //}

            ActionHistory = "";
            Authenticate();
            DoBittrexNetStuff();

            var market24hoursummary = CallRestMethod("https://api.bittrex.com/v3/markets/summaries");
            marketSummaries = JsonConvert.DeserializeObject<MarketSummary[]>(market24hoursummary);

            ActiveLineOrders = new ObservableCollection<LineOrder>();
            FulfilledOrders = new ObservableCollection<LineOrder>();
            ActiveLineOrdersFiltered = new ObservableCollection<LineOrder>();
            UpcomingOrders = new ObservableCollection<UpcomingOrder>();
            ListOfCoins = new List<string>();
            CandleIntervals = new List<string>();

            CandleIntervals.Add("1 Minute");
            CandleIntervals.Add("5 Minutes");
            CandleIntervals.Add("1 Hour");
            CandleIntervals.Add("1 Day");


            foreach (MarketSummary coinsummary in marketSummaries)
            {
                ListOfCoins.Add(coinsummary.Symbol);
            }
            ListOfCoins = ListOfCoins;

            LoadChart("BTC-USD", CandleInterval.Hour1);

            StartLookingForUpcomingOrders();
            StartLookingToExecuteOrders();
            StartUpdatingGraph();

        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            socketClient.UnsubscribeAll();
            ExecuteOrdersThreadStopped = true;
            UpcomingOrdersThreadStopped = true;
            UpdateGraphThreadStopped = true;
            Thread.Sleep(500);
        }

        private void LoadChart(string symbol, CandleInterval interval)
        {
            CurrentLoadedSymbol = symbol;
            CurrentInterval = interval;
            //client.SetApiCredentials("118180a6a91e29425c90c2e2afe349aa71", "e36a6307025c4b4fb1b20a4a00c4c9ef");
            client.SetApiCredentials(Key, Secret);


            //get min trade size 
            var markets = client.GetSymbols();
            if (markets.Data != null)
            {
                var market = markets.Data.FirstOrDefault(l => l.Symbol == symbol);
                CurrentMinTradeSize = market.MinTradeSize;

            }


            //get high low data
            var summary = client.GetSymbolSummary(symbol);
            if (summary.Data != null)
            {
                CurrentPriceHigh = summary.Data.High;
                CurrentPriceLow = summary.Data.Low;

            }

            //get balance for currency
            var balance = client.GetBalance(symbol.Substring(0, 3));
            if (balance.Data != null)
            {
                AccountBalance = balance.Data.Available.ToString() + " " + balance.Data.Currency;
            }

            //get order history
            var orderhistory = client.GetOrderBook(symbol);
            if (orderhistory.Data != null)
            {
                OrderBook = orderhistory.Data;
            }

            //get closed orders
            var d = client.GetClosedOrders(symbol);
            if (d.Data != null)
            {
                ClosedOrders = new ObservableCollection<BittrexOrderV3>(d.Data);
            }

            //get open orders
            var o = client.GetOpenOrders(symbol);
            if (o.Data != null)
            {
                OpenOrders = new ObservableCollection<BittrexOrderV3>(o.Data);
            }

            //var balance = client.GetBalance(CurrentLoadedSymbol);
            //if (balance.Data != null)
            //{
            //    AccountBalance = balance.Data.Total.ToString();
            //}
            //else
            //{
            //    AccountBalance = "0.00";
            //}

            string intervalstring = "";
            if (interval == CandleInterval.Day1)
            {
                intervalstring = "DAY_1";
            }
            else if (interval == CandleInterval.Hour1)
            {
                intervalstring = "HOUR_1";

            }
            else if (interval == CandleInterval.Minute1)
            {
                intervalstring = "MINUTE_1";

            }
            else if (interval == CandleInterval.Minutes5)
            {
                intervalstring = "MINUTE_5";

            }

            var coincandledatajson = CallRestMethod("https://api.bittrex.com/v3/markets/" + symbol + "/candles/" + intervalstring + "/recent");
            var coincandledata = JsonConvert.DeserializeObject<CandleData[]>(coincandledatajson);

            coinsCb.SelectedItem = symbol; //defualt


            foreach (CandleData candle in coincandledata)
            {
                candle.Time = candle.StartsAt.DateTime;
            }

            if (plotmodel != null)
            {
                plotmodel.Series.Clear();
            }

            OxyPlot.PlotModel model = new OxyPlot.PlotModel();
            //x
            model.Axes.Add(new DateTimeAxis
            {

                //StringFormat = "hh:mm",
                Title = "Time",
                AxislineColor = OxyColors.White,
                TitleColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                TextColor = OxyColors.White,
                MinorIntervalType = DateTimeIntervalType.Auto,

                Position = AxisPosition.Bottom,

            });
            XAxis = model.Axes[0];

            //y
            model.Axes.Add(new LinearAxis()
            {
                Title = "Market Price",

                Position = AxisPosition.Left,
                AxislineColor = OxyColors.White,
                TitleColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                TextColor = OxyColors.White,
            });
            YAxis = model.Axes[1];


            //create plot model and add the line series
            CandleStickSeries data = new CandleStickSeries() { Title = symbol };
            data.DataFieldClose = "Close";
            data.DataFieldHigh = "High";
            data.DataFieldLow = "Low";
            data.DataFieldOpen = "Open";
            data.DataFieldX = "Time";
            data.Color = OxyColors.DarkGray;
            data.IncreasingColor = OxyColors.Green;
            data.DecreasingColor = OxyColors.Red;
            data.ItemsSource = coincandledata;
            data.TrackerFormatString = "Date: {2}\nOpen: {5:0.00000}\nHigh: {3:0.00000}\nLow: {4:0.00000}\nClose: {6:0.00000}";




            plotmodel = model;
            model.PlotAreaBorderColor = OxyColors.White;
            model.LegendTextColor = OxyColors.YellowGreen;
            model.Series.Add(data);
            model.Background = OxyColors.Black;
            model.MouseUp += Model_MouseUp; ;
            model.MouseDown += Model_MouseDown; ;

            plotview.Model = model;

            if (LimitLineSeries != null)
            {
                DrawLimitLine(OrderSide.Buy, LimitLineSeries);
            }
        }
        bool drawingline;
        private void Model_MouseDown(object sender, OxyMouseDownEventArgs e)
        {
            if (e.IsControlDown || drawingline)
            {
                var startpos = e.Position;
                drawingline = true;
                var datapoint = Axis.InverseTransform(startpos, XAxis, YAxis);
                StartPoint = datapoint;
            }

        }

        private void Model_MouseUp(object sender, OxyMouseEventArgs e)
        {
            if (e.IsControlDown || drawingline)
            {
                var endpos = e.Position;
                var datapoint = Axis.InverseTransform(endpos, XAxis, YAxis);
                EndPoint = datapoint;
                drawingline = false;
                Mouse.OverrideCursor = Cursors.Arrow;
                DrawLimitLine(OrderSide.Buy);
                ShowLimitConfigWindow();

            }

        }



        public void StartLookingToExecuteOrders()
        {
            new Thread(() =>
            {
                while (!ExecuteOrdersThreadStopped)
                {
                    var timedouble = DateTimeAxis.ToDouble(DateTime.Now);
                    var currettime = DateTime.Now;
                    var errorbefore = currettime.AddSeconds(50);
                    var errorafter = currettime.AddSeconds(-50);
                    lock (UpcomingOrders)
                    {
                        foreach (UpcomingOrder order in UpcomingOrders)
                        {//if the current time 30 seconds from now is greater than the upcoming order  time, make the order if it hasnt been made already
                            if (currettime.AddSeconds(10).CompareTo(order.Time) > 0)
                            {
                                if (!order.StatusCompleted)
                                {
                                    order.StatusCompleted = true;
                                    OnPropertyChanged("UpcomingOrders");
                                    MakeOrder(order.Order);

                                }
                            }

                        }
                    }

                }


            }).Start();
        }


        private void UpdateOrders()
        {
            var d = client.GetClosedOrders(CurrentLoadedSymbol);
            if (d.Data != null)
            {
                ClosedOrders = new ObservableCollection<BittrexOrderV3>(d.Data);
            }


            var o = client.GetOpenOrders(CurrentLoadedSymbol);
            if (o.Data != null)
            {
                OpenOrders = new ObservableCollection<BittrexOrderV3>(o.Data);
            }

            var balance = client.GetBalance(CurrentLoadedSymbol);

            if (balance.Data != null)
            {
                AccountBalance = balance.Data.Available.ToString() + " " + balance.Data.Currency;
            }
            
        }
        private bool UpcomingOrdersThreadStopped = false;
        private bool ExecuteOrdersThreadStopped = false;
        private bool UpdateGraphThreadStopped = false;

        public void StartUpdatingGraph()
        {
            new Thread(() =>
            {

                int count = 0;
                while (!UpdateGraphThreadStopped)
                {
                    if (count == 12000)//2 min
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Cursor = Cursors.Wait;
                            LoadChart(CurrentLoadedSymbol, CurrentInterval);
                            Cursor = Cursors.Arrow;
                        });


                    }
                    Thread.Sleep(10);
                    count++;
                }

            }).Start();

        }

        public void StartLookingForUpcomingOrders()
        {
            executeThread = new Thread(() =>
            {
                int count = 500;
                while (!UpcomingOrdersThreadStopped)
                {
                    if (count == 500)//wait 5seconds
                    {
                        UpdateOrders();

                        lock (UpcomingOrders)
                        {
                            //remove any past due upcoming orders from the list
                            for (int i = 0; i < UpcomingOrders.Count; i++)
                            {
                                if (UpcomingOrders[i].Time.CompareTo(DateTime.Now) < 0 || UpcomingOrders[i].StatusCompleted)
                                {
                                    //order time has  passed.  it should have  been attempted or ordered
                                    Dispatcher.Invoke(() =>
                                    {
                                        UpcomingOrders.RemoveAt(i);
                                        OnPropertyChanged("UpcomingOrders");

                                    });
                                }
                            }



                            count = 0;
                            var timedouble = DateTimeAxis.ToDouble(DateTime.Now);
                            foreach (LineOrder order in ActiveLineOrders)
                            {
                                DataPoint nextorderPoint = new DataPoint(0, 0);
                                var points = (List<DataPoint>)(order.LimitLineSeries.ItemsSource);
                                for (int i = 0; i < points.Count; i++)
                                {
                                    if (points[i].X < timedouble)
                                    {
                                        //nextorderPoint = points[i];
                                    }
                                    else
                                    {
                                        nextorderPoint = points[i];
                                        break; //use previous data point
                                    }
                                }
                                if (nextorderPoint.X != 0 && nextorderPoint.Y != 0)
                                {
                                    var next = new UpcomingOrder() { Order = order, Price = nextorderPoint.Y.ToString(), Side = (order.OrderSide == OrderSide.Buy ? "Buy" : "Sell"), Time = DateTimeAxis.ToDateTime(nextorderPoint.X), TimeDisplay = DateTimeAxis.ToDateTime(nextorderPoint.X).ToString() };

                                    if (!UpcomingOrders.Any(l => l.Time == next.Time && l.Price == next.Price))
                                    {
                                        Dispatcher.Invoke(() =>
                                        {
                                            UpcomingOrders.Add(next);
                                            OnPropertyChanged("UpcomingOrders");

                                        });
                                    }
                                    else
                                    {
                                        //already exists
                                    }


                                }
                                else
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        new Popup("Error Finding Next Order", "Could not determine next Order Point").Show();
                                    });
                                }
                            }
                        }

                    }

                    Thread.Sleep(10);
                    count++;

                }

            });
            executeThread.Start();

        }

        private void OverrideAddingUpcoming()
        {
            lock (UpcomingOrders)
            {
                //remove any past due upcoming orders from the list
                for (int i = 0; i < UpcomingOrders.Count; i++)
                {
                    if (UpcomingOrders[i].Time.CompareTo(DateTime.Now) < 0)
                    {
                        //order time has  passed.  it should have  been attempted or ordered
                        Dispatcher.Invoke(() =>
                        {
                            UpcomingOrders.RemoveAt(i);
                            OnPropertyChanged("UpcomingOrders");

                        });
                    }
                }



                var timedouble = DateTimeAxis.ToDouble(DateTime.Now);
                foreach (LineOrder order in ActiveLineOrders)
                {
                    DataPoint nextorderPoint = new DataPoint(0, 0);
                    var points = (List<DataPoint>)(order.LimitLineSeries.ItemsSource);
                    for (int i = 0; i < points.Count; i++)
                    {
                        if (points[i].X < timedouble)
                        {
                            //nextorderPoint = points[i];
                        }
                        else
                        {
                            nextorderPoint = points[i];
                            break; //use previous data point
                        }
                    }
                    if (nextorderPoint.X != 0 && nextorderPoint.Y != 0)
                    {
                        var next = new UpcomingOrder() { Order = order, Price = nextorderPoint.Y.ToString(), Side = (order.OrderSide == OrderSide.Buy ? "Buy" : "Sell"), Time = DateTimeAxis.ToDateTime(nextorderPoint.X), TimeDisplay = DateTimeAxis.ToDateTime(nextorderPoint.X).ToString() };

                        if (!UpcomingOrders.Any(l => l.Time == next.Time && l.Price == next.Price))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                UpcomingOrders.Add(next);
                                OnPropertyChanged("UpcomingOrders");

                            });
                        }
                        else
                        {
                            //already exists
                        }


                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ActionHistory += "Could not determine next Order Point" + "\n";
                            new Popup("Error Finding Next Order", "Could not determine next Order Point").Show();
                        });
                    }
                }
            }
        }

        private void ShowLimitConfigWindow()
        {
            LimitConfigViewModel limitVM = new LimitConfigViewModel(StartPoint, EndPoint, LimitLineSeries, CurrentLoadedSymbol, CurrentMinTradeSize);
            LimitCondig lc = new LimitCondig(limitVM);
            lc.ShowDialog();
            if (limitVM.LineOrder != null)
            {
                ActiveLineOrders.Insert(0, limitVM.LineOrder);
                OverrideAddingUpcoming();
                LimitLineSeries = limitVM.LineOrder.LimitLineSeries;
                plotmodel.InvalidatePlot(true);

                OnPropertyChanged("ActiveLineOrders");
            }
            else
            {
                if (plotmodel.Series.Any(l => l.Title == "Limit Line"))
                {
                    var match = plotmodel.Series.First(l => l.Title == "Limit Line");
                    plotmodel.Series.Remove(LimitLineSeries);
                }
                LimitLineSeries = null;
            }
        }


        private void RemoveLimitLine()
        {
            if (plotmodel.Series.Any(l => l.Title == "Limit Line"))
            {
                while (plotmodel.Series.FirstOrDefault(l => l.Title == "Limit Line") != null)
                {
                    var match = plotmodel.Series.First(l => l.Title == "Limit Line");
                    plotmodel.Series.Remove(match);
                    plotmodel.InvalidatePlot(true);
                }


            }
        }

        private void DrawLimitLine(OrderSide side, LineSeries line = null)
        {
            //remove rpreivoous limit line if exists
            RemoveLimitLine();


            if (line == null)
            {
                //add new limit line
                var listofpoints = new List<DataPoint>() { StartPoint, EndPoint };
                var limitlineseries = new LineSeries();
                limitlineseries.Title = "Limit Line";
                limitlineseries.Color = OxyColors.Magenta;
                limitlineseries.ItemsSource = listofpoints;
                limitlineseries.MarkerFill = OxyColors.Gold;
                limitlineseries.MarkerType = MarkerType.Circle;
                limitlineseries.MarkerSize = 3;
                plotmodel.Series.Add(limitlineseries);
                plotmodel.InvalidatePlot(true);
                LimitLineSeries = limitlineseries;
            }
            else
            {
                if (!plotmodel.Series.Contains(line))
                {
                    plotmodel.Series.Add(line);
                    plotmodel.InvalidatePlot(true);
                    LimitLineSeries = line;
                }

            }


        }

        //public async Task<List<SocketResponse>> Subscribe(string[] channels)
        //{
        //    return await _hubProxy.Invoke<List<SocketResponse>>("Subscribe", (object)channels);
        //}

        public string CallRestMethod(string url)
        {
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
            webrequest.Method = "GET";
            webrequest.ContentType = "application/x-www-form-urlencoded";
            webrequest.Headers.Add("Username", "xyz");
            webrequest.Headers.Add("Password", "abc");
            HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
            Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader responseStream = new StreamReader(webresponse.GetResponseStream(), enc);
            string result = string.Empty;
            result = responseStream.ReadToEnd();
            webresponse.Close();
            return result;
        }

        private void coinsCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (coinsCb.SelectedItem != null && coinsCb.SelectedItem != CurrentLoadedSymbol)
            {
                LoadChart(coinsCb.SelectedItem.ToString(), CandleInterval.Hour1);

            }
        }


        private void intervalCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (intervalCb.SelectedItem != null)
            {

                string selectedstring = (string)intervalCb.SelectedItem;
                CandleInterval intervalToUse = CandleInterval.Day1;

                if (selectedstring == "1 Minute")
                {
                    intervalToUse = CandleInterval.Minute1;
                }
                else if (selectedstring == "5 Minutes")
                {
                    intervalToUse = CandleInterval.Minutes5;

                }
                else if (selectedstring == "1 Hour")
                {
                    intervalToUse = CandleInterval.Hour1;

                }
                else if (selectedstring == "1 Day")
                {
                    intervalToUse = CandleInterval.Day1;

                }




                LoadChart(coinsCb.SelectedItem.ToString(), intervalToUse);

            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (LineOrder)activeordersLB.SelectedItem;
            if (item == null)
                return;
            if (CurrentLoadedSymbol != item.Symbol)
            {

                LoadChart(item.Symbol, CurrentInterval);

            }


            DrawLimitLine(item.OrderSide, item.LimitLineSeries);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (activeordersLB.SelectedItem != null)
            {
                CancelLineOrder();
            }
        }

        private void CancelLineOrder()
        {
            var orderToRemove = (LineOrder)activeordersLB.SelectedItem;
            RemoveLimitLine();
            ActiveLineOrders.Remove(orderToRemove);
            if (UpcomingOrders.FirstOrDefault(l => l.Order.LimitLineSeries == orderToRemove.LimitLineSeries) != null)
            {
                UpcomingOrders.Remove(UpcomingOrders.FirstOrDefault(l => l.Order.LimitLineSeries == orderToRemove.LimitLineSeries));
                OnPropertyChanged("UpcomingOrders");
            }

            OnPropertyChanged("ActiveLineOrders");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!drawingline)
            {
                drawingline = true;
                Mouse.OverrideCursor = Cursors.Cross;
                AddCancelBtn.Content = "Cancel";
            }
            else
            {
                drawingline = false;
                AddCancelBtn.Content = "Add";
                Mouse.OverrideCursor = Cursors.Arrow;
            }

        }

        private void showAllbtn_Checked(object sender, RoutedEventArgs e)
        {

            if ((bool)showAllbtn.IsChecked)
            {
                if (ActiveLineOrders != null)
                {
                    ActiveLineOrdersFiltered = new ObservableCollection<LineOrder>(ActiveLineOrders);

                }

            }
            else if ((bool)buysonlybtn.IsChecked)
            {
                ActiveLineOrdersFiltered = new ObservableCollection<LineOrder>();

                foreach (LineOrder order in ActiveLineOrders)
                {
                    if (order.OrderSide == OrderSide.Buy)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ActiveLineOrdersFiltered.Add(order);
                        });

                    }
                }
            }
            else if ((bool)sellsonlybtn.IsChecked)
            {
                ActiveLineOrdersFiltered = new ObservableCollection<LineOrder>();

                foreach (LineOrder order in ActiveLineOrders)
                {
                    if (order.OrderSide == OrderSide.Sell)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ActiveLineOrdersFiltered.Add(order);
                        });

                    }
                }
            }
            else
            {
                ActiveLineOrdersFiltered = new ObservableCollection<LineOrder>(ActiveLineOrders);

            }
            OnPropertyChanged("ActiveLineOrdersFiltered");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ShowAllLimitLines();
        }

        private void ShowAllLimitLines()
        {
            foreach (LineOrder order in ActiveLineOrders)
            {
                if (!plotmodel.Series.Contains(order.LimitLineSeries))
                {
                    plotmodel.Series.Add(order.LimitLineSeries);
                    plotmodel.InvalidatePlot(true);
                }
            }
        }
    }

    enum CandleInterval
    {
        Minute1,
        Minutes5,
        Hour1,
        Day1,

    }

    public partial class CandleData
    {
        [JsonProperty("startsAt")]
        public DateTimeOffset StartsAt { get; set; }

        [JsonProperty("open")]
        public string Open { get; set; }

        [JsonProperty("high")]
        public string High { get; set; }

        [JsonProperty("low")]
        public string Low { get; set; }

        [JsonProperty("close")]
        public string Close { get; set; }

        [JsonProperty("volume")]
        public string Volume { get; set; }

        [JsonProperty("quoteVolume")]
        public string QuoteVolume { get; set; }

        public DateTime Time { get; set; }
    }

    public partial class MarketSummary
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("high")]
        public string High { get; set; }

        [JsonProperty("low")]
        public string Low { get; set; }

        [JsonProperty("volume")]
        public string Volume { get; set; }

        [JsonProperty("quoteVolume")]
        public string QuoteVolume { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("percentChange", NullValueHandling = NullValueHandling.Ignore)]
        public string PercentChange { get; set; }
    }

    public class CoinSummaryData
    {
        public string symbol { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double volume { get; set; }
        public double quoteVolume { get; set; }
        public DateTime updatedAt { get; set; }
        public double percentChange { get; set; }

    }

    public class SummaryRoot
    {
        public List<CoinSummaryData> Data { get; set; }

    }


    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Result
    {
        public string MarketCurrency { get; set; }
        public string BaseCurrency { get; set; }
        public string MarketCurrencyLong { get; set; }
        public string BaseCurrencyLong { get; set; }
        public double MinTradeSize { get; set; }
        public string MarketName { get; set; }
        public bool IsActive { get; set; }
        public bool IsRestricted { get; set; }
        public DateTime Created { get; set; }
        public string Notice { get; set; }
        public bool? IsSponsored { get; set; }
        public string LogoUrl { get; set; }

    }

    public class Root
    {
        public bool success { get; set; }
        public string message { get; set; }
        public List<Result> result { get; set; }

    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class TickResult
    {
        public double Bid { get; set; }
        public double Ask { get; set; }
        public double Last { get; set; }

    }

    public class TickRoot
    {
        public bool success { get; set; }
        public string message { get; set; }
        public TickResult result { get; set; }

    }




}
