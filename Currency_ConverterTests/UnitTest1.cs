using System;
using Xunit;
using CurrencyConverter;
using CurrencyConverter.Model;

namespace Currency_ConverterTests
{
    public class ConversionLogicTests
    {
        [Theory]
        [InlineData("123.45", 123.45)]
        [InlineData("0", 0)]
        [InlineData("-1.5", -1.5)]
        public void TryParseAmount_ValidStrings_ReturnsTrue(string input, double expected)
        {
            bool ok = ConversionLogic.TryParseAmount(input, out double result);
            Assert.True(ok);
            Assert.Equal(expected, result, 10);
        }

        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        public void TryParseAmount_InvalidStrings_ReturnsFalse(string input)
        {
            bool ok = ConversionLogic.TryParseAmount(input, out double result);
            Assert.False(ok);
        }

        [Fact]
        public void CalculateConvertedAmount_NormalCase_ReturnsExpected()
        {
            double amount = 100;
            double fromRate = 2.0;
            double toRate = 1.0;
            double expected = (toRate / fromRate) * amount;
            double actual = ConversionLogic.CalculateConvertedAmount(amount, fromRate, toRate);
            Assert.Equal(expected, actual, 10);
        }

        [Fact]
        public void CalculateConvertedAmount_FromRateZero_ThrowsDivideByZeroException()
        {
            Assert.Throws<DivideByZeroException>(() => ConversionLogic.CalculateConvertedAmount(100, 0, 1));
        }

        [Fact]
        public void FormatResult_TrimsAndRounds()
        {
            string formatted = ConversionLogic.FormatResult(10, "USD", 42.1299, "EUR");
            Assert.Contains("10 USD", formatted);
            Assert.Contains("EUR", formatted);
            Assert.Contains("42.13", formatted);
        }

        [Fact]
        public void RateFallback_GetFromRoot_ReturnsKnownAndUnknown()
        {
            var root = new Root
            {
                rates = new Rates
                {
                    USD = 1.0,
                    EUR = 0.9,
                    PLN = 4.3
                }
            };

            Assert.Equal(1.0, root.GetRate("USD"), 10);
            Assert.Equal(0.9, root.GetRate("EUR"), 10);
            Assert.Equal(4.3, root.GetRate("PLN"), 10);
            Assert.Equal(0.0, root.GetRate("XXX"), 10);
        }

        [Fact]
        public void RateFallback_NullRoot_ReturnsZero()
        {
            Assert.Equal(0.0, ((Root?)null).GetRate("USD"), 10);
        }
    }
}