namespace ThatCore.Config.Toml.Parsers;

internal class StringParser : ValueParser<string>
{
    protected override void ParseInternal(ITomlSetting<string> entry, TomlLine line)
    {
        entry.Value = line.Value.Trim();
        entry.IsSet = true;
    }
}
