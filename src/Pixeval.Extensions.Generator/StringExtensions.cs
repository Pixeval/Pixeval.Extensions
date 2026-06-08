using System;

namespace Pixeval.Extensions.Generator;

internal static class StringExtensions
{
    public static string TrimStartText(this string value, string text)
    {
        return value.StartsWith(text, StringComparison.Ordinal)
            ? value[text.Length..].TrimStart()
            : value;
    }
}
