using System.Collections.Generic;

namespace Pixeval.Extensions.Generator.Models;

internal sealed class InterfaceDefinition(string fullName)
{
    public string FullName { get; } = fullName;

    public List<PidlAttribute> Attributes { get; } = [];

    public List<string> Documentation { get; } = [];

    public string? Guid { get; set; }

    public string? Inherits { get; set; }

    public string? Editor { get; set; }

    public string? SdkName { get; set; }

    public List<PropertyDefinition> Properties { get; } = [];

    public List<MethodDefinition> Methods { get; } = [];
}
