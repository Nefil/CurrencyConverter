using CurrencyConverter.Model;
using System.Reflection;


namespace CurrencyConverter.Model
{
    public static class RatesExtensions
    {
        private static readonly Dictionary<string, PropertyInfo> _propMap;

        static RatesExtensions()
        {
            _propMap = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            var props = typeof(Rates).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var p in props)
            {
                // Mapujemy nazwę właściwości (np. "USD") -> PropertyInfo
                _propMap[p.Name.ToUpperInvariant()] = p;
            }
        }

        // Zwraca kurs z obiektu Rates na podstawie kodu waluty; 0.0 gdy brak
        public static double GetByCode(this Rates rates, string currencyCode)
        {
            if (rates == null || string.IsNullOrWhiteSpace(currencyCode))
                return 0.0;

            if (_propMap.TryGetValue(currencyCode.ToUpperInvariant(), out var prop))
            {
                var value = prop.GetValue(rates);
                return value is double d ? d : 0.0;
            }

            return 0.0;
        }

        // Wygodne rozszerzenie dla Root
        public static double GetRate(this Root root, string currencyCode)
        {
            return root?.rates.GetByCode(currencyCode) ?? 0.0;
        }
    }
}