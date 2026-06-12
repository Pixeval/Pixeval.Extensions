namespace Pixeval.Extensions.Generator.Models;

internal sealed class DictionaryExpansion(string name, string keyType, string valueType, string keysName, string valuesName, string countName)
{
    public string Name { get; } = name;

    public string KeyType { get; } = keyType;

    public string ValueType { get; } = valueType;

    public string KeysName { get; } = keysName;

    public string ValuesName { get; } = valuesName;

    public string CountName { get; } = countName;
}