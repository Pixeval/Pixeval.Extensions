namespace Pixeval.Extensions.Generator.Models;

internal sealed class AsyncResultGetterDefinition(MethodDefinition sourceMethod)
{
    public MethodDefinition SourceMethod { get; } = sourceMethod;
}