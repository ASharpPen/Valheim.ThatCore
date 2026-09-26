using System;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace ThatCore.Valheim.Generator;

[Generator(LanguageNames.CSharp)]
public sealed class SourceGenerator : IIncrementalGenerator
{
    internal const string Settings_Prefix = "build_property.";
    internal const string GeneratorSettings_Internalize = $"{Settings_Prefix}ThatCore_{nameof(GeneratorSettings.Internalize)}";

    private static BindingFlags SourcePropertyFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<GeneratorSettings> settings = context.AnalyzerConfigOptionsProvider.Select(static (provider, _) =>
        {
                provider.GlobalOptions.TryGetValue(GeneratorSettings_Internalize, out string internalize);

                return new GeneratorSettings() { Internalize = internalize ?? string.Empty };
        });

        context.RegisterSourceOutput(settings, static (context, settings) =>
        {
            var settingsContext = new GeneratorSettingsContext(settings);

            foreach (var type in typeof(SourceGenerator).Assembly
                .GetTypes()
                .Where(x =>
                    x.Namespace.StartsWith("ThatCore.Valheim.Generator.Generated") &&
                    x.GetCustomAttribute<SourceFileAttribute>() is not null))
            {
                SourceFile sourceFile = new(
                    hintName: type.GetProperty("HintName", SourcePropertyFlags).GetValue(null) as string,
                    source: type.GetProperty("Source", SourcePropertyFlags).GetValue(null) as string
                    );

                ApplySettings(ref sourceFile, settingsContext);

                context.AddSource(sourceFile.HintName, sourceFile.Source);
            }
        });
    }

    private static void ApplySettings(ref SourceFile source, in GeneratorSettingsContext settings)
    {
        if (settings.InternalizeNamespaces.Length > 0 &&
            SourceInternalizer.ShouldInternalize(source.Source, settings.InternalizeNamespaces))
        {
            try
            {
                source.Source = SourceInternalizer.Internalize(source.Source);
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new ArgumentOutOfRangeException(source.HintName, e);
            }
        }
    }
}
