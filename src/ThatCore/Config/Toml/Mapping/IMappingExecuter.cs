namespace ThatCore.Config.Toml.Mapping;

internal interface IMappingExecuter<TTarget>
{
    void Execute(TomlConfig config, TTarget target);
}
