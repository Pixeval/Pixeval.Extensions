namespace Pixeval.Extensions.Generator.Models;

internal sealed class PropertyDefinition(string name, string type, string? defaultValue)
{
    public string Name { get; } = name;

    public string Type { get; } = type;

    public string? DefaultValue { get; } = defaultValue;

    public MethodDefinition? Getter { get; set; }

    public MethodDefinition? Setter { get; set; }
}