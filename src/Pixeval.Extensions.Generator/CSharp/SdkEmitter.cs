using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixeval.Extensions.Generator;

internal static class SdkEmitter
{
    public static string Emit(PidlModel model)
    {
        var context = new SdkContext(model);
        var definitions = model.Interfaces
            .Where(static definition => definition.SdkName is not null)
            .ToList();

        var builder = new StringBuilder();
        AppendHeader(builder, model);
        foreach (var definition in definitions)
            AppendSdkClass(builder, context, definition);

        return builder.ToString();
    }

    private static void AppendHeader(StringBuilder builder, PidlModel model)
    {
        CSharpEmitterSupport.AppendHeader(builder, CSharpEmitterSupport.SdkNamespaces(model));
    }

    private static void AppendSdkClass(StringBuilder builder, SdkContext context, InterfaceDefinition definition)
    {
        var sdkName = definition.SdkName!;
        var sdkNamespace = SdkNamespace(definition);
        var baseInterface = context.ResolveSdkBaseInterface(definition);
        var baseName = baseInterface?.SdkName;
        var interfaceName = ShortName(definition.FullName);
        var baseClause = baseName is null
            ? $" : {interfaceName}"
            : $" : {baseName}, {interfaceName}";
        var inheritedInterfaces = baseInterface is null
            ? []
            : context.FlattenInterfaces(baseInterface).ToArray();
        var inherited = inheritedInterfaces.ToHashSet();
        var handled = new HashSet<MethodDefinition>();

        using (Namespace(builder, sdkNamespace))
        {
            _ = builder.AppendLine(
                $$"""
                    /// <inheritdoc cref="{{interfaceName}}" />
                    [GeneratedComClass]
                    public abstract partial class {{sdkName}}{{baseClause}}
                    {
                """);

            if (definition.Special is "ExtensionsHostStatics")
                AppendHostMembers(builder, definition);

            if (IsConcreteSettingsInterface(context, definition, out var settingsType))
            {
                _ = builder.AppendLine(
                    $"""
                            public override SettingsType SettingsType => SettingsType.{settingsType};

                    """);
            }

            var methods = context.FlattenMethods(definition)
                .Where(method => !inherited.Contains(method.Owner))
                .ToList();

            foreach (var method in methods)
            {
                if (handled.Contains(method.Method))
                    continue;

                switch (definition.Special)
                {
                    case "ExtensionsHostStatics" when method.Method.Name is "Initialize":
                        _ = handled.Add(method.Method);
                        continue;
                    case "ExtensionsHostStatics" when method.Method.Name is "GetSdkVersion":
                        AppendHostSdkVersionMethod(builder, method);
                        _ = handled.Add(method.Method);
                        continue;
                }

                if (method.Method.IsAsync)
                {
                    var asyncResult = FindAsyncResultGetter(methods, method.Method);
                    AppendTaskWrapper(builder, method, asyncResult);
                    _ = handled.Add(method.Method);
                    if (asyncResult is not null)
                        _ = handled.Add(asyncResult.Method);
                    continue;
                }

                if (TryAppendDictionaryProperty(builder, method)
                    || TryAppendDateTimeOffsetProperty(builder, method)
                    || TryAppendDictionaryMethod(builder, method)
                    || TryAppendDateTimeOffsetChanged(builder, method))
                {
                    _ = handled.Add(method.Method);
                    continue;
                }

                if (method.Method.PropertyName is not null)
                {
                    AppendPropertyWrapper(builder, method);
                    _ = handled.Add(method.Method);
                    continue;
                }

                if (HasArrayParameters(method.Method))
                {
                    AppendArrayParameterWrapper(builder, method);
                    _ = handled.Add(method.Method);
                    continue;
                }

                AppendDirectMethod(builder, method);
                _ = handled.Add(method.Method);
            }

            _ = builder.AppendLine(
                "    }");
        }
    }

