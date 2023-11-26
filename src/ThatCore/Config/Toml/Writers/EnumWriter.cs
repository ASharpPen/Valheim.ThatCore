using System;

namespace ThatCore.Config.Toml.Writers;

internal class EnumWriter<T> : ValueWriter<T>
    where T : struct, Enum
{
    protected override string WriteInternal(ITomlSetting<T> entry)
    {
        return entry.Value.ToString();
    }
}

internal class NullableEnumWriter<T> : ValueWriter<T?>
    where T : struct, Enum
{
    protected override string WriteInternal(ITomlSetting<T?> entry)
    {
        if (entry.IsSet && entry.Value is not null)
        {
            return entry.Value.Value.ToString();
        }
        else
        {
            return string.Empty;
        }
    }
}