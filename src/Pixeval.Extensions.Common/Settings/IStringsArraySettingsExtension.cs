// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Settings;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("F0184180-57D6-4397-B9DF-8C816F283F30")]
public partial interface IStringsArraySettingsExtension : ISettingsExtension
{
    [return: MarshalUsing(CountElementName = nameof(count))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    string[] GetDefaultValue(out int count);

    [EditorBrowsable(EditorBrowsableState.Never)]
    void OnValueChanged([MarshalUsing(CountElementName = nameof(count))] string[] value, int count);
}

public static class StringsArraySettingsExtensionHelper
{
    extension(IStringsArraySettingsExtension extension)
    {
        /// <inheritdoc cref="IStringsArraySettingsExtension.GetDefaultValue"/>
        public string[] DefaultValue => extension.GetDefaultValue(out _);

        /// <inheritdoc cref="IStringsArraySettingsExtension.OnValueChanged"/>
        public void OnValueChanged(string[] value) => extension.OnValueChanged(value, value.Length);
    }
}
