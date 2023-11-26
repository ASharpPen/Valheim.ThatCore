using System;
using ThatCore.Models;

namespace ThatCore.Config.Toml.Extensions;

public static class TomlConfigExtensions
{
    public static Optional<T> GetSettingAsOptional<T>(this TomlConfig config, string name)
    {
        if (config.Settings.TryGetValue(name, out var setting) &&
            setting is TomlSetting<T> typedSetting &&
            typedSetting.IsSet)
        {
            return new(typedSetting.Value);
        }

        return new();
    }

    public static TomlConfig DoIfAnySet<T>(
        this TomlConfig config,
        string settingName,
        Action<T> action)
    {
        var setting = config.GetSetting<T>(settingName);
        if (setting is not null &&
            setting.IsSet)
        {
            action(setting);
        }

        return config;
    }

    public static TomlConfig DoIfAnySet<T1, T2>(
        this TomlConfig config,
        string setting1Name,
        string setting2Name,
        Action<T1, T2> action)
    {
        var setting1 = config.GetSetting<T1>(setting1Name);
        var setting2 = config.GetSetting<T2>(setting2Name);

        if (setting1?.IsSet == true ||
            setting2?.IsSet == true)
        {
            action(setting1, setting2);
        }

        return config;
    }

    public static TomlConfig DoIfAnySet<T1, T2, T3>(
        this TomlConfig config,
        string setting1Name,
        string setting2Name,
        string setting3Name,
        Action<T1, T2, T3> action)
    {
        var setting1 = config.GetSetting<T1>(setting1Name);
        var setting2 = config.GetSetting<T2>(setting2Name);
        var setting3 = config.GetSetting<T3>(setting3Name);

        if (setting1?.IsSet == true ||
            setting2?.IsSet == true ||
            setting3?.IsSet == true)
        {
            action(setting1, setting2, setting3);
        }

        return config;
    }

    public static TomlConfig DoIfAnySet<T1, T2, T3, T4>(
        this TomlConfig config,
        string setting1Name,
        string setting2Name,
        string setting3Name,
        string setting4Name,
        Action<T1, T2, T3, T4> action)
    {
        var setting1 = config.GetSetting<T1>(setting1Name);
        var setting2 = config.GetSetting<T2>(setting2Name);
        var setting3 = config.GetSetting<T3>(setting3Name);
        var setting4 = config.GetSetting<T4>(setting4Name);

        if (setting1?.IsSet == true ||
            setting2?.IsSet == true ||
            setting3?.IsSet == true ||
            setting4?.IsSet == true)
        {
            action(setting1, setting2, setting3, setting4);
        }

        return config;
    }
}
