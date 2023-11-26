using System;

namespace ThatCore.Config;

public interface IFileMappingBuilder
{
}

public interface IFileMappingBuilder<TTarget> : IFileMappingBuilder
{
    IFileMappingBuilder<TTarget> Map<TOption>(
        string configName,
        TOption defaultValue,
        string description,
        Action<TOption, TTarget> fileToTargetMapping);

    IFileMappingBuilder<TSubTarget> Using<TSubTarget>(
        Func<TTarget, TSubTarget> selector,
        bool skipIfNull = true);
}