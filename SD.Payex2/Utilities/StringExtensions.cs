using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace SD.Payex2.Utilities
{
    /// <summary>
    /// Kopierat från SD.Core
    /// </summary>
    [DebuggerStepThrough]
    internal static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        public static bool EqualsIgnoreCase(this string s, string value)
        {
            return s != null && s.Equals(value, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool StartsWithIgnoreCase(this string s, string value)
        {
            return s != null && s.StartsWith(value, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool EndsWithIgnoreCase(this string s, string value)
        {
            return s != null && s.EndsWith(value, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool ContainsIgnoreCase(this string s, string value)
        {
            return s != null && s.IndexOf(value, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        public static bool ContainsIgnoreCase(this IEnumerable<string> enumerable, string value)
        {
            return enumerable.Any(o => o.ContainsIgnoreCase(value));
        }

        /// <summary>
        /// Returns null if the string is empty or whitespace, else the trimmed string.
        /// </summary>
        public static string NullIfEmptyOrWhitespace(this string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        /// <summary>
        /// Retrieves the first length number of characters from this instance.
        /// The whole string is returned if length is greater than the length of this instance.
        /// </summary>
        public static string Left(this string s, int length)
        {
            if (s != null && s.Length > length)
                return s.Substring(0, length);

            return s;
        }

        /// <summary>
        /// Retrieves the last length number of characters from this instance.
        /// The whole string is returned if length is greater than the length of this instance.
        /// </summary>
        public static string Right(this string s, int length)
        {
            if (s != null && s.Length > length)
                return s.Substring(s.Length - length, length);

            return s;
        }

        public static string ToVisma(this string s, int? maxLength = null)
        {
            return (maxLength != null ? s.Left(maxLength.Value) : s).NullIfEmptyOrWhitespace() ?? " ";
        }

        public static int ToInt(this string s)
        {
            return int.Parse(s);
        }

        public static int? TryParseInt(this string s)
        {
            int i;
            return int.TryParse(s, out i) ? (int?)i : null;
        }

        public static decimal? TryParseDecimal(this string s, IFormatProvider formatProvider = null)
        {
            decimal d;
            return decimal.TryParse(s, NumberStyles.Number, formatProvider ?? CultureInfo.CurrentCulture, out d)
                ? (decimal?)d
                : null;
        }

        public static decimal ToDecimal(this string s, IFormatProvider formatProvider = null)
        {
            return decimal.Parse(s, formatProvider ?? CultureInfo.CurrentCulture);
        }

        public static string Format(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        [Obsolete("Use JoinStrings")]
        public static string Join(this IEnumerable<string> strings, string separator)
        {
            return string.Join(separator, strings);
        }

        public static string JoinStrings(this IEnumerable<string> strings, string separator)
        {
            return strings == null ? null : string.Join(separator, strings);
        }
    }
}