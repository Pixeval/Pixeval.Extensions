// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Settings;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("5494F273-034E-46CE-936F-ECCECFBDA4C9")]
public partial interface IIntOrEnumSettingsExtension : ISettingsExtension
{
    int GetDefaultValue();
}
