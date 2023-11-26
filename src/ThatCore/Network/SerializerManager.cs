using System;
using System.Collections.Generic;

namespace ThatCore.Network;

public static class SerializerManager
{
    private static Dictionary<Type, Serializer> TypeSerializers { get; } = new();

    public static Serializer GetSerializer<T>() =>
        GetSerializer(typeof(T));

    public static Serializer GetSerializer(Type type)
    {
        if (TypeSerializers.TryGetValue(type, out var existing))
        {
            return existing;
        }

        return TypeSerializers[type] = new();
    }
}
