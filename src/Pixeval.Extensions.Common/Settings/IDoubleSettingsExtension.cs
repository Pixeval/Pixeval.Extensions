// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Settings;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("06583FB2-4577-4E2A-B7F1-654B7C504D01")]
public partial interface IDoubleSettingsExtension : ISettingsExtension
{
    double GetDefaultValue();

    double GetMinValue();

    double GetMaxValue();

    double GetLargeChange();

    double GetSmallChange();

    string? GetPlaceholder();
}
