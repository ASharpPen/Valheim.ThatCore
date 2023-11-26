using System;

namespace ThatCore.Config.Toml.Extensions;

public static class TomlSettingExtensions
{
    public static void DoIfSet<T>(this TomlSetting<T> setting, Action<T> action)
    {
        if (setting.IsSet)
        {
            action(setting);
        }
    }
}
