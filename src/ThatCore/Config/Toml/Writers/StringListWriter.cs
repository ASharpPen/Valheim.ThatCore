using System.Collections.Generic;
using System.Linq;
using ThatCore.Extensions;

namespace ThatCore.Config.Toml.Writers;

internal class StringListWriter : ValueWriter<List<string>>
{
    protected override string WriteInternal(ITomlSetting<List<string>> entry)
    {
        if (entry.IsSet && entry.Value is not null)
        {
            return entry.Value.Join();
        }
        else
        {
            return string.Empty;
        }
    }
}
