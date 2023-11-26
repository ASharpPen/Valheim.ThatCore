using System;

namespace ThatCore.Config.Toml;

public interface ITomlSetting
{
    string Name { get; }

    string Description { get; }

    public Type SettingType { get; }

    public bool IsSet { get; set; }

    public object GetValue();

    /// <param name="includeValue">Should clone also copy IsSet and Value.</param>
    public ITomlSetting Clone(bool includeValue = true);
}

public interface ITomlSetting<T> : ITomlSetting
{
    T Value { get; set; }

    T DefaultValue { get; set; }
}

