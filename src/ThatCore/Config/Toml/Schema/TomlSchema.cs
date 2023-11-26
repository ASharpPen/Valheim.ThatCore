using System.Collections.Generic;

namespace ThatCore.Config.Toml.Schema;

public interface ITomlSchemaLayer
{
    ITomlSchemaCollectionLayer AsIndexed();

    ITomlSchemaNamedLayer AsNamed();

    ITomlSchemaNode ParentNode { get; }
}

public interface ITomlSchemaNamedLayer : ITomlSchemaLayer
{
    Dictionary<string, ITomlSchemaNode> Nodes { get; }
}

public interface ITomlSchemaCollectionLayer : ITomlSchemaLayer
{
    ITomlSchemaNode Node { get; }
}

public interface ITomlSchemaNode
{
    Dictionary<string, ITomlSetting> Settings { get; }

    ITomlSchemaLayer NextLayer { get; }

    ITomlSchemaLayer ParentLayer { get; }

    TomlPathSegment PathSegment { get; }
}

public abstract class TomlSchemaLayer : ITomlSchemaLayer
{
    public ITomlSchemaNode ParentNode { get; set; }

    public ITomlSchemaCollectionLayer AsIndexed() => this as ITomlSchemaCollectionLayer;

    public ITomlSchemaNamedLayer AsNamed() => this as ITomlSchemaNamedLayer;
}

public class SchemaNamedLayer : TomlSchemaLayer, ITomlSchemaNamedLayer
{
    public Dictionary<string, ITomlSchemaNode> Nodes { get; set; } = new();
}

public class NamedSchemaNode : ITomlSchemaNode
{
    public NamedSchemaNode(TomlPathSegment segment)
    {
        Name = segment.Name;
        PathSegment = segment;
    }

    public string Name { get; set; }

    public Dictionary<string, ITomlSetting> Settings { get; set; } = new();

    public ITomlSchemaLayer NextLayer { get; set; }

    public ITomlSchemaLayer ParentLayer { get; set; }

    public TomlPathSegment PathSegment { get; set; }
}

public class SchemaCollectionLayer : TomlSchemaLayer, ITomlSchemaCollectionLayer
{
    public ITomlSchemaNode Node { get; set; } = new SchemaCollectionNode();
}

public class SchemaCollectionNode : ITomlSchemaNode
{
    public Dictionary<string, ITomlSetting> Settings { get; set; }

    public ITomlSchemaLayer NextLayer { get; set; }

    public ITomlSchemaLayer ParentLayer { get; set; }

    public TomlPathSegment PathSegment { get; set; }
}