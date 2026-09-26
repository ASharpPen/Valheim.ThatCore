using System;
using System.Collections.Generic;
using System.Linq;

namespace ThatCore.Valheim.Extensions;

public static class HeightmapBiomeExtensions
{
    public static List<Heightmap.Biome> GetBiomes(bool includeCombined = false)
    {
        var possibleValues = (Heightmap.Biome[])Enum.GetValues(typeof(Heightmap.Biome));

        if (includeCombined)
        {
            return possibleValues.ToList();
        }

        bool[] added = new bool[possibleValues.Length];
        List<Heightmap.Biome> results = [];

        for (int i = 0; i < possibleValues.Length; ++i)
        {
            var value = possibleValues[i];

            if (value != 0 &&
                (value & (value - 1)) == 0 && // Check if only one (or 0) bit is set.
                !added[i])
            {
                added[i] = true;
                results.Add(value);
            }
        }

        return results;
    }

    public static List<Heightmap.Biome> Split(this Heightmap.Biome biomeMask)
    {
        List<Heightmap.Biome> result = new List<Heightmap.Biome>();

        foreach (Heightmap.Biome value in GetBiomes())
        {
            if ((biomeMask & value) > 0)
            {
                result.Add(value);
            }
        }

        return result;
    }

    public static Heightmap.Biome ToBitmask(this IList<Heightmap.Biome> biomes)
    {
        var bitmask = (Heightmap.Biome)0;

        for(int i = 0; i < biomes.Count; ++i)
        {
            bitmask |= biomes[i];
        }

        return bitmask;
    }

    public static Heightmap.Biome ToBitmask(this IEnumerable<Heightmap.Biome> biomes)
    {
        var bitmask = (Heightmap.Biome)0;

        foreach (var biome in biomes)
        {
            bitmask |= biome;
        }

        return bitmask;
    }
}
