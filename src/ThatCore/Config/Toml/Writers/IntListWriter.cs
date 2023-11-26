using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HarmonyLib;

namespace ThatCore.Config.Toml.Writers;

internal class IntListWriter : ValueWriter<List<int>>
{
    protected override string WriteInternal(ITomlSetting<List<int>> entry)
    {
        if (entry.IsSet && entry.Value is not null)
        {
            return entry.Value.Select(x => x.ToString(CultureInfo.InvariantCulture)).Join();
        }
        else
        {
            return string.Empty;
        }
    }
}
