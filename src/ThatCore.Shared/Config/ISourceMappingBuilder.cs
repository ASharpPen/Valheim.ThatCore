using System;

namespace ThatCore.Config;

public interface ISourceMappingBuilder
{
}

public interface ISourceMappingBuilder<TSource> : ISourceMappingBuilder
{
    ISourceMappingBuilder<TSource> Map<T>(string configName, Func<TSource, T> selector);

    ISourceMappingBuilder<TSubSource> Using<TSubSource>(
        Func<TSource, TSubSource> selector,
        bool skipIfNull = true);
}