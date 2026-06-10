using System.Text;

namespace Pixeval.Extensions.Generator;

internal static class CppSymbolEmitter
{
    public static string Emit()
    {
        var builder = new StringBuilder();
        _ = builder.AppendLine(
            """
            #pragma once

            #include <cstdint>

            namespace pixeval::extensions
            {
                enum class Symbol : std::int32_t
                {
            """);

        foreach (var symbol in SymbolCatalog.Values)
            _ = builder.AppendLine($"        {symbol.Name} = {symbol.Value},");

        _ = builder.AppendLine(
            """
                };
            }
            """);
        return builder.ToString();
    }
}
