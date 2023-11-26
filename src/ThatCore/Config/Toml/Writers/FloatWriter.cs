using System.Globalization;

namespace ThatCore.Config.Toml.Writers;

internal class FloatWriter : ValueWriter<float>
{
    protected override string WriteInternal(ITomlSetting<float> entry)
    {
        return entry.Value.ToString(CultureInfo.InvariantCulture);
    }
}

internal class NullableFloatWriter : ValueWriter<float?>
{
    protected override string WriteInternal(ITomlSetting<float?> entry)
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
