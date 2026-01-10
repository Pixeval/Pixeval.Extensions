// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Settings;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("E6E22298-4D0C-4041-B925-320EE3A7AF33")]
public partial interface IDateTimeOffsetSettingsExtension : ISettingsExtension
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    void GetDefaultValue(out long utcDateTimeTicks, out int minutesOffset);

    [EditorBrowsable(EditorBrowsableState.Never)]
    void OnValueChanged(long utcDateTimeTicks, int minutesOffset);
}

public static partial class SettingsExtensionHelper
{
    extension(IDateTimeOffsetSettingsExtension extension)
    {
        /// <inheritdoc cref="IDateTimeOffsetSettingsExtension.GetDefaultValue"/>
        public DateTimeOffset DefaultValue
        {
            get
            {
                extension.GetDefaultValue(out var utcDateTimeTicks, out var minutesOffset);
                return new(utcDateTimeTicks, TimeSpan.FromMinutes(minutesOffset));
            }
        }

        /// <inheritdoc cref="IDateTimeOffsetSettingsExtension.OnValueChanged"/>
        public void OnValueChanged(DateTimeOffset dateTimeOffset)
        {
            extension.OnValueChanged(dateTimeOffset.UtcTicks, dateTimeOffset.Offset.Minutes);
        }
    }
}
