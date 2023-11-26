using System;
using ThatCore.Config.Toml.Schema;

namespace ThatCore.Config.Toml.Mapping;

public class OptionMappingBuilder<TParent, TTarget, TSource> 
    : IOptionBuilder<TTarget, TSource>
    where TParent : IMappingLayer<TTarget, TSource>, IHaveOptions<TTarget, TSource>
{
    private IMappingExecuter<TTarget> FileMappingBuilder { get; set; }

    private IMappingExecuter<TSource> SourceMappingBuilder { get; set; }
    
    private TParent Parent { get; }

    private ITomlSchemaNodeBuilder NodeBuilder { get; }

    internal OptionMappingBuilder(TParent parent, ITomlSchemaNodeBuilder nodeBuilder)
    {
        Parent = parent;
        NodeBuilder = nodeBuilder;
    }

    internal Action<TomlConfig, TTarget> BuildFileMappings()
    {
        if (FileMappingBuilder is not null &&
            FileMappingBuilder is IHaveMapping<TTarget> typedBuilder)
        {
            return typedBuilder.BuildMapping();
        }

        return null;
    }

    public IOptionBuilder<TTarget, TSource> FromFile(
        Action<IFileMappingBuilder<TTarget>> configure)
    {
        var builder = new FileMappingBuilder<TTarget>(NodeBuilder);

        FileMappingBuilder = builder;

        configure(builder);

        return this;
    }

    public IOptionBuilder<TTarget, TSource> ToFile(
        Action<ISourceMappingBuilder<TSource>> configure)
    {
        var builder = new SourceMappingBuilder<TSource>();

        SourceMappingBuilder = builder;

        configure(builder);

        return this;
    }

    internal void ExecuteFileMapping(TomlConfig config, TTarget target) => FileMappingBuilder?.Execute(config, target);

    internal void ExecuteSourceMapping(TSource source, TomlConfig config) => SourceMappingBuilder?.Execute(config, source);

    public IOptionBuilder<TTarget, TSource> AddOption() => Parent.AddOption();
}


