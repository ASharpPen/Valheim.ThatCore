using System.Linq;
using System.Text.RegularExpressions;

namespace ThatCore.Valheim.Generator;

internal sealed record GeneratorSettings
{
    public string Internalize { get; set; }
}

internal readonly ref struct GeneratorSettingsContext
{
    public GeneratorSettingsContext(GeneratorSettings settings)
    {
        Internalize = settings.Internalize
            .Split([';'], System.StringSplitOptions.RemoveEmptyEntries);
        InternalizeNamespaces = Internalize
            .Select(x => "^" + Regex.Escape(x).Replace(@"\*", ".*").Replace(@"\?", ".") + "$")
            .Select(x => new Regex(x))
            .ToArray();
    }

    public string[] Internalize { get; }

    public Regex[] InternalizeNamespaces { get; }
}