    private static void AppendHostMembers(StringBuilder builder, InterfaceDefinition definition)
    {
        _ = builder.AppendLine(
            $$"""
                      public static string TempDirectory { get; protected set; } = "";
                      public static string ExtensionDirectory { get; protected set; } = "";

                      public ILogger Logger { get; private set; } = null!;

                      void IExtensionsHost.Initialize(string cultureName, string tempDirectory, string extensionDirectory, ILogger logger)
                      {
                          TempDirectory = tempDirectory;
                          ExtensionDirectory = extensionDirectory;
                          CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new(cultureName);
                          Logger = logger;
                          Initialize();
                      }

                      public virtual void Initialize()
                      {
                      }

                      public static unsafe int DllGetExtensionsHost(void** ppv, {{definition.SdkName}} current)
                      {
                          if (_CcwCache is null)
                          {
                              var comWrappers = new StrategyBasedComWrappers();
                              _CcwCache = (void*)comWrappers.GetOrCreateComInterfaceForObject(current, CreateComInterfaceFlags.None);
                          }

                          *ppv = _CcwCache;
                          return 0;
                      }

                      private static unsafe void* _CcwCache;

              """);
    }

    private static void AppendHostSdkVersionMethod(StringBuilder builder, MethodWithOwner method)
    {
        _ = builder.AppendLine(
            $"""
                    {method.Method.ReturnType} {ShortName(method.Owner.FullName)}.{method.Method.Name}() => IExtensionsHost.CurrentSdkVersion.ToString();

            """);
    }

    private static void AppendPropertyWrapper(StringBuilder builder, MethodWithOwner method)
    {
        var returnType = method.Method.ReturnType;
        var propertyName = method.Method.PropertyName!;
        if (method.Method.ReturnArrayCountName is not null)
        {
            _ = builder.AppendLine(
                $$"""
                        public abstract {{returnType}} {{propertyName}} { get; }

                        {{returnType}} {{ShortName(method.Owner.FullName)}}.{{method.Method.Name}}(out int {{method.Method.ReturnArrayCountName}})
                        {
                            var value = {{propertyName}};
                            {{method.Method.ReturnArrayCountName}} = value?.Length ?? 0;
                            return {{ArrayReturnExpression(returnType, "value")}};
                        }

                """);
            return;
        }

        AppendPropertyDeclaration(builder, method, propertyName, returnType);
        _ = builder.AppendLine(
            $"""

                    {returnType} {ShortName(method.Owner.FullName)}.{method.Method.Name}() => {propertyName};

            """);
    }

    private static void AppendPropertyDeclaration(StringBuilder builder, MethodWithOwner method, string propertyName, string returnType)
    {
        if (method.Owner.SdkName is "ExtensionBase" &&
            (propertyName is "OnExtensionLoaded" or "OnExtensionUnloaded"))
        {
            _ = builder.AppendLine($"        public virtual {returnType} {propertyName} {{ get; }}");
            return;
        }

        if (returnType.EndsWith('?') && method.Owner.SdkName is "SettingsExtensionBase")
        {
            _ = builder.AppendLine($"        public virtual {returnType} {propertyName} => null;");
            return;
        }

        if (propertyName is "StepValue")
        {
            switch (returnType)
            {
                case "double":
                    _ = builder.AppendLine($"        public virtual {returnType} {propertyName} => 1.0;");
                    return;
                case "int":
                    _ = builder.AppendLine($"        public virtual {returnType} {propertyName} => 1;");
                    return;
            }
        }

        _ = builder.AppendLine($"        public abstract {returnType} {propertyName} {{ get; }}");
    }

    private static void AppendTaskWrapper(StringBuilder builder, MethodWithOwner method, MethodWithOwner? resultGetter)
    {
        var interfaceName = ShortName(method.Owner.FullName);
        var explicitParameters = string.Join(", ", method.Method.Parameters.Select(FormatParameter));
        var asyncParameters = SdkParameterList(method.Method, skipTask: true);
        if (method.Method.AsyncReturnType is not null and not "void" && resultGetter is null)
            throw new InvalidOperationException($"Async method '{method.Method.Name}' is missing its generated result getter.");

        var resultType = resultGetter is null ? null : SdkReturnType(resultGetter.Method);
        var taskType = resultType is null ? "Task" : $"Task<{resultType}>";
        var asyncName = method.Method.Name.EndsWith("Async", StringComparison.Ordinal)
            ? method.Method.Name
            : method.Method.Name + "Async";
        var convertedArguments = string.Join(", ", SdkArgumentList(method.Method, skipTask: true));

        if (resultType is not null)
        {
            var fieldName = "_" + LowerFirst(resultGetter!.Method.PropertyName ?? GetDefaultPropertyName(resultGetter.Method.Name));
            _ = builder.AppendLine(
                $"""
                        private {resultType} {fieldName} = default!;

                """);
            AppendAsyncResultGetterWrapper(builder, resultGetter, fieldName);
            _ = builder.AppendLine(
                $$"""
                        async void {{interfaceName}}.{{method.Method.Name}}({{explicitParameters}})
                        {
                            try
                            {
                                {{fieldName}} = await {{asyncName}}({{convertedArguments}});
                """);
        }
        else
        {
            _ = builder.AppendLine(
                $$"""
                        async void {{interfaceName}}.{{method.Method.Name}}({{explicitParameters}})
                        {
                            try
                            {
                                await {{asyncName}}({{convertedArguments}});
                """);
        }

        _ = builder.AppendLine(
            $$"""
                            task.SetCompleted();
                        }
                        catch (Exception e)
                        {
                            task.SetException(e);
                        }
                    }

                    public abstract {{taskType}} {{asyncName}}({{asyncParameters}});

            """);
    }

