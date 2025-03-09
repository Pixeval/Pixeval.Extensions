using System;
using System.Diagnostics.CodeAnalysis;

namespace Pixeval.Extensions.SDK.Internal;

internal static class Misc
{
    [return: NotNullIfNotNull(nameof(original))]
    public static T[]? GetArray<T>(this T[]? original, out int count)
    {
        count = original?.Length ?? 0;
        return original;
    }

    [return: NotNullIfNotNull(nameof(original))]
    public static Array? GetArray(this Array? original, out int count)
    {
        count = original?.Length ?? 0;
        return original;
    }
}
