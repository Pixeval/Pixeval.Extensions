// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Settings;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("8C8B59D6-861A-4CBA-9DEC-9C78EEE6C819")]
public partial interface IEnumSettingsExtension : IIntOrEnumSettingsExtension
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    void GetEnumKeyValues(
        out int count,
        [MarshalUsing(CountElementName = nameof(count))]
        out string[] enumNames,
        [MarshalUsing(CountElementName = nameof(count))]
        out int[] enumValues);
}

public static partial class SettingsExtensionHelper
{
    extension(IEnumSettingsExtension extension)
    {
        /// <inheritdoc cref="IEnumSettingsExtension.GetEnumKeyValues"/>
        public Dictionary<string, int> GetEnumKeyValues()
        {
            extension.GetEnumKeyValues(out _, out var enumNames, out var enumValues);
            return enumNames.Zip(enumValues, (name, value) => (name, value)).ToDictionary(x => x.name, x => x.value);
        }
    }
}
