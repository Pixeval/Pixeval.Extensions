using System;

namespace Pixeval.Extensions.Generator.Models;

internal sealed class AsyncMethodDefinition(string returnType)
{
    public string ReturnType { get; } = returnType;

    public string GetResultGetterName(string methodName)
    {
        var name = methodName.EndsWith("Async", StringComparison.Ordinal)
            ? methodName[..^"Async".Length]
            : methodName;

        return "Get" + name + "Result";
    }
}