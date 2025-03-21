// Copyright (c) Pixeval.Extensions.SDK.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Extensions.SDK.Settings;

/// <inheritdoc cref="IDoubleSettingsExtension" />
[GeneratedComClass]
public abstract partial class DoubleSettingsExtensionBase : SettingsExtensionBase, IDoubleSettingsExtension
{
    /// <inheritdoc />
    public override SettingsType SettingsType => SettingsType.Double;

    /// <inheritdoc cref="IDoubleSettingsExtension.GetDefaultValue" />
    public abstract double DefaultValue { get; }

    /// <inheritdoc cref="IDoubleSettingsExtension.GetMinValue" />
    public abstract double MinValue { get; }

    /// <inheritdoc cref="IDoubleSettingsExtension.GetMaxValue" />
    public abstract double MaxValue { get; }

    /// <inheritdoc cref="IDoubleSettingsExtension.GetLargeChange" />
    public virtual int LargeChange => 10;

    /// <inheritdoc cref="IDoubleSettingsExtension.GetSmallChange" />
    public virtual int SmallChange => 1;

    /// <inheritdoc cref="IDoubleSettingsExtension.GetPlaceholder" />
    public abstract string? Placeholder { get; }

    /// <inheritdoc />
    public abstract void OnValueChanged(double value);

    /// <inheritdoc />
    double IDoubleSettingsExtension.GetDefaultValue() => DefaultValue;

    /// <inheritdoc />
    double IDoubleSettingsExtension.GetMinValue() => MinValue;

    /// <inheritdoc />
    double IDoubleSettingsExtension.GetMaxValue() => MaxValue;

    /// <inheritdoc />
    double IDoubleSettingsExtension.GetLargeChange() => LargeChange;

    /// <inheritdoc />
    double IDoubleSettingsExtension.GetSmallChange() => SmallChange;

    /// <inheritdoc />
    string? IDoubleSettingsExtension.GetPlaceholder() => Placeholder;
}
