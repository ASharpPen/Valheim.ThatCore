
namespace ThatCore.Config.Toml.Schema;

/*
internal class TomlSchemaBuilderTests
{
    [Test]
    public void Test()
    {
        var builder = new TomlSchemaBuilder();

        var worldSpawner = builder.SetLayerAsNamed().AddNode("WorldSpawner");

        var worldSpawner_ID = worldSpawner.SetNextLayerAsIndexed().GetNode();

        var worldSpawner_ID_Layer = worldSpawner_ID.SetNextLayerAsNamed();

        var worldSpawner_ID_Mod1 = worldSpawner_ID_Layer.AddNode("Mod1");
        var worldSpawner_ID_Mod1_layer = worldSpawner_ID_Mod1.SetNextLayerAsNamed();

        var schema = builder.Build();

        var worldspawner_ID_mod1 = schema
            .Nodes["WorldSpawner"]
            .NextLayer.AsIndexed().Node
            .NextLayer.AsNamed().Nodes["Mod1"];
    }
}
*/