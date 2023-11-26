namespace ThatCore.Config.Toml;

public static class TomlPath
{
    public static TomlPathSegment Create(TomlPathSegmentType segmentType, string name = null)
    {
        return new TomlPathSegment(segmentType, name);
    }

    public static TomlPathSegment Chain(this TomlPathSegment parent, TomlPathSegmentType segmentType, string name = null)
    {
        return new TomlPathSegment(segmentType, name, parent);
    }
}
