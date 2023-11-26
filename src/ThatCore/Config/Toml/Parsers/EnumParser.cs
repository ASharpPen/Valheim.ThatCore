using System;
using ThatCore.Logging;

namespace ThatCore.Config.Toml.Parsers;

internal class EnumParser<T> : ValueParser<T>
    where T : struct, Enum
{
    protected override void ParseInternal(ITomlSetting<T> entry, TomlLine line)
    {
        if (Enum.TryParse<T>(line.Value, true, out var result))
        {
            entry.Value = result;
            entry.IsSet = true;
        }
        else
        {
            Log.Warning?.Log($"{line.FileName}, Line {line.LineNr}: Unable to parse '{line.Value}'. Verify spelling.");
            entry.IsSet = false;
        }
    }
}

internal class NullableEnumParser<T> : ValueParser<T?>
    where T : struct, Enum
{
    protected override void ParseInternal(ITomlSetting<T?> entry, TomlLine line)
    {
        if (Enum.TryParse<T>(line.Value, true, out var result))
        {
            entry.Value = result;
            entry.IsSet = true;
        }
        else
        {
            Log.Warning?.Log($"{line.FileName}, Line {line.LineNr}: Unable to parse '{line.Value}'. Verify spelling.");
            entry.IsSet = false;
        }
    }
}
