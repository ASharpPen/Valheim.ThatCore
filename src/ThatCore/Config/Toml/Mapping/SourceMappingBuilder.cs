using System;
using System.Collections.Generic;

namespace ThatCore.Config.Toml.Mapping;

public class SourceMappingBuilder<TSource> 
    : ISourceMappingBuilder<TSource>
    , IMappingExecuter<TSource>
{
    private Action<TSource, TomlConfig> Mapping { get; set; }

    private List<IMappingExecuter<TSource>> SubBuilders { get; } = new();

    public ISourceMappingBuilder<TSource> Map<T>(string configName, Func<TSource, T> selector)
    {
        Mapping += (TSource source, TomlConfig config) =>
        {
            var setting = selector(source);

            if (setting is not null)
            {
                config.SetSetting(configName, setting);
            }
        };
        return this;
    }

    public void Execute(TomlConfig config, TSource source)
    {
        if (Mapping is not null)
        {
            Mapping(source, config);
        }

        foreach (var builder in SubBuilders)
        {
            builder.Execute(config, source);
        }
    }

    public ISourceMappingBuilder<TSubSource> Using<TSubSource>(
        Func<TSource, TSubSource> selector,
        bool skipIfNull = true)
    {
        var builder = new SubSourceMappingBuilder<TSource, TSubSource>(
            selector,
            skipIfNull);

        SubBuilders.Add(builder);

        return builder;
    }
}

internal class SubSourceMappingBuilder<TSource, TSubSource>
    : ISourceMappingBuilder<TSubSource>
    , IMappingExecuter<TSource>
{
    private Func<TSource, TSubSource> SubSelector { get; }

    private Action<TSubSource, TomlConfig> Mapping { get; set; }

    private List<IMappingExecuter<TSubSource>> SubBuilders { get; } = new();

    private bool SkipIfSubSourceNull { get; set; }

    public SubSourceMappingBuilder(Func<TSource, TSubSource> subSelector, bool skipIfNull)
    {
        SubSelector = subSelector;
        SkipIfSubSourceNull = skipIfNull;
    }

    public ISourceMappingBuilder<TSubSource> Map<T>(string configName, Func<TSubSource, T> selector)
    {
        Mapping += (TSubSource subSource, TomlConfig config) =>
        {
            var setting = selector(subSource);

            if (setting is not null)
            {
                config.SetSetting(configName, setting);
            }
        };

        return this;
    }

    public void Execute(TomlConfig config, TSource source)
    {
        var subSource = SubSelector(source);

        if (SkipIfSubSourceNull &&
            subSource is null)
        {
            return;
        }

        if (Mapping is not null)
        {
            Mapping(subSource, config);
        }

        foreach (var builder in SubBuilders)
        {
            builder.Execute(config, subSource);
        }
    }

    public ISourceMappingBuilder<T> Using<T>(
        Func<TSubSource, T> selector,
        bool skipIfNull = true)
    {
        var builder = new SubSourceMappingBuilder<TSubSource, T>(
            selector, 
            skipIfNull);

        SubBuilders.Add(builder);

        return builder;
    }
}