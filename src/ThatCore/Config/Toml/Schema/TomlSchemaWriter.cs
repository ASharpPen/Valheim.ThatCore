using System;
using System.Text;
using ThatCore.Config.Toml.Writers;
using ThatCore.Extensions;
using ThatCore.Logging;

namespace ThatCore.Config.Toml.Schema;

public static class TomlSchemaWriter
{
    public static string WriteToString(ITomlSchemaLayer schema, TomlWriterSettings settings)
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

        WriteSectionsRecursively(stringBuilder, schema, settings, string.Empty);

        return stringBuilder.ToString();
    }

    private static void WriteSectionsRecursively(StringBuilder builder, ITomlSchemaLayer schema, TomlWriterSettings settings, string currentPath)
    {
        switch(schema)
        {
            case ITomlSchemaNamedLayer namedLayer:
                foreach (var node in namedLayer.Nodes)
                {
                    var sectionName = node.Key;
                    var sectionSettings = node.Value;

                    var path = string.IsNullOrWhiteSpace(currentPath)
                        ? sectionName
                        : string.Join(".", currentPath, sectionName);

                    WriteSectionHeader(path);
                    WriteOptions(builder, sectionSettings, settings);

                    builder.AppendLine();

                    if (sectionSettings.NextLayer is not null)
                    {
                        WriteSectionsRecursively(builder, sectionSettings.NextLayer, settings, path);
                    }
                }
                break;
            case ITomlSchemaCollectionLayer indexedLayer:
                {
                    var sectionName = "ID";
                    var sectionSettings = indexedLayer.Node;

                    var path = string.IsNullOrWhiteSpace(currentPath)
                        ? sectionName
                        : string.Join(".", currentPath, sectionName);

                    WriteSectionHeader(path);
                    WriteOptions(builder, sectionSettings, settings);

                    builder.AppendLine();

                    if (sectionSettings.NextLayer is not null)
                    {
                        WriteSectionsRecursively(builder, sectionSettings.NextLayer, settings, path);
                    }
                }
                break;
            default:
                throw new NotSupportedException(schema.GetType().FullName);
        }

        void WriteSectionHeader(string sectionName)
        {
            builder.AppendLine($"[{sectionName}]");
        }
    }

    private static void WriteOptions(StringBuilder builder, ITomlSchemaNode node, TomlWriterSettings settings)
    {
        var entries = node.Settings;

        foreach (var entry in entries)
        {
            var entryName = entry.Key;
            var entryValue = entry.Value;

            if (settings?.AddComments == true)
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
                Log.Warning?.Log($"Error while attempting to write setting '{entryName}' to file '{settings.FileName}'. Skipping setting.", e);
            }
        }
    }
}
