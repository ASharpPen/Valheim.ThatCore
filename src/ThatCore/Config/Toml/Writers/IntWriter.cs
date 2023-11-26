using System.Globalization;

namespace ThatCore.Config.Toml.Writers;

internal class IntWriter : ValueWriter<int>
{
    protected override string WriteInternal(ITomlSetting<int> entry)
    {
        return entry.Value.ToString(CultureInfo.InvariantCulture);
    }
}

internal class NullableIntWriter : ValueWriter<int?>
{
    protected override string WriteInternal(ITomlSetting<int?> entry)
    {
        if (entry.IsSet && entry.Value is not null)
        {
            return entry.Value.Value.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            return string.Empty;
        }
    }
}
