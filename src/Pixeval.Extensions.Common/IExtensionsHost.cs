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

    int GetExtensionsCount();

    [return: MarshalUsing(CountElementName = nameof(count))]
    IExtension[] GetExtension(int count);

    void Initialize(string cultureBcl47);

    void OnStringPropertyChanged(string name, string value);

    void OnIntOrEnumPropertyChanged(string name, int value);

    void OnDoublePropertyChanged(string name, double value);

    void OnColorPropertyChanged(string name, uint value);


    void OnBoolPropertyChanged(string name, [MarshalAs(UnmanagedType.Bool)] bool value);

    void OnStringsArrayPropertyChanged(string name, [MarshalUsing(CountElementName = nameof(count))] string[] value,
        int count);
}
