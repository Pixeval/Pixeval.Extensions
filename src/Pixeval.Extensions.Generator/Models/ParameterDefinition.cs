namespace Pixeval.Extensions.Generator.Models;

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