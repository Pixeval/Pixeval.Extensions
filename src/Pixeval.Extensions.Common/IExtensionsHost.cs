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

    void Initialize(string cultureBcl47, string tempDirectory);

    void OnStringPropertyChanged(string token, string value);

    void OnIntOrEnumPropertyChanged(string token, int value);

    void OnDoublePropertyChanged(string token, double value);

    void OnColorPropertyChanged(string token, uint value);


    void OnBoolPropertyChanged(string token, [MarshalAs(UnmanagedType.Bool)] bool value);

    void OnStringsArrayPropertyChanged(string token, [MarshalUsing(CountElementName = nameof(count))] string[] value, int count);

    public delegate int DllGetExtensionsHost(out nint ppv);
}

public static class ExtensionsHostHelper
{
    public static IExtension[] GetExtensions(this IExtensionsHost host)
    {
        var count = host.GetExtensionsCount();
        return host.GetExtensions(count);
    }
}
