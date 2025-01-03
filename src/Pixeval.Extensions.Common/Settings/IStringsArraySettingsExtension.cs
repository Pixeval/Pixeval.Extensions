// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Settings;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("F0184180-57D6-4397-B9DF-8C816F283F30")]
public partial interface IStringsArraySettingsExtension : ISettingsExtension
{
    int GetDefaultValueCount();

    [return: MarshalUsing(CountElementName = nameof(count))]
    string[] GetDefaultValue(int count);

    void OnValueChanged([MarshalUsing(CountElementName = nameof(count))] string[] value, int count);
}

public static class StringsArraySettingsExtensionHelper
{
    public static string[] GetDefaultValue(this IStringsArraySettingsExtension extension)
    {
        return extension.GetDefaultValue(extension.GetDefaultValueCount());
    }

    public static void OnValueChanged(this IStringsArraySettingsExtension extension, string[] value)
    {
        extension.OnValueChanged(value, value.Length);
    }
}
