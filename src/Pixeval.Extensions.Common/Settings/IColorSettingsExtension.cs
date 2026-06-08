// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Settings;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("6EC83E36-7F38-4D4C-8B82-98F8A81B328A")]
[EditorBrowsable(EditorBrowsableState.Advanced)]
public partial interface IColorSettingsExtension : ISettingsExtension
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    uint GetDefaultValue();

    void OnValueChanged(uint value);
}

public static partial class SettingsExtensionHelper
{
    extension(IColorSettingsExtension extension)
    {
        /// <inheritdoc cref="IColorSettingsExtension.GetDefaultValue"/>
        public uint DefaultValue => extension.GetDefaultValue();
    }
}

