using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Pixeval.Extensions.Generator;

internal static class PidlParser
{
    public static PidlModel Parse(IEnumerable<string> sources)
    {
        var sourceArray = sources.ToArray();
        var metadataSources = sourceArray.Select(TryParseMetadataSource).OfType<PidlMetadata>().ToArray();
        if (metadataSources.Length is not 1)
            throw new InvalidOperationException("Exactly one metadata source is required.");

        var metadata = metadataSources[0];
        var model = new PidlModel { Metadata = metadata };
        foreach (var source in sourceArray)
        {
            if (IsMetadataSource(source))
                continue;

            ParseSource(source, model);
        }

        return model;
    }

    private static void ParseSource(string source, PidlModel model)
    {
        EnumDefinition? currentEnum = null;
        InterfaceDefinition? currentInterface = null;
        string? currentNamespace = null;
        var pendingAttributes = new List<PidlAttribute>();
        var pendingDocumentation = new List<string>();

        foreach (var rawLine in source.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
        {
            var trimmedLine = rawLine.Trim();
            if (trimmedLine.Length is 0)
            {
                pendingAttributes.Clear();
                pendingDocumentation.Clear();
                continue;
            }

            if (trimmedLine.StartsWith("//", StringComparison.Ordinal))
            {
                pendingDocumentation.Add(trimmedLine["//".Length..].Trim());
                continue;
            }

            var line = CleanLine(rawLine);
            if (line.Length is 0)
                continue;

            if (IsAttribute(line))
            {
                pendingAttributes.Add(ParseAttribute(line));
                continue;
            }

            if (line is "{")
                continue;

            if (line is "}" or "end")
            {
                currentEnum = null;
                currentInterface = null;
                pendingAttributes.Clear();
                pendingDocumentation.Clear();
                continue;
            }

            if (currentEnum is not null)
            {
                currentEnum.Values.Add(ParseEnumValue(TrimStatement(line).TrimStartText("value ")));
                pendingDocumentation.Clear();
                continue;
            }

            if (currentInterface is not null)
            {
                var method = NormalizeMethod(ParseMethod(TrimStatement(line)), pendingDocumentation, pendingAttributes);
                pendingAttributes.Clear();
                currentInterface.Methods.Add(method);
                if (TryCreateAsyncResultGetter(method, out var resultGetter))
                    currentInterface.Methods.Add(resultGetter);
                continue;
            }

            if (line.StartsWith("enum ", StringComparison.Ordinal))
            {
                currentEnum = new EnumDefinition(QualifyName(TrimBlockStart(line["enum ".Length..].Trim()), currentNamespace));
                TransferDocumentation(currentEnum.Documentation, pendingDocumentation);
                pendingAttributes.Clear();
                model.Enums.Add(currentEnum);
                continue;
            }

            if (line.StartsWith("interface ", StringComparison.Ordinal))
            {
                currentInterface = ParseInterfaceDeclaration(TrimBlockStart(line["interface ".Length..].Trim()), currentNamespace);
                TransferDocumentation(currentInterface.Documentation, pendingDocumentation);
                ApplyInterfaceAttributes(currentInterface, pendingAttributes);
                pendingAttributes.Clear();
                model.Interfaces.Add(currentInterface);
                continue;
            }

            if (line.StartsWith("namespace ", StringComparison.Ordinal))
            {
                currentNamespace = TrimStatement(TrimBlockStart(line["namespace ".Length..].Trim()));
                pendingAttributes.Clear();
                pendingDocumentation.Clear();
            }
        }
    }

    private static string CleanLine(string rawLine)
    {
        var line = rawLine.Trim();
        var commentIndex = line.IndexOf("//", StringComparison.Ordinal);
        if (commentIndex >= 0)
            line = line[..commentIndex].Trim();
        return line;
    }

    private static bool IsAttribute(string line)
    {
        return line.StartsWith('[') && line.EndsWith(']');
    }

    private static PidlAttribute ParseAttribute(string line)
    {
        var text = line.Substring(1, line.Length - 2).Trim();
        var separator = text.IndexOf(':');
        return separator < 0
            ? new PidlAttribute(text, null)
            : new PidlAttribute(text[..separator].Trim(), text[(separator + 1)..].Trim());
    }

    private static string TrimBlockStart(string line)
    {
        return line.EndsWith('{')
            ? line[..^1].Trim()
            : line;
    }

    private static string TrimStatement(string line)
    {
        return line.EndsWith(';')
            ? line[..^1].Trim()
            : line;
    }

    private static EnumValue ParseEnumValue(string text)
    {
        var equalsIndex = text.IndexOf('=');
        if (equalsIndex < 0)
            return new EnumValue(text.Trim(), null);

        return new EnumValue(text[..equalsIndex].Trim(), text[(equalsIndex + 1)..].Trim());
    }

    private static InterfaceDefinition ParseInterfaceDeclaration(string text, string? currentNamespace)
    {
        var separator = text.IndexOf(':');
        if (separator < 0)
            return new InterfaceDefinition(QualifyName(text.Trim(), currentNamespace));

        return new InterfaceDefinition(QualifyName(text[..separator].Trim(), currentNamespace))
        {
            Inherits = text[(separator + 1)..].Trim()
        };
    }

    private static string QualifyName(string name, string? currentNamespace)
    {
        if (currentNamespace is null || name.StartsWith(currentNamespace + ".", StringComparison.Ordinal))
            return name;

        return currentNamespace + "." + name;
    }

    private static MethodDefinition ParseMethod(string text)
    {
        var signature = text.TrimStartText("method ").Trim();
        var isAsync = signature.StartsWith("async ", StringComparison.Ordinal);
        if (isAsync)
            signature = signature["async ".Length..].Trim();

        var openParen = signature.IndexOf('(');
        var closeParen = signature.LastIndexOf(')');
        var head = signature[..openParen].Trim();
        var parametersText = signature.Substring(openParen + 1, closeParen - openParen - 1).Trim();
        var lastSpace = head.LastIndexOf(' ');
        var method = new MethodDefinition(
            head[..lastSpace].Trim(),
            head[(lastSpace + 1)..].Trim(),
            ParseParameters(parametersText));
        method.IsAsync = isAsync;
        if (isAsync)
            method.AsyncReturnType = method.ReturnType;

        return method;
    }

    private static MethodDefinition NormalizeMethod(
        MethodDefinition method,
        List<string> documentation,
        IReadOnlyList<PidlAttribute> attributes)
    {
        TransferDocumentation(method.Documentation, documentation);
        ApplyMethodAttributes(method, attributes);
        ExpandAsyncMethod(method);
        NormalizeStreamTypes(method);
        ExpandDictionaries(method);
        ExpandDateTimeOffset(method);
        EnsureArrayCountParameters(method);
        return method;
    }

    private static void ExpandAsyncMethod(MethodDefinition method)
    {
        if (!method.IsAsync)
            return;

        if (method.Parameters.Any(static parameter => parameter.Type is "ITaskCompletionSource"))
            throw new InvalidOperationException($"Async method '{method.Name}' must not declare an ITaskCompletionSource parameter.");

        method.Hidden = true;
        method.ReturnType = "void";
        method.Parameters.Insert(0, new ParameterDefinition("ITaskCompletionSource", "task", false, null));
        if (method.AsyncReturnType is not null and not "void")
            method.AsyncResultGetterName = GetAsyncResultGetterName(method.Name);
    }

    private static bool TryCreateAsyncResultGetter(MethodDefinition method, out MethodDefinition resultGetter)
    {
        resultGetter = null!;
        if (method is not { IsAsync: true, AsyncReturnType: { } returnType, AsyncResultGetterName: { } resultGetterName } ||
            returnType is "void")
            return false;

        resultGetter = new MethodDefinition(returnType, resultGetterName, [])
        {
            Hidden = true,
            IsAsyncResultGetter = true,
            AsyncSourceMethod = method
        };

        NormalizeStreamTypes(resultGetter);
        ExpandDictionaries(resultGetter);
        ExpandDateTimeOffset(resultGetter);
        EnsureArrayCountParameters(resultGetter);
        return true;
    }

    private static string GetAsyncResultGetterName(string methodName)
    {
        var name = methodName.EndsWith("Async", StringComparison.Ordinal)
            ? methodName[..^"Async".Length]
            : methodName;

        return "Get" + name + "Result";
    }

    private static List<ParameterDefinition> ParseParameters(string text)
    {
        var parameters = new List<ParameterDefinition>();
        if (string.IsNullOrWhiteSpace(text))
            return parameters;

        foreach (var rawParameter in SplitParameters(text))
        {
            var parameterText = rawParameter.Trim();
            string? defaultValue = null;
            var equalsIndex = parameterText.IndexOf('=');
            if (equalsIndex >= 0)
            {
                defaultValue = parameterText[(equalsIndex + 1)..].Trim();
                parameterText = parameterText[..equalsIndex].Trim();
            }

            var isOut = parameterText.StartsWith("out ", StringComparison.Ordinal);
            if (isOut)
                parameterText = parameterText["out ".Length..].Trim();

            var lastSpace = parameterText.LastIndexOf(' ');
            parameters.Add(new ParameterDefinition(
                parameterText[..lastSpace].Trim(),
                parameterText[(lastSpace + 1)..].Trim(),
                isOut,
                defaultValue));
        }

        return parameters;
    }

    private static IEnumerable<string> SplitParameters(string text)
    {
        var start = 0;
        var depth = 0;
        for (var i = 0; i < text.Length; ++i)
        {
            depth += text[i] switch
            {
                '<' => 1,
                '>' => -1,
                _ => 0
            };
            if (text[i] is not ',' || depth is not 0)
                continue;

            yield return text[start..i];
            start = i + 1;
        }

        yield return text[start..];
    }

    private static void ApplyInterfaceAttributes(InterfaceDefinition definition, IEnumerable<PidlAttribute> attributes)
    {
        foreach (var attribute in attributes)
        {
            definition.Attributes.Add(attribute);
            switch (attribute.Name)
            {
                case "guid":
                    definition.Guid = RequireValue(attribute);
                    break;
                case "inherits":
                    definition.Inherits = RequireValue(attribute);
                    break;
                case "editor":
                    definition.Editor = RequireValue(attribute);
                    break;
                case "special":
                    definition.Special = RequireValue(attribute);
                    break;
                case "sdk":
                    definition.SdkName = string.IsNullOrWhiteSpace(attribute.Value)
                        ? GetDefaultSdkName(definition.FullName)
                        : attribute.Value;
                    break;
            }
        }
    }

    private static void ApplyMethodAttributes(MethodDefinition method, IEnumerable<PidlAttribute> attributes)
    {
        foreach (var attribute in attributes)
        {
            method.Attributes.Add(attribute);
            switch (attribute.Name)
            {
                case "hidden":
                    method.Hidden = true;
                    break;
                case "property":
                    method.PropertyName = string.IsNullOrWhiteSpace(attribute.Value)
                        ? GetDefaultPropertyName(method.Name)
                        : attribute.Value;
                    method.Hidden = true;
                    break;
                case "paramIn":
                    _ = method.ParamIn.Add(RequireValue(attribute));
                    break;
                case "paramOut":
                    _ = method.ParamOut.Add(RequireValue(attribute));
                    break;
            }
        }
    }

    private static bool IsMetadataSource(string source)
    {
        return TryParseMetadataSource(source) is not null;
    }

    private static PidlMetadata? TryParseMetadataSource(string source)
    {
        var trimmed = source.TrimStart();
        if (!trimmed.StartsWith('{'))
            return null;

        using var document = JsonDocument.Parse(source);
        var root = document.RootElement;
        return new PidlMetadata
        {
            Version = ReadRequiredString(root, "version"),
            StringMarshalling = ParseStringMarshalling(ReadRequiredString(root, "stringMarshalling")),
            BoolMarshalling = ParseBoolMarshalling(ReadRequiredString(root, "boolMarshalling"))
        };
    }

    private static string ReadRequiredString(JsonElement root, string name)
    {
        return root.GetProperty(name).GetString() ??
               throw new InvalidOperationException($"Metadata field '{name}' must be a string.");
    }

    private static StringMarshallingMode ParseStringMarshalling(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "utf16" => StringMarshallingMode.Utf16,
            _ => throw new InvalidOperationException($"Unsupported string marshalling mode: {value}")
        };
    }

