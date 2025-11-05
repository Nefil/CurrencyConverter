using System;
using System.Globalization;

namespace CurrencyConverter
{
    public static class ConversionLogic
    {
        // Parses numbers using invariant culture (dot as decimal separator)
        public static bool TryParseAmount(string input, out double result)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                result = 0;
                return false;
            }

            return double.TryParse(input, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out result);
        }

        // Throws DivideByZeroException when fromRate == 0 (tests expect this behavior)
        public static double CalculateConvertedAmount(double amount, double fromRate, double toRate)
        {
            if (fromRate == 0.0)
                throw new DivideByZeroException("fromRate cannot be zero.");

            return (toRate / fromRate) * amount;
        }

        // Simple formatter used by tests: rounds to 2 decimal places (invariant culture)
        public static string FormatResult(double amount, string fromCurrency, double convertedAmount, string toCurrency)
        {
            return $"{amount} {fromCurrency} = {convertedAmount.ToString("N2", CultureInfo.InvariantCulture)} {toCurrency}";
        }
    }
}