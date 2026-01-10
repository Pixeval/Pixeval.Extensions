// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Settings;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("E8625476-1693-47CE-8450-C9F3585573CC")]
public partial interface IStringSettingsExtension : ISettingsExtension
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    string GetDefaultValue();

    void OnValueChanged(string value);
}

public static partial class SettingsExtensionHelper
{
    extension(IStringSettingsExtension extension)
    {
        /// <inheritdoc cref="IStringSettingsExtension.GetDefaultValue"/>
        public string DefaultValue => extension.GetDefaultValue();
    }
}
