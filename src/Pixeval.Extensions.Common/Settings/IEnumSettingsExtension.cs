// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Settings;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("8C8B59D6-861A-4CBA-9DEC-9C78EEE6C819")]
public partial interface IEnumSettingsExtension : IIntOrEnumSettingsExtension
{
    /// <summary>
    /// Get the items count of <see cref="GetEnumKeyValues"/>.
    /// </summary>
    int GetEnumCount();

    void GetEnumKeyValues(
        int count,
        [MarshalUsing(CountElementName = nameof(count))]
        out string[] enumNames,
        [MarshalUsing(CountElementName = nameof(count))]
        out int[] enumValues);
}

public static class EnumSettingsExtensionHelper
{
    /// <inheritdoc cref="IEnumSettingsExtension.GetEnumKeyValues"/>
    public static Dictionary<string, int> GetEnumKeyValues(this IEnumSettingsExtension extension)
    {
        var count = extension.GetEnumCount();
        extension.GetEnumKeyValues(count, out var enumNames, out var enumValues);
        return enumNames.Zip(enumValues, (name, value) => (name, value)).ToDictionary(x => x.name, x => x.value);
    }
}
