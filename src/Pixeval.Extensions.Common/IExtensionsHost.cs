// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

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

    string GetVersion();

    int GetExtensionsCount();

    [return: MarshalUsing(CountElementName = nameof(count))]
    IExtension[] GetExtensions(int count);

    int GetIconBytesCount();

    [return: MarshalUsing(CountElementName = nameof(count))]
    byte[]? GetIcon(int count);

    void Initialize(string cultureName, string tempDirectory, string extensionDirectory);

    public delegate int DllGetExtensionsHost(out nint ppv);
}

public static class ExtensionsHostHelper
{
    public static IExtension[] GetExtensions(this IExtensionsHost host)
    {
        var count = host.GetExtensionsCount();
        return host.GetExtensions(count);
    }

    public static byte[]? GetIcon(this IExtensionsHost host)
    {
        var count = host.GetIconBytesCount();
        return count < 0 ? null : host.GetIcon(count);
    }
}
