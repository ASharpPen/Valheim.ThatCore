using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThatCore.Config.Toml.Schema;
using ThatCore.Extensions;

namespace ThatCore.Config.Toml.Mapping;

[TestClass]
public class MapperLayerTests
{
    [TestMethod]
    public void Test()
    {
        // Define toml layers
        var builder = new TomlSchemaBuilder();

        var topLayer = builder.SetLayerAsNamed()
            .AddNode("WorldSpawner");

        var prefabLayer = topLayer
            .SetNextLayerAsCollection()
            .GetNode();

        var modLayer = prefabLayer
            .SetNextLayerAsNamed();

        // Create mapping layers for config
        var prefabLayerMappings = new MappingLayer<Template, SpawnEntry, SpawnEntry>(
            TomlPathSegmentType.Collection,
            x => x.Index.ToString(),
            prefabLayer);

        var testModLayerMappings = new MappingLayer<Template, SpawnEntry, SpawnEntry>(
            TomlPathSegmentType.Named,
            _ => "TestMod",
            modLayer.AddNode("TestMod"));

        // Create mapping
        prefabLayerMappings
            .AddOption()
            .FromFile(
                config => config
                .Map<bool?>(
                    "Enabled",
                    true,
                    "Test Description",
                    (value, entry) => entry.Enabled = value))
            .ToFile(x => x.Map("Enabled", x => x.Enabled))

            .AddOption()
            .FromFile(config => config
                .Using((SpawnEntry entry) => entry.Amount ??= new())
                .Map<int?>(
                    "Amount.Min",
                    1,
                    "Amount Min Desc",
                    (value, entry) => entry.Min = value)
                .Map<int?>(
                    "Amount.Max",
                    1,
                    "Amount Max Desc",
                    (value, entry) => entry.Max = value))
            .ToFile(c => c
                .Using(x => x.Amount)
                .Map("Amount.Min", x => x.Min)
                .Map("Amount.Max", x => x.Max))
            ;

        testModLayerMappings
            .AddOption()
            .FromFile(config => config
                .Using((SpawnEntry entry) =>
                {
                    var modSetting = new TestModSetting();
                    entry.GenericSettings.Add(modSetting);
                    return modSetting;
                })
                .Map<string>(
                    "ModKey",
                    null,
                    "Some desc",
                    (value, entry) => entry.Key = value)
                .Map<string>(
                    "ModValue",
                    null,
                    "Some other Desc",
                    (value, entry) => entry.Value = value))
            .ToFile(c => c
                .Using(x => x.GenericSettings.OfType<TestModSetting>().FirstOrDefault())
                .Map("ModKey", x => x.Key)
                .Map("ModValue", x => x.Value))
            ;

        // Setup fake mapper
        var front = new MappingFrontend()
        {
            TomlBuilder = builder,
            TopNode = topLayer,
            EntryNode = prefabLayer,
            ModNodes = new()
            {
                { "TestMod", modLayer.AddNode("TestMod") }
            },
            EntryLayer = prefabLayerMappings,
            ModLayer = testModLayerMappings,
        };

        // Test file to template mapping
        var templateMapper = front.CreateMapper(new Template());

        var schema = builder.Build();

        // Load schema from file
        string file = @"
[WorldSpawner.1]
Enabled = false
Amount.Min = 5

[WorldSpawner.1.TestMod]
ModKey = TestKeyValue
";

        var config = TomlSchemaFileLoader.Load(file.SplitBy(Separator.Newline), "test.cfg", schema);

        var resultTemplate = templateMapper.Execute(config);

        // Test template to file mapping
        var fileResult = front.MapToConfigFromTemplates(resultTemplate.Entries.Values);

        var fileStringResult = TomlConfigWriter.WriteToString(fileResult, new());

        var fileStringResult2 = TomlConfigWriter.WriteToString(config, new());

        fileStringResult.Trim().Should().Be(file.Trim());
    }

    public class MappingFrontend
    {
        public TomlSchemaBuilder TomlBuilder { get; set; }

        public ITomlSchemaNodeBuilder TopNode { get; set; }

        public ITomlSchemaNodeBuilder EntryNode { get; set; }

        public Dictionary<string, ITomlSchemaNodeBuilder> ModNodes { get; set; }

        public MappingLayer<Template, SpawnEntry, SpawnEntry> EntryLayer { get; set; }

        public MappingLayer<Template, SpawnEntry, SpawnEntry> ModLayer { get; set; }

        public ConfigToObjectMapper<Template> Mapper { get; set; }

        public ConfigToObjectMapper<Template> CreateMapper(Template template)
        {
            var modLayerMapper = new MappingInstantiationForParent<SpawnEntry, SpawnEntry>
            {
                SubPath = new()
                {
                    TomlPath.Create(TomlPathSegmentType.Named, "TestMod")
                },
                Instantiation = new()
                {
                    Instantiation = (SpawnEntry parent, TomlConfig config) => parent,
                    InstanceActions = new()
                    {
                        ModLayer.BuildMapping()
                    }
                },
            };

            var entryLayerMapper = new MappingInstantiationForParent<Template, SpawnEntry>
            {
                SubPath = new()
                {
                    TomlPath.Create(TomlPathSegmentType.Collection)
                },
                Instantiation = new()
                {
                    Instantiation = (Template parent, TomlConfig config) =>
                    {
                        var index = int.Parse(config.PathSegment.Name);
                        
                        var entry = new SpawnEntry()
                        {
                            Index = index
                        };

                        parent.Entries.Add(index, entry);

                        return entry;
                    },
                    InstanceActions = new() 
                    { 
                        EntryLayer.BuildMapping(),
                    },
                },
                SubInstantiations = new()
                {
                    modLayerMapper
                }
            };

            Mapper = new ConfigToObjectMapper<Template>()
            {
                Path = new()
                {
                    TomlPath.Create(TomlPathSegmentType.Named, "WorldSpawner")
                },
                Instantiation = new()
                {
                    Instantiation = (_) => template,
                },
                SubInstantiations = new()
                {
                    entryLayerMapper,
                }
            };

            return Mapper;
        }

        public TomlConfig MapToConfigFromTemplates(IEnumerable<SpawnEntry> entries)
        {
            TomlConfig config = new();
            
            var topConfig = config.CreateSubsection(
                config.PathSegment.Chain(TomlPathSegmentType.Named, "WorldSpawner"));

            foreach(var entry in entries)
            {
                var entryLayerConfig = EntryLayer.Execute(entry, topConfig);
                ModLayer.Execute(entry, entryLayerConfig);
            }

            return config;
        }
    }

    public class Template
    {
        public Dictionary<int, SpawnEntry> Entries { get; set; } = new();
    }

    public class SpawnEntry
    {
        public int Index { get; set; }

        public bool? Enabled { get; set; }

        public Amount Amount { get; set; }

        public List<ISpawnSetting> GenericSettings { get; set; } = new();
    }

    public class Amount
    {
        public int? Min { get; set; }

        public int? Max { get; set; }
    }

    public interface ISpawnSetting
    {
    }

    public class TestModSetting : ISpawnSetting
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}


