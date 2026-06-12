using System.Collections.Generic;

namespace Pixeval.Extensions.Generator.Models;

internal sealed class EnumDefinition(string fullName)
{
    public string FullName { get; } = fullName;

    public List<string> Documentation { get; } = [];

    public List<EnumValue> Values { get; } = [];
}