// Copyright (c) Pixeval.Extensions.SDK.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Extensions.SDK.Settings;

[GeneratedComClass]
[Guid("5E318661-8570-4217-BC84-F93697A12A62")]
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

    /// <inheritdoc />
    double IDoubleSettingsExtension.GetDefaultValue() => DefaultValue;

    /// <inheritdoc />
    double IDoubleSettingsExtension.GetMinValue() => MinValue;

    /// <inheritdoc />
    double IDoubleSettingsExtension.GetMaxValue() => MaxValue;
}
