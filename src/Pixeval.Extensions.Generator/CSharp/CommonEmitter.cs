using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pixeval.Extensions.Generator.Models;

namespace Pixeval.Extensions.Generator;

internal static class CommonEmitter
{
    public static string Emit(PidlModel model)
    {
        var builder = new StringBuilder();
        AppendHeader(builder, model);

        foreach (var enumDefinition in model.Enums)
            AppendEnum(builder, enumDefinition);

        foreach (var interfaceDefinition in model.Interfaces)
            AppendInterface(builder, model.Metadata, interfaceDefinition);

        CSharpPropertyExtensionEmitter.Append(builder, model);
        return builder.ToString();
    }

    private static void AppendHeader(StringBuilder builder, PidlModel model)
    {
        CSharpEmitterSupport.AppendHeader(builder, CSharpEmitterSupport.CommonNamespaces(model));
    }

    private static void AppendEnum(StringBuilder builder, EnumDefinition definition)
    {
        using (Namespace(builder, NamespaceOf(definition.FullName)))
        {
            AppendDocumentation(builder, definition.Documentation);
            _ = builder.AppendLine(
                $$"""
                public enum {{ShortName(definition.FullName)}}
                {
                """);
            foreach (var value in definition.Values)
            {
                _ = builder.Append("    ").Append(value.Name);
                if (value.Value is not null)
                    _ = builder.Append(" = ").Append(value.Value);
                _ = builder.AppendLine(",");
            }
            _ = builder.AppendLine("}");
        }
    }

    private static void AppendInterface(StringBuilder builder, PidlMetadata metadata, InterfaceDefinition definition)
    {
        using (Namespace(builder, NamespaceOf(definition.FullName)))
        {
            AppendDocumentation(builder, definition.Documentation);
            if (definition.Guid is not null)
            {
                _ = builder.AppendLine(
                    $"""
                    [GeneratedComInterface(StringMarshalling = StringMarshalling.{metadata.StringMarshalling})]
                    [Guid("{definition.Guid}")]
                    """);
            }

            if (definition.Editor is not null)
                _ = builder.AppendLine($"[EditorBrowsable(EditorBrowsableState.{definition.Editor})]");

            var partial = definition.Guid is null ? "" : " partial";
            var inheritance = definition.Inherits is null ? "" : $" : {definition.Inherits}";
            _ = builder.AppendLine(
                $$"""
                public{{partial}} interface {{ShortName(definition.FullName)}}{{inheritance}}
                {
                """);

            foreach (var method in definition.Methods.Where(static method => !method.IsOverride))
                AppendMethod(builder, metadata, method);

            _ = builder.AppendLine("}");
        }
    }

    private static void AppendMethod(StringBuilder builder, PidlMetadata metadata, MethodDefinition method)
    {
        AppendDocumentation(builder, method.Documentation, "    ");

        if (method.ReturnArrayCountName is not null)
            _ = builder.AppendLine($"    [return: MarshalUsing(CountElementName = nameof({method.ReturnArrayCountName}))]");

        if (IsBoolType(method.ReturnType))
            _ = builder.AppendLine($"    [return: MarshalAs(UnmanagedType.{metadata.BoolMarshalling})]");

        if (method.Hidden)
            _ = builder.AppendLine("    [EditorBrowsable(EditorBrowsableState.Never)]");

        _ = builder
            .Append("    ")
            .Append(method.ReturnType)
            .Append(' ')
            .Append(method.Name)
            .Append('(')
            .Append(string.Join(", ", method.Parameters.Select(parameter => FormatParameter(metadata, method, parameter))))
            .AppendLine(");");
        _ = builder.AppendLine();
    }

    private static void AppendDocumentation(StringBuilder builder, IReadOnlyList<string> documentation, string indent = "")
    {
        if (documentation.Count is 0)
            return;

        _ = builder.Append(indent).AppendLine("/// <summary>");
        foreach (var line in documentation)
        {
            _ = builder.Append(indent).Append("///");
            if (line.Length > 0)
                _ = builder.Append(' ').Append(EscapeXml(line));
            _ = builder.AppendLine();
        }
        _ = builder.Append(indent).AppendLine("/// </summary>");
    }

    private static string EscapeXml(string value)
    {
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    private static string FormatParameter(PidlMetadata metadata, MethodDefinition method, ParameterDefinition parameter)
    {
        var attributes = new List<string>();
        if (parameter.ArrayCountName is { } countName)
        {
            var direction = method.ParamIn.Contains(parameter.Name)
                ? "In, "
                : method.ParamOut.Contains(parameter.Name)
                    ? "Out, "
                    : "";
            attributes.Add($"[{direction}MarshalUsing(CountElementName = nameof({countName}))]");
        }

        if (IsBoolType(parameter.Type))
            attributes.Add($"[MarshalAs(UnmanagedType.{metadata.BoolMarshalling})]");

        var builder = new StringBuilder();
        if (attributes.Count > 0)
            _ = builder.Append(string.Join(" ", attributes)).Append(' ');

        if (parameter.IsOut)
            _ = builder.Append("out ");

        _ = builder.Append(parameter.Type).Append(' ').Append(parameter.Name);
        if (parameter.DefaultValue is not null)
            _ = builder.Append(" = ").Append(parameter.DefaultValue);

        return builder.ToString();
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
        return fullName[..separator];
    }

    private static string ShortName(string fullName)
    {
        var separator = fullName.LastIndexOf('.');
        return separator < 0 ? fullName : fullName[(separator + 1)..];
    }

    private static bool IsBoolType(string type)
    {
        return type is "bool";
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
}
