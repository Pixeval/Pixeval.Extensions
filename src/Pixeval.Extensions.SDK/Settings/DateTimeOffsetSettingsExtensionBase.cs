// Copyright (c) Pixeval.Extensions.SDK.
// Licensed under the GPL v3 License.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Extensions.SDK.Settings;

[GeneratedComClass]
[Guid("0F0FD657-5775-4049-9F86-B4BA0F1433A1")]
public abstract partial class DateTimeOffsetSettingsExtensionBase : SettingsExtensionBase, IDateTimeOffsetSettingsExtension
{
    /// <inheritdoc />
    public override SettingsType SettingsType => SettingsType.DateTimeOffset;

    /// <inheritdoc cref="IDateTimeOffsetSettingsExtension.GetDefaultValue" />
    public abstract DateTimeOffset DefaultValue { get; }

    /// <inheritdoc cref="IDateTimeOffsetSettingsExtension.GetDefaultValue" />
    public void GetDefaultValue(out long utcDateTimeTicks, out int minutesOffset)
    {
        utcDateTimeTicks = DefaultValue.UtcTicks;
        minutesOffset = DefaultValue.TotalOffsetMinutes;
    }

    /// <inheritdoc />
    void IDateTimeOffsetSettingsExtension.OnValueChanged(long utcDateTimeTicks, int minutesOffset)
    {
        OnValueChanged(new(utcDateTimeTicks, TimeSpan.FromMinutes(minutesOffset)));
    }

    /// <inheritdoc cref="IDateTimeOffsetSettingsExtension.OnValueChanged" />
    public abstract void OnValueChanged(DateTimeOffset dateTimeOffset);
}
