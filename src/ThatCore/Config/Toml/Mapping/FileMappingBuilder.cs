using System;
using System.Collections.Generic;
using ThatCore.Config.Toml.Schema;

namespace ThatCore.Config.Toml.Mapping;

public class FileMappingBuilder<TTarget>
    : IFileMappingBuilder<TTarget>
    , IHaveMapping<TTarget>
{
    public Action<TomlConfig, TTarget> Mapping { get; set; }

    private ITomlSchemaNodeBuilder NodeBuilder { get; }

    private List<IHaveMapping<TTarget>> SubBuilders { get; } = new();

    public FileMappingBuilder(ITomlSchemaNodeBuilder nodeBuilder)
    {
        NodeBuilder = nodeBuilder;
    }

    public IFileMappingBuilder<TTarget> Map<TOption>(
        string configName,
        TOption defaultValue,
        string description,
        Action<TOption, TTarget> fileToTargetMapping)
    {
        NodeBuilder
            .AddSetting(new TomlSetting<TOption>(configName, defaultValue, description)
            { 
                Value = defaultValue 
            });

        Mapping += (TomlConfig config, TTarget target) =>
        {
            var setting = config.GetSetting<TOption>(configName);

            if (setting is not null &&
                setting.IsSet)
            {
                fileToTargetMapping(setting.Value, target);
            }
        };

        return this;
    }

    public void Execute(TomlConfig config, TTarget target)
    {
        if (Mapping is not null)
        {
            Mapping(config, target);
        }

        foreach (var builder in SubBuilders)
        {
            builder.Execute(config, target);
        }
    }

    public Action<TomlConfig, TTarget> BuildMapping() => Execute;

    public IFileMappingBuilder<TSubTarget> Using<TSubTarget>(
        Func<TTarget, TSubTarget> selector,
        bool skipIfNull = true)
    {
        var builder = new SubFileMappingBuilder<TTarget, TSubTarget>(
            NodeBuilder, 
            selector,
            skipIfNull);

        SubBuilders.Add(builder);

        return builder;
    }
}

internal class SubFileMappingBuilder<TTarget, TSubTarget>
    : IFileMappingBuilder<TSubTarget>
    , IHaveMapping<TTarget>
{
    private ITomlSchemaNodeBuilder NodeBuilder { get; }

    private Func<TTarget, TSubTarget> SubSelector { get; }

    private Action<TomlConfig, TSubTarget> Mapping { get; set; }

    private List<IHaveMapping<TSubTarget>> SubBuilders { get; } = new();

    private bool SkipIfSubTargetNull { get; set; }

    public SubFileMappingBuilder(
        ITomlSchemaNodeBuilder nodeBuilder,
        Func<TTarget, TSubTarget> subSelector,
        bool skipIfSubTargetNull)
    {
        NodeBuilder = nodeBuilder;
        SubSelector = subSelector;
        SkipIfSubTargetNull = skipIfSubTargetNull;
    }

    public IFileMappingBuilder<TSubTarget> Map<TOption>(
        string configName,
        TOption defaultValue,
        string description,
        Action<TOption, TSubTarget> fileToTargetMapping
        )
    {
        NodeBuilder
            .AddSetting(new TomlSetting<TOption>(configName, defaultValue, description)
            {
                Value = defaultValue
            });

        Mapping += (TomlConfig config, TSubTarget target) =>
        {
            var setting = config.GetSetting<TOption>(configName);

            if (setting is not null &&
                setting.IsSet)
            {
                fileToTargetMapping(setting.Value, target);
            }
        };

        return this;
    }

    public Action<TomlConfig, TTarget> BuildMapping() => Execute;

    public void Execute(TomlConfig config, TTarget target)
    {
        var subTarget = SubSelector(target);

        if (SkipIfSubTargetNull && 
            subTarget is null)
        {
            return;
        }

        if (Mapping is not null)
        {
            Mapping(config, subTarget);
        }

        foreach (var builder in SubBuilders)
        {
            builder.Execute(config, subTarget);
        }
    }

    public IFileMappingBuilder<T> Using<T>(
        Func<TSubTarget, T> selector,
        bool skipIfNull = true)
    {
        var builder = new SubFileMappingBuilder<TSubTarget, T>(
            NodeBuilder, 
            selector,
            skipIfNull
            );

        SubBuilders.Add(builder);

        return builder;
    }
}
