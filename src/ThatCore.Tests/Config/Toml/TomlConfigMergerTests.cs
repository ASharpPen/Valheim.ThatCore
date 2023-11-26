using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThatCore.Config.Toml.Schema;
using ThatCore.Extensions;
using static ThatCore.Config.Toml.Mapping.MapperLayerTests;

namespace ThatCore.Config.Toml;

[TestClass]
public class TomlConfigMergerTests
{
    [TestMethod]
    public void CanMerge()
    {
        // Arrange
        TomlSchemaBuilder builder = new TomlSchemaBuilder();
        builder
            .SetLayerAsNamed()
            .AddNode("WorldSpawner")
            .SetNextLayerAsCollection()
            .GetNode()
            .AddSetting<bool?>("Enabled")
            .AddSetting<int?>("Amount.Min")
            .SetNextLayerAsNamed()
            .AddNode("TestMod")
            .AddSetting<string>("ModKey");

        var schema = builder.Build();

        // Load schema from file
        string file1 = @"
[WorldSpawner.1]
Enabled = false

[WorldSpawner.1.TestMod]
ModKey = TestKeyValue
";

        string file2 = @"
[WorldSpawner.0]
Enabled = false
Amount.Min = 5

[WorldSpawner.1.TestMod]
ModKey = TestKeyValue

[WorldSpawner.1]
Enabled = true
Amount.Min = 2

[WorldSpawner.1.TestMod]
ModKey = OtherKey";

        var toml1 = TomlSchemaFileLoader.Load(file1.SplitBy(Separator.Newline), "", schema);
        var toml2 = TomlSchemaFileLoader.Load(file2.SplitBy(Separator.Newline), "", schema);

        // Act
        TomlConfigMerger.Merge(toml1, toml2);

        // Verify
        var topLayer = toml2.Sections.First().Value;
        topLayer.Sections.Count.Should().Be(2);

        var sub1 = topLayer.Sections.Values.ToList()[0];
        var sub2 = topLayer.Sections.Values.ToList()[1];

        sub1.Settings["Enabled"].GetValue().Should().Be(false);
        sub2.Settings["Enabled"].GetValue().Should().Be(false);
    }
}
