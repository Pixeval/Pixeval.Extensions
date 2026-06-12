namespace Pixeval.Extensions.Generator.Models;

internal sealed class DateTimeOffsetExpansion(string name, string ticksName, string offsetName)
{
    public string Name { get; } = name;

    public string TicksName { get; } = ticksName;

    public string OffsetName { get; } = offsetName;
}