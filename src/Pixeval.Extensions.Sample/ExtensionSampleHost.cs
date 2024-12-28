// Copyright (c) Pixeval.Extensions.Sample.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.SDK;

namespace Pixeval.Extensions.Sample;

public class ExtensionSampleHost : ExtensionsHostBase
{
    internal static StrategyBasedComWrappers ComWrappers { get; } = new();

    public override string ExtensionName => "Pixeval Extension Sample";

    public override IExtension[] Extensions { get; }

    [UnmanagedCallersOnly(EntryPoint = nameof(DllGetAllObject))]
    private static unsafe int DllGetAllObject(Guid* riid, void*** pppv, int* count)
    {
    }

    public override void Initialize(string cultureBcl47) => throw new NotImplementedException();

    public override void OnStringPropertyChanged(string name, string value) => throw new NotImplementedException();

    public override void OnIntOrEnumPropertyChanged(string name, int value) => throw new NotImplementedException();

    public override void OnDoublePropertyChanged(string name, double value) => throw new NotImplementedException();

    public override void OnColorPropertyChanged(string name, uint value) => throw new NotImplementedException();

    public override void OnBoolPropertyChanged(string name, bool value) => throw new NotImplementedException();

    public override void OnStringsArrayPropertyChanged(string name, string[] value) =>
        throw new NotImplementedException();
}
