using System;
using System.Collections.Generic;
using System.Linq;

namespace ThatCore.Extensions;

public static class ListExtensions
{
    public static void AddOrReplaceByType<T, TEntry>(this List<T> list, TEntry entry)
        where T : class
        where TEntry : class, T
    {
        var entryType = typeof(TEntry);

        int existingIndex = -1;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].GetType() == entryType)
            {
                existingIndex = i;
                break;
            }
        }

        if (existingIndex < 0)
        {
            list.Add(entry);
        }
        else
        {
            list[existingIndex] = entry;
        }
    }

    public static void AddOrConfigureByType<T, TEntry>(this List<T> list, Action<TEntry> configure)
        where T : class
        where TEntry : T, new()
    {
        TEntry existing = list
            .OfType<TEntry>()
            .FirstOrDefault();

        if (existing is not null)
        {
            configure(existing);
        }
        else
        {
            TEntry newEntry = new();
            configure(newEntry);

            list.Add(newEntry);
        }
    }
}
