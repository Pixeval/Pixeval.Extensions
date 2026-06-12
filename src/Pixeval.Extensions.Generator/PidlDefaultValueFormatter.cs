using System;
using System.Text.RegularExpressions;
using Pixeval.Extensions.Generator.Models;

namespace Pixeval.Extensions.Generator;

internal static partial class PidlDefaultValueFormatter
{
    public static string CSharp(string value)
    {
        ValidateSimpleLiteral(value);
        return value.Trim();
    }

    public static string Python(string value, MethodDefinition method)
    {
        var literal = value.Trim();
        ValidateSimpleLiteral(literal);

        return literal switch
        {
            "null" => "None",
            "default" => PythonDefaultValue(method),
            "true" => "True",
            "false" => "False",
            _ when IsNumericLiteral(literal) => NormalizePythonNumber(literal),
            _ => literal
        };
    }

    public static string Cpp(string value, string pidlType)
    {
        var literal = value.Trim();
        ValidateSimpleLiteral(literal);

        return literal switch
        {
            "null" => "nullptr",
            "default" => CppDefaultValue(pidlType),
            _ when IsStringLiteral(literal) => "u" + literal,
            _ when IsNumericLiteral(literal) => NormalizeCppNumber(literal),
            _ => literal
        };
    }

    public static string CppType(string pidlType, string value)
    {
        var literal = value.Trim();
        ValidateSimpleLiteral(literal);

        if (literal is "null")
            return "decltype(nullptr)";

        if (literal is "default" && pidlType is "string" or "string?")
            return "decltype(nullptr)";

        if (IsStringLiteral(literal))
            return "std::u16string_view";

        return pidlType switch
        {
            "bool" => "bool",
            "byte" => "std::uint8_t",
            "short" => "std::int16_t",
            "ushort" => "std::uint16_t",
            "int" => "std::int32_t",
            "uint" => "std::uint32_t",
            "long" => "std::int64_t",
            "ulong" => "std::uint64_t",
            "double" => "double",
            _ when pidlType.EndsWith("Type", StringComparison.Ordinal) => ShortName(pidlType),
            _ => ShortName(pidlType)
        };
    }

    private static void ValidateSimpleLiteral(string value)
    {
        if (value is "null" or "default" or "true" or "false" ||
            IsStringLiteral(value) ||
            IsNumericLiteral(value))
        {
            return;
        }

        throw new InvalidOperationException($"Unsupported PIDL default value: {value}");
    }

    private static string PythonDefaultValue(MethodDefinition method)
    {
        if (method.ReturnDictionary is not null)
            return "{}";

        if (method.ReturnDateTimeOffset is not null)
            return "(0, 0)";

        if (method.ReturnArrayCountName is not null)
        {
            return method.ReturnType switch
            {
                "byte[]" or "byte[]?" => "b\"\"",
                _ => "[]"
            };
        }

        return method.ReturnType switch
        {
            "string" or "string?" => "None",
            "bool" => "False",
            "IStream" => "_abi.Stream(None)",
            _ => "0"
        };
    }

    private static string CppDefaultValue(string pidlType)
    {
        return pidlType switch
        {
            "string" or "string?" => "nullptr",
            "bool" => "false",
            "double" => "0",
            _ when pidlType.EndsWith("Type", StringComparison.Ordinal) => $"{ShortName(pidlType)}{{}}",
            _ => "0"
        };
    }

    private static string NormalizePythonNumber(string value)
    {
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("+0x", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("-0x", StringComparison.OrdinalIgnoreCase))
        {
            return TrimUnsignedSuffix(value);
        }

        return TrimFloatingSuffix(TrimUnsignedSuffix(value));
    }

    private static string NormalizeCppNumber(string value)
    {
        return TrimCSharpOnlyNumericSuffix(value);
    }

    private static string TrimUnsignedSuffix(string value)
    {
        return value.EndsWith('u') || value.EndsWith('U') ? value[..^1] : value;
    }

    private static string TrimFloatingSuffix(string value)
    {
        return value.EndsWith('d') || value.EndsWith('D') ||
               value.EndsWith('f') || value.EndsWith('F') ||
               value.EndsWith('m') || value.EndsWith('M')
            ? value[..^1]
            : value;
    }

    private static string TrimCSharpOnlyNumericSuffix(string value)
    {
        return value.EndsWith('d') || value.EndsWith('D') ||
               value.EndsWith('m') || value.EndsWith('M')
            ? value[..^1]
            : value;
    }

    private static bool IsStringLiteral(string value)
    {
        return value.Length >= 2 && value[0] is '"' && value[^1] is '"';
    }

    private static bool IsNumericLiteral(string value)
    {
        return NumericLiteralRegex().IsMatch(value);
    }

    private static string ShortName(string fullName)
    {
        var separator = fullName.LastIndexOf('.');
        return separator < 0 ? fullName : fullName[(separator + 1)..];
    }

    [GeneratedRegex("""^[+-]?(?:0[xX][0-9a-fA-F]+|(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?)(?:[uUlLfFdDmM])?$""")]
    private static partial Regex NumericLiteralRegex();
}
