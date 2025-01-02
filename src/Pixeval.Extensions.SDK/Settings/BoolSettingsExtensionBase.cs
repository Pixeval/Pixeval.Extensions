// Copyright (c) Pixeval.Extensions.SDK.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Extensions.SDK.Settings;

[GeneratedComClass]
[Guid("76625EC0-6AC3-44C9-9FF0-03D74AFCB2ED")]
public abstract partial class BoolSettingsExtensionBase : SettingsExtensionBase, IBoolSettingsExtension
{
    /// <inheritdoc />
    public override SettingsType SettingsType => SettingsType.Bool;

    /// <inheritdoc cref="IBoolSettingsExtension.GetDefaultValue" />
    public abstract bool DefaultValue { get; }

    /// <inheritdoc />
    bool IBoolSettingsExtension.GetDefaultValue() => DefaultValue;
}
