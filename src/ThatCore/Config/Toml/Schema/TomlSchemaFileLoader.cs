using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ThatCore.Config.Toml.Parsers;
using ThatCore.Extensions;
using ThatCore.Logging;

namespace ThatCore.Config.Toml.Schema;

public static class TomlSchemaFileLoader
{
    private static readonly Regex SectionHeader = new Regex(@"^\s*[[](.+)[]]", RegexOptions.Compiled);
    private static readonly Regex SectionSanitizer = new Regex(@"[^\p{L}\d.\[\]_]|\s", RegexOptions.Compiled);

    private static readonly Regex SettingIdentifier = new Regex(@"^\s*([\p{L}.\d]+)\s*=(.*)", RegexOptions.Compiled);
    private static readonly Regex CommentIdentifier = new Regex(@"^\s*(//|#|--)", RegexOptions.Compiled);

    public static TomlConfig LoadFile(string path, ITomlSchemaLayer schema)
    {
        var lines = File.ReadAllLines(path);

        return Load(lines, Path.GetFileName(path), schema);
    }

    public static TomlConfig Load(IList<string> lines, string fileName, ITomlSchemaLayer schema)
    {
        var configFile = new TomlConfig();

        int currentLine = 1;
        string currentSection = null;
        List<TomlPathSegment> currentSectionParts;
        TomlConfig currentSectionConfig = configFile;
        ITomlSchemaNode currentSchemaNode = null;

        TomlLine fileLine = new();

        for (; currentLine <= lines.Count; ++currentLine)
        {
            var line = lines[currentLine - 1];

            // Skip empty lines.
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            // Check for comment
            if (CommentIdentifier.IsMatch(line))
            {
                continue;
            }

            // Set current line data 
            fileLine.FileName = fileName;
            fileLine.LineNr = currentLine;
            fileLine.Value = line;

            // Check for header
            var sectionHeaderMatch = SectionHeader.Match(line);
            if (sectionHeaderMatch.Success)
            {
                var section = sectionHeaderMatch.Groups[1].Value;

                // Run cleanup and sanitizing
                var sanitized = SectionSanitizer.Replace(section, "");

                // Warn about potential issues in header
                if (string.IsNullOrWhiteSpace(sanitized))
                {
                    Log.Warning?.Log($"{fileName}, Line {currentLine}: Section name '{section}' empty after sanitizing. Unable to load section.");
                    currentSection = null;
                    continue;
                }

                // Grab section and parts
                currentSection = sanitized;
                currentSectionParts = ConvertToPath(sanitized, schema, fileLine);
                currentSectionConfig =  GetOrCreateSection(configFile, currentSectionParts);
                currentSchemaNode = FindNode(schema, currentSectionParts);

                if (currentSectionConfig is null)
                {
                    // Warn about unknown section
                    Log.Warning?.Log($"{fileName}, Line {currentLine}: Unable to find valid config for section '{sanitized}'.");
                }

                continue;
            }

            // Check for setting
            var settingMatch = SettingIdentifier.Match(line);
            if (settingMatch.Success)
            {
                var settingName = settingMatch.Groups[1].Value;
                var settingValue = settingMatch.Groups[2].Value;

                if (string.IsNullOrWhiteSpace(currentSection))
                {
                    // Log warning about settings outside section scope.
                    Log.Warning?.Log($"{fileName}, Line {currentLine}: Setting '{settingName}' was not inside any section. Ignoring setting.");
                    continue;
                }
                else if (currentSectionConfig is null)
                {
                    Log.Warning?.Log($"{fileName}, Line {currentLine}: Skipping setting '{settingName}' due to not finding valid config for section.");
                    continue;
                }

                // Look up config using section path.
                // Find matching entry in config
                if (currentSectionConfig.Settings.TryGetValue(StringExtensions.Normalize(settingName), out var configEntry))
                {
                    ParseSetting(configEntry, settingValue, fileLine);
                }
                else
                {
                    // Try find setting in schema
                    if (currentSchemaNode?.Settings.TryGetValue(StringExtensions.Normalize(settingName), out var schemaSetting) == true)
                    {
                        var setting = schemaSetting.Clone(false);
                        ParseSetting(setting, settingValue, fileLine);

                        currentSectionConfig.SetSetting(settingName, setting);
                    }
                    else
                    {
                        // Log warning about unknown entry if no entry matches.
                        Log.Warning?.Log($"{fileName}, Line {currentLine}: Setting '{settingName}' did not match any known setting for section '{currentSectionConfig.PathSegment.GetPath()}'. Ignoring setting.");
                    }
                }

                static void ParseSetting(ITomlSetting setting, string value, TomlLine fileLine)
                {
                    try
                    {
                        setting.Read(
                            new()
                            {
                                FileName = fileLine.FileName,
                                LineNr = fileLine.LineNr,
                                Value = value
                            });
                    }
                    catch (Exception e)
                    {
                        Log.Warning?.Log($"{fileLine.FileName}, Line {fileLine.Value}: Unexpected error during parsing.", e);
                    }
                }

                continue;
            }

            // Log warning about unknown text line if we reached this far.
            Log.Warning?.Log($"{fileName}, Line {currentLine}: Unknown text '{line}' did not match any known format. Ignoring setting.");
        }

        return configFile;
    }

