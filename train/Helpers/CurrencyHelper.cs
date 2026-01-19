using System.Globalization;

namespace train.Helpers
{
    public static class CurrencyHelper
    {
        /// <summary>
        /// Formats a decimal value as Pakistani Rupees (PKR)
        /// </summary>
        public static string FormatPKR(this decimal amount)
        {
            return $"PKR {amount:N0}";
        }

        /// <summary>
        /// Formats a decimal value as Pakistani Rupees (PKR) with commas
        /// </summary>
        public static string ToPKR(this decimal amount)
        {
            var culture = new CultureInfo("en-PK");
            return amount.ToString("C0", culture).Replace("Rs", "PKR");
        }
    }
}
