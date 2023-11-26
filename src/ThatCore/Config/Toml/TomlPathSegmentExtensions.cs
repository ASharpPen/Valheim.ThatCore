namespace ThatCore.Config.Toml;

public static class TomlPathSegmentExtensions
{
    public static TomlPathSegment FindClosest(this TomlPathSegment segment, TomlPathSegmentType segmentType)
    {
        while (segment is not null)
        {
            if (segment.SegmentType == segmentType)
            {
                return segment;
            }

            segment = segment.Parent;
        }

        return null;
    }
}
