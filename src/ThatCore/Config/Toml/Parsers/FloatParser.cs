using System.Globalization;
using ThatCore.Logging;

namespace ThatCore.Config.Toml.Parsers;

internal class FloatParser : ValueParser<float>
{
    protected override void ParseInternal(ITomlSetting<float> entry, TomlLine line)
    {
        if (float.TryParse(line.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            entry.Value = result;
            entry.IsSet = true;
        }
        else
        {
            Log.Warning?.Log($"{line.FileName}, Line {line.LineNr}: Unable to parse '{line.Value}'. Expected decimal number. Eg., 0.5 or 3.");
            entry.IsSet = false;
        }
    }
}

internal class NullableFloatParser : ValueParser<float?>
{
    protected override void ParseInternal(ITomlSetting<float?> entry, TomlLine line)
    {
        if (float.TryParse(line.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            entry.Value = result;
            entry.IsSet = true;
        }
        else
        {
            Log.Warning?.Log($"{line.FileName}, Line {line.LineNr}: Unable to parse '{line.Value}'. Expected decimal number. Eg., 0.5 or 3.");
            entry.IsSet = false;
        }
    }
}
