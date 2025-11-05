using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using CurrencyConverter.Model;
using CurrencyConverter.Data;
using System.Windows.Media;

namespace CurrencyConverter
{
    public partial class MainWindow : Window
    {
        private Root val = new Root();
        private bool isDarkTheme = false;

        public MainWindow()
        {
            InitializeComponent();
            GetValue();
        }

        // Convert button click handler
        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate amount input
                if (string.IsNullOrEmpty(txtAmount.Text))
                {
                    MessageBox.Show("Please enter an amount", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtAmount.Focus();
                    return;
                }

                // Validate currency selection
                if (cmbFromCurrency.SelectedItem == null ||
                    cmbToCurrency.SelectedItem == null ||
                    ((ComboBoxItem)cmbFromCurrency.SelectedItem).Content.ToString() == "--SELECT--" ||
                    ((ComboBoxItem)cmbToCurrency.SelectedItem).Content.ToString() == "--SELECT--")
                {
                    MessageBox.Show("Please select both currencies", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Parse amount using conversion logic (culture invariant)
                if (!ConversionLogic.TryParseAmount(txtAmount.Text, out double amount))
                {
                    MessageBox.Show("Invalid amount format", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtAmount.Focus();
                    return;
                }

                string fromCurrency = ((ComboBoxItem)cmbFromCurrency.SelectedItem).Content.ToString();
                string toCurrency = ((ComboBoxItem)cmbToCurrency.SelectedItem).Content.ToString();

                double fromRate = GetRate(fromCurrency);
                double toRate = GetRate(toCurrency);

                if (fromRate > 0 && toRate > 0)
                {
                    double convertedAmount = ConversionLogic.CalculateConvertedAmount(amount, fromRate, toRate);
                    txtResult.Text = ConversionLogic.FormatResult(amount, fromCurrency, convertedAmount, toCurrency);
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

        // Clear button handler
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtAmount.Text = "";
            txtResult.Text = "";
            cmbFromCurrency.SelectedIndex = 0;
            cmbToCurrency.SelectedIndex = 0;
            txtAmount.Focus();
        }

        // Gets a rate from database, falls back to cached Root if DB fails
        private double GetRate(string currencyCode)
        {
            try
            {
                using (var db = new ConverterDbContext("Data Source=ConverterDB.db"))
                {
                    // Ensure DB file and tables exist (creates schema if missing)
                    db.Database.EnsureCreated();

                    // Get exchange rate from database for the given currency code
                    var rate = db.Rates.FirstOrDefault(r => r.CurrencyCode == currencyCode);

                    // If rate is found, return its value
                    if (rate != null)
                    {
                        return rate.ExchangeRate;
                    }

                    // If USD is not found in database, return 1.0 (base currency)
                    if (currencyCode == "USD")
                    {
                        return 1.0;
                    }

                    // Otherwise, return 0.0
                    return 0.0;
                }
            }
            catch (Exception ex)
            {
                // In case of database connection error, fallback to cached values
                MessageBox.Show($"Database access error: {ex.Message}\nUsing cached values instead.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Preserve previous behavior for USD defaulting to 1.0 when missing
                if (currencyCode == "USD")
                    return val?.rates?.USD ?? 1.0;

                // Use extension method to read from cached 'val' Root
                return val.GetRate(currencyCode);
            }
        }

        // Fetches latest rates from API and updates local DB
        private async void GetValue()
        {
            val = await GetData("https://openexchangerates.org/api/latest.json?app_id=de491accbb8548a7926ef9258677ab6c");

            if (val?.rates != null)
            {
                try
                {
                    using (var db = new ConverterDbContext("Data Source=ConverterDB.db"))
                    {
                        // Ensure DB file and tables exist (creates schema if missing)
                        db.Database.EnsureCreated();

                        // Remove old records if any
                        if (await db.Rates.AnyAsync())
                        {
                            db.Rates.RemoveRange(db.Rates);
                            await db.SaveChangesAsync();
                        }

                        // Add new data to Rates table
                        db.Rates.AddRange(new[]
                        {
                            new Rate { CurrencyCode = "USD", ExchangeRate = val.rates.USD, Timestamp = DateTime.Now },
                            new Rate { CurrencyCode = "EUR", ExchangeRate = val.rates.EUR, Timestamp = DateTime.Now },
                            new Rate { CurrencyCode = "PLN", ExchangeRate = val.rates.PLN, Timestamp = DateTime.Now },
                            new Rate { CurrencyCode = "GBP", ExchangeRate = val.rates.GBP, Timestamp = DateTime.Now },
                            new Rate { CurrencyCode = "INR", ExchangeRate = val.rates.INR, Timestamp = DateTime.Now },
                            new Rate { CurrencyCode = "JPY", ExchangeRate = val.rates.JPY, Timestamp = DateTime.Now },
                            new Rate { CurrencyCode = "NZD", ExchangeRate = val.rates.NZD, Timestamp = DateTime.Now },
                            new Rate { CurrencyCode = "CAD", ExchangeRate = val.rates.CAD, Timestamp = DateTime.Now },
                            new Rate { CurrencyCode = "ISK", ExchangeRate = val.rates.ISK, Timestamp = DateTime.Now },
                            new Rate { CurrencyCode = "PHP", ExchangeRate = val.rates.PHP, Timestamp = DateTime.Now },
                            new Rate { CurrencyCode = "DKK", ExchangeRate = val.rates.DKK, Timestamp = DateTime.Now },
                            new Rate { CurrencyCode = "CZK", ExchangeRate = val.rates.CZK, Timestamp = DateTime.Now }
                        });

                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error while updating database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Failed to fetch data from API.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Fetch JSON and deserialize into Root
        public static async Task<Root> GetData(string url)
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
                        var responseString = await response.Content.ReadAsStringAsync();
                        var responseObject = JsonConvert.DeserializeObject<Root>(responseString);
                        return responseObject;
                    }
                    return _root;
                }
            }
            catch
            {
                return _root;
            }
        }

        // Theme toggle handler
        private void btnThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            // Toggle between themes
            isDarkTheme = !isDarkTheme;

            // Get references to existing resources
            SolidColorBrush backgroundBrush = (SolidColorBrush)Resources["BackgroundColor"];
            SolidColorBrush primaryBrush = (SolidColorBrush)Resources["PrimaryColor"];
            SolidColorBrush textBrush = (SolidColorBrush)Resources["TextColor"];

            if (isDarkTheme)
            {
                // Enable dark theme
                btnThemeToggle.Content = "☀️";
                backgroundBrush.Color = (Color)ColorConverter.ConvertFromString("#1E1E1E");
                primaryBrush.Color = (Color)ColorConverter.ConvertFromString("#9C27B0");
                textBrush.Color = Colors.White;
            }
            else
            {
                // Enable light theme
                btnThemeToggle.Content = "🌙";
                backgroundBrush.Color = Colors.White;
                primaryBrush.Color = (Color)ColorConverter.ConvertFromString("#E91E63");
                textBrush.Color = Colors.Black;
            }
        }
    }
}