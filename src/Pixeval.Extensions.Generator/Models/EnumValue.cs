namespace Pixeval.Extensions.Generator.Models;

internal sealed class EnumValue(string name, string? value)
{
    public string Name { get; } = name;

    public string? Value { get; } = value;
}