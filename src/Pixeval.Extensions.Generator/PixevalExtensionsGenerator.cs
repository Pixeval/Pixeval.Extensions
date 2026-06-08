using System;
using System.Collections.Generic;

namespace Pixeval.Extensions.Generator;

public static class PixevalExtensionsCodeGenerator
{
    public static string Generate(string target, IEnumerable<string> sources)
    {
        return target.ToLowerInvariant() switch
        {
            "common" => CommonEmitter.Emit(PidlParser.Parse(sources)),
            "sdk" => SdkEmitter.Emit(PidlParser.Parse(sources)),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Unsupported generator target.")
        };
    }
}
