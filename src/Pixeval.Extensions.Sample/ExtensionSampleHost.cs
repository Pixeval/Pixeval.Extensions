// Copyright (c) Pixeval.Extensions.Sample.
// Licensed under the GPL v3 License.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.SDK;

namespace Pixeval.Extensions.Sample;

public class ExtensionSampleHost : ExtensionsHostBase
{
    internal static StrategyBasedComWrappers ComWrappers { get; } = new();

    public override string ExtensionName => "Pixeval Extension Sample";

    public override IExtension[] Extensions { get; } = [];

    public static ExtensionSampleHost Current { get; } = new();

    [UnmanagedCallersOnly(EntryPoint = nameof(DllGetMetadata))]
    private static unsafe int DllGetMetadata(void** ppv)
    {
        return DllGetMetadata(ppv, Current);
    }

    public override void Initialize(string cultureBcl47, string tempDirectory) => throw new NotImplementedException();

    public override void OnStringPropertyChanged(string token, string value) => throw new NotImplementedException();

    public override void OnIntOrEnumPropertyChanged(string token, int value) => throw new NotImplementedException();

    public override void OnDoublePropertyChanged(string token, double value) => throw new NotImplementedException();

    public override void OnColorPropertyChanged(string token, uint value) => throw new NotImplementedException();

    public override void OnBoolPropertyChanged(string token, bool value) => throw new NotImplementedException();

    public override void OnStringsArrayPropertyChanged(string token, string[] value) =>
        throw new NotImplementedException();
}
