using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThatCore.Config.Toml.Schema;
using ThatCore.Extensions;

namespace ThatCore.Config.Toml.Mapping;

[TestClass]
public class SectionToObjectMappingTests
{
    [TestMethod]
    public void Test()
    {
        // Prepare schema
        var builder = new TomlSchemaBuilder();

        builder.SetLayerAsNamed()
            .AddNode("WorldSpawner")
            .SetNextLayerAsCollection()
            .GetNode()
            .AddSetting(new TomlSetting<bool?>("Enabled", true))
            .AddSetting(new TomlSetting<int?>("Amount.Min", 1))
            .AddSetting(new TomlSetting<int?>("Amount.Max", 3))
            .SetNextLayerAsNamed();

        var schema = builder.Build();

        // Load schema from file
        string file = @"
[WorldSpawner.1]
Enabled = false
Amount.Min = 5
";

        var config = TomlSchemaFileLoader.Load(file.SplitBy(Separator.Newline), "test.cfg", schema);

        // Register mappings
        var entryMappings = new MappingInstantiationForParent<Template, SpawnEntry>()
        {
            SubPath = new()
            {
                new TomlPathSegment(TomlPathSegmentType.Collection),
            },
            Instantiation = new()
            {
                Instantiation = (Template, config) => new SpawnEntry()
            },
            Action = (Template mainSettings, SpawnEntry instance, TomlConfig config) =>
            {
                var id = int.Parse(config.PathSegment.Name);
                mainSettings.Entries.Add(id, instance);
            }
        };

        var mapper = new ConfigToObjectMapper<Template>()
        {
            Path = new List<TomlPathSegment>
            {
                new TomlPathSegment(TomlPathSegmentType.Named, "WorldSpawner"),
            },
            Instantiation = new()
            {
                Instantiation = (TomlConfig config) =>
                {
                    return new();
                }
            },
            SubInstantiations = new()
            {
                entryMappings
            }
        };

        entryMappings.Instantiation.InstanceActions.Add(
            (TomlConfig section, SpawnEntry settings) =>
            {
                if (section.Settings.TryGetValue(nameof(settings.Enabled), out var setting))
                {
                    settings.Enabled = (bool?)setting.GetValue();
                }
            });
        entryMappings.Instantiation.InstanceActions.Add(
            (TomlConfig section, SpawnEntry settings) =>
            {
                if (section.Settings.TryGetValue("Amount.Min", out var setting))
                {
                    settings.Amount ??= new();
                    settings.Amount.Min = (int?)setting.GetValue();
                }
            });

        // Map config to object
        var template = mapper.Execute(config);

        Assert.IsNotNull(template);
        Assert.AreEqual(1, template.Entries.Count);
        Assert.IsTrue(template.Entries.ContainsKey(1));
        Assert.AreEqual(false, template.Entries[1].Enabled);
        Assert.IsNotNull(template.Entries[1].Amount);
        Assert.AreEqual(5, template.Entries[1].Amount.Min);
    }

    public class Template
    {
        public Dictionary<int, SpawnEntry> Entries { get; set; } = new();
    }

    public class SpawnEntry
    {
        public bool? Enabled { get; set; }

        public Amount Amount { get; set; }
    }

    public class Amount
    {
        public int? Min { get; set; }

        public int? Max { get; set; }
    }
}
