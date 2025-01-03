// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Settings;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("09AF84C5-58F9-4E3C-9541-25AC1CF16D78")]
public partial interface IBoolSettingsExtension : ISettingsExtension
{
    [return: MarshalAs(UnmanagedType.Bool)]
    bool GetDefaultValue();

    void OnValueChanged([MarshalAs(UnmanagedType.Bool)] bool value);
}
