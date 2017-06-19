using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace SD.Payex2.Utilities
{
    /// <summary>
    /// Kopierat från SD.Core
    /// </summary>
    internal static class LinqToXmlExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XElement GetElement(this XElement el, XName name)
        {
            return el.Element(QualifyName(el, name));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<XElement> GetElements(this XElement el, XName name)
        {
            return el.Elements(QualifyName(el, name));
        }

        private static XName QualifyName(XElement el, XName name)
        {
            return name.Namespace == "" ? el.Name.Namespace + name.LocalName : name;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString(this XElement el, XName name)
        {
            return el.GetElement(name)?.Value.NullIfEmptyOrWhitespace();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal GetDecimal(this XElement el, XName name)
        {
            return el.GetElement(name).Value.ToDecimal(CultureInfo.InvariantCulture);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal? GetNullableDecimal(this XElement el, XName name)
        {
            return el.GetElement(name)?.Value.ToDecimal(CultureInfo.InvariantCulture);
        }

        public static string GetStringTrimNull(this XElement el, XName name)
        {
            return el.GetString(name)?.NullIfEmptyOrWhitespace();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetInt(this XElement el, XName name)
        {
            return el.GetString(name).ToInt();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int? GetNullableInt(this XElement el, XName name)
        {
            return el.GetString(name)?.ToInt();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddNonEmpty(this XElement el, XName name, string value)
        {
            if (!value.IsNullOrEmpty())
                el.Add(new XElement(name, value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEnum GetEnum<TEnum>(this XElement el, XName name) where TEnum : struct
        {
            return GetEnum<TEnum>(el.GetInt(name));
        }

        public static IEnumerable<TEnum> GetEnums<TEnum>(this XElement el, XName name) where TEnum : struct
        {
            return el.GetElements(name).Select(e => GetEnum<TEnum>(e.Value.ToInt()));
        }

        public static TEnum? GetNullableEnum<TEnum>(this XElement el, XName name) where TEnum : struct
        {
            var value = el.GetNullableInt(name);
            return GetNullableEnum<TEnum>(value);
        }

        public static TEnum? GetAttributeEnum<TEnum>(this XElement el, XName name) where TEnum : struct
        {
            var attr = el.Attribute(name.LocalName);
            return GetNullableEnum<TEnum>(attr?.Value.ToInt());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TEnum GetEnum<TEnum>(int value) where TEnum : struct
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TEnum? GetNullableEnum<TEnum>(int? value) where TEnum : struct
        {
            return value != null ? (TEnum?)Enum.ToObject(typeof(TEnum), value.Value) : null;
        }
    }
}