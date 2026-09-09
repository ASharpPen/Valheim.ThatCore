using Microsoft.CodeAnalysis;
using ThatCore.Valheim.Generator.Generated;

namespace ThatCore.Valheim.Generator;

[Generator(LanguageNames.CSharp)]
public sealed class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context) => 
        context.RegisterPostInitializationOutput(SourceManifest.AddSourceTexts);
}