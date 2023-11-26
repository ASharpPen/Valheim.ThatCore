namespace ThatCore.Config.Toml.Writers;

internal class BoolWriter : ValueWriter<bool>
{
    protected override string WriteInternal(ITomlSetting<bool> entry)
    {
        return entry.Value ? "true" : "false";
    }
}

internal class NullableBoolWriter : ValueWriter<bool?>
{
    protected override string WriteInternal(ITomlSetting<bool?> entry)
    {
        if (entry.IsSet && entry.Value is not null)
        {
            return entry.Value.Value ? "true" : "false";
        }
        else
        {
            return string.Empty;
        }
    }
}
