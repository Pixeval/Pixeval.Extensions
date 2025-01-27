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

    public override byte[]? Icon => null;

    public static ExtensionSampleHost Current { get; } = new();

    [UnmanagedCallersOnly(EntryPoint = nameof(DllGetMetadata))]
    private static unsafe int DllGetMetadata(void** ppv)
    {
        return DllGetExtensionsHost(ppv, Current);
    }

    public override void Initialize(string cultureName, string tempDirectory, string extensionDirectory) => throw new NotImplementedException();
}
