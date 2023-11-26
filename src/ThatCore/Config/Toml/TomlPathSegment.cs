using System;
using System.Text;
using ThatCore.Extensions;

namespace ThatCore.Config.Toml;

public class TomlPathSegment : IEquatable<TomlPathSegment>
{
    internal static TomlPathSegment Default { get; } = new TomlPathSegment(TomlPathSegmentType.Start);

    public TomlPathSegment(
        TomlPathSegmentType type,
        string name = null,
        TomlPathSegment parent = null)
    {
        Name = name;
        SegmentType = type;
        Parent = parent;

        NormalizedName = StringExtensions.Normalize(Name);
    }

    public string Name { get; }

    public string NormalizedName { get; }

    public TomlPathSegmentType SegmentType { get; }

    public TomlPathSegment Parent { get; }

    public string GetPath()
    {
        StringBuilder builder = new();
        var segment = this;

        do
        {
            if (segment.SegmentType != TomlPathSegmentType.Start)
            {
                if (builder.Length > 0)
                {
                    builder.Insert(0, ".");
                }

                builder.Insert(0, segment.Name);
            }

            segment = segment.Parent;
        }
        while (segment is not null);

        return builder.ToString();
    }

    public bool Equals(TomlPathSegment other)
    {
        if (base.Equals(other))
        {
            return true;
        }

        return
            SegmentType == other.SegmentType &&
            NormalizedName == other.NormalizedName;
    }

    public bool Equals(TomlPathSegment x, TomlPathSegment y) => x.Equals(y);

    public override int GetHashCode()
    {
        return
            NormalizedName.GetHashCode() +
            SegmentType.GetHashCode();
    }
}
