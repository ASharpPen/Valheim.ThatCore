﻿using ThatCore.Logging;

namespace ThatCore.Config.Toml.Parsers;

internal class IntParser : ValueParser<int>
{
    protected override void ParseInternal(ITomlSetting<int> entry, TomlLine line)
    {
        if (int.TryParse(line.Value, out var result))
        {
            entry.Value = result;
            entry.IsSet = true;
        }
        else
        {
            Log.Warning?.Log($"{line.FileName}, Line {line.LineNr}: Unable to parse '{line.Value}'. Expected integer number. Eg., 0, 1 or 3.");
            entry.IsSet = false;
        }
    }
}

internal class NullableIntParser : ValueParser<int?>
{
    protected override void ParseInternal(ITomlSetting<int?> entry, TomlLine line)
    {
        if (int.TryParse(line.Value, out var result))
        {
            entry.Value = result;
            entry.IsSet = true;
        }
        else
        {
            Log.Warning?.Log($"{line.FileName}, Line {line.LineNr}: Unable to parse '{line.Value}'. Expected integer number. Eg., 0, 1 or 3.");
            entry.IsSet = false;
        }
    }
}
