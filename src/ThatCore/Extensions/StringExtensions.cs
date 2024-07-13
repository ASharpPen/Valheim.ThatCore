using System;
using System.Collections.Generic;
using System.Linq;

namespace ThatCore.Extensions;

public enum Separator
{
    Comma,
    Slash,
    Dot,
    Newline,
}

public static class StringExtensions
{
    private static readonly char[] Comma = [','];
    private static readonly char[] Slash = ['/', '\\'];
    private static readonly char[] Dot = ['.'];
    private static readonly char[] Newline = ['\n'];

    public static List<string> SplitBy(this string value, Separator separator, bool toUpper = false)
    {
        var sep = separator switch
        {
            Separator.Comma => Comma,
            Separator.Slash => Slash,
            Separator.Dot => Dot,
            Separator.Newline => Newline,
            _ => throw new NotSupportedException(nameof(Separator))
        };

        return SplitBy(value, sep, toUpper).ToList();
    }

    public static List<string> SplitByComma(this string value, bool toUpper = false)
        => SplitBy(value, Comma, toUpper).ToList();

    public static string[] SplitBySlash(this string value, bool toUpper = false)
        => SplitBy(value, Slash, toUpper).ToArray();

    public static IEnumerable<string> SplitBy(this string value, char[] chars, bool toUpper = false)
    {
        if (value is null)
        {
            return Enumerable.Empty<string>();
        }

        var split = value
            .Trim()
            .Split(chars, StringSplitOptions.RemoveEmptyEntries);

        if ((split?.Length ?? 0) == 0)
        {
            return Enumerable.Empty<string>();
        }

        return split.Select(Clean);

        string Clean(string x)
        {
            var result = x.Trim();
            if (toUpper)
            {
                return result.ToUpperInvariant();
            }
            return result;
        }
    }

    public static bool IsEmpty(this string s)
    {
        return string.IsNullOrWhiteSpace(s);
    }

    public static bool IsNotEmpty(this string s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Trims the string.
    /// If null, returns empty string.
    /// </summary>
    public static string Normalize(this string s)
    {
        return s?.Trim() ?? string.Empty;
    }
}