    private static void AppendAsyncResultGetterWrapper(StringBuilder builder, MethodWithOwner resultGetter, string fieldName)
    {
        var interfaceName = ShortName(resultGetter.Owner.FullName);
        var method = resultGetter.Method;
        var explicitParameters = string.Join(", ", method.Parameters.Select(FormatParameter));

        if (method.ReturnDictionary is { } dictionary)
        {
            _ = builder.AppendLine(
                $$"""
                        void {{interfaceName}}.{{method.Name}}({{explicitParameters}})
                        {
                            {{dictionary.KeysName}} = [.. {{fieldName}}.Keys{{DictionarySdkKeyProjection(dictionary)}}];
                            {{dictionary.ValuesName}} = [.. {{fieldName}}.Values{{DictionarySdkValueProjection(dictionary)}}];
                            {{dictionary.CountName}} = {{fieldName}}.Count;
                        }

                """);
            return;
        }

        if (method.ReturnDateTimeOffset is { } dateTimeOffset)
        {
            _ = builder.AppendLine(
                $$"""
                        void {{interfaceName}}.{{method.Name}}({{explicitParameters}})
                        {
                            {{dateTimeOffset.TicksName}} = {{fieldName}}.UtcTicks;
                            {{dateTimeOffset.OffsetName}} = (int){{fieldName}}.Offset.TotalMinutes;
                        }

                """);
            return;
        }

        if (method.ReturnArrayCountName is { } returnCountName)
        {
            _ = builder.AppendLine(
                $$"""
                        {{method.ReturnType}} {{interfaceName}}.{{method.Name}}({{explicitParameters}})
                        {
                            var value = {{fieldName}};
                            {{returnCountName}} = value?.Length ?? 0;
                            return {{ArrayReturnFromSdkExpression(method, "value")}};
                        }

                """);
            return;
        }

        _ = builder.AppendLine(
            $"""
                    {method.ReturnType} {interfaceName}.{method.Name}() => {ReturnToAbiExpression(method, fieldName)};

            """);
    }

    private static bool TryAppendDateTimeOffsetProperty(StringBuilder builder, MethodWithOwner method)
    {
        if (method.Method is not { PropertyName: { } propertyName, ReturnDateTimeOffset: { } dateTimeOffset })
            return false;

        _ = builder.AppendLine(
            $$"""
                    public abstract DateTimeOffset {{propertyName}} { get; }

                    void {{ShortName(method.Owner.FullName)}}.{{method.Method.Name}}(out long {{dateTimeOffset.TicksName}}, out int {{dateTimeOffset.OffsetName}})
                    {
                        {{dateTimeOffset.TicksName}} = {{propertyName}}.UtcTicks;
                        {{dateTimeOffset.OffsetName}} = (int){{propertyName}}.Offset.TotalMinutes;
                    }

            """);
        return true;
    }

