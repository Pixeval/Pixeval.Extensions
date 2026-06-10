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
            "cpp-common" => CppEmitter.EmitCommon(PidlParser.Parse(sources)),
            "cpp-sdk" => CppEmitter.EmitSdk(PidlParser.Parse(sources)),
            "cpp-symbols" => CppSymbolEmitter.Emit(),
            "python-common" => PythonEmitter.EmitCommon(PidlParser.Parse(sources)),
            "python-sdk" => PythonEmitter.EmitSdk(PidlParser.Parse(sources)),
            "python-symbols" => PythonSymbolEmitter.Emit(),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Unsupported generator target.")
        };
    }
}
