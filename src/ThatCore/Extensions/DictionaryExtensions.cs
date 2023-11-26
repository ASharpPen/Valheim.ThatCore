using System;
using System.Collections.Generic;

namespace ThatCore.Extensions;

public static class DictionaryExtensions
{
    public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key)
        where TValue : new()
    {
        if (!dic.TryGetValue(key, out var value))
        {
            value = new();

            dic[key] = value;
        }

        return value;
    }

    public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key)
    {
        if (dic.TryGetValue(key, out var value))
        {
            return value;
        }

        return default;
    }
}