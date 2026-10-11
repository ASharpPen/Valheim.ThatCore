using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ThatCore.Valheim.Generator;

internal static class SourceInternalizer
{
    private const string TypeAccessor = @"\w+";
    private const string TypeModifier = "(?<TypeModifier>static|sealed|abstract|partial|unsafe|readonly|ref)";
    private const string TypeDef = @"(?<TypeDef>class|struct|interface|enum|record|delegate)";
    private const string TypeAccessorPublic = @"(?<TypeAccesor>public)";

    public static Regex TypeRegex = new Regex($@"({TypeAccessor})\s+({TypeModifier})\s+({TypeDef})\s+");
    public static Regex PublicTypeRegex = new Regex($@"^\s*{TypeAccessorPublic}\s+({TypeModifier}\s+)*{TypeDef}\s+", RegexOptions.Multiline);

    private static Regex NamespaceRegex = new Regex(@"^\s*namespace\s+(?<namespace>\w+(\.\w+)*)\b", RegexOptions.Multiline);

    public static string Internalize(string source)
    {
        const string internalized = "internal";
        const int internalizedLength = 8;

        return PublicTypeRegex.Replace(source, match =>
        {
            var target = match.Groups["TypeAccesor"];

            var index = target.Index - match.Groups[0].Index;

            var matchValue = match.Value.AsSpan();
            var prefix = matchValue.Slice(0, index);
            var postfix = matchValue.Slice(index + target.Length);

            char[] result = new char[prefix.Length + internalizedLength + postfix.Length];
            Span<char> resultBuffer = result;

            prefix.CopyTo(resultBuffer);
            internalized.AsSpan().CopyTo(resultBuffer.Slice(prefix.Length, internalizedLength));
            postfix.CopyTo(resultBuffer.Slice(prefix.Length + internalizedLength));

            return new string(result);
        });
    }

    public static bool ShouldInternalize(string source, Regex[] namespacePatterns)
    {
        var match = NamespaceRegex.Match(source);

        if (!match.Success)
        {
            return false;
        }

        var sourceNamespace = match.Groups["namespace"].Value;

        return namespacePatterns.Any(x => x.IsMatch(sourceNamespace));
    }
}
