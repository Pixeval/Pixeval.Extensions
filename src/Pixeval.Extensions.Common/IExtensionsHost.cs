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

    [return: MarshalUsing(CountElementName = nameof(count))]
    IExtension[] GetExtensions(out int count);

    [return: MarshalUsing(CountElementName = nameof(count))]
    byte[]? GetIcon(out int count);

    void Initialize(string cultureName, string tempDirectory, string extensionDirectory);

    public delegate int DllGetExtensionsHost(out nint ppv);

    /// <inheritdoc cref="GetSdkVersion" />
    public static Version SdkVersion => typeof(IExtensionsHost).Assembly.GetName().Version ?? new Version();
}

public static class ExtensionsHostHelper
{
    /// <inheritdoc cref="IExtensionsHost.GetExtensions"/>
    public static IExtension[] GetExtensions(this IExtensionsHost host) => host.GetExtensions(out _);

    /// <inheritdoc cref="IExtensionsHost.GetIcon"/>
    public static byte[]? GetIcon(this IExtensionsHost host) => host.GetIcon(out _);
}
