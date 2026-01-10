// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common.Settings;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("3A4512B2-19FD-4950-9161-E1B622059F37")]
public partial interface ISettingsExtension : IEntryExtension
{
    /// <summary>
    /// <see cref="GetDescription"/>的超链接
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    string? GetDescriptionUri();

    /// <summary>
    /// 设置项名
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    string GetToken();

    /// <summary>
    /// 设置项占位符
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    string? GetPlaceholder();

    /// <summary>
    /// 设置类型
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    SettingsType GetSettingsType();
}

public static partial class SettingsExtensionHelper
{
    extension(ISettingsExtension extension)
    {
        /// <inheritdoc cref="ISettingsExtension.GetDescriptionUri"/>
        public string? DescriptionUri => extension.GetDescriptionUri();

        /// <inheritdoc cref="ISettingsExtension.GetToken"/>
        public string Token => extension.GetToken();

        /// <inheritdoc cref="ISettingsExtension.GetPlaceholder"/>
        public string? Placeholder => extension.GetPlaceholder();

        /// <inheritdoc cref="ISettingsExtension.GetSettingsType"/>
        public SettingsType SettingsType => extension.GetSettingsType();
    }
}
