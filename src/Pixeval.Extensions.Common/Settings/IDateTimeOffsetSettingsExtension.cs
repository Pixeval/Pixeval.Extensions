// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Settings;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("E6E22298-4D0C-4041-B925-320EE3A7AF33")]
public partial interface IDateTimeOffsetSettingsExtension : ISettingsExtension
{
    void GetDefaultValue(out long utcDateTimeTicks, out int minutesOffset);
}

public static class DateTimeOffsetSettingsExtensionHelper
{
    public static DateTimeOffset GetDefaultValue(this IDateTimeOffsetSettingsExtension settings)
    {
        settings.GetDefaultValue(out var utcDateTimeTicks, out var minutesOffset);
        return new DateTimeOffset(utcDateTimeTicks, TimeSpan.FromMinutes(minutesOffset));
    }
}