    private static bool TryAppendDateTimeOffsetChanged(StringBuilder builder, MethodWithOwner method)
    {
        if (method.Method.DateTimeOffsetParameters.Count is 0)
            return false;

        var explicitParameters = string.Join(", ", method.Method.Parameters.Select(FormatParameter));
        var publicParameters = string.Join(", ", DateTimeOffsetSdkParameters(method.Method));
        var convertedArguments = string.Join(", ", DateTimeOffsetSdkArguments(method.Method));
        var returnStatement = method.Method.ReturnType is "void" ? "" : "return ";
        _ = builder.AppendLine(
            $$"""
                    {{method.Method.ReturnType}} {{ShortName(method.Owner.FullName)}}.{{method.Method.Name}}({{explicitParameters}})
                    {
                        {{returnStatement}}{{method.Method.Name}}({{convertedArguments}});
                    }

                    public abstract {{method.Method.ReturnType}} {{method.Method.Name}}({{publicParameters}});

            """);
        return true;
    }

    private static bool TryAppendDictionaryProperty(StringBuilder builder, MethodWithOwner method)
    {
        if (method.Method is not { PropertyName: { } propertyName, ReturnDictionary: { } dictionary })
            return false;

        var publicType = DictionarySdkType(dictionary);
        _ = builder.AppendLine(
            $$"""
                    public abstract {{publicType}} {{propertyName}} { get; }

                    void {{ShortName(method.Owner.FullName)}}.{{method.Method.Name}}(out {{dictionary.KeyType}}[] {{dictionary.KeysName}}, out {{dictionary.ValueType}}[] {{dictionary.ValuesName}}, out int {{dictionary.CountName}})
                    {
                        {{dictionary.KeysName}} = [.. {{propertyName}}.Keys{{DictionarySdkKeyProjection(dictionary)}}];
                        {{dictionary.ValuesName}} = [.. {{propertyName}}.Values{{DictionarySdkValueProjection(dictionary)}}];
                        {{dictionary.CountName}} = {{propertyName}}.Count;
                    }

            """);
        return true;
    }

    private static IEnumerable<string> DateTimeOffsetSdkParameters(MethodDefinition method)
    {
        for (var i = 0; i < method.Parameters.Count; ++i)
        {
            if (TryMatchDateTimeOffsetExpansion(method, i, out var dateTimeOffset))
            {
                yield return $"DateTimeOffset {dateTimeOffset.Name}";
                ++i;
                continue;
            }

            if (method.Parameters[i].IsGeneratedArrayCount)
                continue;

            yield return FormatSdkParameter(method, method.Parameters[i]);
        }
    }

    private static IEnumerable<string> DateTimeOffsetSdkArguments(MethodDefinition method)
    {
        for (var i = 0; i < method.Parameters.Count; ++i)
        {
            if (TryMatchDateTimeOffsetExpansion(method, i, out var dateTimeOffset))
            {
                yield return $"new({dateTimeOffset.TicksName}, TimeSpan.FromMinutes({dateTimeOffset.OffsetName}))";
                ++i;
                continue;
            }

            if (method.Parameters[i].IsGeneratedArrayCount)
                continue;

            yield return FormatSdkArgument(method, method.Parameters[i]);
        }
    }

    private static bool TryAppendDictionaryMethod(StringBuilder builder, MethodWithOwner method)
    {
        if (method.Method.DictionaryParameters.Count is 0)
            return false;

        var explicitParameters = string.Join(", ", method.Method.Parameters.Select(FormatParameter));
        var publicParameters = SdkParameterList(method.Method, skipTask: false);
        var convertedArguments = string.Join(", ", SdkArgumentList(method.Method, skipTask: false));
        var returnStatement = method.Method.ReturnType is "void" ? "" : "return ";

        _ = builder.AppendLine(
            $$"""
                    void {{ShortName(method.Owner.FullName)}}.{{method.Method.Name}}({{explicitParameters}})
                    {
                        {{returnStatement}}{{method.Method.Name}}({{convertedArguments}});
                    }

                    public abstract {{method.Method.ReturnType}} {{method.Method.Name}}({{publicParameters}});

            """);
        return true;
    }

