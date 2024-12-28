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
    public override SettingsType SettingsType => SettingsType.Double;

    public abstract double DefaultValue { get; }

    double IDoubleSettingsExtension.GetDefaultValue() => DefaultValue;
}
