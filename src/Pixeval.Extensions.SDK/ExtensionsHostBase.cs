// Copyright (c) Pixeval.Extensions.SDK.
// Licensed under the GPL v3 License.

using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common;

namespace Pixeval.Extensions.SDK;

[GeneratedComClass]
public abstract partial class ExtensionsHostBase : IExtensionsHost
{
    private static readonly StrategyBasedComWrappers _ComWrappers = new();

    private static unsafe void* _CcwCache;

    public static string TempDirectory { get; protected set; } = "";

    public static string ExtensionDirectory { get; protected set; } = "";

    public ILogger Logger { get; private set; } = null!;

    public abstract string ExtensionName { get; }

    public abstract string AuthorName { get; }

    public abstract string ExtensionLink { get; }

    public abstract string HelpLink { get; }

    public abstract string Description { get; }

    public abstract string Version { get; }

    public abstract IExtension[] Extensions { get; }

    public abstract byte[]? Icon { get; }

    public virtual void Initialize()
    {
    }

    string IExtensionsHost.GetExtensionName() => ExtensionName;

    string IExtensionsHost.GetAuthorName() => AuthorName;

    string IExtensionsHost.GetExtensionLink() => ExtensionLink;

    string IExtensionsHost.GetHelpLink() => HelpLink;

    string IExtensionsHost.GetDescription() => Description;

    string IExtensionsHost.GetSdkVersion() => ExtensionsHostStatics.CurrentSdkVersion.ToString();

    string IExtensionsHost.GetVersion() => Version;

    IExtension[] IExtensionsHost.GetExtensions(out int returnCount)
    {
        var value = Extensions;
        returnCount = value.Length;
        return value;
    }

    byte[]? IExtensionsHost.GetIcon(out int returnCount)
    {
        var value = Icon;
        returnCount = value?.Length ?? 0;
        return value;
    }

    void IExtensionsHost.Initialize(string cultureName, string tempDirectory, string extensionDirectory, ILogger logger)
    {
        TempDirectory = tempDirectory;
        ExtensionDirectory = extensionDirectory;
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new(cultureName);
        Logger = logger;
        Initialize();
    }

    public static unsafe int GetExtensionsHost(void** ppv, ExtensionsHostBase current)
    {
        if (ppv is null)
            return HResults.EPointer;

        if (_CcwCache is null)
            _CcwCache = (void*)_ComWrappers.GetOrCreateComInterfaceForObject(current, CreateComInterfaceFlags.None);

        *ppv = _CcwCache;
        return HResults.SOk;
    }

    private static class HResults
    {
        public const int SOk = 0;

        public const int EPointer = unchecked((int) 0x80004003);
    }
}
