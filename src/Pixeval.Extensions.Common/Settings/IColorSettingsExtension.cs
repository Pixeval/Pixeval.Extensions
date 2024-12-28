// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Settings;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("71EA31D8-437F-4B0C-80A4-8FC6DBC2AB65")]
public partial interface IColorSettingsExtension : ISettingsExtension
{
    uint GetDefaultValue();
}