    private static BoolMarshallingMode ParseBoolMarshalling(string value)
    {
        return value switch
        {
            "BOOL" => BoolMarshallingMode.BOOL,
            _ => throw new InvalidOperationException($"Unsupported bool marshalling mode: {value}")
        };
    }

    private static void EnsureArrayCountParameters(MethodDefinition method)
    {
        foreach (var parameter in method.Parameters.Where(static parameter => IsArrayType(parameter.Type) && parameter.DictionaryElementKind is DictionaryElementKind.None).ToList())
        {
            var countName = UniqueParameterName(method, parameter.Name + "Count");
            parameter.ArrayCountName = countName;
            method.Parameters.Add(new ParameterDefinition("int", countName, parameter.IsOut, null)
            {
                IsGeneratedArrayCount = true
            });
        }

        if (IsArrayType(method.ReturnType))
        {
            method.ReturnArrayCountName = UniqueParameterName(method, "returnCount");
            method.Parameters.Add(new ParameterDefinition("int", method.ReturnArrayCountName, true, null)
            {
                IsGeneratedArrayCount = true
            });
        }
    }

    private static void NormalizeStreamTypes(MethodDefinition method)
    {
        method.ReturnType = NormalizeStreamType(method.ReturnType, out var hasStreamReturn);
        if (hasStreamReturn)
        {
            method.Hidden = true;
            method.ReturnIsBuiltInStream = true;
        }

        foreach (var parameter in method.Parameters)
        {
            parameter.Type = NormalizeStreamType(parameter.Type, out var hasStreamParameter);
            if (hasStreamParameter)
            {
                method.Hidden = true;
                parameter.IsBuiltInStream = true;
            }
        }
    }

