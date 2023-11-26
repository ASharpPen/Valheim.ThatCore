namespace ThatCore.Config.Toml.Writers;

internal class StringWriter : ValueWriter<string>
{
    protected override string WriteInternal(ITomlSetting<string> entry)
    {
        return entry.Value ?? string.Empty;
    }
}
