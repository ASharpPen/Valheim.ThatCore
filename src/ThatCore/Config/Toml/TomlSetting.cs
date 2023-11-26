using System;

namespace ThatCore.Config.Toml;

public class TomlSetting<T> : ITomlSetting<T>
{
    private T value;

    internal TomlSetting()
    { }

    public TomlSetting(string name, T defaultValue, string description = null)
    {
        Name = name;
        DefaultValue = defaultValue;
        Description = description;
    }

    public string Name { get; set; }

    public Type SettingType { get; } = typeof(T);

    public T Value {
        get => value;
        set
        {
            this.value = value;
            IsSet = true;
        } 
    }

    public T DefaultValue { get; set; }

    public string Description { get; set; }

    public bool IsSet { get; set; }

    public object GetValue() => Value;

    public void Set(T value)
    {
        Value = value;
        IsSet = true;
    }

    public static implicit operator T(TomlSetting<T> entry) => entry.IsSet ? entry.Value : entry.DefaultValue;

    public ITomlSetting Clone(bool includeValue = true)
    {
        var entry = new TomlSetting<T>(Name, DefaultValue, Description);

        if (includeValue)
        {
            if (IsSet)
            {
                entry.IsSet = true;
                entry.Value = Value;
            }
        }

        return entry;
    }
}
