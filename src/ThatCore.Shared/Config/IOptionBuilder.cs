using System;

namespace ThatCore.Config;

public interface IOptionBuilder<TTarget, TSource> : IOptionBuilderActions<TTarget, TSource>
{
    IOptionBuilder<TTarget, TSource> AddOption();
}

public interface IOptionBuilderActions<TTarget, TSource>
{
    IOptionBuilder<TTarget, TSource> FromFile(
        Action<IFileMappingBuilder<TTarget>> configure);

    IOptionBuilder<TTarget, TSource> ToFile(
        Action<ISourceMappingBuilder<TSource>> configure);
}