    private static void ExpandDictionaries(MethodDefinition method)
    {
        if (TryParseDictionaryType(method.ReturnType, out var returnKeyType, out var returnValueType))
        {
            method.Hidden = true;
            method.ReturnType = "void";
            var keysName = UniqueParameterName(method, "returnKeys");
            var valuesName = UniqueParameterName(method, "returnValues");
            var countName = UniqueParameterName(method, "returnCount");
            var expansion = new DictionaryExpansion("return", NormalizeDictionaryElementType(returnKeyType), NormalizeDictionaryElementType(returnValueType), keysName, valuesName, countName);
            method.ReturnDictionary = expansion;
            method.Parameters.Add(DictionaryArrayParameter(expansion.KeyType, keysName, true, expansion, DictionaryElementKind.Keys));
            method.Parameters.Add(DictionaryArrayParameter(expansion.ValueType, valuesName, true, expansion, DictionaryElementKind.Values));
            method.Parameters.Add(DictionaryCountParameter(countName, true, expansion));
        }

        for (var i = 0; i < method.Parameters.Count; ++i)
        {
            var parameter = method.Parameters[i];
            if (!TryParseDictionaryType(parameter.Type, out var keyType, out var valueType))
                continue;

            method.Hidden = true;
            method.Parameters.RemoveAt(i);
            var keysName = UniqueParameterName(method, parameter.Name + "Keys");
            var valuesName = UniqueParameterName(method, parameter.Name + "Values");
            var countName = UniqueParameterName(method, parameter.Name + "Count");
            var expansion = new DictionaryExpansion(parameter.Name, NormalizeDictionaryElementType(keyType), NormalizeDictionaryElementType(valueType), keysName, valuesName, countName);
            method.DictionaryParameters.Add(expansion);
            method.Parameters.Insert(i++, DictionaryArrayParameter(expansion.KeyType, keysName, parameter.IsOut, expansion, DictionaryElementKind.Keys));
            method.Parameters.Insert(i, DictionaryArrayParameter(expansion.ValueType, valuesName, parameter.IsOut, expansion, DictionaryElementKind.Values));
        }

        foreach (var dictionary in method.DictionaryParameters)
        {
            var isOut = method.Parameters.First(parameter => ReferenceEquals(parameter.Dictionary, dictionary)).IsOut;
            method.Parameters.Add(DictionaryCountParameter(dictionary.CountName, isOut, dictionary));
        }
    }