    private static void AppendArrayParameterWrapper(StringBuilder builder, MethodWithOwner method)
    {
        var interfaceName = ShortName(method.Owner.FullName);
        var explicitParameters = string.Join(", ", method.Method.Parameters.Select(FormatParameter));
        var publicParameters = SdkParameterList(method.Method, skipTask: false);
        var convertedArguments = string.Join(", ", SdkArgumentList(method.Method, skipTask: false));

        _ = builder.AppendLine(
            $$"""
                    void {{interfaceName}}.{{method.Method.Name}}({{explicitParameters}})
                    {
                        {{method.Method.Name}}({{convertedArguments}});
                    }

            """);
        var publicArrayType = method.Method.Parameters.Any(parameter =>
            parameter.ArrayCountName is not null &&
            parameter.Type is "string[]")
            ? "string[]"
            : null;
        if (publicArrayType is not null && method.Method.Parameters.Count(static parameter => parameter.ArrayCountName is not null) is 1)
        {
            var arrayParameter = method.Method.Parameters.First(static parameter => parameter.ArrayCountName is not null);
            _ = builder.AppendLine($"        public abstract void {method.Method.Name}({arrayParameter.Type} {arrayParameter.Name});");
        }
        else
        {
            _ = builder.AppendLine($"        public abstract void {method.Method.Name}({publicParameters});");
        }
        _ = builder.AppendLine();
    }

    private static void AppendDirectMethod(StringBuilder builder, MethodWithOwner method)
    {
        if (method is { Method: { ReturnType: "void", Parameters.Count: 0, Name: "OnExtensionLoaded" or "OnExtensionUnloaded" }, Owner.SdkName: "ExtensionBase" })
        {
            _ = builder.AppendLine(
                $$"""
                        public virtual void {{method.Method.Name}}()
                        {
                        }

                """);
            return;
        }

        var parameters = string.Join(", ", method.Method.Parameters.Select(FormatParameter));
        _ = builder.AppendLine(
            $"""
                    public abstract {method.Method.ReturnType} {method.Method.Name}({parameters});

            """);
    }

    private static MethodWithOwner? FindAsyncResultGetter(IReadOnlyList<MethodWithOwner> methods, MethodDefinition taskMethod)
    {
        return methods
            .FirstOrDefault(method => ReferenceEquals(method.Method.AsyncSourceMethod, taskMethod));
    }

    private static string SdkParameterList(MethodDefinition method, bool skipTask)
    {
        return string.Join(", ", SdkParameters(method, skipTask));
    }

    private static IReadOnlyList<string> SdkArgumentList(MethodDefinition method, bool skipTask)
    {
        return SdkArguments(method, skipTask).ToList();
    }

    private static IEnumerable<string> SdkParameters(MethodDefinition method, bool skipTask)
    {
        var start = skipTask ? 1 : 0;
        for (var i = start; i < method.Parameters.Count; ++i)
        {
            if (TryMatchDictionaryExpansion(method, i, out var dictionary))
            {
                yield return $"{DictionarySdkType(dictionary)} {dictionary.Name}";
                ++i;
                continue;
            }

            if (TryMatchDateTimeOffsetExpansion(method, i, out var dateTimeOffset))
            {
                yield return $"DateTimeOffset {dateTimeOffset.Name}";
                ++i;
                continue;
            }

            var parameter = method.Parameters[i];
            if (parameter.IsGeneratedArrayCount)
                continue;

            yield return FormatSdkParameter(method, parameter);
        }
    }

    private static IEnumerable<string> SdkArguments(MethodDefinition method, bool skipTask)
    {
        var start = skipTask ? 1 : 0;
        for (var i = start; i < method.Parameters.Count; ++i)
        {
            if (TryMatchDictionaryExpansion(method, i, out var dictionary))
            {
                yield return DictionaryFromAbiExpression(dictionary);
                ++i;
                continue;
            }

            if (TryMatchDateTimeOffsetExpansion(method, i, out var dateTimeOffset))
            {
                yield return $"new({dateTimeOffset.TicksName}, TimeSpan.FromMinutes({dateTimeOffset.OffsetName}))";
                ++i;
                continue;
            }

            var parameter = method.Parameters[i];
            if (parameter.IsGeneratedArrayCount)
                continue;

            yield return FormatSdkArgument(method, parameter);
        }
    }

    private static string FormatSdkParameter(MethodDefinition method, ParameterDefinition parameter)
    {
        var type = SdkType(method, parameter);
        return $"{type} {parameter.Name}";
    }

    private static string FormatSdkArgument(MethodDefinition method, ParameterDefinition parameter)
    {
        if (parameter.ArrayCountName is { } countName)
        {
            var value = $"{parameter.Name}.Take({countName})";
            if (ElementType(parameter.Type) is "IStream")
                return $"[.. {value}.Select(static stream => stream.ToStream())]";

            return $"[.. {value}]";
        }

        return IsBuiltInStream(parameter)
            ? $"{parameter.Name}.ToStream()"
            : parameter.Name;
    }

