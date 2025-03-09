// Copyright (c) Pixeval.Extensions.Sample.
// Licensed under the GPL v3 License.

using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Sample.Strings;
using Pixeval.Extensions.SDK;

namespace Pixeval.Extensions.Sample;

[GeneratedComClass]
public partial class ExtensionHost : ExtensionsHostBase
{
    public override string ExtensionName => Resource.ExtensionHostName;

    public override string AuthorName => CultureInfo.CurrentUICulture.Name;

    public override string ExtensionLink => "https://github.com/Pixeval/Pixeval.Extensions/";

    public override string HelpLink => "https://help.of.extension/";

    public override string Description => "This is a sample extension for Pixeval.";

    public override string Version => "1.0.0";

    public override IExtension[] Extensions { get; } = [];

    public override byte[]? Icon
    {
        get
        {
            var stream = typeof(ExtensionHost).Assembly.GetManifestResourceStream("logo");
            if (stream is null)
                return null;
            var array = new byte[stream.Length];
            _ = stream.Read(array);
            return array;
        }
    }

    public static ExtensionHost Current { get; } = new();

    [UnmanagedCallersOnly(EntryPoint = nameof(DllGetExtensionsHost))]
    private static unsafe int DllGetExtensionsHost(void** ppv) => DllGetExtensionsHost(ppv, Current);

    public override void Initialize()
    {
        // initialized
    }
}
