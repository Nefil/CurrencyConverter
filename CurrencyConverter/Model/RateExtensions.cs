using System;
using System.Collections.Generic;
using System.Reflection;

namespace CurrencyConverter.Model
{
    public static class RatesExtensions
    {
        // Property map (case-insensitive) for Rates properties (USD, EUR, ...)
        private static readonly Dictionary<string, PropertyInfo> _propMap;

        static RatesExtensions()
        {
            _propMap = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            var props = typeof(Rates).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var p in props)
            {
                // Map property name (e.g. "USD") -> PropertyInfo
                _propMap[p.Name] = p;
            }
        }

        // Returns the exchange rate from Rates by currency code; returns 0.0 when missing
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

        // Convenient extension for Root to get a rate
        public static double GetRate(this Root root, string currencyCode)
        {
            return root?.rates.GetByCode(currencyCode) ?? 0.0;
        }
    }
}