    private static ParameterDefinition DictionaryArrayParameter(
        string type,
        string name,
        bool isOut,
        DictionaryExpansion dictionary,
        DictionaryElementKind kind)
    {
        return new ParameterDefinition(type + "[]", name, isOut, null)
        {
            ArrayCountName = dictionary.CountName,
            Dictionary = dictionary,
            DictionaryElementKind = kind,
            IsBuiltInStream = type is "IStream"
        };
    }

    private static ParameterDefinition DictionaryCountParameter(
        string name,
        bool isOut,
        DictionaryExpansion dictionary)
    {
        return new ParameterDefinition("int", name, isOut, null)
        {
            IsGeneratedArrayCount = true,
            Dictionary = dictionary,
            DictionaryElementKind = DictionaryElementKind.Count
        };
    }

    private static bool TryParseDictionaryType(string type, out string keyType, out string valueType)
    {
        keyType = "";
        valueType = "";
        if (!type.StartsWith("dictionary<", StringComparison.Ordinal) || !type.EndsWith('>'))
            return false;

        var inner = type["dictionary<".Length..^1].Trim();
        var parts = SplitParameters(inner).Select(static part => part.Trim()).ToArray();
        if (parts.Length is not 2)
            throw new InvalidOperationException($"Invalid dictionary type: {type}");

        keyType = parts[0];
        valueType = parts[1];
        return true;
    }

