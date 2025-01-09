// Copyright (c) Pixeval.Extensions.SDK.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Extensions.SDK.Settings;

/// <inheritdoc cref="IColorSettingsExtension" />
[GeneratedComClass]
public abstract partial class ColorSettingsExtensionBase : SettingsExtensionBase, IColorSettingsExtension
{
    /// <inheritdoc />
    public override SettingsType SettingsType => SettingsType.Color;

    /// <inheritdoc cref="IColorSettingsExtension.GetDefaultValue" />
    public abstract uint DefaultValue { get; }

    /// <inheritdoc />
    uint IColorSettingsExtension.GetDefaultValue() => DefaultValue;

    /// <inheritdoc />
    public abstract void OnValueChanged(uint value);
}
