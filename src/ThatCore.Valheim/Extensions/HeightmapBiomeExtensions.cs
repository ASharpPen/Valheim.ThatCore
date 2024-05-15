using System;
using System.Collections.Generic;

namespace ThatCore.Utilities.Valheim;

public static class HeightmapBiomeExtensions
{
    public static List<Heightmap.Biome> Split(this Heightmap.Biome biomeMask)
    {
        List<Heightmap.Biome> result = new List<Heightmap.Biome>();

        foreach (Heightmap.Biome value in Enum.GetValues(typeof(Heightmap.Biome)))
        {
            if (value == Heightmap.Biome.All)
            {
                if (biomeMask == Heightmap.Biome.All)
                {
                    result.Add(value);
                }
            }
            else if ((biomeMask & value) > 0)
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
