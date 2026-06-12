using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using Pixeval.Extensions.Generator.Models;

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
            if (TryParseMetadataSource(source) is not null)
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

            if (trimmedLine is ['/', '/', .. { } text])
            {
                pendingDocumentation.Add(text.Trim());
                continue;
            }

            var line = CleanLine(rawLine);
            if (line.Length is 0)
                continue;

            if (TryParseAttribute(line, out var attributes))
            {
                pendingAttributes.AddRange(attributes);
                continue;
            }

            if (line is "{")
                continue;

            if (line is "}")
            {
                currentEnum = null;
                currentInterface = null;
                pendingAttributes.Clear();
                pendingDocumentation.Clear();
                continue;
            }

            if (currentEnum is not null)
            {
                currentEnum.Values.Add(ParseEnumValue(RequireStatement(line, "enum value")));
                pendingDocumentation.Clear();
                continue;
            }

            if (currentInterface is not null)
            {
                var methods = ParseInterfaceMember(currentInterface, RequireStatement(line, "interface member"), pendingDocumentation, pendingAttributes);
                pendingAttributes.Clear();
                pendingDocumentation.Clear();
                foreach (var method in methods)
                {
                    currentInterface.Methods.Add(method);
                    if (TryCreateAsyncResultGetter(method, out var resultGetter))
                        currentInterface.Methods.Add(resultGetter);
                }
                continue;
            }

            switch (line)
            {
                case ['e', 'n', 'u', 'm', ' ', .. { } text1]:
                    currentEnum = new EnumDefinition(QualifyName(ParseBlockHeader(text1.Trim(), "enum"), currentNamespace));
                    TransferDocumentation(currentEnum.Documentation, pendingDocumentation);
                    pendingAttributes.Clear();
                    model.Enums.Add(currentEnum);
                    continue;
                case ['i', 'n', 't', 'e', 'r', 'f', 'a', 'c', 'e', ' ', .. { } text2]:
                    currentInterface = ParseInterfaceDeclaration(ParseBlockHeader(text2.Trim(), "interface"), currentNamespace);
                    TransferDocumentation(currentInterface.Documentation, pendingDocumentation);
                    ApplyInterfaceAttributes(currentInterface, pendingAttributes);
                    pendingAttributes.Clear();
                    model.Interfaces.Add(currentInterface);
                    continue;
                case ['n', 'a', 'm', 'e', 's', 'p', 'a', 'c', 'e', ' ', .. { } text3]:
                    currentNamespace = RequireStatement(text3, "namespace declaration").Trim();
                    pendingAttributes.Clear();
                    pendingDocumentation.Clear();
                    continue;
                default:
                    throw new InvalidOperationException($"Unsupported PIDL syntax: {line}");
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

    private static bool TryParseAttribute(string line, [NotNullWhen(true)] out IEnumerable<PidlAttribute>? attributes)
    {
        if (line is not ['[', .. { } text, ']'])
        {
            attributes = null;
            return false;
        }

        attributes = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(s =>
        {
            var separator = s.IndexOf(':');
            return separator < 0
                ? new PidlAttribute(s, null)
                : new PidlAttribute(s[..separator].Trim(), s[(separator + 1)..].Trim());
        });
        return true;
    }

    private static string ParseBlockHeader(string text, string kind)
    {
        if (text.Length is 0)
            throw new InvalidOperationException($"{kind} declaration is missing a name.");

        return text;
    }

    private static string RequireStatement(string line, string kind)
    {
        if (!line.EndsWith(';'))
            throw new InvalidOperationException($"{kind} must end with ';': {line}");

        return line[..^1].Trim();
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

    private static IReadOnlyList<MethodDefinition> ParseInterfaceMember(
        InterfaceDefinition currentInterface,
        string text,
        IReadOnlyList<string> documentation,
        IReadOnlyList<PidlAttribute> attributes)
    {
        return text.Contains('(', StringComparison.Ordinal)
            ? [NormalizeMethod(ParseMethod(text), documentation, attributes)]
            : ParseProperty(currentInterface, text, documentation, attributes);
    }

    private static MethodDefinition ParseMethod(string text)
    {
        var signature = text.Trim();
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
        if (isAsync)
            method.Async = new AsyncMethodDefinition(method.ReturnType);

        return method;
    }

    private static MethodDefinition NormalizeMethod(
        MethodDefinition method,
        IReadOnlyList<string> documentation,
        IReadOnlyList<PidlAttribute> attributes)
    {
        method.Documentation.AddRange(documentation);
        ApplyMethodAttributes(method, attributes);
        ExpandAsyncMethod(method);
        NormalizeStreamTypes(method);
        ExpandDictionaries(method);
        ExpandDateTimeOffset(method);
        EnsureArrayCountParameters(method);
        return method;
    }

    private static IReadOnlyList<MethodDefinition> ParseProperty(
        InterfaceDefinition currentInterface,
        string text,
        IReadOnlyList<string> documentation,
        IReadOnlyList<PidlAttribute> attributes)
    {
        string? defaultValue = null;
        var equalsIndex = text.IndexOf('=');
        if (equalsIndex >= 0)
        {
            defaultValue = text[(equalsIndex + 1)..].Trim();
            text = text[..equalsIndex].Trim();
        }

        var lastSpace = text.LastIndexOf(' ');
        if (lastSpace <= 0 || lastSpace == text.Length - 1)
            throw new InvalidOperationException($"Invalid property declaration: {text}");

        var propertyType = text[..lastSpace].Trim();
        var propertyName = text[(lastSpace + 1)..].Trim();

        var accessors = attributes
            .Where(static attribute => attribute.Name is "get" or "set")
            .Select(static attribute => attribute.Name)
            .ToHashSet(StringComparer.Ordinal);
        var hasSetter = accessors.Contains("set");
        var hasGetter = !hasSetter || accessors.Contains("get");

        var methodAttributes = attributes
            .Where(static attribute => attribute.Name is not "get" and not "set")
            .ToArray();
        if (defaultValue is null && methodAttributes.Any(static attribute => attribute.Name is "sealed"))
            throw new InvalidOperationException($"Property attribute 'sealed' requires a default value: {propertyName}");

        var property = new PropertyDefinition(propertyName, propertyType, defaultValue);
        currentInterface.Properties.Add(property);

        var methods = new List<MethodDefinition>();
        if (hasGetter)
        {
            var getter = new MethodDefinition(propertyType, "Get" + propertyName, []);
            property.Getter = getter;
            methods.Add(NormalizePropertyAccessor(getter, property, documentation, methodAttributes));
        }

        if (hasSetter)
        {
            var setter = new MethodDefinition("void", "Set" + propertyName, [new ParameterDefinition(propertyType, "value", false, null)]);
            property.Setter = setter;
            methods.Add(NormalizePropertyAccessor(setter, property, documentation, methodAttributes));
        }

        return methods;
    }

    private static MethodDefinition NormalizePropertyAccessor(
        MethodDefinition method,
        PropertyDefinition property,
        IReadOnlyList<string> documentation,
        IReadOnlyList<PidlAttribute> attributes)
    {
        method.Property = property;
        method.Hidden = true;
        return NormalizeMethod(method, documentation, attributes);
    }

    private static void ExpandAsyncMethod(MethodDefinition method)
    {
        if (method.Async is null)
            return;

        if (method.Parameters.Any(static parameter => parameter.Type is "ITaskCompletionSource"))
            throw new InvalidOperationException($"Async method '{method.Name}' must not declare an ITaskCompletionSource parameter.");

        method.Hidden = true;
        method.ReturnType = "void";
        method.Parameters.Insert(0, new ParameterDefinition("ITaskCompletionSource", "task", false, null));
    }

    private static bool TryCreateAsyncResultGetter(MethodDefinition method, out MethodDefinition resultGetter)
    {
        resultGetter = null!;
        if (method.Async is not { ReturnType: { } returnType } ||
            returnType is "void")
            return false;

        resultGetter = new MethodDefinition(returnType, method.Async.GetResultGetterName(method.Name), [])
        {
            Hidden = true,
            AsyncResultGetter = new AsyncResultGetterDefinition(method)
        };

        NormalizeStreamTypes(resultGetter);
        ExpandDictionaries(resultGetter);
        ExpandDateTimeOffset(resultGetter);
        EnsureArrayCountParameters(resultGetter);
        return true;
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
                case "editor":
                    definition.Editor = RequireValue(attribute);
                    break;
                case "sdk":
                    definition.SdkName = string.IsNullOrWhiteSpace(attribute.Value)
                        ? GetDefaultSdkName(definition.FullName)
                        : attribute.Value;
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported interface attribute: {attribute.Name}");
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
                    RequireNoValue(attribute);
                    method.Hidden = true;
                    break;
                case "virtual":
                    RequireNoValue(attribute);
                    if (method.IsAsync || method.ReturnType is not "void")
                        throw new InvalidOperationException($"Method attribute 'virtual' can only be applied to non-async void methods: {method.Name}");
                    method.IsVirtual = true;
                    break;
                case "override":
                    RequireNoValue(attribute);
                    if (method.Property is null)
                        throw new InvalidOperationException($"Method attribute 'override' can only be applied to properties: {method.Name}");
                    method.IsOverride = true;
                    break;
                case "sealed":
                    RequireNoValue(attribute);
                    if (method.Property is null)
                        throw new InvalidOperationException($"Method attribute 'sealed' can only be applied to properties: {method.Name}");
                    method.IsSealed = true;
                    break;
                case "paramIn":
                    _ = method.ParamIn.Add(RequireValue(attribute));
                    break;
                case "paramOut":
                    _ = method.ParamOut.Add(RequireValue(attribute));
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported method attribute: {attribute.Name}");
            }
        }

        if (method is { IsSealed: true, IsOverride: false })
            throw new InvalidOperationException($"Method attribute 'sealed' requires 'override': {method.Name}");
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

    private static StringMarshalling ParseStringMarshalling(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "utf16" => StringMarshalling.Utf16,
            _ => throw new InvalidOperationException($"Unsupported string marshalling mode: {value}")
        };
    }

    private static UnmanagedType ParseBoolMarshalling(string value)
    {
        return value switch
        {
            "BOOL" => UnmanagedType.Bool,
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
            var countName = UniqueParameterName(method, "returnCount");
            method.ReturnArray = new ReturnArrayDefinition(countName);
            method.Parameters.Add(new ParameterDefinition("int", countName, true, null)
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
        if (type is not ['d', 'i', 'c', 't', 'i', 'o', 'n', 'a', 'r', 'y', '<', .. var innerRaw, '>'])
            return false;

        var inner = innerRaw.Trim();
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
        switch (type)
        {
            case "stream":
                hasStream = true;
                return "IStream";
            case "stream[]":
                hasStream = true;
                return "IStream[]";
            case "stream[]?":
                hasStream = true;
                return "IStream[]?";
            default:
                return type;
        }
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
            var ticksName = UniqueParameterName(method, parameter.Name + "UtcDateTimeTicks");
            method.Parameters.Insert(i++, new ParameterDefinition("long", ticksName, parameter.IsOut, null));
            var offsetName = UniqueParameterName(method, parameter.Name + "MinutesOffset");
            method.Parameters.Insert(i, new ParameterDefinition("int", offsetName, parameter.IsOut, null));
            method.DateTimeOffsetParameters.Add(new DateTimeOffsetExpansion(parameter.Name, ticksName, offsetName));
        }
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
        if (!string.IsNullOrWhiteSpace(attribute.Value))
            return attribute.Value;

        throw new InvalidOperationException($"Attribute '{attribute.Name}' requires a value.");
    }

    private static void RequireNoValue(PidlAttribute attribute)
    {
        if (attribute.Value is not null)
            throw new InvalidOperationException($"Attribute '{attribute.Name}' must not specify a value.");
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

    private static string ShortName(string fullName)
    {
        var separator = fullName.LastIndexOf('.');
        return separator < 0 ? fullName : fullName[(separator + 1)..];
    }
}
