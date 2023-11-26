using ThatCore.Logging;

namespace ThatCore.Config.Toml.Parsers;

internal class BoolParser : ValueParser<bool>
{
    protected override void ParseInternal(ITomlSetting<bool> entry, TomlLine line)
    {
        if (bool.TryParse(line.Value, out var result))
        {
            entry.Value = result;
            entry.IsSet = true;
        }
        else
        {
            Log.Warning?.Log($"{line.FileName}, Line {line.LineNr}: Unable to parse '{line.Value}' as a boolean. Verify spelling. Valid values are \"true\" and \"false\".");
            entry.IsSet = false;
        }
    }
}

internal class NullableBoolParser : ValueParser<bool?>
{
    protected override void ParseInternal(ITomlSetting<bool?> entry, TomlLine line)
    {
        if (bool.TryParse(line.Value, out var result))
        {
            entry.Value = result;
            entry.IsSet = true;
        }
        else
        {
            Log.Warning?.Log($"{line.FileName}, Line {line.LineNr}: Unable to parse '{line.Value}' as a boolean. Verify spelling. Valid values are \"true\" and \"false\".");
            entry.IsSet = false;
        }
    }
}
