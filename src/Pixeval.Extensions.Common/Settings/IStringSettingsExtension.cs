// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Settings;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("E8625476-1693-47CE-8450-C9F3585573CC")]
public partial interface IStringSettingsExtension : ISettingsExtension
{
    string GetDefaultValue();

    string? GetPlaceholder();

    void OnValueChanged(string value);
}
