using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Core.Events;

namespace ThatCore.Network;

public class Serializer
{
    private HashSet<Type> RegisteredTypes { get; } = new();

    private ISerializer CachedSerializer { get; set; }
    private IDeserializer CachedDeserializer { get; set; }

    private bool RefreshSerializerCache { get; set; } = true;
    private bool RefreshDeserializerCache { get; set; } = true;

    public Serializer RegisterType(Type type)
    {
        if (RegisteredTypes.Add(type))
        {
            RefreshSerializerCache = true;
        }

        return this;
    }

    public Serializer RegisterTypes(IEnumerable<Type> types)
    {
        foreach (var type in types)
        {
            RegisterType(type);
        }

        return this;
    }

    public ISerializer GetSerializer()
    {
        if (RefreshSerializerCache)
        {
            CachedSerializer = ConfigureSerializer();
            RefreshSerializerCache = false;
        }

        return CachedSerializer;
    }

    public IDeserializer GetDeserializer()
    {
        if (RefreshDeserializerCache)
        {
            CachedDeserializer = ConfigureDeserializer();
            RefreshDeserializerCache = false;
        }

        return CachedDeserializer;
    }

    private ISerializer ConfigureSerializer()
    {
        var builder = new SerializerBuilder();

        if (RegisteredTypes is not null)
        {
            foreach (var type in RegisteredTypes)
            {
                builder.WithTagMapping("!" + type.AssemblyQualifiedName, type);
            }
        }

        return builder
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitEmptyCollections)
            .WithTypeConverter(new YamlEnumWriter())
            .Build();
    }

    public IDeserializer ConfigureDeserializer()
    {
        var deserializer = new DeserializerBuilder()
            .WithNodeTypeResolver(new TypeResolver(RegisteredTypes))
            .Build();

        return deserializer;
    }

    public byte[] SerializeAndCompress(object obj)
    {
        var serialized = GetSerializer().Serialize(obj);

        var encoded = Encoding.Unicode.GetBytes(serialized);

        using var decompressedStream = new MemoryStream(encoded);
        using var compressedStream = new MemoryStream();

        using (var zipStream = new GZipStream(compressedStream, CompressionLevel.Optimal))
        {
            decompressedStream.CopyTo(zipStream);
        }

        var compressedSerialized = compressedStream.ToArray();

        return compressedSerialized;
    }

    public object DeserializeCompressed(Type type, Stream stream)
    {
        using var decompressedStream = new MemoryStream();

        using (var zipStream = new GZipStream(stream, CompressionMode.Decompress, true))
        {
            zipStream.CopyTo(decompressedStream);
        }

        decompressedStream.Position = 0;
        using TextReader reader = new StreamReader(decompressedStream, Encoding.Unicode);

        var responseObject = GetDeserializer()
            .Deserialize(reader, type);

        return responseObject;
    }

    public object DeserializeCompressed(Type type, byte[] content)
    {
        using var compressedStream = new MemoryStream(content);
        using var decompressedStream = new MemoryStream();

        using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress, true))
        {
            zipStream.CopyTo(decompressedStream);
        }

        decompressedStream.Position = 0;
        using TextReader reader = new StreamReader(decompressedStream, Encoding.Unicode);

        var responseObject = GetDeserializer()
            .Deserialize(reader, type);

        return responseObject;
    }

    public T DeserializeCompressed<T>(Stream stream)
    {
        using var decompressedStream = new MemoryStream();

        using (var zipStream = new GZipStream(stream, CompressionMode.Decompress, true))
        {
            zipStream.CopyTo(decompressedStream);
        }

        decompressedStream.Position = 0;
        using TextReader reader = new StreamReader(decompressedStream, Encoding.Unicode);

        var responseObject = GetDeserializer()
            .Deserialize<T>(reader);

        return responseObject;
    }

    /// <summary>
    /// Resolver for interface based-types, that would otherwise have trouble deserializing.
    /// </summary>
    private class TypeResolver : INodeTypeResolver
    {
        public HashSet<Type> WhitelistedTypes { get; }

        public TypeResolver(HashSet<Type> whitelistedTypes)
        {
            WhitelistedTypes = whitelistedTypes;
        }

        public bool Resolve(NodeEvent nodeEvent, ref Type currentType)
        {
            if (nodeEvent.Tag.IsEmpty || string.IsNullOrWhiteSpace(nodeEvent.Tag.Value))
            {
                return false;
            }

            // Retrieve type based on tag.
            var type = Type.GetType(nodeEvent.Tag.Value.Substring(1));

            if (type is null)
            {
                return false;
            }

            // Verify type is one of the whitelisted types.
            if (WhitelistedTypes.Any(x => x.IsAssignableFrom(type)))
            {
                // Set resolved type to tags type.
                currentType = type;
                return true;
            }

            return false;
        }
    }
}