    private static List<TomlPathSegment> ConvertToPath(string header, ITomlSchemaLayer schema, TomlLine fileLine)
    {
        var segments = header.SplitBy(Separator.Dot);

        List<TomlPathSegment> pathSegments = new(segments.Count + 1);

        ITomlSchemaLayer currentLayer = schema;
        TomlPathSegment currentSegment = TomlPathSegment.Default;

        pathSegments.Add(currentSegment);

        foreach(var segment in segments)
        {
            switch (currentLayer)
            {
                case ITomlSchemaNamedLayer named:
                    if (named.Nodes.TryGetValue(StringExtensions.Normalize(segment), out var segmentNode))
                    {
                        currentSegment = new TomlPathSegment(TomlPathSegmentType.Named, segment, currentSegment);
                        pathSegments.Add(currentSegment);

                        currentLayer = segmentNode.NextLayer;
                    }
                    else
                    {
                        Log.Warning?.Log($"{fileLine.FileName}, Line {fileLine.LineNr}: Unable to find valid section with name '{header}'.");
                        return null;
                    }

                    break;
                case ITomlSchemaCollectionLayer indexed:
                    currentSegment = new TomlPathSegment(TomlPathSegmentType.Collection, segment, currentSegment);
                    pathSegments.Add(currentSegment);
                    
                    currentLayer = indexed.Node.NextLayer;
                    break;
            }
        }

        return pathSegments;
    }

    private static TomlConfig GetOrCreateSection(TomlConfig configFile, List<TomlPathSegment> path)
    {
        TomlConfig config = configFile;

        foreach (var segment in path.Skip(1)) // Skip the start section itself.
        {
            config = config.CreateSubsection(segment);
        }

        return config;
    }

    private static ITomlSchemaNode FindNode(ITomlSchemaLayer schema, List<TomlPathSegment> sectionParts)
    {
        ITomlSchemaLayer current = schema;
        ITomlSchemaNode currentNode = null;

        foreach (var sectionPart in sectionParts.Skip(1)) // Skip the start segment
        {
            switch (current)
            {
                case ITomlSchemaCollectionLayer indexed:
                    // TODO: Validate that section part is a number

                    current = indexed.Node.NextLayer;
                    currentNode = indexed.Node;
                    break;
                case ITomlSchemaNamedLayer named:
                    if (named.Nodes.TryGetValue(sectionPart.NormalizedName, out var node))
                    {
                        current = node.NextLayer;
                        currentNode = node;
                    }
                    break;
                default:
                    return null;
            }
        }

        return currentNode;
    }
}
