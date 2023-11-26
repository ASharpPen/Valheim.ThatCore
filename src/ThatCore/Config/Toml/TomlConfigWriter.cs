using System;
using System.Text;
using ThatCore.Config.Toml.Writers;
using ThatCore.Extensions;
using ThatCore.Logging;

namespace ThatCore.Config.Toml;

public static class TomlConfigWriter
{
    public static string WriteToString(TomlConfig config, TomlWriterSettings settings)
    {
        var stringBuilder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(settings.Header))
        {
            var headerLines = settings.Header.SplitBy(Separator.Newline);

            foreach (var headerLine in headerLines)
            {
                stringBuilder.AppendLine(headerLine);
            }

            stringBuilder.AppendLine();
        }

        WriteSubsectionsRecursively(stringBuilder, config, settings);

        return stringBuilder.ToString();
    }

    private static void WriteSubsectionsRecursively(StringBuilder builder, TomlConfig config, TomlWriterSettings settings)
    {
        foreach (var subsection in config.Sections)
        {
            // Only print sections when they have options associated.
            if (subsection.Value.Settings?.Count > 0)
            {
                builder.AppendLine($"[{subsection.Value.PathSegment.GetPath()}]");
                WriteSettings(builder, subsection.Value, settings);
                builder.AppendLine();
            }

            WriteSubsectionsRecursively(builder, subsection.Value, settings);
        }
    }

    private static void WriteSettings(StringBuilder builder, TomlConfig config, TomlWriterSettings options)
    {
        foreach (var entry in config.Settings)
        {
            var entryName = entry.Key;
            var entryValue = entry.Value;

            // Skip entries that were never used.
            if (!entryValue.IsSet)
            {
                continue;
            }

            if (options?.AddComments == true)
            {
                var descriptionLines = entryValue.Description.SplitBy(Separator.Newline);

                foreach (var descriptionLine in descriptionLines)
                {
                    builder.AppendLine("# " + descriptionLine);
                }
            }

            try
            {
                var value = TomlWriterFactory.Write(entryValue);

                builder.AppendLine($"{entryName} = {value}");
            }
            catch (Exception e)
            {
                Log.Warning?.Log($"Error while attempting to write setting '{entryName}' to file '{options.FileName}'. Skipping setting.", e);
            }
        }
    }
}
