// Copyright (c) Pixeval.Extensions.SDK.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Extensions.SDK.Settings;

[GeneratedComClass]
[Guid("29E75ED9-784A-4F5D-95A8-F7187569435E")]
public abstract partial class StringsArraySettingsExtensionBase : SettingsExtensionBase, IStringsArraySettingsExtension
{
    /// <inheritdoc />
    public override SettingsType SettingsType => SettingsType.StringsArray;

    /// <inheritdoc cref="IStringsArraySettingsExtension.GetDefaultValue" />
    public abstract string[] DefaultValue { get; }

    /// <inheritdoc />
    int IStringsArraySettingsExtension.GetDefaultValueCount() => DefaultValue.Length;

    /// <inheritdoc />
    string[] IStringsArraySettingsExtension.GetDefaultValue(int count) => count == DefaultValue.Length ? DefaultValue : [];
}
