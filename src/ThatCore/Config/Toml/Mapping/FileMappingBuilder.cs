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
        bool skipIfNull = true,
        bool skipIfNoSettingsSet = true)
    {
        var builder = new SubFileMappingBuilder<TTarget, TSubTarget>(
            NodeBuilder, 
            selector,
            skipIfNull,
            skipIfNoSettingsSet);

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

    private Action<ExecutionContext> Mapping { get; set; }

    private List<IHaveMapping<TSubTarget>> SubBuilders { get; } = new();

    private bool SkipIfSubTargetNull { get; set; }

    private bool SkipIfNoSettingsSet { get; set; }

    private sealed class ExecutionContext(SubFileMappingBuilder<TTarget, TSubTarget> builder)
    {
        public Func<TTarget, TSubTarget> SubSelector = builder.SubSelector;
        public bool SkipIfSubTargetNull = builder.SkipIfSubTargetNull;
        public TomlConfig Config;
        public TTarget Target;
        public TSubTarget SubSelected;
    }

    public SubFileMappingBuilder(
        ITomlSchemaNodeBuilder nodeBuilder,
        Func<TTarget, TSubTarget> subSelector,
        bool skipIfSubTargetNull,
        bool skipIfNoSettingsSet)
    {
        NodeBuilder = nodeBuilder;
        SubSelector = subSelector;
        SkipIfSubTargetNull = skipIfSubTargetNull;
        SkipIfNoSettingsSet = skipIfNoSettingsSet;
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

        Mapping += (ExecutionContext context) =>
        {
            var setting = context.Config.GetSetting<TOption>(configName);

            if (setting is not null &&
                setting.IsSet)
            {
                context.SubSelected ??= context.SubSelector(context.Target);

                if (context.SkipIfSubTargetNull && 
                    context.SubSelected is null)
                {
                    return;
                }

                fileToTargetMapping(setting.Value, context.SubSelected);
            }
        };

        return this;
    }

    public Action<TomlConfig, TTarget> BuildMapping() => Execute;

    public void Execute(TomlConfig config, TTarget target)
    {
        ExecutionContext context = new(this)
        {
            Config = config,
            Target = target,
            SubSelected = SkipIfNoSettingsSet ? default : SubSelector(target),
        };

        if (!SkipIfNoSettingsSet &&
            SkipIfSubTargetNull &&
            context.SubSelected is null)
        {
            return;
        }

        if (Mapping is not null)
        {
            Mapping(context);
        }

        foreach (var builder in SubBuilders)
        {
            builder.Execute(config, context.SubSelected);
        }
    }

    public IFileMappingBuilder<T> Using<T>(
        Func<TSubTarget, T> selector,
        bool skipIfNull = true,
        bool skipIfNoSettingsSet = true)
    {
        var builder = new SubFileMappingBuilder<TSubTarget, T>(
            NodeBuilder, 
            selector,
            skipIfNull,
            skipIfNoSettingsSet
            );

        SubBuilders.Add(builder);

        return builder;
    }
}
