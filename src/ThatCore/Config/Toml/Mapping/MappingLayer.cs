using System;
using System.Collections.Generic;
using System.Linq;
using ThatCore.Config.Toml.Schema;

namespace ThatCore.Config.Toml.Mapping;

public interface IMappingLayer<TTarget, TSource> : IHaveOptions<TTarget, TSource>
{
}

public class MappingLayer<TParent, TSource, TTarget> : IMappingLayer<TTarget, TSource>
{
    private List<OptionMappingBuilder<MappingLayer<TParent, TSource, TTarget>, TTarget, TSource>> Options { get; } = new();

    private List<Func<bool>> LayerRequirements { get; } = new();
    public TomlPathSegmentType SegmentType { get; }
    public Func<TSource, string> PathIdentifier { get; }
    private ITomlSchemaNodeBuilder NodeBuilder { get; set; }

    public MappingLayer(
        TomlPathSegmentType segmentType,
        Func<TSource, string> pathIdentifier,
        ITomlSchemaNodeBuilder nodeBuilder)
    {
        SegmentType = segmentType;
        PathIdentifier = pathIdentifier;
        NodeBuilder = nodeBuilder;
    }

    public IOptionBuilder<TTarget, TSource> AddOption()
    {
        var newOption = new OptionMappingBuilder<
            MappingLayer<TParent, TSource, TTarget>, TTarget, TSource>(this, NodeBuilder);

        Options.Add(newOption);

        return newOption;
    }

    public MappingLayer<TParent, TSource, TTarget> AddLayerRequirement(Func<bool> requirement)
    {
        LayerRequirements.Add(requirement);
        return this;
    }

    public Action<TomlConfig, TTarget> BuildMapping()
    {
        var layerMappings = Options
            .Select(x => x.BuildFileMappings())
            .ToList();

        return (TomlConfig config, TTarget target) =>
        {
            if (LayerRequirements.All(x => x()))
            {
                layerMappings.ForEach(x => x(config, target));
            }
        };
    }

    public TomlConfig Execute(TSource source, TomlConfig config)
    {
        TomlConfig layerConfig = config.CreateSubsection(
            TomlPath.Create(SegmentType, PathIdentifier(source)));

        foreach (var option in Options)
        {
            option.ExecuteSourceMapping(source, layerConfig);
        }

        return layerConfig;
    }
}
