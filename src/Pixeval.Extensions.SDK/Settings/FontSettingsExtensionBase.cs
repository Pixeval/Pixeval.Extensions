// Copyright (c) Pixeval.Extensions.SDK.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Extensions.SDK.Settings;

/// <inheritdoc cref="IEnumSettingsExtension" />
[GeneratedComClass]
public abstract partial class FontSettingsExtensionBase : SettingsExtensionBase, IStringSettingsExtension
{
    /// <inheritdoc />
    public override SettingsType SettingsType => SettingsType.Font;

    /// <inheritdoc cref="IStringSettingsExtension.GetDefaultValue" />
    public abstract string DefaultValue { get; }

    /// <inheritdoc />
    string IStringSettingsExtension.GetDefaultValue() => DefaultValue;

    /// <inheritdoc cref="IStringSettingsExtension.GetPlaceholder" />
    public string GetPlaceholder() => "";

    /// <inheritdoc />
    public abstract void OnValueChanged(string value);
}
