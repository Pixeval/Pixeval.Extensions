// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("3FE29B79-550D-48A3-800F-F884145FA514")]
public partial interface IExtensionsHost
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    string GetExtensionName();

    [EditorBrowsable(EditorBrowsableState.Never)]
    string GetAuthorName();

    [EditorBrowsable(EditorBrowsableState.Never)]
    string GetExtensionLink();

    [EditorBrowsable(EditorBrowsableState.Never)]
    string GetHelpLink();

    [EditorBrowsable(EditorBrowsableState.Never)]
    string GetDescription();

    [EditorBrowsable(EditorBrowsableState.Never)]
    string GetSdkVersion();

    [EditorBrowsable(EditorBrowsableState.Never)]
    string GetVersion();

    [return: MarshalUsing(CountElementName = nameof(count))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    IExtension[] GetExtensions(out int count);

    [return: MarshalUsing(CountElementName = nameof(count))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    byte[]? GetIcon(out int count);

    void Initialize(string cultureName, string tempDirectory, string extensionDirectory, ILogger logger);

    delegate int DllGetExtensionsHost(out nint ppv);

    /// <inheritdoc cref="GetSdkVersion" />
    static Version SdkVersion => typeof(IExtensionsHost).Assembly.GetName().Version ?? new Version();
}

public static partial class ExtensionHelper
{
    extension(IExtensionsHost host)
    {
        /// <inheritdoc cref="IExtensionsHost.GetExtensionName"/>
        public string ExtensionName => host.GetExtensionName();

        /// <inheritdoc cref="IExtensionsHost.GetAuthorName"/>
        public string AuthorName => host.GetAuthorName();

        /// <inheritdoc cref="IExtensionsHost.GetExtensionLink"/>
        public string ExtensionLink => host.GetExtensionLink();

        /// <inheritdoc cref="IExtensionsHost.GetHelpLink"/>
        public string HelpLink => host.GetHelpLink();

        /// <inheritdoc cref="IExtensionsHost.GetDescription"/>
        public string Description => host.GetDescription();

        /// <inheritdoc cref="IExtensionsHost.GetSdkVersion"/>
        public string SdkVersion => host.GetSdkVersion();

        /// <inheritdoc cref="IExtensionsHost.GetVersion"/>
        public string Version => host.GetVersion();

        /// <inheritdoc cref="IExtensionsHost.GetExtensions"/>
        public IExtension[] Extensions => host.GetExtensions(out _);

        /// <inheritdoc cref="IExtensionsHost.GetIcon"/>
        public byte[]? Icon => host.GetIcon(out _);
    }
}
