using System.Collections.Generic;
using ThatCore.Extensions;

namespace ThatCore.Config.Toml.Parsers;

internal class StringListParser : ValueParser<List<string>>
{
    protected override void ParseInternal(ITomlSetting<List<string>> entry, TomlLine line)
    {
        entry.Value = line.Value.SplitByComma();
        entry.IsSet = true;
    }
}
