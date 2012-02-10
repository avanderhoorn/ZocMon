using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZocMonLib
{
    public static class CollectionExtension
    {
        public static string FormatAsString<T>(this IEnumerable<T> collection)
        {
            return string.Empty;
        }

        public static TValue SetDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            TValue result;
            // If we can't find the value, return the default
            if (!dictionary.TryGetValue(key, out result))
                return dictionary[key] = defaultValue;
            return result;
        }
    }
}
