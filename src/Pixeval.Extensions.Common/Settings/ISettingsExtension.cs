// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

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
    string? GetDescriptionUri();

    /// <summary>
    /// 设置项名
    /// </summary>
    string GetToken();

    /// <summary>
    /// 设置类型
    /// </summary>
    SettingsType GetSettingsType();
}
