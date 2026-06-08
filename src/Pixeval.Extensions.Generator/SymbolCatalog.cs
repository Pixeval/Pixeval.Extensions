using System;
using System.Collections.Generic;
using System.Linq;
using FluentIcons.Common;

namespace Pixeval.Extensions.Generator;

internal static class SymbolCatalog
{
    public static IReadOnlyList<SymbolValue> Values { get; } =
    [
        .. Enum.GetNames<Symbol>()
            .Select(static name => new SymbolValue(name, (int) Enum.Parse<Symbol>(name)))
            .OrderBy(static symbol => symbol.Value)
            .ThenBy(static symbol => symbol.Name, StringComparer.Ordinal)
    ];
}

internal sealed record SymbolValue(string Name, int Value);
