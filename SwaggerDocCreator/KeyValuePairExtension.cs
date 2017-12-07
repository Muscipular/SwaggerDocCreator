using System;
using System.Collections.Generic;
using System.Linq;

namespace SwaggerDocCreator
{
    public static class KeyValuePairExtension
    {
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> source, out TKey Key, out TValue Value)
        {
            Key = source.Key;
            Value = source.Value;
        }

        public static void Deconstruct<TKey, TValue>(this IGrouping<TKey, TValue> source, out TKey Key, out IEnumerable<TValue> Value)
        {
            Key = source.Key;
            Value = source;
        }
    }
}