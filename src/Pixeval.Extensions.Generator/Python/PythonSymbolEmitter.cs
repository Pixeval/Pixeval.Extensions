using System.Text;

namespace Pixeval.Extensions.Generator;

internal static class PythonSymbolEmitter
{
    public static string Emit()
    {
        var builder = new StringBuilder();
        _ = builder.AppendLine(
            """
            from __future__ import annotations

            from enum import IntEnum


            class Symbol(IntEnum):
            """);

        foreach (var symbol in SymbolCatalog.Values)
            _ = builder.AppendLine($"    {symbol.Name} = {symbol.Value}");

        return builder.ToString();
    }
}
