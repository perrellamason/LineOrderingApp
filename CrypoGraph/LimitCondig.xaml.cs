using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CrypoGraph
{
    /// <summary>
    /// Interaction logic for LimitCondig.xaml
    /// </summary>
    public partial class LimitCondig : Window
    {
        private LimitConfigViewModel VM;
        public LimitCondig(LimitConfigViewModel vm)
        {

            VM = vm;
            this.DataContext = vm;
            InitializeComponent();
            VM.Screen = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var choice = (ComboBoxItem)ordertypecb.SelectedValue;
            switch (choice.Content)
            {
                case "Limit":
                    VM.OrderType = Bittrex.Net.Objects.OrderTypeV3.Limit;
                    break;
                case "Market":
                    VM.OrderType = Bittrex.Net.Objects.OrderTypeV3.Market;
                    break;
                case "CeilingLimit":
                    VM.OrderType = Bittrex.Net.Objects.OrderTypeV3.CeilingLimit;
                    break;
                case "CeilingMarket":
                    VM.OrderType = Bittrex.Net.Objects.OrderTypeV3.CeilingMarket;
                    break;
            }
        }

        private void timeinforce_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var choice = (ComboBoxItem)timeinforce.SelectedValue;
            switch (choice.Content)
            {
                case "Fill or Kill":
                    VM.TimeInForce = Bittrex.Net.Objects.TimeInForce.FillOrKill;
                    break;
                case "Good Till Cancelled":
                    VM.TimeInForce = Bittrex.Net.Objects.TimeInForce.GoodTillCancelled;
                    break;
                case "Immediate or Cancel":
                    VM.TimeInForce = Bittrex.Net.Objects.TimeInForce.ImmediateOrCancel;
                    break;
                
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //VM.OrderPlacedEvent.WaitOne();
            //if(VM.LineOrder != null)
            //{
            //    this.Close();
            //}
            //VM.OrderPlacedEvent.Reset();
        }

        private void limitOrdersPerHour_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VM.OrdersPerHour = (double)limitOrdersPerHour.SelectedValue;
        }
    }
}
