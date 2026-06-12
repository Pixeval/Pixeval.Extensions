using System;
using System.Collections.Generic;

namespace Pixeval.Extensions.Generator.Models;

internal sealed class MethodDefinition(string returnType, string name, List<ParameterDefinition> parameters)
{
    public string ReturnType { get; set; } = returnType;

    public string Name { get; } = name;

    public List<PidlAttribute> Attributes { get; } = [];

    public List<ParameterDefinition> Parameters { get; } = parameters;

    public List<string> Documentation { get; } = [];

    public bool Hidden { get; set; }

    public bool IsVirtual { get; set; }

    public bool IsOverride { get; set; }

    public bool IsSealed { get; set; }

    public AsyncMethodDefinition? Async { get; set; }

    public bool IsAsync => Async is not null;

    public AsyncResultGetterDefinition? AsyncResultGetter { get; set; }

    public bool IsAsyncResultGetter => AsyncResultGetter is not null;

    public ReturnArrayDefinition? ReturnArray { get; set; }

    public string? ReturnArrayCountName => ReturnArray?.CountName;

    public DictionaryExpansion? ReturnDictionary { get; set; }

    public bool ReturnIsBuiltInStream { get; set; }

    public DateTimeOffsetExpansion? ReturnDateTimeOffset { get; set; }

    public PropertyDefinition? Property { get; set; }

    public PropertyAccessorKind PropertyAccessor => Property is null
        ? PropertyAccessorKind.None
        : ReferenceEquals(Property.Getter, this)
            ? PropertyAccessorKind.Getter
            : ReferenceEquals(Property.Setter, this)
                ? PropertyAccessorKind.Setter
                : PropertyAccessorKind.None;

    public List<DateTimeOffsetExpansion> DateTimeOffsetParameters { get; } = [];

    public List<DictionaryExpansion> DictionaryParameters { get; } = [];

    public HashSet<string> ParamIn { get; } = new(StringComparer.Ordinal);

    public HashSet<string> ParamOut { get; } = new(StringComparer.Ordinal);

}
