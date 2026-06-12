using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pixeval.Extensions.Generator.Models;

namespace Pixeval.Extensions.Generator;

internal static class CSharpPropertyExtensionEmitter
{
    public static void Append(StringBuilder builder, PidlModel model)
    {
        foreach (var group in model.Interfaces
                     .Select(static interfaceDefinition => new
                     {
                         Interface = interfaceDefinition,
                         Properties = BuildProperties(interfaceDefinition),
                         TaskMethods = BuildTaskMethods(interfaceDefinition),
                         DictionaryMethods = BuildDictionaryMethods(interfaceDefinition),
                         DateTimeOffsetMethods = BuildDateTimeOffsetMethods(interfaceDefinition),
                         ArrayMethods = BuildArrayMethods(interfaceDefinition)
                     })
                     .Where(static value =>
                         value.Properties.Count > 0 ||
                         value.TaskMethods.Count > 0 ||
                         value.DictionaryMethods.Count > 0 ||
                         value.DateTimeOffsetMethods.Count > 0 ||
                         value.ArrayMethods.Count > 0)
                     .GroupBy(static value => NamespaceOf(value.Interface.FullName), StringComparer.Ordinal))
        {
            using (Namespace(builder, group.Key))
            {
                _ = builder.AppendLine(
                    """
                    public static class GeneratedExtensionPropertyHelper
                    {
                    """);

                foreach (var item in group)
                    AppendExtensionBlock(builder, item.Interface, item.Properties, item.TaskMethods, item.DictionaryMethods, item.DateTimeOffsetMethods, item.ArrayMethods);

                _ = builder.AppendLine("}");
            }
        }
    }

    private static IReadOnlyList<ExtensionProperty> BuildProperties(InterfaceDefinition definition)
    {
        var properties = new List<ExtensionProperty>();
        var propertyIndexes = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var method in definition.Methods.Where(static method => method is { Property: not null, IsOverride: false }))
        {
            var propertyName = method.Property!.Name;
            if (!propertyIndexes.TryGetValue(propertyName, out var index))
            {
                index = properties.Count;
                propertyIndexes.Add(propertyName, index);
                properties.Add(new ExtensionProperty(propertyName));
            }

            var property = properties[index];
            if (method.PropertyAccessor is PropertyAccessorKind.Getter)
                property.Getter = method;
            else if (method.PropertyAccessor is PropertyAccessorKind.Setter)
                property.Setter = method;
        }

