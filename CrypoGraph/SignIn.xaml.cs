using Bittrex.Net;
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
    /// Interaction logic for SignIn.xaml
    /// </summary>
    public partial class SignIn : Window
    {
        public string Key;
        public string Secret;
        public SignIn()
        {
           
            InitializeComponent();

            if (Properties.Settings.Default.Secret != null)
            {
                secrettb.Text = Properties.Settings.Default.Secret;
            }
            if (Properties.Settings.Default.Key != null)
            {
                keytb.Text = Properties.Settings.Default.Key;
            }
        }
        BittrexClientV3 client = new BittrexClientV3();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                client.SetApiCredentials(keytb.Text, secrettb.Text);
                var x = client.GetAccount();
                if (x.Error != null && x.Error.Message == "Server error: APIKEY_INVALID")
                {
                    errormsg.Content = "Invalid API Key";
                    return;
                }
            }
            catch (Exception ex)
            {
                errormsg.Content = ex.Message;
                return;
            }
           

            //good key and secret.  continue
            Key = keytb.Text;
            Secret = secrettb.Text;


            this.Close();
        }
    }
}
