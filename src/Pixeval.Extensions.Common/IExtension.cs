// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("4D3DD2EB-9DF8-4D50-A104-8704400BB8EC")]
public partial interface IExtension
{
    void OnExtensionLoaded();

    void OnExtensionUnloaded();
}
