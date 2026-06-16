using System;
using System.Text.RegularExpressions;
using Pixeval.Extensions.Generator.Models;
using CppName = Pixeval.Extensions.Generator.TypeNames.Cpp;
using Literal = Pixeval.Extensions.Generator.TypeNames.Literal;
using PidlType = Pixeval.Extensions.Generator.TypeNames.Pidl;
using PythonType = Pixeval.Extensions.Generator.TypeNames.Python;

namespace Pixeval.Extensions.Generator;

internal static partial class PidlDefaultValueFormatter
{
    public static string CSharp(string value, string pidlType)
    {
        var literal = value.Trim();
        if (IsSimpleLiteral(literal))
            return literal;

        if (TryFormatEnumLiteral(literal, pidlType, EnumLanguage.CSharp, out var enumLiteral))
            return enumLiteral;

        throw new InvalidOperationException($"Unsupported PIDL default value: {value}");
    }

    public static string Python(string value, MethodDefinition method)
    {
        var literal = value.Trim();

        if (IsSimpleLiteral(literal))
        {
            return literal switch
            {
                Literal.Null => PythonType.None,
                Literal.Default => PythonDefaultValue(method),
                Literal.True => "True",
                Literal.False => "False",
                _ when IsNumericLiteral(literal) => NormalizePythonNumber(literal),
                _ => literal
            };
        }

        if (TryFormatEnumLiteral(literal, method.Property?.Type ?? method.ReturnType, EnumLanguage.Python, out var enumLiteral))
            return enumLiteral;

        throw new InvalidOperationException($"Unsupported PIDL default value: {value}");
    }

    public static string Cpp(string value, string pidlType)
    {
        var literal = value.Trim();
        if (IsSimpleLiteral(literal))
        {
            return literal switch
            {
                Literal.Null => CppName.Nullptr,
                Literal.Default => CppDefaultValue(pidlType),
                _ when IsStringLiteral(literal) => "u" + literal,
                _ when IsNumericLiteral(literal) => NormalizeCppNumber(literal),
                _ => literal
            };
        }

        if (TryFormatEnumLiteral(literal, pidlType, EnumLanguage.Cpp, out var enumLiteral))
            return enumLiteral;

        throw new InvalidOperationException($"Unsupported PIDL default value: {value}");
    }

    public static string CppType(string pidlType, string value)
    {
        var literal = value.Trim();
        if (!IsSimpleLiteral(literal) && !TryFormatEnumLiteral(literal, pidlType, EnumLanguage.Cpp, out _))
            throw new InvalidOperationException($"Unsupported PIDL default value: {value}");

        if (literal is Literal.Null)
            return CppName.DecltypeNullptr;

        if (literal is Literal.Default && pidlType is PidlType.String or PidlType.NullableString)
            return CppName.DecltypeNullptr;

        if (IsStringLiteral(literal))
            return CppName.U16StringView;

        return pidlType switch
        {
            PidlType.Bool => CppName.Bool,
            PidlType.Byte => CppName.UInt8,
            PidlType.Short => CppName.Int16,
            PidlType.UShort => CppName.UInt16,
            PidlType.Int => CppName.Int32,
            PidlType.UInt => CppName.UInt32,
            PidlType.Long => CppName.Int64,
            PidlType.ULong => CppName.UInt64,
            PidlType.Double => CppName.Double,
            _ when pidlType.EndsWith(PidlType.EnumTypeSuffix, StringComparison.Ordinal) => ShortName(pidlType),
            _ => ShortName(pidlType)
        };
    }

    private static bool IsSimpleLiteral(string value)
    {
        if (value is Literal.Null or Literal.Default or Literal.True or Literal.False ||
            IsStringLiteral(value) ||
            IsNumericLiteral(value))
        {
            return true;
        }

        return false;
    }

    private static bool TryFormatEnumLiteral(string value, string pidlType, EnumLanguage language, out string result)
    {
        result = "";
        if (!IsEnumTargetType(pidlType) ||
            !TrySplitEnumLiteral(value, out var explicitType, out var member))
        {
            return false;
        }

        var enumType = ShortName(explicitType ?? pidlType);
        result = language switch
        {
            EnumLanguage.CSharp => explicitType is null ? $"{enumType}.{member}" : value,
            EnumLanguage.Cpp => $"{enumType}::{member}",
            EnumLanguage.Python => enumType is PidlType.Symbol ? $"{PythonType.Symbol}.{member}" : $"_abi.{enumType}.{member}",
            _ => value
        };
        return true;
    }

    private static bool TrySplitEnumLiteral(string value, out string? enumType, out string member)
    {
        enumType = null;
        member = "";
        if (!EnumLiteralRegex().IsMatch(value))
            return false;

        var separator = value.LastIndexOf('.');
        if (separator < 0)
        {
            member = value;
            return true;
        }

        enumType = value[..separator];
        member = value[(separator + 1)..];
        return true;
    }

    private static bool IsEnumTargetType(string pidlType)
    {
        var type = pidlType.EndsWith(PidlType.NullableSuffix, StringComparison.Ordinal) ? pidlType[..^1] : pidlType;
        if (type.EndsWith(PidlType.ArraySuffix, StringComparison.Ordinal) ||
            type is PidlType.Bool or PidlType.Byte or PidlType.Short or PidlType.UShort or PidlType.Int or PidlType.UInt or PidlType.Long or PidlType.ULong or PidlType.Double or PidlType.String or PidlType.Stream or PidlType.DateTimeOffset ||
            type.Length is 0 ||
            type[0] is 'I' && type.Length > 1 && char.IsUpper(type[1]))
        {
            return false;
        }

        return char.IsUpper(ShortName(type)[0]);
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
                PidlType.ByteArray or PidlType.NullableByteArray => "b\"\"",
                _ => "[]"
            };
        }

        return method.ReturnType switch
        {
            PidlType.String or PidlType.NullableString => PythonType.None,
            PidlType.Bool => "False",
            PidlType.IStream => $"{PythonType.AbiStream}({PythonType.None})",
            _ => "0"
        };
    }

    private static string CppDefaultValue(string pidlType)
    {
        return pidlType switch
        {
            PidlType.String or PidlType.NullableString => CppName.Nullptr,
            PidlType.Bool => "false",
            PidlType.Double => "0",
            _ when pidlType.EndsWith(PidlType.EnumTypeSuffix, StringComparison.Ordinal) => $"{ShortName(pidlType)}{{}}",
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

    [GeneratedRegex("""^[A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*$""")]
    private static partial Regex EnumLiteralRegex();

    private enum EnumLanguage
    {
        CSharp,
        Cpp,
        Python
    }
}