    private static string SdkType(MethodDefinition method, ParameterDefinition parameter)
    {
        if (parameter.ArrayCountName is not null)
        {
            var elementType = ElementType(parameter.Type);
            return elementType switch
            {
                "IStream" when IsBuiltInStream(parameter) => "IReadOnlyList<Stream>",
                _ => $"IReadOnlyList<{elementType}>"
            };
        }

        return IsBuiltInStream(parameter) ? "Stream" : parameter.Type;
    }

    private static string SdkReturnType(MethodDefinition method)
    {
        if (method.ReturnDictionary is { } dictionary)
            return DictionarySdkType(dictionary);

        if (method.ReturnDateTimeOffset is not null)
            return nameof(DateTimeOffset);

        if (method.ReturnArrayCountName is not null)
            return SdkArrayReturnType(method);

        return method.ReturnIsBuiltInStream ? "Stream" : method.ReturnType;
    }

    private static string SdkArrayReturnType(MethodDefinition method)
    {
        var elementType = ElementType(method.ReturnType) is "IStream" && method.ReturnIsBuiltInStream
            ? "Stream"
            : ElementType(method.ReturnType);
        return IsNullableArrayType(method.ReturnType) ? $"{elementType}[]?" : $"{elementType}[]";
    }

    private static bool HasArrayParameters(MethodDefinition method)
    {
        return method.Parameters.Any(static parameter => parameter.ArrayCountName is not null);
    }

