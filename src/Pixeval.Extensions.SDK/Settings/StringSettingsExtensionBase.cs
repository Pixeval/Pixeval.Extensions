// Copyright (c) Pixeval.Extensions.SDK.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Extensions.SDK.Settings;

/// <inheritdoc cref="IStringSettingsExtension" />
[GeneratedComClass]
public abstract partial class StringSettingsExtensionBase : SettingsExtensionBase, IStringSettingsExtension
{
    /// <inheritdoc />
    public override SettingsType SettingsType => SettingsType.String;

    /// <inheritdoc cref="IStringSettingsExtension.GetDefaultValue" />
    public abstract string DefaultValue { get; }

    /// <inheritdoc />
    string? IStringSettingsExtension.GetPlaceholder() => Placeholder;

    /// <inheritdoc />
    string IStringSettingsExtension.GetDefaultValue() => DefaultValue;

    /// <inheritdoc />
    public abstract void OnValueChanged(string value);
}
