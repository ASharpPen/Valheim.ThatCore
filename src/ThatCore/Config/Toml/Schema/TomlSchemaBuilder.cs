using System.Collections.Generic;
using System.Linq;
using ThatCore.Extensions;

namespace ThatCore.Config.Toml.Schema;

public interface ITomlSchemaBuilder
{
    ITomlSchemaLayerBuilder FirstLayer { get; }

    ITomlNamedSchemaLayerBuilder SetLayerAsNamed();

    ITomlSchemaCollectionLayerBuilder SetLayerAsCollection();

    ITomlSchemaLayer Build();
}

public interface ITomlSchemaLayerBuilder
{
    ITomlSchemaLayer Build(ITomlSchemaNode parent);
}

public interface ITomlNamedSchemaLayerBuilder : ITomlSchemaLayerBuilder
{
    ITomlSchemaNodeBuilder AddNode(string name);

    ITomlSchemaNamedLayer BuildNamed(ITomlSchemaNode parent);
}

public interface ITomlSchemaCollectionLayerBuilder : ITomlSchemaLayerBuilder
{
    ITomlSchemaNodeBuilder GetNode();

    ITomlSchemaCollectionLayer BuildCollection(ITomlSchemaNode parent);
}

public interface ITomlSchemaNodeBuilder
{
    ITomlSchemaNodeBuilder AddSetting(ITomlSetting entry);

    ITomlSchemaNodeBuilder AddSetting<T>(string name, T defaultValue = default, string description = null);

    ITomlNamedSchemaLayerBuilder SetNextLayerAsNamed();

    ITomlSchemaCollectionLayerBuilder SetNextLayerAsCollection();

    ITomlSchemaNode Build(ITomlSchemaLayer parent);
}

public class TomlSchemaBuilder : ITomlSchemaBuilder
{
    private ITomlSchemaLayerBuilder _firstLayer = new TomlNamedSchemaLayerBuilder();

    public ITomlSchemaLayerBuilder FirstLayer => _firstLayer;

    public ITomlNamedSchemaLayerBuilder SetLayerAsNamed()
    {
        var layer = new TomlNamedSchemaLayerBuilder();
        _firstLayer = layer;
        return layer;
    }

    public ITomlSchemaCollectionLayerBuilder SetLayerAsCollection()
    {
        var layer = new TomlSchemaCollectionLayerBuilder();
        _firstLayer = layer;
        return layer;
    }

    public ITomlSchemaLayer Build()
    {
        return _firstLayer.Build(null);
    }
}

internal class TomlNamedSchemaLayerBuilder : ITomlNamedSchemaLayerBuilder
{
    private Dictionary<string, TomlSchemaNodeBuilder> Nodes { get; } = new();

    public ITomlSchemaNodeBuilder AddNode(string name)
    {
        string normalizedName = StringExtensions.Normalize(name);

        if(Nodes.TryGetValue(normalizedName, out var existing))
        {
            return existing;
        }

        return Nodes[normalizedName] = new TomlSchemaNodeBuilder(name);
    }

    public ITomlSchemaLayer Build(ITomlSchemaNode parent) => BuildNamed(parent);

    public ITomlSchemaNamedLayer BuildNamed(ITomlSchemaNode parent)
    {
        var layer = new SchemaNamedLayer();
        layer.ParentNode = parent;

        layer.Nodes = Nodes.ToDictionary(x => StringExtensions.Normalize(x.Value.Name), x => x.Value.Build(layer));

        return layer;
    }
}

internal class TomlSchemaCollectionLayerBuilder : ITomlSchemaCollectionLayerBuilder
{
    private TomlSchemaNodeBuilder Node { get; } = new();

    public ITomlSchemaNodeBuilder GetNode()
    {
        return Node;
    }

    public ITomlSchemaLayer Build(ITomlSchemaNode parent) => BuildCollection(parent);

    public ITomlSchemaCollectionLayer BuildCollection(ITomlSchemaNode parent)
    {
        var layer = new SchemaCollectionLayer();

        layer.ParentNode = parent;
        layer.Node = Node.Build(layer);

        return layer;
    }
}

internal class TomlSchemaNodeBuilder : ITomlSchemaNodeBuilder
{
    public string Name { get; }

    public Dictionary<string, ITomlSetting> Settings { get; } = new();

    public ITomlSchemaLayerBuilder NextLayer { get; set; }

    public TomlSchemaNodeBuilder(string name = null)
    {
        Name = name;
    }

    public ITomlSchemaNodeBuilder AddSetting(ITomlSetting entry)
    {
        Settings[StringExtensions.Normalize(entry.Name)] = entry;
        return this;
    }

    public ITomlSchemaNodeBuilder AddSetting<T>(string name, T defaultValue = default, string description = null)
    {
        var setting = new TomlSetting<T>(name, defaultValue, description)
        {
            Value = defaultValue,
        };
        Settings[StringExtensions.Normalize(setting.Name)] = setting;

        return this;
    }

    public ITomlSchemaCollectionLayerBuilder SetNextLayerAsCollection()
    {
        var nextLayer = new TomlSchemaCollectionLayerBuilder();
        NextLayer = nextLayer;
        return nextLayer;
    }

    public ITomlNamedSchemaLayerBuilder SetNextLayerAsNamed()
    {
        var nextLayer = new TomlNamedSchemaLayerBuilder();
        NextLayer = nextLayer;
        return nextLayer;
    }

    public ITomlSchemaNode Build(ITomlSchemaLayer parent)
    {
        if (Name is null)
        {
            var node = new SchemaCollectionNode();

            node.Settings = new(Settings);
            node.ParentLayer = parent;
            node.NextLayer = NextLayer?.Build(node);
            node.PathSegment = new(
                TomlPathSegmentType.Collection,
                null,
                parent?.ParentNode?.PathSegment
                );

            return node;
        }
        else
        {
            var segment = new TomlPathSegment(
                TomlPathSegmentType.Named,
                Name,
                parent?.ParentNode?.PathSegment);

            var node = new NamedSchemaNode(segment);

            node.Settings = new(Settings);
            node.ParentLayer = parent;
            node.NextLayer = NextLayer?.Build(node);

            return node;
        }
    }
}
