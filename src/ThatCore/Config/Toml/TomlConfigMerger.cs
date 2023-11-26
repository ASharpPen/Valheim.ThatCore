namespace ThatCore.Config.Toml;

public static class TomlConfigMerger
{
    public static void Merge(TomlConfig source, TomlConfig destination)
    {
        MergeSettings(source, destination);

        foreach (var sourceSection in source.Sections)
        {
            var section = destination.CreateSubsection(sourceSection.Key);

            Merge(sourceSection.Value, section);
        }
    }

    private static void MergeSettings(TomlConfig source, TomlConfig destination)
    {
        foreach (var setting in source.Settings)
        {
            if (setting.Value.IsSet)
            {
                destination.SetSetting(setting.Key, setting.Value);
            }
        }
    }
}
