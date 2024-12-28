// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Settings;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("8C8B59D6-861A-4CBA-9DEC-9C78EEE6C819")]
public partial interface IEnumSettingsExtension : ISettingsExtension
{
    int GetDefaultValue();

    int GetEnumCount();

    void GetEnumKeyValues(
        int count,
        [MarshalUsing(CountElementName = nameof(count))]
        out string[] enumNames,
        [MarshalUsing(CountElementName = nameof(count))]
        out int[] enumValues);
}
