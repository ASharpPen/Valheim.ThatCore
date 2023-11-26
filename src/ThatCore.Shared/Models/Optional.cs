using System;

namespace ThatCore.Models;

public struct Optional<T>
{
    public T Value { get; private set; } = default;

    public bool IsSet { get; private set; }

    public Optional()
    {
    }

    public Optional(T value)
    {
        Set(value);
    }

    public void Set(T value)
    {
        IsSet = true;
        Value = value;
    }

    public static implicit operator Optional<T>(T value) => new Optional<T>(value);
    public static implicit operator T(Optional<T> optional) => optional.Value;

    public void DoIfSet(Action<T> action)
    {
        if (IsSet)
        {
            action(Value);
        }
    }

    public void AssignIfSet(ref T dest)
    {
        if (IsSet)
        {
            dest = Value;
        }
    }
}
