using System;
using System.Collections;
using System.Collections.Generic;

namespace ThatCore.Models;

public class TypeSet<TKey> : IEnumerable<TKey>, ICollection<TKey>
{
    private Dictionary<Type, TKey> _dic = new();

    public TypeSet() { }

    public TypeSet(TypeSet<TKey> input)
    {
        _dic = new(input._dic);
    }

    public T GetOrCreate<T>()
        where T : TKey, new()
    {
        if (_dic.TryGetValue(typeof(T), out var value) &&
            value is T valueKey)
        {
            return valueKey;
        }

        valueKey = new();

        _dic[typeof(T)] = valueKey;

        return valueKey;
    }

    public T GetOrDefault<T>()
        where T : TKey
    {
        if (_dic.TryGetValue(typeof(T), out var value) &&
            value is T valueKey)
        {
            return valueKey;
        }

        return default;
    }

    /// <summary>
    /// Adds item to TypeSet.
    /// Will replace existing key with same Type.
    /// </summary>
    public void Set(TKey item) => _dic[item.GetType()] = item;

    public TKey this[TKey item]
    {
        get => _dic[item.GetType()];
        set => _dic[item.GetType()] = item;
    }

    public bool Remove<T>() where T : TKey => _dic.Remove(typeof(T));

    public bool Remove(Type type) => _dic.Remove(type);

    public TypeSet<TKey> Clone() => new() { _dic = new(_dic) };

    #region IEnumerable<TKey>

    public IEnumerator<TKey> GetEnumerator() => _dic.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _dic.Values.GetEnumerator();

    #endregion
    #region ICollection<TKey>

    public int Count { get => _dic.Count; }

    public bool IsReadOnly { get; } = false;

    /// <summary>
    /// Adds item to TypeSet.
    /// Throws if Type of key is already added.
    /// </summary>
    public void Add(TKey item) => _dic.Add(item.GetType(), item);

    public void Clear() => _dic.Clear();

    public bool Contains(TKey item) => _dic.ContainsKey(item.GetType());

    public void CopyTo(TKey[] array, int arrayIndex)
    {
        _dic.Values.CopyTo(array, arrayIndex);
    }

    public bool Remove(TKey item) => _dic.Remove(item.GetType());

    #endregion
}
