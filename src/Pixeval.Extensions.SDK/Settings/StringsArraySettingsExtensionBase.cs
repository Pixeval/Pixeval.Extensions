// Copyright (c) Pixeval.Extensions.SDK.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common.Settings;
using Pixeval.Extensions.SDK.Internal;

namespace Pixeval.Extensions.SDK.Settings;

/// <inheritdoc cref="IStringsArraySettingsExtension" />
[GeneratedComClass]
public abstract partial class StringsArraySettingsExtensionBase : SettingsExtensionBase, IStringsArraySettingsExtension
{
    /// <inheritdoc />
    public override SettingsType SettingsType => SettingsType.StringsArray;

    /// <inheritdoc cref="IStringsArraySettingsExtension.GetDefaultValue" />
    public abstract string[] DefaultValue { get; }

    /// <inheritdoc cref="IStringsArraySettingsExtension.OnValueChanged" />
    public abstract void OnValueChanged(string[] value);

    /// <inheritdoc />
    public void OnValueChanged([MarshalUsing(CountElementName = nameof(count))] string[] value, int count)
    {
        if (count == value.Length)
            OnValueChanged(value);
    }

    /// <inheritdoc />
    string? IStringsArraySettingsExtension.GetPlaceholder() => Placeholder;

    /// <inheritdoc />
    string[] IStringsArraySettingsExtension.GetDefaultValue(out int count) => DefaultValue.GetArray(out count);
}
