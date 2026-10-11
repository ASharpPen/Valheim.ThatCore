using System.Collections.Generic;

namespace ThatCore.Valheim.Extensions;

public static class HitDataDamageTypeExtensions
{
    public static HitData.DamageType ToBitmask(this IList<HitData.DamageType> damageTypes)
    {
        HitData.DamageType bitmask = 0;

        for (int i = 0; i < damageTypes.Count; ++i)
        {
            bitmask |= damageTypes[i];
        }

        return bitmask;
    }

    public static HitData.DamageType ToBitmask(this IEnumerable<HitData.DamageType> damageTypes)
    {
        HitData.DamageType bitmask = 0;

        foreach (var  damageType in damageTypes)
        {
            bitmask |= damageType;
        }

        return bitmask;
    }
}
