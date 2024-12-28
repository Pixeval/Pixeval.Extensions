// Copyright (c) Pixeval.Extensions.SDK.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Extensions.SDK.Settings;

[GeneratedComClass]
[Guid("4108CBF6-C6CA-48C3-821B-D7E01F5C3ED9")]
public abstract partial class ColorSettingsExtensionBase : SettingsExtensionBase, IColorSettingsExtension
{
    public override SettingsType SettingsType => SettingsType.Color;

    public abstract uint DefaultValue { get; }

    uint IColorSettingsExtension.GetDefaultValue() => DefaultValue;
}
