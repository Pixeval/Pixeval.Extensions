// Copyright (c) Pixeval.Extensions.Sample.
// Licensed under the GPL v3 License.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.SDK;

namespace Pixeval.Extensions.Sample;

[GeneratedComClass]
public partial class ExtensionSampleHost : ExtensionsHostBase
{
    public override string ExtensionName => "Pixeval Extension Sample";

    public override string AuthorName => "my name";

    public override string ExtensionLink => "https://link.to.extension/";

    public override string HelpLink => "https://help.of.extension/";

    public override string Description => "This is a sample extension for Pixeval.";

    public override string Version => "1.0.0";

    public override IExtension[] Extensions { get; } = [];

    public static ExtensionSampleHost Current { get; } = new();

    [UnmanagedCallersOnly(EntryPoint = nameof(DllGetMetadata))]
    private static unsafe int DllGetMetadata(void** ppv)
    {
        return DllGetExtensionsHost(ppv, Current);
    }

    public override void Initialize(string cultureBcl47, string tempDirectory) => throw new NotImplementedException();

    public override void OnStringPropertyChanged(string token, string value) => throw new NotImplementedException();
    
    public override void OnIntPropertyChanged(string token, int value) => throw new NotImplementedException();

    public override void OnDoublePropertyChanged(string token, double value) => throw new NotImplementedException();
    
    public override void OnUIntPropertyChanged(string token, uint value) => throw new NotImplementedException();

    public override void OnBoolPropertyChanged(string token, bool value) => throw new NotImplementedException();

    public override void OnStringsArrayPropertyChanged(string token, string[] value) => throw new NotImplementedException();
    
    public override void OnDateTimeOffsetPropertyChanged(string token, DateTimeOffset dateTimeOffset) => throw new NotImplementedException();
}
