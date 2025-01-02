// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Settings;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("F0184180-57D6-4397-B9DF-8C816F283F30")]
public partial interface IStringsArrayExtension : ISettingsExtension
{
    int GetDefaultValueCount();

    [return: MarshalUsing(CountElementName = nameof(count))]
    string[] GetDefaultValue(int count);
}

public static class StringsArrayExtensionHelper
{
    public static string[] GetDefaultValue(this IStringsArrayExtension extension)
    {
        return extension.GetDefaultValue(extension.GetDefaultValueCount());
    }
}
