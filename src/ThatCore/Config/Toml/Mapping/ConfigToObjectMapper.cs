using System;
using System.Collections.Generic;

namespace ThatCore.Config.Toml.Mapping;

public class ConfigToObjectMapper<T>
{
    public List<TomlPathSegment> Path { get; set; }

    public MappingInstantiation<T> Instantiation { get; set; }

    public List<IMappingInstantiationForParent<T>> SubInstantiations { get; set; }

    public T Execute(TomlConfig config)
    {
        // Traverse
        var currentSection = config;
        foreach (var segment in Path)
        {
            if (segment.SegmentType == TomlPathSegmentType.Collection)
            {
                throw new NotSupportedException("Cannot handle enumerated segments in initial path.");
            }

            currentSection = currentSection.Sections[segment];
        }

        var instance = Instantiation.Execute(currentSection);

        foreach (var subInstance in SubInstantiations)
        {
            subInstance.Execute(instance, currentSection);
        }

        return instance;
    }
}

public class MappingInstantiation<T>
{
    public Func<TomlConfig, T> Instantiation;

    public List<Action<TomlConfig, T>> InstanceActions { get; set; } = new();

    public T Execute(TomlConfig config)
    {
        T instance = Instantiation(config);

        foreach (var action in InstanceActions)
        {
            action(config, instance);
        }

        return instance;
    }
}

public class MappingInstantiation<TParent, T>
{
    public Func<TParent, TomlConfig, T> Instantiation;

    public List<Action<TomlConfig, T>> InstanceActions { get; set; } = new();

    public T Execute(TParent parent, TomlConfig config)
    {
        T instance = Instantiation(parent, config);

        foreach (var action in InstanceActions)
        {
            action(config, instance);
        }

        return instance;
    }
}

public interface IMappingInstantiationForParent<TParent>
{
    List<TomlPathSegment> SubPath { get; set; }

    void Execute(TParent parent, TomlConfig config);
}

public class MappingInstantiationForParent<TParent, T> : IMappingInstantiationForParent<TParent>
{
    public MappingInstantiation<TParent, T> Instantiation;

    public Action<TParent, T, TomlConfig> Action;

    public List<TomlPathSegment> SubPath { get; set; } = new();

    public List<IMappingInstantiationForParent<T>> SubInstantiations { get; set; } = new();

    public void Execute(TParent parent, TomlConfig config)
    {
        // Traverse config
        RecursiveScan(SubPath, 0, config);

        void RecursiveScan(List<TomlPathSegment> path, int depth, TomlConfig section)
        {
            // Check if we found the end.
            if (depth >= path.Count)
            {
                T newObj = Instantiation.Execute(parent, section);

                if (Action is not null)
                {
                    Action(parent, newObj, section);
                }

                foreach (var sub in SubInstantiations)
                {
                    sub.Execute(newObj, section);
                }

                return;
            }

            // Scan deeper
            var currentSegment = path[depth];

            if (currentSegment.SegmentType == TomlPathSegmentType.Collection)
            {
                foreach (var entry in section.Sections)
                {
                    RecursiveScan(path, ++depth, entry.Value);
                }
            }
            else if (section.Sections.TryGetValue(currentSegment, out var nextSection))
            {
                RecursiveScan(path, ++depth, nextSection);
            }
        }
    }
}