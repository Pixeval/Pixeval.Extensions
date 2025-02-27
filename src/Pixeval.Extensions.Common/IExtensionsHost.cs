// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("3FE29B79-550D-48A3-800F-F884145FA514")]
public partial interface IExtensionsHost
{
    string GetExtensionName();

    string GetAuthorName();

    string GetExtensionLink();

    string GetHelpLink();

    string GetDescription();

    string GetSdkVersion();

    string GetVersion();

    /// <summary>
    /// Get the items count of <see cref="GetExtensions"/>.
    /// </summary>
    int GetExtensionsCount();

    [return: MarshalUsing(CountElementName = nameof(count))]
    IExtension[] GetExtensions(int count);

    /// <summary>
    /// Get the items count of <see cref="GetIcon"/>.
    /// </summary>
    int GetIconBytesCount();

    [return: MarshalUsing(CountElementName = nameof(count))]
    byte[]? GetIcon(int count);

    void Initialize(string cultureName, string tempDirectory, string extensionDirectory);

    public delegate int DllGetExtensionsHost(out nint ppv);

    /// <inheritdoc cref="GetSdkVersion" />
    public static Version SdkVersion => typeof(IExtensionsHost).Assembly.GetName().Version ?? new Version();
}

public static class ExtensionsHostHelper
{
    /// <inheritdoc cref="IExtensionsHost.GetExtensions"/>
    public static IExtension[] GetExtensions(this IExtensionsHost host)
    {
        var count = host.GetExtensionsCount();
        return host.GetExtensions(count);
    }

    /// <inheritdoc cref="IExtensionsHost.GetIcon"/>
    public static byte[]? GetIcon(this IExtensionsHost host)
    {
        var count = host.GetIconBytesCount();
        return count < 0 ? null : host.GetIcon(count);
    }
}
