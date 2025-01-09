// Copyright (c) Pixeval.Extensions.SDK.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Extensions.SDK.Settings;

/// <inheritdoc cref="IBoolSettingsExtension" />
[GeneratedComClass]
public abstract partial class BoolSettingsExtensionBase : SettingsExtensionBase, IBoolSettingsExtension
{
    /// <inheritdoc />
    public override SettingsType SettingsType => SettingsType.Bool;

    /// <inheritdoc cref="IBoolSettingsExtension.GetDefaultValue" />
    public abstract bool DefaultValue { get; }

    /// <inheritdoc />
    bool IBoolSettingsExtension.GetDefaultValue() => DefaultValue;

    /// <inheritdoc />
    public abstract void OnValueChanged(bool value);
}