    private static string NormalizeDictionaryElementType(string type)
    {
        return NormalizeStreamType(type, out _);
    }

    private static string NormalizeStreamType(string type, out bool hasStream)
    {
        hasStream = false;
        if (type is "stream")
        {
            hasStream = true;
            return "IStream";
        }

        if (type is "stream[]")
        {
            hasStream = true;
            return "IStream[]";
        }

        if (type is "stream[]?")
        {
            hasStream = true;
            return "IStream[]?";
        }

        return type;
    }

    private static void ExpandDateTimeOffset(MethodDefinition method)
    {
        if (method.ReturnType is "dateTimeOffset")
        {
            method.Hidden = true;
            method.ReturnType = "void";
            var ticksName = UniqueParameterName(method, "returnUtcDateTimeTicks");
            method.Parameters.Add(new ParameterDefinition("long", ticksName, true, null));
            var offsetName = UniqueParameterName(method, "returnMinutesOffset");
            method.Parameters.Add(new ParameterDefinition("int", offsetName, true, null));
            method.ReturnDateTimeOffset = new DateTimeOffsetExpansion("return", ticksName, offsetName);
        }

        for (var i = 0; i < method.Parameters.Count; ++i)
        {
            var parameter = method.Parameters[i];
            if (parameter.Type is not "dateTimeOffset")
                continue;

            method.Hidden = true;
            method.Parameters.RemoveAt(i);
            var ticksName = UniqueParameterName(method, DateTimeOffsetTicksName(parameter.Name));
            method.Parameters.Insert(i++, new ParameterDefinition("long", ticksName, parameter.IsOut, null));
            var offsetName = UniqueParameterName(method, DateTimeOffsetOffsetName(parameter.Name));
            method.Parameters.Insert(i, new ParameterDefinition("int", offsetName, parameter.IsOut, null));
            method.DateTimeOffsetParameters.Add(new DateTimeOffsetExpansion(parameter.Name, ticksName, offsetName));
        }
    }

    private static string DateTimeOffsetTicksName(string parameterName)
    {
        return parameterName + "UtcDateTimeTicks";
    }

    private static string DateTimeOffsetOffsetName(string parameterName)
    {
        return parameterName + "MinutesOffset";
    }

    private static string UniqueParameterName(MethodDefinition method, string preferredName)
    {
        if (method.Parameters.All(parameter => parameter.Name != preferredName))
            return preferredName;

        for (var i = 1;; ++i)
        {
            var name = preferredName + i;
            if (method.Parameters.All(parameter => parameter.Name != name))
                return name;
        }
    }

    private static bool IsArrayType(string type)
    {
        return type.EndsWith("[]", StringComparison.Ordinal) ||
               type.EndsWith("[]?", StringComparison.Ordinal);
    }

    private static string RequireValue(PidlAttribute attribute)
    {
        return attribute.Value ?? "";
    }

    private static void TransferDocumentation(List<string> target, List<string> source)
    {
        target.AddRange(source);
        source.Clear();
    }

    private static string GetDefaultSdkName(string interfaceFullName)
    {
        var shortName = ShortName(interfaceFullName);
        return (shortName.StartsWith('I') ? shortName[1..] : shortName) + "Base";
    }

    private static string GetDefaultPropertyName(string methodName)
    {
        if (methodName.Length > 3 &&
            (methodName.StartsWith("Get", StringComparison.Ordinal) || methodName.StartsWith("Set", StringComparison.Ordinal)))
            return methodName[3..];

        return methodName;
    }

    private static string ShortName(string fullName)
    {
        var separator = fullName.LastIndexOf('.');
        return separator < 0 ? fullName : fullName[(separator + 1)..];
    }
}
