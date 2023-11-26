using System;

namespace ThatCore.Config.Toml.Mapping;

internal interface IHaveMapping<TTarget> : IMappingExecuter<TTarget>
{
    Action<TomlConfig, TTarget> BuildMapping();
}