        return properties
            .Where(static property => property.Getter is not null || property.Setter is not null)
            .ToList();
    }

    private static IReadOnlyList<MethodDefinition> BuildDateTimeOffsetMethods(InterfaceDefinition definition)
    {
        return definition.Methods
            .Where(static method => !method.IsOverride)
            .Where(CanBuildDateTimeOffsetMethod)
            .ToList();
    }

    private static IReadOnlyList<MethodDefinition> BuildTaskMethods(InterfaceDefinition definition)
    {
        if (ShortName(definition.FullName) is "IStream")
            return [];

        return definition.Methods
            .Where(static method => !method.IsOverride)
            .Where(IsTaskMethod)
            .ToList();
    }

    private static IReadOnlyList<MethodDefinition> BuildDictionaryMethods(InterfaceDefinition definition)
    {
        return definition.Methods
            .Where(static method => !method.IsOverride)
            .Where(CanBuildDictionaryMethod)
            .ToList();
    }

    private static IReadOnlyList<MethodDefinition> BuildArrayMethods(InterfaceDefinition definition)
    {
        return definition.Methods
            .Where(static method => !method.IsOverride)
            .Where(CanBuildArrayMethod)
            .ToList();
    }

    private static void AppendExtensionBlock(
        StringBuilder builder,
        InterfaceDefinition definition,
        IReadOnlyList<ExtensionProperty> properties,
        IReadOnlyList<MethodDefinition> taskMethods,
        IReadOnlyList<MethodDefinition> dictionaryMethods,
        IReadOnlyList<MethodDefinition> dateTimeOffsetMethods,
        IReadOnlyList<MethodDefinition> arrayMethods)
    {
        var receiverType = ShortName(definition.FullName);
        _ = builder.AppendLine(
            $$"""
                extension({{receiverType}} receiver)
                {
            """);

        foreach (var property in properties)
        {
            var type = PropertyType(property);
            if (property is { Getter: { } getter, Setter: { } setter })
            {
                _ = builder.AppendLine(
                    $$"""
                            public {{type}} {{property.Name}}
                            {
                                get => {{GetterExpression(getter)}};
                                set => receiver.{{setter.Name}}(value);
                            }

                    """);
                continue;
            }

            if (property.Getter is { } getterOnly)
            {
                if (getterOnly.ReturnDictionary is { } dictionary)
                {
                    _ = builder.AppendLine(
                        $$"""
                                public {{DictionaryPublicType(dictionary)}} {{property.Name}}
                                {
                                    get
                                    {
                                        receiver.{{getterOnly.Name}}(out var {{dictionary.KeysName}}, out var {{dictionary.ValuesName}}, out var {{dictionary.CountName}});
                                        return {{DictionaryFromArraysExpression(dictionary, dictionary.KeysName, dictionary.ValuesName, dictionary.CountName)}};
                                    }
                                }

                        """);
                    continue;
                }

                if (getterOnly.ReturnArrayCountName is { } returnCountName)
                {
                    _ = builder.AppendLine(
                        $$"""
                                public {{type}} {{property.Name}}
                                {
                                    get
                                    {
                                        var result = receiver.{{getterOnly.Name}}(out var {{returnCountName}});
                                        return {{TrimArrayExpression(type, "result", returnCountName)}};
                                    }
                                }

                        """);
                    continue;
                }

                if (getterOnly.ReturnDateTimeOffset is { } dateTimeOffset)
                {
                    _ = builder.AppendLine(
                        $$"""
                                public {{type}} {{property.Name}}
                                {
                                    get
                                    {
                                        receiver.{{getterOnly.Name}}(out var {{dateTimeOffset.TicksName}}, out var {{dateTimeOffset.OffsetName}});
                                        return new({{dateTimeOffset.TicksName}}, TimeSpan.FromMinutes({{dateTimeOffset.OffsetName}}));
                                    }
                                }

                        """);
                    continue;
                }

                _ = builder.AppendLine(
                    $"""
                            public {type} {property.Name} => {GetterExpression(getterOnly)};

                    """);
                continue;
            }

            var setterOnly = property.Setter!;
            _ = builder.AppendLine(
                $$"""
                        public {{type}} {{property.Name}}
                        {
                            set => receiver.{{setterOnly.Name}}(value);
                        }

                """);
        }

        foreach (var method in taskMethods)
            AppendTaskMethod(builder, definition, method);

        foreach (var method in dictionaryMethods)
            AppendDictionaryMethod(builder, method);

        foreach (var method in dateTimeOffsetMethods)
            AppendDateTimeOffsetMethod(builder, method);

        foreach (var method in arrayMethods)
            AppendArrayMethod(builder, method);

        _ = builder.AppendLine(
            """
                }

            """);
    }

    private static void AppendDateTimeOffsetMethod(StringBuilder builder, MethodDefinition method)
    {
        var parameters = string.Join(", ", DateTimeOffsetMethodParameters(method));
        var arguments = string.Join(", ", DateTimeOffsetMethodArguments(method));
        _ = builder.AppendLine(
            $"""
                    public void {method.Name}({parameters}) => receiver.{method.Name}({arguments});

            """);
    }

    private static void AppendTaskMethod(StringBuilder builder, InterfaceDefinition definition, MethodDefinition method)
    {
        var resultGetter = FindAsyncResultGetter(definition.Methods, method);
        var parameters = string.Join(", ", PublicMethodParameters(method, skipTask: true));
        var receiverCall = BuildReceiverCall(method, skipTask: true);
        var taskType = resultGetter is null ? "Task" : $"Task<{PublicReturnType(resultGetter)}>";
        var methodName = method.Name.EndsWith("Async", StringComparison.Ordinal) ? method.Name : method.Name + "Async";

        _ = builder.AppendLine(
            $$"""
                    public async {{taskType}} {{methodName}}({{parameters}})
                    {
            """);
        foreach (var line in receiverCall.Preamble)
            _ = builder.AppendLine($"            {line}");
        _ = builder.AppendLine(
            "            var source = new TaskCompletionSource();");
        _ = builder.AppendLine($"            receiver.{method.Name}(source.ToITaskCompletionSource(){(receiverCall.Arguments.Count is 0 ? "" : ", " + string.Join(", ", receiverCall.Arguments))});");
        _ = builder.AppendLine(
            "            await source.Task;");
        if (resultGetter is not null)
        {
            foreach (var line in AsyncResultReturnLines(resultGetter))
                _ = builder.AppendLine($"            {line}");
        }
        _ = builder.AppendLine(
            """
                    }

            """);
    }

    private static void AppendArrayMethod(StringBuilder builder, MethodDefinition method)
    {
        var parameters = string.Join(", ", ArrayMethodParameters(method));
        var arguments = string.Join(", ", ArrayMethodArguments(method));
        _ = builder.AppendLine(
            $$"""
                    public {{method.ReturnType}} {{method.Name}}({{parameters}})
                    {
            """);

        if (method.ReturnArrayCountName is not null)
            _ = builder.AppendLine($"            var result = receiver.{method.Name}({arguments});");
        else if (method.ReturnType is "void")
            _ = builder.AppendLine($"            receiver.{method.Name}({arguments});");
        else
            _ = builder.AppendLine($"            var result = receiver.{method.Name}({arguments});");

        foreach (var parameter in method.Parameters.Where(static parameter => parameter is { IsOut: true, ArrayCountName: not null }))
            _ = builder.AppendLine($"            {parameter.Name} = {TrimArrayExpression(parameter.Type, parameter.Name, parameter.ArrayCountName!)};");

        if (method.ReturnArrayCountName is not null)
            _ = builder.AppendLine($"            return {TrimArrayExpression(method.ReturnType, "result", method.ReturnArrayCountName)};");
        else if (method.ReturnType is not "void")
            _ = builder.AppendLine("            return result;");

        _ = builder.AppendLine(
            """
                    }

            """);
    }

    private static void AppendDictionaryMethod(StringBuilder builder, MethodDefinition method)
    {
        if (TryAppendReturnDictionaryMethod(builder, method))
            return;

        var parameters = string.Join(", ", PublicMethodParameters(method, skipTask: false));
        var receiverCall = BuildReceiverCall(method, skipTask: false);
        var returnPrefix = method.ReturnType is "void" ? "" : "return ";
        _ = builder.AppendLine(
            $$"""
                    public {{method.ReturnType}} {{method.Name}}({{parameters}})
                    {
            """);
        foreach (var line in receiverCall.Preamble)
        {
            _ = builder.AppendLine($"            {line}");
        }

        _ = builder.AppendLine($"            {returnPrefix}receiver.{method.Name}({string.Join(", ", receiverCall.Arguments)});");
        foreach (var dictionary in method.DictionaryParameters.Where(dictionary => IsDictionaryOut(method, dictionary)))
            _ = builder.AppendLine($"            {dictionary.Name} = {DictionaryFromArraysExpression(dictionary, dictionary.KeysName, dictionary.ValuesName, dictionary.CountName)};");
        _ = builder.AppendLine(
            """
                    }

            """);
    }

    private static bool TryAppendReturnDictionaryMethod(StringBuilder builder, MethodDefinition method)
    {
        if (method.ReturnDictionary is not { } dictionary)
            return false;

        _ = builder.AppendLine(
            $$"""
                    public {{DictionaryPublicType(dictionary)}} {{method.Name}}()
                    {
                        receiver.{{method.Name}}(out var {{dictionary.KeysName}}, out var {{dictionary.ValuesName}}, out var {{dictionary.CountName}});
                        return {{DictionaryFromArraysExpression(dictionary, dictionary.KeysName, dictionary.ValuesName, dictionary.CountName)}};
                    }

            """);
        return true;
    }

    private static string PropertyType(ExtensionProperty property)
    {
        if (property.Getter is { } getter)
        {
            if (getter.ReturnDateTimeOffset is not null)
                return nameof(DateTimeOffset);

            if (getter.ReturnDictionary is { } dictionary)
                return DictionaryPublicType(dictionary);

            return getter.ReturnType;
        }

        return property.Setter!.Parameters[0].Type;
    }

    private static string GetterExpression(MethodDefinition method)
    {
        if (method.ReturnArrayCountName is not null)
            return $"receiver.{method.Name}(out _)";

        return $"receiver.{method.Name}()";
    }

    private static bool CanBuildDateTimeOffsetMethod(MethodDefinition method)
    {
        return !method.IsAsyncResultGetter &&
               method is { ReturnType: "void", DateTimeOffsetParameters.Count: > 0 } &&
               method.DateTimeOffsetParameters.All(dateTimeOffset =>
                   TryFindDateTimeOffsetAbiParameters(method, dateTimeOffset, out var ticks, out var offset) &&
                   !ticks.IsOut &&
                   !offset.IsOut);
    }

    private static bool IsTaskMethod(MethodDefinition method)
    {
        return method.IsAsync;
    }

    private static MethodDefinition? FindAsyncResultGetter(IReadOnlyList<MethodDefinition> methods, MethodDefinition taskMethod)
    {
        return methods
            .FirstOrDefault(method => ReferenceEquals(method.AsyncResultGetter?.SourceMethod, taskMethod));
    }

    private static bool CanBuildDictionaryMethod(MethodDefinition method)
    {
        return method.Property is null &&
               !method.IsAsyncResultGetter &&
               !IsTaskMethod(method) &&
               (method.ReturnDictionary is not null || method.DictionaryParameters.Count > 0);
    }

    private static bool CanBuildArrayMethod(MethodDefinition method)
    {
        return method.ReturnDictionary is null &&
               method.DictionaryParameters.Count is 0 &&
               (method.Property is null || method.ReturnArrayCountName is null) &&
               !method.IsAsyncResultGetter &&
               !IsTaskMethod(method) &&
               (method.ReturnArrayCountName is not null ||
                method.Parameters.Any(static parameter => parameter.IsGeneratedArrayCount));
    }

    private static IEnumerable<string> PublicMethodParameters(MethodDefinition method, bool skipTask)
    {
        var skip = skipTask ? 1 : 0;
        for (var i = skip; i < method.Parameters.Count; ++i)
        {
            if (TryMatchDictionaryExpansion(method, i, out var dictionary))
            {
                var direction = IsDictionaryOut(method, dictionary) ? "out " : "";
                yield return $"{direction}{DictionaryPublicType(dictionary)} {dictionary.Name}";
                ++i;
                continue;
            }

            var parameter = method.Parameters[i];
            if (parameter.IsGeneratedArrayCount)
                continue;

            yield return $"{(parameter.IsOut ? "out " : "")}{PublicParameterType(parameter)} {parameter.Name}";
        }
    }

    private static ReceiverCall BuildReceiverCall(MethodDefinition method, bool skipTask)
    {
        var preamble = new List<string>();
        var arguments = new List<string>();
        var skip = skipTask ? 1 : 0;
        for (var i = skip; i < method.Parameters.Count; ++i)
        {
            if (TryMatchDictionaryExpansion(method, i, out var dictionary))
            {
                if (IsDictionaryOut(method, dictionary))
                {
                    arguments.Add($"out var {dictionary.KeysName}");
                    arguments.Add($"out var {dictionary.ValuesName}");
                }
                else
                {
                    preamble.Add($"var {dictionary.KeysName} = {DictionaryKeysToAbiExpression(dictionary)};");
                    preamble.Add($"var {dictionary.ValuesName} = {DictionaryValuesToAbiExpression(dictionary)};");
                    arguments.Add(dictionary.KeysName);
                    arguments.Add(dictionary.ValuesName);
                }

                ++i;
                continue;
            }

            var parameter = method.Parameters[i];
            if (parameter.IsGeneratedArrayCount)
            {
                if (parameter.Dictionary is { } countDictionary)
                    arguments.Add(IsDictionaryOut(method, countDictionary) ? $"out var {parameter.Name}" : $"{countDictionary.Name}.Count");
                else
                    arguments.Add(parameter.IsOut ? $"out var {parameter.Name}" : PublicArrayCountExpression(FindArrayParameter(method, parameter.Name)));
                continue;
            }

            arguments.Add(parameter.IsOut ? $"out {parameter.Name}" : ParameterToAbiExpression(parameter));
        }

        return new ReceiverCall(preamble, arguments);
    }

    private static IEnumerable<string> DateTimeOffsetMethodParameters(MethodDefinition method)
    {
        for (var i = 0; i < method.Parameters.Count; ++i)
        {
            if (TryMatchDateTimeOffsetExpansion(method, i, out var dateTimeOffset))
            {
                yield return $"DateTimeOffset {dateTimeOffset.Name}";
                ++i;
                continue;
            }

            yield return FormatParameter(method.Parameters[i]);
        }
    }

    private static IEnumerable<string> DateTimeOffsetMethodArguments(MethodDefinition method)
    {
        for (var i = 0; i < method.Parameters.Count; ++i)
        {
            if (TryMatchDateTimeOffsetExpansion(method, i, out var dateTimeOffset))
            {
                yield return $"{dateTimeOffset.Name}.UtcTicks";
                yield return $"(int){dateTimeOffset.Name}.Offset.TotalMinutes";
                ++i;
                continue;
            }

            yield return method.Parameters[i].IsOut ? $"out {method.Parameters[i].Name}" : method.Parameters[i].Name;
        }
    }

    private static IEnumerable<string> ArrayMethodParameters(MethodDefinition method)
    {
        foreach (var parameter in method.Parameters)
        {
            if (parameter.IsGeneratedArrayCount)
                continue;

            yield return FormatParameter(parameter);
        }
    }

    private static IEnumerable<string> ArrayMethodArguments(MethodDefinition method)
    {
        foreach (var parameter in method.Parameters)
        {
            if (parameter.IsGeneratedArrayCount)
            {
                if (parameter.Name == method.ReturnArrayCountName)
                {
                    yield return $"out var {parameter.Name}";
                    continue;
                }

                var arrayParameter = FindArrayParameter(method, parameter.Name);
                yield return parameter.IsOut ? $"out var {parameter.Name}" : ArrayCountExpression(arrayParameter);
                continue;
            }

            yield return parameter.IsOut ? $"out {parameter.Name}" : parameter.Name;
        }
    }

    private static bool TryMatchDictionaryExpansion(MethodDefinition method, int index, out DictionaryExpansion dictionary)
    {
        dictionary = null!;
        if (index + 1 >= method.Parameters.Count)
            return false;

        var keys = method.Parameters[index];
        var values = method.Parameters[index + 1];
        var match = method.DictionaryParameters.FirstOrDefault(expansion =>
            expansion.KeysName == keys.Name &&
            expansion.ValuesName == values.Name);
        if (match is null)
            return false;

        dictionary = match;
        return true;
    }

    private static bool IsDictionaryOut(MethodDefinition method, DictionaryExpansion dictionary)
    {
        return method.Parameters.First(parameter => ReferenceEquals(parameter.Dictionary, dictionary)).IsOut;
    }

    private static ParameterDefinition FindArrayParameter(MethodDefinition method, string countName)
    {
        return method.Parameters.First(parameter => parameter.ArrayCountName == countName);
    }

    private static string ArrayCountExpression(ParameterDefinition parameter)
    {
        return IsNullableArrayType(parameter.Type)
            ? $"{parameter.Name}?.Length ?? 0"
            : $"{parameter.Name}.Length";
    }

    private static string PublicArrayCountExpression(ParameterDefinition parameter)
    {
        return IsNullableArrayType(parameter.Type)
            ? $"{parameter.Name}?.Count ?? 0"
            : $"{parameter.Name}.Count";
    }

    private static string PublicParameterType(ParameterDefinition parameter)
    {
        if (parameter.ArrayCountName is not null)
        {
            var elementType = PublicType(ElementType(parameter.Type), parameter.IsBuiltInStream);
            return $"IReadOnlyList<{elementType}>";
        }

        return PublicType(parameter.Type, parameter.IsBuiltInStream);
    }

    private static string PublicReturnType(MethodDefinition method)
    {
        if (method.ReturnDictionary is { } dictionary)
            return DictionaryPublicType(dictionary);

        if (method.ReturnDateTimeOffset is not null)
            return nameof(DateTimeOffset);

        if (method.ReturnArrayCountName is not null)
            return PublicArrayReturnType(method);

        return PublicType(method.ReturnType, method.ReturnIsBuiltInStream);
    }

    private static string PublicArrayReturnType(MethodDefinition method)
    {
        var elementType = PublicType(ElementType(method.ReturnType), method.ReturnIsBuiltInStream);
        return IsNullableArrayType(method.ReturnType) ? $"{elementType}[]?" : $"{elementType}[]";
    }

    private static string PublicType(string type, bool isBuiltInStream)
    {
        return isBuiltInStream && type is "IStream" ? "Stream" : type;
    }

    private static string ParameterToAbiExpression(ParameterDefinition parameter)
    {
        if (parameter.ArrayCountName is not null && parameter.IsBuiltInStream)
            return $"[.. {parameter.Name}.Select(static stream => stream.ToIStream())]";

        return parameter.IsBuiltInStream ? $"{parameter.Name}.ToIStream()" : parameter.Name;
    }

    private static string ReturnConversionExpression(MethodDefinition method, string expression)
    {
        return method.ReturnIsBuiltInStream ? $"{expression}.ToStream()" : expression;
    }

    private static IEnumerable<string> AsyncResultReturnLines(MethodDefinition method)
    {
        if (method.ReturnDictionary is { } dictionary)
        {
            yield return $"receiver.{method.Name}(out var {dictionary.KeysName}, out var {dictionary.ValuesName}, out var {dictionary.CountName});";
            yield return $"return {DictionaryFromArraysExpression(dictionary, dictionary.KeysName, dictionary.ValuesName, dictionary.CountName)};";
            yield break;
        }

        if (method.ReturnDateTimeOffset is { } dateTimeOffset)
        {
            yield return $"receiver.{method.Name}(out var {dateTimeOffset.TicksName}, out var {dateTimeOffset.OffsetName});";
            yield return $"return new({dateTimeOffset.TicksName}, TimeSpan.FromMinutes({dateTimeOffset.OffsetName}));";
            yield break;
        }

        if (method.ReturnArrayCountName is { } returnCountName)
        {
            yield return $"var result = receiver.{method.Name}(out var {returnCountName});";
            yield return $"return {ArrayFromAbiExpression(method, "result", returnCountName)};";
            yield break;
        }

        yield return $"return {ReturnConversionExpression(method, $"receiver.{method.Name}()")};";
    }

    private static string ArrayFromAbiExpression(MethodDefinition method, string value, string countName)
    {
        if (method.ReturnIsBuiltInStream && ElementType(method.ReturnType) is "IStream")
        {
            var converted = $"[.. {value}.Take({countName}).Select(static stream => stream.ToStream())]";
            return IsNullableArrayType(method.ReturnType)
                ? $"{value} is null ? null : {converted}"
                : $"{value} is null ? [] : {converted}";
        }

        return TrimArrayExpression(PublicArrayReturnType(method), value, countName);
    }

    private static string DictionaryPublicType(DictionaryExpansion dictionary)
    {
        return $"IReadOnlyDictionary<{DictionaryElementPublicType(dictionary.KeyType)}, {DictionaryElementPublicType(dictionary.ValueType)}>";
    }

    private static string DictionaryElementPublicType(string type)
    {
        return type is "IStream" ? "Stream" : type;
    }

    private static string DictionaryKeysToAbiExpression(DictionaryExpansion dictionary)
    {
        return DictionaryElementToAbiExpression(dictionary.Name + ".Keys", dictionary.KeyType);
    }

    private static string DictionaryValuesToAbiExpression(DictionaryExpansion dictionary)
    {
        return DictionaryElementToAbiExpression(dictionary.Name + ".Values", dictionary.ValueType);
    }

    private static string DictionaryElementToAbiExpression(string expression, string elementType)
    {
        return elementType is "IStream"
            ? $"{expression}.Select(static stream => stream.ToIStream()).ToArray()"
            : $"{expression}.ToArray()";
    }

    private static string DictionaryFromArraysExpression(DictionaryExpansion dictionary, string keysName, string valuesName, string countName)
    {
        var keys = DictionaryElementFromAbiExpression(dictionary.KeyType, keysName, countName);
        var values = DictionaryElementFromAbiExpression(dictionary.ValueType, valuesName, countName);
        return $"{keys}.Zip({values}, static (key, value) => new KeyValuePair<{DictionaryElementPublicType(dictionary.KeyType)}, {DictionaryElementPublicType(dictionary.ValueType)}>(key, value)).ToDictionary(static pair => pair.Key, static pair => pair.Value)";
    }

    private static string DictionaryElementFromAbiExpression(string type, string arrayName, string countName)
    {
        var source = $"{arrayName}.Take({countName})";
        return type is "IStream" ? $"{source}.Select(static stream => stream.ToStream())" : source;
    }

    private static string ElementType(string type)
    {
        if (type.EndsWith("[]?", StringComparison.Ordinal))
            return type[..^3];

        return type.EndsWith("[]", StringComparison.Ordinal) ? type[..^2] : type;
    }

    private static string TrimArrayExpression(string type, string value, string countName)
    {
        return IsNullableArrayType(type)
            ? $"{value} is null ? null : [.. {value}.Take({countName})]"
            : $"{value} is null ? [] : [.. {value}.Take({countName})]";
    }

    private static bool IsNullableArrayType(string type)
    {
        return type.EndsWith("[]?", StringComparison.Ordinal);
    }

    private static bool TryMatchDateTimeOffsetExpansion(MethodDefinition method, int index, out DateTimeOffsetExpansion dateTimeOffset)
    {
        dateTimeOffset = null!;
        if (index + 1 >= method.Parameters.Count)
            return false;

        var ticks = method.Parameters[index];
        var offset = method.Parameters[index + 1];
        var match = method.DateTimeOffsetParameters.FirstOrDefault(expansion =>
            expansion.TicksName == ticks.Name &&
            expansion.OffsetName == offset.Name);
        if (match is null)
            return false;

        dateTimeOffset = match;
        return true;
    }

    private static bool TryFindDateTimeOffsetAbiParameters(
        MethodDefinition method,
        DateTimeOffsetExpansion dateTimeOffset,
        out ParameterDefinition ticks,
        out ParameterDefinition offset)
    {
        ticks = null!;
        offset = null!;
        for (var i = 0; i + 1 < method.Parameters.Count; ++i)
        {
            if (method.Parameters[i].Name != dateTimeOffset.TicksName ||
                method.Parameters[i + 1].Name != dateTimeOffset.OffsetName)
                continue;

            ticks = method.Parameters[i];
            offset = method.Parameters[i + 1];
            return true;
        }

        return false;
    }

    private static string FormatParameter(ParameterDefinition parameter)
    {
        var direction = parameter.IsOut ? "out " : "";
        return $"{direction}{parameter.Type} {parameter.Name}";
    }

    private static IDisposable Namespace(StringBuilder builder, string namespaceName)
    {
        _ = builder.AppendLine(
            $$"""
            namespace {{namespaceName}}
            {
            """);
        return new NamespaceScope(builder);
    }

    private static string NamespaceOf(string fullName)
    {
        var separator = fullName.LastIndexOf('.');
        return separator < 0 ? "" : fullName[..separator];
    }

    private static string ShortName(string fullName)
    {
        var separator = fullName.LastIndexOf('.');
        return separator < 0 ? fullName : fullName[(separator + 1)..];
    }

    private sealed class ExtensionProperty(string name)
    {
        public string Name { get; } = name;

        public MethodDefinition? Getter { get; set; }

        public MethodDefinition? Setter { get; set; }
    }

    private sealed record ReceiverCall(IReadOnlyList<string> Preamble, IReadOnlyList<string> Arguments);

    private sealed class NamespaceScope(StringBuilder builder) : IDisposable
    {
        public void Dispose()
        {
            _ = builder.AppendLine(
                """
                }

                """);
        }
    }
}
