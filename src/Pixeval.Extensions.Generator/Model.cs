using System;
using System.Collections.Generic;

namespace Pixeval.Extensions.Generator;

internal sealed class PidlModel
{
    public List<EnumDefinition> Enums { get; } = [];

    public List<InterfaceDefinition> Interfaces { get; } = [];

    public required PidlMetadata Metadata { get; set; }

    public string Version => Metadata.Version;

    public string SdkVersion
    {
        get
        {
            var version = System.Version.Parse(Version);
            return string.Join(
                ".",
                version.Major,
                version.Minor,
                version.Build < 0 ? 0 : version.Build,
                version.Revision < 0 ? 0 : version.Revision);
        }
    }
}

internal sealed class PidlMetadata
{
    public required string Version { get; init; }

    public required StringMarshallingMode StringMarshalling { get; init; }

    public required BoolMarshallingMode BoolMarshalling { get; init; }
}

internal enum StringMarshallingMode
{
    Utf16
}

internal enum BoolMarshallingMode
{
    BOOL
}

internal sealed class EnumDefinition(string fullName)
{
    public string FullName { get; } = fullName;

    public List<string> Documentation { get; } = [];

    public List<EnumValue> Values { get; } = [];
}

internal sealed class EnumValue(string name, string? value)
{
    public string Name { get; } = name;

    public string? Value { get; } = value;
}

internal sealed class InterfaceDefinition(string fullName)
{
    public string FullName { get; } = fullName;

    public List<PidlAttribute> Attributes { get; } = [];

    public List<string> Documentation { get; } = [];

    public string? Guid { get; set; }

    public string? Inherits { get; set; }

    public string? Editor { get; set; }

    public string? Special { get; set; }

    public string? SdkName { get; set; }

    public List<MethodDefinition> Methods { get; } = [];
}

internal sealed class MethodDefinition(string returnType, string name, List<ParameterDefinition> parameters)
{
    public string ReturnType { get; set; } = returnType;

    public string Name { get; } = name;

    public List<PidlAttribute> Attributes { get; } = [];

    public List<ParameterDefinition> Parameters { get; } = parameters;

    public List<string> Documentation { get; } = [];

    public bool Hidden { get; set; }

    public bool IsAsync { get; set; }

    public string? AsyncReturnType { get; set; }

    public string? AsyncResultGetterName { get; set; }

    public bool IsAsyncResultGetter { get; set; }

    public MethodDefinition? AsyncSourceMethod { get; set; }

    public string? ReturnArrayCountName { get; set; }

    public DictionaryExpansion? ReturnDictionary { get; set; }

    public bool ReturnIsBuiltInStream { get; set; }

    public DateTimeOffsetExpansion? ReturnDateTimeOffset { get; set; }

    public string? PropertyName { get; set; }

    public List<DateTimeOffsetExpansion> DateTimeOffsetParameters { get; } = [];

    public List<DictionaryExpansion> DictionaryParameters { get; } = [];

    public HashSet<string> ParamIn { get; } = new(StringComparer.Ordinal);

    public HashSet<string> ParamOut { get; } = new(StringComparer.Ordinal);

}

internal sealed class DateTimeOffsetExpansion(string name, string ticksName, string offsetName)
{
    public string Name { get; } = name;

    public string TicksName { get; } = ticksName;

    public string OffsetName { get; } = offsetName;
}

internal sealed class DictionaryExpansion(string name, string keyType, string valueType, string keysName, string valuesName, string countName)
{
    public string Name { get; } = name;

    public string KeyType { get; } = keyType;

    public string ValueType { get; } = valueType;

    public string KeysName { get; } = keysName;

    public string ValuesName { get; } = valuesName;

    public string CountName { get; } = countName;
}

internal sealed class ParameterDefinition(string type, string name, bool isOut, string? defaultValue)
{
    public string Type { get; set; } = type;

    public string Name { get; } = name;

    public bool IsOut { get; } = isOut;

    public string? DefaultValue { get; } = defaultValue;

    public string? ArrayCountName { get; set; }

    public bool IsGeneratedArrayCount { get; set; }

    public DictionaryExpansion? Dictionary { get; set; }

    public DictionaryElementKind DictionaryElementKind { get; set; }

    public bool IsBuiltInStream { get; set; }
}

internal enum DictionaryElementKind
{
    None,
    Keys,
    Values,
    Count
}

internal sealed class PidlAttribute(string name, string? value)
{
    public string Name { get; } = name;

    public string? Value { get; } = value;
}