    private static string FormatParameter(ParameterDefinition parameter)
    {
        var direction = parameter.IsOut ? "out " : "";
        var defaultValue = parameter.DefaultValue is null ? "" : $" = {parameter.DefaultValue}";
        return $"{direction}{parameter.Type} {parameter.Name}{defaultValue}";
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

    private static bool IsConcreteSettingsInterface(SdkContext context, InterfaceDefinition definition, out string settingsType)
    {
        settingsType = "";
        if (ShortName(definition.FullName) is "ISettingsExtension" ||
            !context.FlattenInterfaces(definition).Any(static value => ShortName(value.FullName) is "ISettingsExtension"))
            return false;

        var shortName = ShortName(definition.FullName);
        if (!shortName.StartsWith('I') ||
            !shortName.EndsWith("SettingsExtension", StringComparison.Ordinal))
            return false;

        settingsType = shortName[1..^"SettingsExtension".Length];
        return settingsType.Length > 0;
    }

    private static string SdkNamespace(InterfaceDefinition definition)
    {
        const string sourcePrefix = "Pixeval.Extensions.Common";
        const string targetPrefix = "Pixeval.Extensions.SDK";

        var sourceNamespace = NamespaceOf(definition.FullName);
        if (!sourceNamespace.StartsWith(sourcePrefix, StringComparison.Ordinal))
            return targetPrefix;

        var suffix = sourceNamespace[sourcePrefix.Length..];
        if (suffix.StartsWith(".Commands", StringComparison.Ordinal))
            suffix = suffix[".Commands".Length..];

        return targetPrefix + suffix;
    }

    private static string ElementType(string type)
    {
        return type.EndsWith("[]", StringComparison.Ordinal) ? type[..^2] : type;
    }

    private static string ArrayReturnExpression(string returnType, string value)
    {
        return IsNullableArrayType(returnType) ? value : $"{value} ?? []";
    }

    private static string ArrayReturnFromSdkExpression(MethodDefinition method, string value)
    {
        if (ElementType(method.ReturnType) is "IStream" && method.ReturnIsBuiltInStream)
        {
            var expression = $"[.. {value}.Select(static stream => stream.ToIStream())]";
            return IsNullableArrayType(method.ReturnType)
                ? $"{value} is null ? null : {expression}"
                : $"{value} is null ? [] : {expression}";
        }

        return ArrayReturnExpression(method.ReturnType, value);
    }

    private static string ReturnToAbiExpression(MethodDefinition method, string value)
    {
        return method.ReturnIsBuiltInStream ? $"{value}.ToIStream()" : value;
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

    private static string DictionarySdkType(DictionaryExpansion dictionary)
    {
        return $"IReadOnlyDictionary<{SdkDictionaryElementType(dictionary.KeyType)}, {SdkDictionaryElementType(dictionary.ValueType)}>";
    }

    private static string SdkDictionaryElementType(string type)
    {
        return type is "IStream" ? "Stream" : type;
    }

    private static string DictionarySdkKeyProjection(DictionaryExpansion dictionary)
    {
        return dictionary.KeyType is "IStream" ? ".Select(static stream => stream.ToIStream())" : "";
    }

    private static string DictionarySdkValueProjection(DictionaryExpansion dictionary)
    {
        return dictionary.ValueType is "IStream" ? ".Select(static stream => stream.ToIStream())" : "";
    }

    private static string DictionaryFromAbiExpression(DictionaryExpansion dictionary)
    {
        var keyType = SdkDictionaryElementType(dictionary.KeyType);
        var valueType = SdkDictionaryElementType(dictionary.ValueType);
        var keyExpression = DictionaryElementFromAbiExpression(dictionary.KeyType, "key");
        var valueExpression = DictionaryElementFromAbiExpression(dictionary.ValueType, "value");
        return $"{dictionary.KeysName}.Take({dictionary.CountName}).Zip({dictionary.ValuesName}.Take({dictionary.CountName}), static (key, value) => new KeyValuePair<{keyType}, {valueType}>({keyExpression}, {valueExpression})).ToDictionary(static pair => pair.Key, static pair => pair.Value)";
    }

    private static string DictionaryElementFromAbiExpression(string type, string name)
    {
        return type is "IStream" ? $"{name}.ToStream()" : name;
    }

    private static bool IsBuiltInStream(ParameterDefinition parameter)
    {
        return parameter.IsBuiltInStream || parameter.Type is "IStream" && parameter.Dictionary is { ValueType: "IStream" };
    }

    private static bool IsNullableArrayType(string type)
    {
        return type.EndsWith("[]?", StringComparison.Ordinal);
    }

    private static string GetDefaultPropertyName(string methodName)
    {
        if (methodName.Length > 3 &&
            (methodName.StartsWith("Get", StringComparison.Ordinal) || methodName.StartsWith("Set", StringComparison.Ordinal)))
            return methodName[3..];

        return methodName;
    }

    private static string LowerFirst(string value)
    {
        return value.Length is 0 ? value : char.ToLowerInvariant(value[0]) + value[1..];
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

    private sealed class SdkContext
    {
        private readonly Dictionary<string, InterfaceDefinition> _interfaces;

        public SdkContext(PidlModel model) => _interfaces = model.Interfaces
                .SelectMany(static definition => new[]
                {
                    new KeyValuePair<string, InterfaceDefinition>(definition.FullName, definition),
                    new KeyValuePair<string, InterfaceDefinition>(ShortName(definition.FullName), definition)
                })
                .GroupBy(static pair => pair.Key, StringComparer.Ordinal)
                .ToDictionary(static group => group.Key, static group => group.First().Value, StringComparer.Ordinal);

        public InterfaceDefinition? ResolveSdkBaseInterface(InterfaceDefinition definition)
        {
            if (definition.Inherits is null)
                return null;

            var current = RequireInterface(definition.Inherits);
            while (true)
            {
                if (current.SdkName is not null)
                    return current;

                if (current.Inherits is null)
                    return null;

                current = RequireInterface(current.Inherits);
            }
        }

        public InterfaceDefinition RequireInterface(string name)
        {
            if (_interfaces.TryGetValue(name, out var definition))
                return definition;

            throw new InvalidOperationException($"Interface '{name}' was not found in PIDL.");
        }

        public IReadOnlyList<InterfaceDefinition> FlattenInterfaces(InterfaceDefinition definition)
        {
            var result = new List<InterfaceDefinition>();
            AppendInterfaceChain(definition, result);
            return result;
        }

        public IReadOnlyList<MethodWithOwner> FlattenMethods(InterfaceDefinition definition)
        {
            return FlattenInterfaces(definition)
                .SelectMany(static owner => owner.Methods.Select(method => new MethodWithOwner(owner, method)))
                .ToList();
        }

        private void AppendInterfaceChain(InterfaceDefinition definition, List<InterfaceDefinition> result)
        {
            if (definition.Inherits is not null)
                AppendInterfaceChain(RequireInterface(definition.Inherits), result);

            result.Add(definition);
        }
    }

    private sealed record MethodWithOwner(InterfaceDefinition Owner, MethodDefinition Method);
}
