using System;
using System.Globalization;

namespace SD.Payex2.Utilities
{
    internal static class PayExHelpers
    {
        /// <summary>
        /// Converts an amount to be used in the web service.
        /// "Price must be submitted in the lowest monetary unit of the selected currency. Example: 1200 = 12.00 NOK. "
        /// </summary>
        /// <param name="amount">Amount to be converted</param>
        public static int ToPayEx(this decimal amount)
        {
            return (int)Math.Round(amount * 100M);
        }

        public static string ToPayEx(this DateTime value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static DateTime DateFromPayEx(this string value)
        {
            return DateTime.ParseExact(value, "yyyyMMdd", CultureInfo.CurrentCulture);
        }

        public static string ToPayEx(this DateTime? value)
        {
            if (value.HasValue)
                return value.Value.ToPayEx();

            return string.Empty;
        }

        public static string ToPayEx(this PayexInterface.PurchaseOperation value)
        {
            return value.ToString().ToUpper();
        }

        public static string ToPayEx(this PayexInterface.PurchaseOperation? value)
        {
            if (value.HasValue)
                return value.Value.ToPayEx();

            return string.Empty;
        }
    }
}