using Newtonsoft.Json;
using System;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CurrencyConverter
{
    public partial class MainWindow : Window
    {
        Root val = new Root();
        
        public MainWindow()
        {
            InitializeComponent();
            GetValue();
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Checking if the amount has been entered
                if (string.IsNullOrEmpty(txtAmount.Text))
                {
                    MessageBox.Show("Please enter the amount", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtAmount.Focus();
                    return;
                }

                // Checking if currencies are selected
                if (cmbFromCurrency.SelectedItem == null || 
                    cmbToCurrency.SelectedItem == null || 
                    ((ComboBoxItem)cmbFromCurrency.SelectedItem).Content.ToString() == "--SELECT--" || 
                    ((ComboBoxItem)cmbToCurrency.SelectedItem).Content.ToString() == "--SELECT--")
                {
                    MessageBox.Show("Please enter the amount", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Downloading the amount and currency codes
                double amount = double.Parse(txtAmount.Text);
                string fromCurrency = ((ComboBoxItem)cmbFromCurrency.SelectedItem).Content.ToString();
                string toCurrency = ((ComboBoxItem)cmbToCurrency.SelectedItem).Content.ToString();

                // Calculating the conversion result
                double fromRate = GetRate(fromCurrency);
                double toRate = GetRate(toCurrency);
                
                if (fromRate > 0 && toRate > 0)
                {
                    double convertedAmount = (toRate / fromRate) * amount;
                    txtResult.Text = $"{amount} {fromCurrency} = {convertedAmount:N2} {toCurrency}";
                }
                else
                {
                    MessageBox.Show("Unable to get exchange rates", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtAmount.Text = "";
            txtResult.Text = "";
            cmbFromCurrency.SelectedIndex = 0;
            cmbToCurrency.SelectedIndex = 0;
            txtAmount.Focus();
        }

        private double GetRate(string currencyCode)
        {
            switch (currencyCode)
            {
                case "USD": return val.rates?.USD ?? 1.0;
                case "EUR": return val.rates?.EUR ?? 0.0;
                case "PLN": return val.rates?.PLN ?? 0.0;
                case "GBP": return val.rates?.GBP ?? 0.0;
                case "INR": return val.rates?.INR ?? 0.0;
                case "JPY": return val.rates?.JPY ?? 0.0;
                case "NZD": return val.rates?.NZD ?? 0.0;
                case "CAD": return val.rates?.CAD ?? 0.0;
                case "ISK": return val.rates?.ISK ?? 0.0;
                case "PHP": return val.rates?.PHP ?? 0.0;
                case "DKK": return val.rates?.DKK ?? 0.0;
                case "CZK": return val.rates?.CZK ?? 0.0;
                default: return 0.0;
            }
        }

        private async void GetValue()
        {
            val = await GetData<Root>("https://openexchangerates.org/api/latest.json?app_id=de491accbb8548a7926ef9258677ab6c");
        }

        //Root class is the main class. API return rates in the rates. It returns all currency name with value.
        public class Root
        {
            //Get all record in rates and set in rate class as currency name wise
            public Model.Rate rates { get; set; }
            public long timestamp;
            public string license;
        }

        public static async Task<Root> GetData<T>(string url)
        {
            var _root = new Root();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(1);
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var ResponseString = await response.Content.ReadAsStringAsync();
                        var ResponseObject = JsonConvert.DeserializeObject<Root>(ResponseString);
                        return ResponseObject;
                    }
                    return _root;
                }
            }
            catch
            {
                return _root;
            }
        }
    }
}