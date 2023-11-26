using System.Collections.Generic;

namespace ThatCore.Config.Toml;

public class TomlConfig
{
    public Dictionary<string, ITomlSetting> Settings = new();
    public Dictionary<TomlPathSegment, TomlConfig> Sections = new();

    public TomlPathSegment PathSegment { get; set; } = TomlPathSegment.Default;

    public TomlConfig CreateSubsection(TomlPathSegment segment)
    {
        if (Sections.TryGetValue(segment, out var existing))
        {
            return existing;
        }

        TomlPathSegment newSegment = new(segment.SegmentType, segment.Name, PathSegment);

        return Sections[segment] = new TomlConfig()
        {
            PathSegment = newSegment,
        };
    }

    public void SetSettings(IReadOnlyDictionary<string, ITomlSetting> settings)
    {
        foreach (var setting in settings)
        {
            Settings[setting.Key] = setting.Value;
        }
    }

    public void SetSetting(string name, ITomlSetting setting)
        => Settings[name] = setting;

    public T GetSettingValue<T>(string name, T defaultValue = default)
    {
        if (Settings.TryGetValue(name, out var setting) &&
            setting is TomlSetting<T> typedSetting)
        {
            return typedSetting;
        }

        return defaultValue;
    }

    public TomlSetting<T> GetSetting<T>(string name)
    {
        if (Settings.TryGetValue(name, out var setting) &&
            setting is TomlSetting<T> typedSetting)
        {
            return typedSetting;
        }

        return null;
    }

    public bool TryGetSetting<T>(string name, out TomlSetting<T> setting)
    {
        if (Settings.TryGetValue(name, out var namedSetting) &&
            namedSetting is TomlSetting<T> typedSetting)
        {
            setting = typedSetting;
            return true;
        }

        setting = null;
        return false;
    }

    public TomlConfig SetSetting<T>(string name, T value, string description = null)
    {
        if (Settings.TryGetValue(name, out var namedSetting) &&
            namedSetting is TomlSetting<T> typedSetting)
        {
            typedSetting.Set(value);
        }
        else
        {
            Settings[name] = new TomlSetting<T>(name, default, description)
            {
                Value = value,
            };
        }

        return this;
    }

    public static TomlConfig FindSection(TomlConfig root, List<TomlPathSegment> path)
    {
        TomlConfig section = root;

        foreach (var segment in path)
        {
            if (!section.Sections.TryGetValue(segment, out section))
            {
                return null;
            }
        }

        return section;
    }
}
