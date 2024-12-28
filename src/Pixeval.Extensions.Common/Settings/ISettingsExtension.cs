// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using FluentIcons.Common;

namespace Pixeval.Extensions.Common.Settings;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("3A4512B2-19FD-4950-9161-E1B622059F37")]
public partial interface ISettingsExtension : IExtension
{
    /// <summary>
    /// 设置项图标
    /// </summary>
    Symbol GetSymbol();

    /// <summary>
    /// 设置项名
    /// </summary>
    string GetName();

    /// <summary>
    /// 标题
    /// </summary>
    string GetHeader();

    /// <summary>
    /// 描述
    /// </summary>
    string GetDescription();

    /// <summary>
    /// 设置类型
    /// </summary>
    SettingsType GetSettingsType();
}
