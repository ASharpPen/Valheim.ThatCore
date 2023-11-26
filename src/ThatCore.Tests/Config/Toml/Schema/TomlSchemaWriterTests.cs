using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThatCore.Config.Toml.Schema;

[TestClass]
public class TomlSchemaWriterTests
{
    [TestMethod]
    public void CanWriteSchema()
    {
        var builder = new TomlSchemaBuilder();

        var worldSpawner = builder.SetLayerAsNamed().AddNode("WorldSpawner");

        var worldSpawner_ID = worldSpawner.SetNextLayerAsCollection().GetNode();

        worldSpawner_ID.AddSetting("SpawnSetting1", 1, "Some description");
        worldSpawner_ID.AddSetting("SpawnSetting2", new List<string> { "Test1", "Test2"}, "Some other description");

        var worldSpawner_ID_Layer = worldSpawner_ID.SetNextLayerAsNamed();

        var worldSpawner_ID_Mod1 = worldSpawner_ID_Layer.AddNode("Mod1");
        var worldSpawner_ID_Mod1_layer = worldSpawner_ID_Mod1.SetNextLayerAsNamed();

        worldSpawner_ID_Mod1.AddSetting<bool?>("ModdedSetting", true, "Modded Setting desc");

        // Act
        var schema = builder.Build();
        var result = TomlSchemaWriter.WriteToString(schema, new() { AddComments = true });

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
    }
}
